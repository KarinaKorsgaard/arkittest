using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class LookAtControllerMixerBehaviour : PlayableBehaviour
{
	bool m_FirstFrameHappened = false;

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		Transform trackBinding = playerData as Transform;

		if(trackBinding == null)
			return;

		Vector3 defaultPosition = trackBinding.position;
		Quaternion defaultRotation = trackBinding.rotation;

		Vector3 defaultheadPosition = new Vector3(0f, 0f, 0f);
		Quaternion defaultheadRotation = new Quaternion(0f, 0f, 0f, 0f);

		int inputCount = playable.GetInputCount ();

		float positionTotalWeight = 0f;
		float rotationTotalWeight = 0f;

		Quaternion blendedRotation = new Quaternion(0f, 0f, 0f, 0f);
		Quaternion blendedheadRotation = new Quaternion(0f, 0f, 0f, 0f);
		Transform headTransform = trackBinding;


		for (int i = 0; i < inputCount; i++)
		{
			ScriptPlayable<LookAtControllerBehaviour> playableInput = (ScriptPlayable<LookAtControllerBehaviour>)playable.GetInput (i);
			LookAtControllerBehaviour input = playableInput.GetBehaviour ();

			headTransform = input.neckBone;

			if(input.endLocation == null)
				continue;

			float inputWeight = playable.GetInputWeight(i);

			input.startingPosition = defaultPosition;
			input.startingRotation = defaultRotation;

			if (!m_FirstFrameHappened) {
				m_FirstFrameHappened = true;
				Debug.Log("SET ROTATION");
				defaultheadRotation = input.neckBone.rotation;
				defaultheadPosition = input.neckBone.position;
			}

			float normalisedTime = (float)(playableInput.GetTime() * input.inverseDuration);
			float tweenProgress = input.currentCurve.Evaluate(normalisedTime);

			rotationTotalWeight += inputWeight;

			var lookPos = input.endLocation.position - input.startingPosition;
			lookPos.y = 0;
			var newRotation = Quaternion.LookRotation(lookPos);
			Quaternion desiredRotation = Quaternion.Lerp(input.startingRotation, newRotation, tweenProgress*tweenProgress*tweenProgress);

			var lookPoshead = input.endLocation.position - defaultheadPosition;
			var newheadRotation = Quaternion.LookRotation(lookPoshead);

			Quaternion desiredheadRotation = Quaternion.Lerp(defaultheadRotation, newheadRotation, tweenProgress);
			//Debug.Log (tweenProgress + " tweenprogress");

			desiredRotation = NormalizeQuaternion(desiredRotation);

			desiredheadRotation = NormalizeQuaternion(desiredheadRotation);

			if (Quaternion.Dot (blendedRotation, desiredRotation) < 0f)
				desiredRotation = ScaleQuaternion (desiredRotation, -1f);
			
			// head
			//if (Quaternion.Dot (blendedheadRotation, desiredheadRotation) < 0f)
			//	desiredheadRotation = ScaleQuaternion (desiredheadRotation, -1f);
			

			desiredRotation = ScaleQuaternion(desiredRotation, inputWeight);
			blendedRotation = AddQuaternions (blendedRotation, desiredRotation);

			desiredheadRotation = ScaleQuaternion(desiredheadRotation, inputWeight);
			blendedheadRotation = AddQuaternions (blendedheadRotation, desiredheadRotation);

			Quaternion weightedheadDefaultRotation = ScaleQuaternion (defaultheadRotation, 1f - rotationTotalWeight);
			blendedheadRotation = AddQuaternions (blendedheadRotation, weightedheadDefaultRotation);
		
			input.neckBone.rotation = blendedheadRotation;

			if (tweenProgress == 1.0f)
				m_FirstFrameHappened = false;
		
		}




		Quaternion weightedDefaultRotation = ScaleQuaternion (defaultRotation, 1f - rotationTotalWeight);
		blendedRotation = AddQuaternions (blendedRotation, weightedDefaultRotation);

		trackBinding.rotation = blendedRotation;
	}

	static Quaternion AddQuaternions (Quaternion first, Quaternion second)
	{
		first.w += second.w;
		first.x += second.x;
		first.y += second.y;
		first.z += second.z;
		return first;
	}

	static Quaternion ScaleQuaternion (Quaternion rotation, float multiplier)
	{
		rotation.w *= multiplier;
		rotation.x *= multiplier;
		rotation.y *= multiplier;
		rotation.z *= multiplier;
		return rotation;
	}

	static float QuaternionMagnitude (Quaternion rotation)
	{
		return Mathf.Sqrt ((Quaternion.Dot (rotation, rotation)));
	}

	static Quaternion NormalizeQuaternion (Quaternion rotation)
	{
		float magnitude = QuaternionMagnitude (rotation);

		if (magnitude > 0f)
			return ScaleQuaternion (rotation, 1f / magnitude);

		Debug.LogWarning ("Cannot normalize a quaternion with zero magnitude.");
		return Quaternion.identity;
	}
}
