//© Dicewrench Designs LLC 2018-2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;
using UnityEditor;

namespace DWD.MaterialManager.Editor
{
   [CustomPropertyDrawer(typeof(SearchFolder))]
   public class SearchFolderPropertyDrawer : PropertyDrawer
   {
      public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
      {
         return EditorGUIUtility.singleLineHeight * LineCount(property);
      }

      private float LineCount(SerializedProperty property)
      {
         return 1.0f;
      }

      private Rect left;
      private Rect middle;
      private Rect right;

      private void InitializeLineRects(Rect r, SerializedProperty p)
      {
         Rect baseLine = new Rect(r);
         left = new Rect(baseLine);

         left.width *= 0.5f;

         middle = new Rect(left);
         middle.width *= 0.5f;
         middle.x += left.width;

         right = new Rect(middle);
         right.x += middle.width;
      }

      public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
      {
         SerializedProperty folder = property.FindPropertyRelative("folder");
         SerializedProperty recursive = property.FindPropertyRelative("recursive");

         InitializeLineRects(position, property);

         EditorGUI.LabelField(left, new GUIContent(folder.stringValue, folder.stringValue));

         if (GUI.Button(middle, new GUIContent("Select", "Select a new folder path..."), EditorStyles.miniButton))
         {
            //Its ok if this is serialized where the path separators don't match your current dev platform...
            //we'll validate their format before we use them
            string absolutePath = EditorUtility.OpenFolderPanel("Select a folder", folder.stringValue, "");
            folder.stringValue = absolutePath.Substring(absolutePath.IndexOf("Assets/"));
         }

         recursive.boolValue = EditorGUI.ToggleLeft(right, new GUIContent("Recursive?", "Search this folder and all child folders?"), recursive.boolValue);
         property.serializedObject.ApplyModifiedProperties();
      }


   }
}