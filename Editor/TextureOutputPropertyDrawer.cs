//© Dicewrench Designs LLC 2026
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;
using UnityEditor;

namespace DWD.MaterialManager
{
    [CustomPropertyDrawer(typeof(TextureOutput))]
    public class TextureOutputPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // If the foldout is closed, just one line. 
            // If open, we need space for the suffix field + 4 channel drawers.
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            float height = EditorGUIUtility.singleLineHeight; // Foldout header
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Suffix field

            // Add heights of child properties (r, g, b, a channels)
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("rChannel"));
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("gChannel"));
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("bChannel"));
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("aChannel"));

            return height + (EditorGUIUtility.standardVerticalSpacing * 4);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                // 1. Determine the Label for Output Suffix
                string suffixLabel = "Output Suffix";
                string suffixTooltip = "The suffix to add to the output file.";
                if (property.serializedObject.targetObject is AbstractTexturePackerConfig)
                {
                    suffixLabel = "Output Property Name";
                    suffixTooltip = "The target Shader Property name to assign the newly packed texture to in the target Material.";
                }

                // 2. Draw the Suffix field
                SerializedProperty suffixProp = property.FindPropertyRelative("outputSuffix");
                Rect suffixRect = new Rect(position.x, foldoutRect.yMax + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(suffixRect, suffixProp, new GUIContent(suffixLabel, suffixTooltip));

                // 3. Draw the Channels
                float currentY = suffixRect.yMax + EditorGUIUtility.standardVerticalSpacing;

                string[] channels = { "rChannel", "gChannel", "bChannel", "aChannel" };
                foreach (string channelName in channels)
                {
                    SerializedProperty channelProp = property.FindPropertyRelative(channelName);
                    float h = EditorGUI.GetPropertyHeight(channelProp);
                    Rect r = new Rect(position.x, currentY, position.width, h);

                    EditorGUI.PropertyField(r, channelProp, true);
                    currentY += h + EditorGUIUtility.standardVerticalSpacing;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }
    }
}