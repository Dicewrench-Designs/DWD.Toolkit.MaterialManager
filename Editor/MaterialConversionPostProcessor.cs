//© Dicewrench Designs LLC 2025
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DWD.MaterialManager.Editor
{
    /// <summary>
    /// Caches pending texture-to-material assignments.
    /// Key: The full asset path of the texture being imported (e.g., "Assets/MyFolder/MyTex.png")
    /// Value: A struct containing the material to assign to and the property name to set.
    /// </summary>
    public static class MaterialConversionProcessorCache
    {
        public struct PendingTextureAssignment
        {
            public Material TargetMaterial;
            public string PropertyName;
        }

        public static readonly Dictionary<string, PendingTextureAssignment> PendingAssignments =
            new Dictionary<string, PendingTextureAssignment>();

        public static void AddPendingAssignment(string assetPath, Material material, string propertyName)
        {
            if (material == null || string.IsNullOrEmpty(propertyName))
            {
                return;
            }

            // Normalize path for consistent dictionary keys
            string normalizedPath = assetPath.Replace(System.IO.Path.DirectorySeparatorChar, '/');
            PendingAssignments[normalizedPath] = new PendingTextureAssignment
            {
                TargetMaterial = material,
                PropertyName = propertyName
            };
        }

        public static void ClearPendingAssignments()
        {
            PendingAssignments.Clear();
        }
    }

    /// <summary>
    /// AssetPostprocessor that checks for imported textures that need to be
    /// assigned to a material by the MaterialConverter.
    /// </summary>
    public class MaterialConversionPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (MaterialConversionProcessorCache.PendingAssignments.Count == 0)
            {
                return;
            }

            bool assetsDirtied = false;

            foreach (string path in importedAssets)
            {
                // Check if this newly imported asset is one we're waiting for
                if (MaterialConversionProcessorCache.PendingAssignments.TryGetValue(path, out var assignment))
                {
                    // Found it. Load the texture (it's guaranteed to be ready now)
                    Texture2D packedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

                    if (packedTexture != null && assignment.TargetMaterial != null)
                    {
                        // Assign the texture to the material
                        assignment.TargetMaterial.SetTexture(assignment.PropertyName, packedTexture);
                        EditorUtility.SetDirty(assignment.TargetMaterial);
                        assetsDirtied = true;
                    }

                    // Remove from the dictionary so we don't process it again
                    MaterialConversionProcessorCache.PendingAssignments.Remove(path);
                }
            }

            if (assetsDirtied && MaterialConversionProcessorCache.PendingAssignments.Count == 0)
            {
                // If we're done with all pending assignments, save the dirtied materials
                AssetDatabase.SaveAssets();
                Debug.Log("MaterialConversionPostprocessor: All pending textures assigned and assets saved.");
            }
        }
    }
}