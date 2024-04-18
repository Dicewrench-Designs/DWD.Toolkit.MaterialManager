//© Dicewrench Designs LLC 2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;
using UnityEditor;

namespace DWD.MaterialManager.Editor
{
    [CustomPropertyDrawer(typeof(ManageableMaterialProperty<>), true)]
    public class ManagedPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }


        private Rect _left;
        private Rect _right;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _left = new Rect(position);
            _left.width *= 0.5f;
            _right = new Rect(_left);
            _right.x += _left.width;

            if (property != null && property.objectReferenceValue != null)
            {
                BaseManageableMaterialProperty bmmp = property.objectReferenceValue as BaseManageableMaterialProperty;
                SerializedObject actualProp = new SerializedObject(bmmp);

                actualProp.Update();

                EditorGUIUtility.labelWidth = 105.0f;
                SerializedProperty name = actualProp.FindProperty("_materialPropertyName");
                EditorGUI.PropertyField(_left, name, true);

                SerializedProperty value = actualProp.FindProperty("_propertyValue");

                if (bmmp.GetMaterialPropertyType() != MaterialPropertyType.HDR)
                    EditorGUI.PropertyField(_right, value, true);
                else
                {
                    value.colorValue = EditorGUI.ColorField(_right, new GUIContent(value.displayName), value.colorValue, true, true, true);
                }
                EditorGUIUtility.labelWidth = 0.0f;
                actualProp.ApplyModifiedProperties();
            }
        }
    }
}
