//© Dicewrench Designs LLC 2019-2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;
using UnityEditor;

namespace DWD.MaterialManager.Editor
{
   [CustomEditor(typeof(MaterialCollection))]
   public class MaterialCollectionEditor : UnityEditor.Editor
   {
      private GUIStyle boxStyle;

      private void TryStyles()
      {
         if(boxStyle == null)
         {
            boxStyle = new GUIStyle(GUI.skin.box);
         }
      }

      public override void OnInspectorGUI()
      {
         TryStyles();

         serializedObject.Update();

         MaterialCollection mc = target as MaterialCollection;

         //draw managed property arrays
         using (new EditorGUILayout.VerticalScope(boxStyle))
         {
            using (new EditorGUILayout.HorizontalScope())
            {
               EditorGUILayout.LabelField("Managed Material Properties");

               if (GUILayout.Button(new GUIContent("Apply All Properties", "Applies each Properties current value to each Material."), EditorStyles.miniButton))
               {                 
                  mc.ApplyAllProperties();
               }
               if (GUILayout.Button(new GUIContent("Recalculate IDs", "Forces Shader Property IDs to update.  You 'SHOULDN'T' need this...."), EditorStyles.miniButton))
               {
                  mc.RecalculateIDs();
               }
            }
            EditorGUILayout.Space();

            DrawManagedPropertyArray(mc);

            EditorGUILayout.Space();

            using (new EditorGUILayout.VerticalScope(boxStyle))
            {
               EditorGUILayout.LabelField("Add New Manageable Property", EditorStyles.miniLabel);
               using (new EditorGUILayout.HorizontalScope())
               {
                  if(GUILayout.Button(new GUIContent("+ Float", "Add a Manageable Float Property"), EditorStyles.miniButton))
                  {
                     AddProperty(mc, typeof(ManageableFloatProperty));
                  }

                  if (GUILayout.Button(new GUIContent("+ Color", "Add a Manageable Color Property"), EditorStyles.miniButton))
                  {
                     AddProperty(mc, typeof(ManageableColorProperty));
                  }

                  if (GUILayout.Button(new GUIContent("+ HDR", "Add a Manageable HDR Color Property"), EditorStyles.miniButton))
                  {
                     AddProperty(mc, typeof(ManageableHDRProperty));
                  }

                  if (GUILayout.Button(new GUIContent("+ Vector", "Add a Manageable Vector Property"), EditorStyles.miniButton))
                  {
                     AddProperty(mc, typeof(ManageableVectorProperty));
                  }

                  if (GUILayout.Button(new GUIContent("+ Texture", "Add a Manageable Texture Property"), EditorStyles.miniButton))
                  {
                     AddProperty(mc, typeof(ManageableTextureProperty));
                  }

                  if (GUILayout.Button(new GUIContent("+ Keyword", "Add a Manageable Keyword Property.  Make sure your property and keyword are named correctly!  i.e. _Emission_Enabled & _EMISSION_ENABLED"), EditorStyles.miniButton))
                  {
                     AddProperty(mc, typeof(ManageableKeywordProperty));
                  }
               }
            }
         }

         //draw material array
         using (new EditorGUILayout.VerticalScope(boxStyle))
         {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("materials"), true);
         }

         serializedObject.ApplyModifiedProperties();
      }

      private void DrawManagedPropertyArray(MaterialCollection mc)
      {
         SerializedProperty array = serializedObject.FindProperty("managedProperties");

         int count = array.arraySize;

         for(int a = 0; a < count; a++)
         {
            SerializedObject element = new SerializedObject(array.GetArrayElementAtIndex(a).objectReferenceValue);
            element.Update();

            if (element != null)
            {
               SerializedProperty elementPropertyName = element.FindProperty("_materialPropertyName");

               using (new EditorGUILayout.HorizontalScope(boxStyle))
               {
                  EditorGUILayout.LabelField(mc.managedProperties[a].name, EditorStyles.miniLabel, GUILayout.Width(155.0f));

                  BaseManageableMaterialProperty bmmp = element.targetObject as BaseManageableMaterialProperty;
                  MaterialPropertyType mpt = bmmp.GetMaterialPropertyType();

                  EditorGUI.BeginChangeCheck();
                  EditorGUIUtility.labelWidth = 105.0f;
                  EditorGUILayout.PropertyField(elementPropertyName, new GUIContent("Property Name", "The Shader name of the Property you want to manage."));
                  
                  EditorGUIUtility.labelWidth = 0.0f;
                  if (EditorGUI.EndChangeCheck())
                  {
                     element.ApplyModifiedProperties();
                     bmmp.OnPropertyNameChanged();
                  }

                  EditorGUI.BeginChangeCheck();
                  if(mpt != MaterialPropertyType.HDR)
                     EditorGUILayout.PropertyField(element.FindProperty("_propertyValue"), GUIContent.none, GUILayout.Width(100.0f));
                  else
                  {
                     SerializedProperty propVal = element.FindProperty("_propertyValue");
                     propVal.colorValue = EditorGUILayout.ColorField(
                        GUIContent.none,
                        propVal.colorValue,
                        showEyedropper: true,
                        showAlpha: true,
                        hdr: true,
                        GUILayout.Width(100.0f));
                  }
                  if (EditorGUI.EndChangeCheck())
                  {
                     element.ApplyModifiedProperties();
                     mc.ApplyProperty(bmmp);
                  }

                  if (GUILayout.Button(new GUIContent("-", "Remove this Property"), EditorStyles.miniButton, GUILayout.Width(18.0f), GUILayout.Height(18.0f)))
                  {
                     RemoveProperty(mc, a);
                  }
               }
            }
         }
      }

      /// <summary>
      /// Create a ManageableMaterialProperty of a given Type and serialize it within
      /// our MaterialCollection
      /// </summary>
      /// <param name="mc"></param>
      /// <param name="t"></param>
      private void AddProperty(MaterialCollection mc, System.Type t)
      {
         BaseManageableMaterialProperty temp = ScriptableObject.CreateInstance(t) as BaseManageableMaterialProperty;
         temp.name = t.Name;
         temp.hideFlags = HideFlags.HideInHierarchy;

         AddToArray<BaseManageableMaterialProperty>(ref mc.managedProperties, temp);

         string path = AssetDatabase.GetAssetPath(mc);
         AssetDatabase.AddObjectToAsset(temp, path);
         AssetDatabase.ImportAsset(path);

         EditorUtility.SetDirty(mc);
         serializedObject.ApplyModifiedProperties();
         serializedObject.Update();
         Repaint();
      }

      private void RemoveProperty(MaterialCollection mc, int index)
      {
         BaseManageableMaterialProperty temp = mc.managedProperties[index];
         RemoveFromArray<BaseManageableMaterialProperty>(ref mc.managedProperties, index);

         UnityEngine.Object.DestroyImmediate(temp, true);
         AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(mc));
         AssetDatabase.SaveAssets();

         EditorUtility.SetDirty(mc);
         serializedObject.ApplyModifiedProperties();
         serializedObject.Update();
         Repaint();
      }

      /// <summary>
      /// Add a thing to an array.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="array"></param>
      /// <param name="temp"></param>
      private void AddToArray<T>(ref T[] array, T temp)
      {
         int count;

         if (array == null)
         {
            array = new T[0];
            count = 0;
         }
         else
            count = array.Length;

         int newCount = count + 1;
         T[] newArray = new T[newCount];

         if (newCount == 1)
         {
            newArray[0] = temp;
         }
         else
         {
            for (int a = 0; a < count; a++)
            {
               newArray[a] = array[a];
            }
            newArray[newCount - 1] = temp;
         }

         array = newArray;
      }

      /// <summary>
      /// Remove a thing from an array.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="array"></param>
      /// <param name="index"></param>
      private void RemoveFromArray<T>(ref T[] array, int index)
      {
         int count = array.Length;
         int newCount = count - 1;

         int currentIndex = 0;

         T[] newArray = new T[newCount];

         for (int a = 0; a < count; a++)
         {
            if (a != index)
            {
               newArray[currentIndex] = array[a];
               currentIndex += 1;
            }
         }

         array = newArray;
      }
   }
}