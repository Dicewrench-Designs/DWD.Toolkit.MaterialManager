//© Dicewrench Designs LLC 2018-2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;
using UnityEditor;

namespace DWD.MaterialManager.Editor
{
   [CustomEditor(typeof(MaterialShaderVariantMaker))]
   public class MaterialShaderVariantMakerEditor : UnityEditor.Editor
   {
      private GUIStyle boxStyle;

      private Vector2 scroll;

      private int modCount = 0;

      private void TryStyles()
      {
         if (boxStyle == null)
            boxStyle = new GUIStyle(GUI.skin.box);
      }

      public override void OnInspectorGUI()
      {
         serializedObject.Update();

         TryStyles();

         SerializedProperty svc = serializedObject.FindProperty("targetShaderVariantCollection");
         SerializedProperty s = serializedObject.FindProperty("targetShaders");
         SerializedProperty f = serializedObject.FindProperty("searchFolders");
         SerializedProperty m = serializedObject.FindProperty("sourceMaterials");

         EditorGUILayout.HelpBox("The Material Shader Variant Maker will help you update and apply complex Shader Variant Collections by scraping keywords off of an array of Materials.  This list is validated at build time to ensure no Mats are built that won't also have their correct Shaders built, so make sure you keep your Maker, Materials and Collections in sync!", MessageType.Info);
         EditorGUILayout.Space();
         if(f.arraySize == 0)
            EditorGUILayout.HelpBox("Be careful! Since you have no Search Folders specified automatic Material finding will get EVERY Material in the entire Project that is using that Shader!", MessageType.Warning);
         EditorGUILayout.Space();

         bool svcPresent = svc.objectReferenceValue != null;
         bool readyToAppend = svcPresent && m.arraySize > 0;
         bool hasMods = modCount > 0;

         using (new EditorGUILayout.VerticalScope(boxStyle))
         {
            EditorGUI.BeginDisabledGroup(!readyToAppend);

            Color old = GUI.backgroundColor;
            GUI.backgroundColor = hasMods ? Color.yellow : old;
            if (GUILayout.Button(new GUIContent("Append Materials to Collection", "Add any new Variants from Material List to Shader Variant Collection"), GUILayout.Height(32.0f)))
            {
               MaterialShaderVariantMaker msvm = target as MaterialShaderVariantMaker;
               ShaderVariantMaker.RebuildVariants(svc.objectReferenceValue as ShaderVariantCollection, msvm.sourceMaterials);
               modCount = 0;
            }
            GUI.backgroundColor = old;

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();

            if (svcPresent == false)
               EditorGUILayout.HelpBox("Assign a Shader Variant Collection to write to.", MessageType.Warning);

            EditorGUILayout.PropertyField(svc, true);
            serializedObject.ApplyModifiedProperties();
         }

         using (var scrollview = new EditorGUILayout.ScrollViewScope(scroll))
         {
            scroll = scrollview.scrollPosition;

            using (new EditorGUILayout.VerticalScope(boxStyle))
            {
               bool shaderPresent = s.arraySize > 0;

               if (shaderPresent == false)
                  EditorGUILayout.HelpBox("Assign Shaders to find all Materials in the project using them.", MessageType.Info);

               DrawShaderArray(s);

            }

            using(new EditorGUILayout.VerticalScope(boxStyle))
            {
               string label = "Search Folders (" + f.arraySize + ")";
               EditorGUILayout.PropertyField(f, new GUIContent(label, "Folders to pick Materials with Target Shaders from."), true);
            }

            using (new EditorGUILayout.VerticalScope(boxStyle))
            {
               DrawMaterialControls();
               string label = "Source Materials (" + m.arraySize + ")";
               EditorGUILayout.PropertyField(m, new GUIContent(label, "Materials to convert into Shader Variants"), true);
            }
         }

         serializedObject.ApplyModifiedProperties();
      }

      private void DrawShaderArray(SerializedProperty array)
      {
         serializedObject.Update();

         bool isExpanded = EditorGUILayout.Foldout(array.isExpanded, new GUIContent(array.displayName, array.tooltip));
         array.isExpanded = isExpanded;

         if(isExpanded)
         {
            EditorGUI.indentLevel += 1;

            array.arraySize = EditorGUILayout.IntField(new GUIContent("Size"), array.arraySize);

            for(int a = 0; a < array.arraySize; a++)
            {
               DrawShaderControls(array, a);
            }

            EditorGUI.indentLevel -= 1;
         }

         serializedObject.ApplyModifiedProperties();
      }

      private void DrawShaderControls(SerializedProperty array, int index)
      {
         SerializedProperty shader = array.GetArrayElementAtIndex(index);

         using(new EditorGUILayout.HorizontalScope(boxStyle))
         {
            EditorGUIUtility.labelWidth = 100.0f;
            string label = shader.objectReferenceValue == null ? "Shader" : shader.objectReferenceValue.name;

            EditorGUILayout.PropertyField(shader, new GUIContent(label, label));

            EditorGUIUtility.labelWidth = 0.0f;

            bool hasShader = shader.objectReferenceValue != null;

            EditorGUI.BeginDisabledGroup(!hasShader);

            if(GUILayout.Button(new GUIContent("Find Mats", "Find ALL Materials in search folders using this Shader."), EditorStyles.miniButtonLeft, GUILayout.Width(65.0f)))
            {
               MaterialShaderVariantMaker msvm = target as MaterialShaderVariantMaker;
               SerializedProperty mats = serializedObject.FindProperty("sourceMaterials");
               ShaderVariantMaker.FindAndSetMaterialArray(mats, msvm.targetShaders, ShaderVariantMaker.SearchFoldersToPaths(msvm.searchFolders));
               modCount++;
            }
            if (GUILayout.Button(new GUIContent("Remove", "Remove this Shader from the Array and update the Material List."), EditorStyles.miniButtonLeft, GUILayout.Width(65.0f)))
            {
               array.DeleteArrayElementAtIndex(index);
               serializedObject.ApplyModifiedProperties();
               serializedObject.Update();
               MaterialShaderVariantMaker msvm = target as MaterialShaderVariantMaker;
               SerializedProperty mats = serializedObject.FindProperty("sourceMaterials");
               ShaderVariantMaker.FindAndSetMaterialArray(mats, msvm.targetShaders, ShaderVariantMaker.SearchFoldersToPaths(msvm.searchFolders));
               modCount++;
            }

            EditorGUI.EndDisabledGroup();
         }
      }

      private void DrawMaterialControls()
      {
         using(new EditorGUILayout.HorizontalScope())
         {
            if(GUILayout.Button(new GUIContent("Refresh", "Clears and Re-finds Materials."), EditorStyles.miniButton))
            {
               MaterialShaderVariantMaker msvm = target as MaterialShaderVariantMaker;
               SerializedProperty materials = serializedObject.FindProperty("sourceMaterials");
               ShaderVariantMaker.FindAndSetMaterialArray(materials, msvm.targetShaders, ShaderVariantMaker.SearchFoldersToPaths(msvm.searchFolders));
               modCount++;
            }
         }
      }
   }
}