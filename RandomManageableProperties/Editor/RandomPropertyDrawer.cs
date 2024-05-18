//© Dicewrench Designs LLC 2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;
using UnityEditor;

namespace DWD.MaterialManager.Editor
{
    [CustomPropertyDrawer(typeof(RandomManageableProperty<>), true)]
    public class RandomPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2.0f;
        }

        private Rect _top;
        private Rect _left;
        private Rect _right;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _top = new Rect(position);
            _top.height *= 0.5f;
            _left = new Rect(_top);
            _left.y += _top.height;
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
                EditorGUI.PropertyField(_top, name, true);

                SerializedProperty value = actualProp.FindProperty("_propertyValue");
                SerializedProperty second = actualProp.FindProperty("_secondValue");

                if (bmmp.GetMaterialPropertyType() != MaterialPropertyType.HDR)
                {
                    EditorGUI.PropertyField(_left, value, true);
                    EditorGUI.PropertyField(_right, second, true);
                }
                else
                {
                    value.colorValue = EditorGUI.ColorField(_left, new GUIContent(value.displayName), value.colorValue, true, true, true);
                    value.colorValue = EditorGUI.ColorField(_right, new GUIContent(value.displayName), value.colorValue, true, true, true);
                }
                EditorGUIUtility.labelWidth = 0.0f;
                actualProp.ApplyModifiedProperties();
            }
        }
    }
}
