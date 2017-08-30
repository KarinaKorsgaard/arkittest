using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(LookatControllerBehaviour))]
public class LookatControllerDrawer : PropertyDrawer
{
    public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
    {
        int fieldCount = 2;
        return fieldCount * EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty speedProp = property.FindPropertyRelative("speed");
        SerializedProperty followProp = property.FindPropertyRelative("follow");

        Rect singleFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(singleFieldRect, speedProp);

        singleFieldRect.y += EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(singleFieldRect, followProp);
    }
}