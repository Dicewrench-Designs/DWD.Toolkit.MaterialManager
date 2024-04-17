//© Dicewrench Designs LLC 2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;
using UnityEditor;

namespace DWD.MaterialManager.Editor
{
    [CustomPropertyDrawer(typeof(SealedMaterialProperty<>), true)]
    public class SealedPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty name = property.FindPropertyRelative("_materialPropertyName");

            GUIContent propLabel = new GUIContent(name.stringValue);

            SerializedProperty value = property.FindPropertyRelative("_propertyValue");
            EditorGUI.PropertyField(position, value, propLabel, true);
        }
    }
}