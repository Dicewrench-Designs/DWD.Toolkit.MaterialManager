//© Dicewrench Designs LLC 2018-2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DWD.MaterialManager.Editor
{
    /// <summary>
    /// Static class with functions to manage <see cref="MaterialShaderVariantMaker"/>
    /// assets and output ShaderVariantCollection assets...
    /// </summary>
    public static class ShaderVariantMaker
   {
      [MenuItem("DWD/Update Material Shader Variant Makers")]
      public static void RefreshAllShaderVariantMakers()
      {
         string[] guids = AssetDatabase.FindAssets("t:MaterialShaderVariantMaker");
         int count = guids.Length;
         Debug.Log("Updating " + count + " Material Shader Variant Makers...");

         for (int a = 0; a < count; a++)
         {
            MaterialShaderVariantMaker msvm = AssetDatabase.LoadAssetAtPath<MaterialShaderVariantMaker>(AssetDatabase.GUIDToAssetPath(guids[a]));
            if (msvm != null)
            {
               SerializedObject o = new SerializedObject(msvm);
               SerializedProperty mats = o.FindProperty("sourceMaterials");
               ShaderVariantMaker.FindAndSetMaterialArray(mats, msvm.targetShaders, ShaderVariantMaker.SearchFoldersToPaths(msvm.searchFolders));
               SerializedProperty svc = o.FindProperty("targetShaderVariantCollection");
               if (svc.objectReferenceValue != null)
               {
                  RebuildVariants(svc.objectReferenceValue as ShaderVariantCollection, msvm.sourceMaterials);
               }
            }
         }
      }

      public static void FindAndSetMaterialArray(SerializedProperty array, Shader s, string[] folders = null)
      {
         if (array == null || array.serializedObject == null || s == null)
         {
            Debug.LogError("Cannot Set Material Array; one or more required elements are null...");
            return;
         }

         Material[] mats = FindMaterialArrayByShader(s, folders);
         SetMaterialArrayPropertyToArray(array, mats);
      }

      public static void FindAndSetMaterialArray(SerializedProperty array, Shader[] s, string[] folders = null)
      {
         if (array == null || array.serializedObject == null || s == null)
         {
            Debug.LogError("Cannot Set Material Array; one or more required elements are null...");
            return;
         }

         Material[] mats = FindMaterialArrayByShaders(s, folders);
         SetMaterialArrayPropertyToArray(array, mats);
      }

      public static void SetMaterialArrayPropertyToArray(SerializedProperty array, Material[] m)
      {
         array.ClearArray();
         int count = m.Length;

         for (int a = 0; a < count; a++)
         {
            array.InsertArrayElementAtIndex(a);
            SerializedProperty temp = array.GetArrayElementAtIndex(a);
            temp.objectReferenceValue = m[a];
         }

         array.serializedObject.ApplyModifiedProperties();
      }

      public static List<Material> FindMaterialListByShader(Shader s, string[] folders = null)
      {
         string[] guids;
         if (folders == null)
            guids = AssetDatabase.FindAssets("t:Material");
         else
            guids = AssetDatabase.FindAssets("t:Material", folders);

         int count = guids.Length;
         List<Material> mats = new List<Material>();

         for (int a = 0; a < count; a++)
         {
            Material m = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[a]), typeof(Material)) as Material;
            if (m != null)
            {
               if (m.shader == s)
                  mats.Add(m);
            }
         }

         return mats;
      }

      public static Material[] FindMaterialArrayByShader(Shader s, string[] folders = null)
      {
         return FindMaterialListByShader(s, folders).ToArray();
      }

      public static Material[] FindMaterialArrayByShaders(Shader[] s, string[] folders = null)
      {
         List<Material> combined = new List<Material>();
         int count = s.Length;

         for (int a = 0; a < count; a++)
         {
            List<Material> temp = FindMaterialListByShader(s[a], folders);
            int tempCount = temp.Count;

            for (int b = 0; b < tempCount; b++)
            {
               if (combined.Contains(temp[b]) == false)
                  combined.Add(temp[b]);
            }
         }

         return combined.ToArray();
      }

      public static int AppendShaderVariantCollectionWithMaterials(ShaderVariantCollection collection, Material[] m)
      {
         int additions = 0;
         int count = m.Length;

         for (int a = 0; a < count; a++)
         {
            ShaderVariantCollection.ShaderVariant[] matVariants = CreateVariantsArrayFromMaterial(m[a]);
            int varCount = matVariants.Length;

            for (int b = 0; b < varCount; b++)
            {
               if (collection.Add(matVariants[b]))
                  additions++;
            }
         }

         Debug.Log("Added " + additions + " Shader Variants to Collection [" + collection.name + "]");

         EditorUtility.SetDirty(collection);
         AssetDatabase.SaveAssets();
         AssetDatabase.Refresh();

         return additions;
      }

      public static List<ShaderVariantCollection.ShaderVariant> CreateVariantsListFromMaterial(Material m)
      {
         if (m == null)
            return null;

         List<ShaderVariantCollection.ShaderVariant> variants = new List<ShaderVariantCollection.ShaderVariant>();
         int passes = m.passCount;

         if (passes > 0)
         {
            for (int a = 0; a < passes; a++)
            {
               ShaderVariantCollection.ShaderVariant variant = CreateVariantFromMaterialPass(m, a);
               if (variants.Contains(variant) == false)
                  variants.Add(variant);
            }
         }
         else
         {
            ShaderVariantCollection.ShaderVariant variant = CreateVariantFromMaterialPass(m, null);
            if (variants.Contains(variant) == false)
               variants.Add(variant);
         }
         return variants;
      }

      public static ShaderVariantCollection.ShaderVariant[] CreateVariantsArrayFromMaterial(Material m)
      {
         List<ShaderVariantCollection.ShaderVariant> variants = CreateVariantsListFromMaterial(m);
         if (variants != null)
            return variants.ToArray();
         else
            return null;
      }

      public static ShaderVariantCollection.ShaderVariant CreateVariantFromMaterialPass(Material m, int? pass)
      {
         ShaderVariantCollection.ShaderVariant variant = new ShaderVariantCollection.ShaderVariant();
         if (m == null)
            return variant;

         variant.shader = m.shader;
         if (pass.HasValue)
            System.Enum.TryParse<UnityEngine.Rendering.PassType>(m.GetPassName(pass.GetValueOrDefault()), out variant.passType);
         else
            variant.passType = UnityEngine.Rendering.PassType.Normal;

         variant.keywords = m.shaderKeywords;
         return variant;
      }

      public static void RebuildVariants(ShaderVariantCollection collection, Material[] m)
      {
         collection.Clear();
         AppendShaderVariantCollectionWithMaterials(collection, m);
      }

      public static Material[] RemoveMaterialsWithShaderFromArray(Material[] m, Shader s)
      {
         List<Material> newMats = new List<Material>();
         int count = m.Length;

         for (int a = 0; a < count; a++)
         {
            Material temp = m[a];
            if (temp != null)
            {
               if (temp.shader != s)
               {
                  newMats.Add(temp);
               }
            }
         }
         return newMats.ToArray();
      }
      public static string[] SearchFoldersToPaths(SearchFolder[] folders)
      {
         int count = folders.Length;
         List<string> validPaths = new List<string>();

         for (int a = 0; a < count; a++)
         {
            SearchFolder temp = folders[a];
            AddRecursiveFolderList(ref validPaths, temp.folder, temp.recursive);
         }
         return validPaths.ToArray();
      }

      public static void AddRecursiveFolderList(ref List<string> folders, string parentFolder, bool recursive)
      {
         if (parentFolder.StartsWith(Application.dataPath))
            parentFolder = "Assets" + parentFolder.Substring(Application.dataPath.Length);

         if (recursive == false)
         {
            if (folders.Contains(parentFolder) == false)
            {
               folders.Add(parentFolder);
            }
            return;
         }
         else
         {
            string[] childFolders = System.IO.Directory.GetDirectories(parentFolder);
            int childCount = childFolders.Length;

            for (int a = 0; a < childCount; a++)
            {
               AddRecursiveFolderList(ref folders, childFolders[a], true);
            }
         }
      }
   }
}