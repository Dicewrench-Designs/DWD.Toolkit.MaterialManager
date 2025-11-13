//© Dicewrench Designs LLC 2025
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
using System.Collections.Generic;
using System.IO; // Required for Path
using UnityEditorInternal; // Required for ReorderableList

namespace DWD.MaterialManager.Editor
{
    /// <summary>
    /// Custom Editor for the <see cref="MaterialConverter"/> ScriptableObject.
    /// Provides a custom interface for mapping shader properties.
    /// </summary>
    [CustomEditor(typeof(MaterialConverter))]
    public class MaterialConverterEditor : UnityEditor.Editor
    {
        // Target
        private MaterialConverter _target;

        // SerializedProperties
        private SerializedProperty _sourceShaderProp;
        private SerializedProperty _destinationShaderProp;
        private SerializedProperty _propertyMapProp;

        // ReorderableList for the property map
        private ReorderableList _propertyMapList;

        /// <summary>
        /// Helper struct to store both display and internal shader property names.
        /// </summary>
        private struct PropertyNameData
        {
            public string DisplayName;
            public string InternalName;
        }

        // Caches for shader properties, sorted by type
        private Dictionary<ShaderPropertyType, List<PropertyNameData>> _sourceShaderProperties;
        private Dictionary<ShaderPropertyType, List<PropertyNameData>> _destShaderProperties;

        // Tracked shaders to detect changes
        private Shader _cachedSourceShader;
        private Shader _cachedDestShader;

        public static GUIStyle headerStyle;

        private void TryStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle();
                headerStyle.alignment = TextAnchor.MiddleCenter;
                headerStyle.normal.textColor = Color.white;
                headerStyle.fontStyle = FontStyle.Bold;
                headerStyle.fontSize = 16;
            }
        }

        private void OnEnable()
        {
            // Get target
            _target = (MaterialConverter)target;

            // Find properties
            _sourceShaderProp = serializedObject.FindProperty("_sourceShader");
            _destinationShaderProp = serializedObject.FindProperty("_destinationShader");
            _propertyMapProp = serializedObject.FindProperty("_propertyMap");

            // Initialize caches
            _sourceShaderProperties = new Dictionary<ShaderPropertyType, List<PropertyNameData>>();
            _destShaderProperties = new Dictionary<ShaderPropertyType, List<PropertyNameData>>();

            // Build initial cache
            UpdateShaderCache(true);
            UpdateShaderCache(false);

            // Set up the ReorderableList
            SetupPropertyMapList();
        }

        /// <summary>
        /// Configures the ReorderableList for drawing the property map.
        /// </summary>
        private void SetupPropertyMapList()
        {
            _propertyMapList = new ReorderableList(serializedObject, _propertyMapProp, true, true, true, true);

            // Draw header
            _propertyMapList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Property Map");
            };

            // Draw element
            _propertyMapList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = _propertyMapList.serializedProperty.GetArrayElementAtIndex(index);
                DrawPropertyMapElement(rect, element);
            };

            // Calculate element height
            _propertyMapList.elementHeightCallback = (int index) =>
            {
                SerializedProperty element = _propertyMapProp.GetArrayElementAtIndex(index);
                if (element == null) return EditorGUIUtility.singleLineHeight;

                SerializedProperty typeProp = element.FindPropertyRelative("propertyType");
                if (typeProp == null) return EditorGUIUtility.singleLineHeight;

                // Get the enum value
                ShaderPropertyType type = (ShaderPropertyType)typeProp.intValue;

                if (type == ShaderPropertyType.Texture)
                    return (EditorGUIUtility.singleLineHeight * 4) + (EditorGUIUtility.standardVerticalSpacing * 3);
                else
                    return (EditorGUIUtility.singleLineHeight * 3) + (EditorGUIUtility.standardVerticalSpacing * 2);
            };
        }

        public override void OnInspectorGUI()
        {
            TryStyles();

            serializedObject.Update();

            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Texture Packer Config", EditorStyles.centeredGreyMiniLabel);
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(serializedObject.targetObject.name, headerStyle);
                    EditorGUILayout.Space();
                }
                EditorGUILayout.Space();
            }

            DrawShaderFields();

            EditorGUILayout.Space();

            _propertyMapList.DoLayoutList();

            EditorGUILayout.Space();

            DrawConvertButtons();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws the Source and Destination Shader fields and checks for changes.
        /// </summary>
        private void DrawShaderFields()
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Shader Targets", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;
                // Check for change in Source Shader
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(_sourceShaderProp);
                // Rebuild cache if shader changed or was just assigned
                if (EditorGUI.EndChangeCheck() || _cachedSourceShader != _target.SourceShader)
                {
                    _cachedSourceShader = _target.SourceShader;
                    UpdateShaderCache(true);
                }

                // Check for change in Destination Shader
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(_destinationShaderProp);
                // Rebuild cache if shader changed or was just assigned
                if (EditorGUI.EndChangeCheck() || _cachedDestShader != _target.DestinationShader)
                {
                    _cachedDestShader = _target.DestinationShader;
                    UpdateShaderCache(false);
                }
                EditorGUI.indentLevel--;
            }
        }

        /// <summary>
        /// Draws a single element of the property map list.
        /// </summary>
        private void DrawPropertyMapElement(Rect rect, SerializedProperty element)
        {
            // Get properties for this element
            SerializedProperty typeProp = element.FindPropertyRelative("propertyType");
            SerializedProperty nameProp = element.FindPropertyRelative("propertyName");
            SerializedProperty destNameProp = element.FindPropertyRelative("destinationName");
            SerializedProperty packerProp = element.FindPropertyRelative("packerConfig");

            ShaderPropertyType currentType = (ShaderPropertyType)typeProp.intValue;

            // --- Line 1: Property Type Enum ---
            Rect lineRect = new Rect(rect.x, rect.y + 2 + (EditorGUIUtility.singleLineHeight * 0.5f), rect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(lineRect, typeProp, new GUIContent("Property Type"));

            // --- Line 2: Source and Destination Dropdowns ---
            lineRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Get the arrays for the dropdowns
            string[] sourceDisplayNames = GetPropertyDisplayNames(true, currentType);
            string[] sourceInternalNames = GetPropertyInternalNames(true, currentType);
            string[] destDisplayNames = GetPropertyDisplayNames(false, currentType);
            string[] destInternalNames = GetPropertyInternalNames(false, currentType);

            // Find current selected index
            int sourceIndex = GetPopupIndex(nameProp.stringValue, sourceInternalNames);
            int destIndex = GetPopupIndex(destNameProp.stringValue, destInternalNames);

            Rect leftRect = new Rect(lineRect.x, lineRect.y, lineRect.width / 2 - 2, lineRect.height);
            Rect rightRect = new Rect(lineRect.x + lineRect.width / 2 + 2, lineRect.y, lineRect.width / 2 - 2, lineRect.height);

            using (new EditorGUILayout.HorizontalScope())
            {
                // Draw Source Property Dropdown
                EditorGUI.BeginChangeCheck();
                int newSourceIndex = EditorGUI.Popup(leftRect, sourceIndex, sourceDisplayNames);
                if (EditorGUI.EndChangeCheck())
                {
                    nameProp.stringValue = sourceInternalNames[newSourceIndex];
                }

                // Draw Destination Property Dropdown
                EditorGUI.BeginChangeCheck();
                int newDestIndex = EditorGUI.Popup(rightRect, destIndex, destDisplayNames);
                if (EditorGUI.EndChangeCheck())
                {
                    destNameProp.stringValue = destInternalNames[newDestIndex];
                }
            }
            // --- Line 3 (Conditional): Texture Packer Config ---
            if (currentType == ShaderPropertyType.Texture)
            {
                lineRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(lineRect, packerProp, new GUIContent("Packer Config"));
            }
        }

        /// <summary>
        /// Rebuilds the cache of shader properties for either the source or destination shader.
        /// </summary>
        private void UpdateShaderCache(bool isSource)
        {
            Shader shader = isSource ? _target.SourceShader : _target.DestinationShader;
            var cache = isSource ? _sourceShaderProperties : _destShaderProperties;

            cache.Clear();
            if (shader == null) return;

            int propertyCount = ShaderUtil.GetPropertyCount(shader);
            for (int i = 0; i < propertyCount; i++)
            {
                // **FIX:** Explicitly cast from UnityEditor.ShaderUtil.ShaderPropertyType
                // to UnityEngine.Rendering.ShaderPropertyType
                ShaderPropertyType type = (ShaderPropertyType)ShaderUtil.GetPropertyType(shader, i);

                // **FIX:** Use GetPropertyDescription for 2021 compatibility
                string displayName = ShaderUtil.GetPropertyDescription(shader, i);
                string internalName = ShaderUtil.GetPropertyName(shader, i);

                // Group Range with Float, as they are both set/get via Material.GetFloat()
                if (type == ShaderPropertyType.Range)
                {
                    type = ShaderPropertyType.Float;
                }

                if (!cache.ContainsKey(type))
                {
                    // Add the "None" option first
                    cache[type] = new List<PropertyNameData> { new PropertyNameData { DisplayName = "None", InternalName = "" } };
                }

                // Add the property, showing both display and internal name for clarity
                cache[type].Add(new PropertyNameData { DisplayName = $"{displayName} ({internalName})", InternalName = internalName });
            }
        }

        /// <summary>
        /// Finds all selected Materials in the Project window and converts them.
        /// </summary>
        private void ConvertSelectedMaterials()
        {
            Object[] selectedAssets = Selection.GetFiltered(typeof(Material), SelectionMode.Assets);

            if (selectedAssets.Length == 0)
            {
                Debug.LogWarning("No Materials selected in the Project window.");
                return;
            }

            int convertedCount = Convert(selectedAssets);

            Debug.Log($"Successfully converted {convertedCount} of {selectedAssets.Length} selected materials. " +
                $"Pending texture assignments will be processed by the AssetPostprocessor.");
        }

        /// <summary>
        /// Finds all the <see cref="Materials"/> in the Project
        /// using the Source Shader and Converts them.
        /// </summary>
        private void ScrapeAndConvertMaterials()
        {
            List<Material> allParts = new List<Material>();

            string[] guids = AssetDatabase.FindAssets("t:Material");

            string tempPath = "";
            int count = guids.Length;
            for (int a = 0; a < count; a++)
            {
                tempPath = AssetDatabase.GUIDToAssetPath(guids[a]);
                Material o = AssetDatabase.LoadAssetAtPath<Material>(tempPath) as Material;
                if (o != null && o.shader == _target.SourceShader) //prelim check so we get a more real count
                    allParts.Add(o);
            }

            if (allParts.Count == 0)
            {
                Debug.LogWarning("No Materials found in the Project with the Source Shader.");
                return;
            }

            int convertedCount = Convert(allParts.ToArray());

            Debug.Log($"Successfully converted {convertedCount} of {allParts.Count} found materials. " +
                $"Pending texture assignments will be processed by the AssetPostprocessor.");
        }

        private int Convert(Object[] objects)
        {
            Undo.RecordObjects(objects, "Convert Materials");

            MaterialConversionProcessorCache.ClearPendingAssignments();

            var processedTextureCache = new Dictionary<string, string>();
            int convertedCount = 0;

            string progressBarTitle = "Material Conversion";
            EditorUtility.DisplayProgressBar(progressBarTitle, "Starting conversion...", 0f);

            AssetDatabase.StartAssetEditing(); // Performance boost
            try
            {
                for (int i = 0; i < objects.Length; i++)
                {
                    Object obj = objects[i];
                    Material mat = obj as Material;
                    if (mat == null) continue;

                    float progress = (float)i / objects.Length;
                    string info = $"Processing: {mat.name} ({i + 1}/{objects.Length})";
                    EditorUtility.DisplayProgressBar(progressBarTitle, info, progress);

                    if (ConvertMaterial(mat, processedTextureCache) != null)
                    {
                        convertedCount++;
                        EditorUtility.SetDirty(mat); // Mark material as dirty to save changes
                    }
                }
            }
            finally
            {
                EditorUtility.DisplayProgressBar(progressBarTitle, "Finalizing...", 1f);
                AssetDatabase.StopAssetEditing(); // Stop editing and save changes
                // No SaveAssets() or Refresh() needed here, the Postprocessor will handle it.
                EditorUtility.ClearProgressBar();
            }
            return convertedCount;
        }

        private Material _workingMaterial = null;
        /// <summary>
        /// Converts a material's shader and syncs properties based on the map.
        /// This modifies the material in-place.
        /// </summary>
        /// <param name="sourceMaterial">The material to convert.</param>
        /// <returns>The modified sourceMaterial, or null if the shader was incorrect.</returns>
        public Material ConvertMaterial(Material sourceMaterial, Dictionary<string, string> processedTextureCache)
        {
            // _target is the MaterialConverter ScriptableObject
            if (sourceMaterial == null)
            {
                Debug.LogWarning("Cannot convert a null material.");
                return null;
            }

            // 1. Check if the material's shader is the one we're converting FROM.
            if (sourceMaterial.shader != _target.SourceShader)
            {
                Debug.LogWarningFormat(sourceMaterial,
                    "Material '{0}' does not use the expected source shader '{1}'. Conversion skipped.",
                    sourceMaterial.name, _target.SourceShader.name);
                return null;
            }

            _workingMaterial = new Material(sourceMaterial);
            sourceMaterial.shader = _target.DestinationShader;

            // 3. Sync properties FROM the temporary block (using old names)
            //    TO the material asset (using new names).
            //    This is the inlined MaterialSyncer.Sync logic.
            int count = _target.PropertyMap.Length;
            for (int a = 0; a < count; a++)
            {
                ShaderPropertyTypePair prop = _target.PropertyMap[a];

                // We read from the MaterialPropertyBlock using the source propertyName
                // and write to the Material asset using the destinationName.
                // We omit the 'HasProperty' check as Get...() on a MaterialPropertyBlock
                // will safely return default values (0, null, etc.) if the property
                // wasn't in the block, which is the desired behavior.
                switch (prop.propertyType)
                {
                    case ShaderPropertyType.Float:
                    case ShaderPropertyType.Range: // Treat Range as Float
                        sourceMaterial.SetFloat(prop.destinationName, _workingMaterial.GetFloat(prop.propertyName));
                        break;
                    case ShaderPropertyType.Int:
                        sourceMaterial.SetInt(prop.destinationName, _workingMaterial.GetInt(prop.propertyName));
                        break;
                    case ShaderPropertyType.Color:
                        sourceMaterial.SetColor(prop.destinationName, _workingMaterial.GetColor(prop.propertyName));
                        break;
                    case ShaderPropertyType.Vector:
                        sourceMaterial.SetVector(prop.destinationName, _workingMaterial.GetVector(prop.propertyName));
                        break;
                    case ShaderPropertyType.Texture:
                        if (prop.packerConfig != null)
                        {
                            // This property IS generated by a packer.
                            // Find the *specific* output in the packer that matches our destination property name.
                            TextureOutput outputToPack = prop.packerConfig.OutputTextures.FirstOrDefault(
                                o => o.outputSuffix == prop.destinationName || (sourceMaterial.name + o.outputSuffix) == prop.destinationName
                            );

                            if (outputToPack != null)
                            {
                                string cacheKey = prop.packerConfig.GetInstanceID() + outputToPack.outputSuffix;
                                string newAssetPath;

                                if (processedTextureCache.TryGetValue(cacheKey, out newAssetPath))
                                {
                                    // 1. Use cached asset path if available
                                    //    We still add it to the pending list for *this* material
                                }
                                else
                                {
                                    // 2. Pack new texture and get its path
                                    // Get the output path from the ORIGINAL material asset
                                    string assetPath = AssetDatabase.GetAssetPath(sourceMaterial);
                                    string outputDirectory = string.IsNullOrEmpty(assetPath) ? "Assets" : Path.GetDirectoryName(assetPath);
                                    if (string.IsNullOrEmpty(outputDirectory))
                                    {
                                        outputDirectory = "Assets"; // Fallback
                                        Debug.LogWarning($"Could not find asset path for {sourceMaterial.name}. Packed texture will be saved to Assets folder.", sourceMaterial);
                                    }

                                    newAssetPath = AbstractTexturePackerConfigEditor.PackAndImport(
                                        prop.packerConfig,
                                        outputToPack,
                                        _workingMaterial, // Read from the in-memory copy
                                        sourceMaterial.name + "_" + prop.destinationName + "_packed",
                                        outputDirectory
                                    );

                                    // Add to cache for next materials in *this* batch
                                    processedTextureCache[cacheKey] = newAssetPath;
                                }

                                if (!string.IsNullOrEmpty(newAssetPath))
                                {
                                    MaterialConversionProcessorCache.AddPendingAssignment(
                                        newAssetPath,
                                        sourceMaterial,
                                        prop.destinationName);
                                }
                                else
                                {
                                    Debug.LogError($"Packing failed for {sourceMaterial.name} -> {prop.destinationName}");
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"MaterialConverter lists {prop.destinationName} as using a packer, " +
                                                   $"but no output in '{prop.packerConfig.name}' matches that destination name (suffix). " +
                                                   $"Falling back to 1-to-1 copy.", sourceMaterial);
                                sourceMaterial.SetTexture(prop.destinationName, _workingMaterial.GetTexture(prop.propertyName));
                            }
                        }
                        else
                        {
                            sourceMaterial.SetTexture(prop.destinationName, _workingMaterial.GetTexture(prop.propertyName));
                        }
                        break;
                }
            }
            DestroyImmediate(_workingMaterial);
            return sourceMaterial;
        }

        // --- Helper functions for populating and reading dropdowns ---

        private string[] GetPropertyDisplayNames(bool isSource, ShaderPropertyType type)
        {
            var cache = isSource ? _sourceShaderProperties : _destShaderProperties;
            if (type == ShaderPropertyType.Range || type == ShaderPropertyType.Int)
                type = ShaderPropertyType.Float;

            if (cache.TryGetValue(type, out var data))
            {
                // Convert List<PropertyNameData> to string[]
                string[] displayNames = new string[data.Count];
                for (int i = 0; i < data.Count; i++)
                {
                    displayNames[i] = data[i].DisplayName;
                }
                return displayNames;
            }
            return new string[] { "None" }; // Default
        }

        private string[] GetPropertyInternalNames(bool isSource, ShaderPropertyType type)
        {
            var cache = isSource ? _sourceShaderProperties : _destShaderProperties;
            if (type == ShaderPropertyType.Range || type == ShaderPropertyType.Int)
                type = ShaderPropertyType.Float;

            if (cache.TryGetValue(type, out var data))
            {
                // Convert List<PropertyNameData> to string[]
                string[] internalNames = new string[data.Count];
                for (int i = 0; i < data.Count; i++)
                {
                    internalNames[i] = data[i].InternalName;
                }
                return internalNames;
            }
            return new string[] { "" }; // Default
        }

        private int GetPopupIndex(string currentInternalName, string[] internalNames)
        {
            for (int i = 0; i < internalNames.Length; i++)
            {
                if (internalNames[i] == currentInternalName)
                {
                    return i;
                }
            }
            return 0; // Default to "None"
        }

        private void DrawConvertButtons()
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Convert Controls", EditorStyles.centeredGreyMiniLabel);

                bool ready = _sourceShaderProp.objectReferenceValue != null &&
                    _destinationShaderProp.objectReferenceValue != null;

                EditorGUI.BeginDisabledGroup(!ready);
                if (GUILayout.Button(new GUIContent("Convert All in Project", "Find all Materials in the Project using " + _sourceShaderProp.objectReferenceValue.name + " and Convert them.")))
                {
                    ScrapeAndConvertMaterials();
                }
                EditorGUI.EndDisabledGroup();

                bool hasSelected = Selection.objects.Length > 0;
                EditorGUI.BeginDisabledGroup(!(ready && hasSelected));
                if (GUILayout.Button(new GUIContent("Convert All in Selection", "Take all Materials currently Selected using " + _sourceShaderProp.objectReferenceValue.name + " and Convert them.")))
                {
                    ConvertSelectedMaterials();
                }
                EditorGUI.EndDisabledGroup();
            }
        }
    }
}