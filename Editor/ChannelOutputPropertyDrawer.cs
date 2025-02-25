//© Dicewrench Designs LLC 2025
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DWD.MaterialManager
{
    [CustomPropertyDrawer(typeof(ChannelOutput))]
    public class ChannelOutputPropertyDrawer :PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded == false)
                return EditorGUIUtility.singleLineHeight;
            else
            {
                return (EditorGUIUtility.singleLineHeight * 4) + (EditorGUIUtility.standardVerticalSpacing * 3);
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            
            SerializedProperty enabled = property.FindPropertyRelative(nameof(ChannelOutput.enabled));
            SerializedProperty texSource = property.FindPropertyRelative(nameof(ChannelOutput.textureSource));
            SerializedProperty channel = property.FindPropertyRelative(nameof(ChannelOutput.channel));

            SerializedProperty texArray = property.serializedObject.FindProperty("_inputTextures");

            Rect one = new Rect(position);
            one.height = EditorGUIUtility.singleLineHeight;

            property.isExpanded = EditorGUI.Foldout(one, property.isExpanded, property.displayName);

            if (property.isExpanded)
            {
                Rect two = new Rect(one);
                two.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.PropertyField(two, enabled, true);

                Rect three = new Rect(two);
                three.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                if (texArray != null && texArray.isArray && texArray.arraySize > 0)
                {
                    GUIContent[] options = GetOptions(texArray);
                    texSource.intValue = EditorGUI.Popup(three, texSource.intValue, options);
                }
                else
                    EditorGUI.PropertyField(three, texSource, true);

                Rect four = new Rect(three);
                four.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(four, channel, true);
            }
        }

        private GUIContent[] GetOptions(SerializedProperty prop)
        {
            if (prop.isArray == false)
                return null;
            if (prop.arraySize == 0)
                return null;

            List<GUIContent> options = new List<GUIContent>();
            for (int a = 0; a < prop.arraySize; a++)
            {
                SerializedProperty element = prop.GetArrayElementAtIndex(a);
                if (element != null)
                {
                    string val = "";
                    if (element.objectReferenceValue != null)
                        val = "[" + a + "] " + element.objectReferenceValue.name;
                    else
                        val = "[" + a + "] Element Currently Empty";
                    options.Add(new GUIContent(val, val));
                }
            }
            return options.ToArray();
        }

    }
}