//© Dicewrench Designs LLC 2025
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DWD.MaterialManager.Editor
{
    [CustomEditor(typeof(AbstractTexturePackerConfig))]
    public class AbstractTexturePackerConfigEditor : UnityEditor.Editor
    {
        private static string _PROP_INPUT = "_inputPropertyNames";
        private static string _PROP_OUTPUT = "_outputTextures";
        // _PROP_PATH has been removed
        public static GUIStyle headerStyle;

        private SerializedProperty _input;
        private SerializedProperty _output;
        // _path has been removed

        private List<ValidationMessage> _messages = new List<ValidationMessage>();

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
            _input = serializedObject.FindProperty(_PROP_INPUT);
            _output = serializedObject.FindProperty(_PROP_OUTPUT);
            // _path is removed
            OnValidate();
        }

        public override void OnInspectorGUI()
        {
            TryStyles();
            serializedObject.Update();

            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Abstract Texture Packer Config", EditorStyles.centeredGreyMiniLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(serializedObject.targetObject.name, headerStyle);
                    EditorGUILayout.Space();
                }
                EditorGUILayout.Space();
            }

            EditorGUILayout.Space();

            // Output path selection GUI has been removed.

            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Input Property Names", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_input, true);
                EditorGUI.indentLevel--;
            }

            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Texture Outputs", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_output, true);
                EditorGUI.indentLevel--;
            }

            if (_messages != null && _messages.Count > 0)
            {
                EditorGUILayout.Space();
                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField("Validation Messages", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    int count = _messages.Count;
                    for (int a = 0; a < count; a++)
                        _messages[a].Draw();
                    EditorGUI.indentLevel--;
                }
            }

            if (serializedObject.ApplyModifiedProperties())
            {
                OnValidate();
            }
        }

        #region Validation

        [System.Serializable]
        private class ValidationMessage
        {
            public string message;
            public MessageType type;

            public ValidationMessage(string message, MessageType type)
            {
                this.message = message;
                this.type = type;
            }

            public void Draw()
            {
                EditorGUILayout.HelpBox(message, type);
            }
        }

        private void OnValidate()
        {
            _messages.Clear();
            AbstractTexturePackerConfig config = serializedObject.targetObject as AbstractTexturePackerConfig;

            int outputCount = config.OutputTextures.Length;
            for (int a = 0; a < outputCount; a++)
            {
                TextureOutputsHaveSameName(config.OutputTextures, a);
                ValidateTextureOutputSources(config.OutputTextures[a], config.InputPropertyNames.Length, a);
            }
        }

        private void ValidateTextureOutputSources(TextureOutput output, int inputCount, int outputIndex)
        {
            if (output.rChannel.enabled && output.rChannel.textureSource >= inputCount)
                _messages.Add(new ValidationMessage($"Output {outputIndex} (R Channel) references source index {output.rChannel.textureSource}, but there are only {inputCount} inputs.", MessageType.Error));
            if (output.gChannel.enabled && output.gChannel.textureSource >= inputCount)
                _messages.Add(new ValidationMessage($"Output {outputIndex} (G Channel) references source index {output.gChannel.textureSource}, but there are only {inputCount} inputs.", MessageType.Error));
            if (output.bChannel.enabled && output.bChannel.textureSource >= inputCount)
                _messages.Add(new ValidationMessage($"Output {outputIndex} (B Channel) references source index {output.bChannel.textureSource}, but there are only {inputCount} inputs.", MessageType.Error));
            if (output.aChannel.enabled && output.aChannel.textureSource >= inputCount)
                _messages.Add(new ValidationMessage($"Output {outputIndex} (A Channel) references source index {output.aChannel.textureSource}, but there are only {inputCount} inputs.", MessageType.Error));
        }

        private void TextureOutputsHaveSameName(TextureOutput[] array, int index)
        {
            string target = array[index].outputSuffix;
            if (string.IsNullOrEmpty(target))
            {
                _messages.Add(new ValidationMessage($"Texture Output {index} has an empty Output Suffix. This is not allowed.", MessageType.Error));
                return;
            }

            for (int a = 0; a < array.Length; a++)
            {
                if (a != index && target == array[a].outputSuffix)
                {
                    _messages.Add(new ValidationMessage(
                        $"Texture Output {index} has the same Output Suffix as Texture Output {a} ('{target}'). This will overwrite data!",
                        MessageType.Warning));
                }
            }
        }

        #endregion

        #region Static Packing Logic

        /// <summary>
        /// This is the core packing function. It dynamically builds the input texture list
        /// from the material and packs a single output texture.
        /// </summary>
        /// <param name="config">The template to use.</param>
        /// <param name="outputToPack">The specific output definition to build.</param>
        /// <param name="sourceMaterial">The material to read textures from.</param>
        /// <param name="newAssetName">The base name for the new texture file (e.g., the material name).</param>
        /// <returns>The newly created and imported Texture2D asset.</returns>
        public static Texture2D PackFromMaterial(AbstractTexturePackerConfig config, TextureOutput outputToPack, Material sourceMaterial, string newAssetName)
        {
            int inputCount = config.InputPropertyNames.Length;
            List<Texture2D> inputTextures = new List<Texture2D>(inputCount);
            List<string> paths = new List<string>(inputCount);
            List<TextureImporter> importers = new List<TextureImporter>(inputCount);
            bool[] readWriteOriginal = new bool[inputCount];

            List<Color[]> originalPixels = new List<Color[]>();

            // 1. Build dynamic Input Texture list and set to readable
            for (int i = 0; i < inputCount; i++)
            {
                string propName = config.InputPropertyNames[i];
                Texture2D temp = sourceMaterial.GetTexture(propName) as Texture2D;
                inputTextures.Add(temp); // Add even if null, so indices match

                if (temp != null)
                {
                    string path = AssetDatabase.GetAssetPath(temp);
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                    if (importer != null)
                    {
                        readWriteOriginal[i] = importer.isReadable;
                        importer.isReadable = true;
                        importers.Add(importer);
                        paths.Add(path);
                    }
                    else
                    {
                        Debug.LogWarning($"Could not find TextureImporter for {temp.name} at {path}. Packing may fail if it's not readable.", temp);
                        readWriteOriginal[i] = true; // Assume readable if no importer
                        importers.Add(null);
                        paths.Add(path); // Still add the path for directory finding
                    }
                }
                else
                {
                    // Add placeholders for null textures
                    readWriteOriginal[i] = true;
                    importers.Add(null);
                    paths.Add(null);
                }
            }

            // Force reimport if we changed any settings
            // Find first valid importer to force update
            string firstValidImporterPath = paths.FirstOrDefault(p => !string.IsNullOrEmpty(p));
            if (!string.IsNullOrEmpty(firstValidImporterPath))
            {
                AssetDatabase.ImportAsset(firstValidImporterPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();
            }

            // ** NEW: Determine output path dynamically **
            string firstValidPath = paths.FirstOrDefault(p => !string.IsNullOrEmpty(p));
            string outputDirectory;
            if (!string.IsNullOrEmpty(firstValidPath))
            {
                outputDirectory = Path.GetDirectoryName(firstValidPath);
            }
            else
            {
                outputDirectory = "Assets/"; // Fallback if no input textures
                Debug.LogWarning($"No input textures found for packing '{newAssetName}'. Defaulting to 'Assets/' folder.");
            }

            // 2. Get Pixels from all input textures
            int firstValidWidth = 512; // Default fallback
            int firstValidHeight = 512;

            for (int i = 0; i < inputCount; i++)
            {
                Texture2D temp = inputTextures[i];
                Color[] pixels;
                if (temp != null)
                {
                    if (firstValidWidth == 512) // Find first valid texture to set dimensions
                    {
                        firstValidWidth = temp.width;
                        firstValidHeight = temp.height;
                    }
                    pixels = temp.GetPixels(0, 0, temp.width, temp.height);
                }
                else
                {
                    // Create black pixels for null texture
                    pixels = Enumerable.Repeat(Color.black, firstValidWidth * firstValidHeight).ToArray();
                }
                originalPixels.Add(pixels);
            }

            // 3. Pack the new texture
            (int width, int height) = GetInputDimensions(inputTextures, outputToPack);
            Texture2D newOutput = new Texture2D(width, height);
            int pixelCount = width * height;
            Color[] newPixels = new Color[pixelCount];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int currentIndex = (y * width) + x;

                    // Handle texture scaling if necessary (simple point sampling)
                    int sourceX = (int)((float)x / width * width);
                    int sourceY = (int)((float)y / height * height);
                    int sourceIndex = (sourceY * width) + sourceX;

                    float r = GetOutputPixelForChannel(originalPixels, outputToPack.rChannel, sourceIndex);
                    float g = GetOutputPixelForChannel(originalPixels, outputToPack.gChannel, sourceIndex);
                    float b = GetOutputPixelForChannel(originalPixels, outputToPack.bChannel, sourceIndex);
                    float a = GetOutputPixelForChannel(originalPixels, outputToPack.aChannel, sourceIndex);

                    newPixels[currentIndex] = new Color(r, g, b, a);
                }
            }

            newOutput.SetPixels(newPixels);
            newOutput.Apply();
            byte[] outputBytes = newOutput.EncodeToPNG();

            // Ensure directory exists
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            string fullOutputPath = Path.Combine(outputDirectory, newAssetName + outputToPack.outputSuffix + ".png");
            File.WriteAllBytes(fullOutputPath, outputBytes);

            // 4. Reset textures to original read/write state
            for (int i = 0; i < inputCount; i++)
            {
                if (importers[i] != null)
                {
                    importers[i].isReadable = readWriteOriginal[i];
                }
            }

            if (!string.IsNullOrEmpty(firstValidImporterPath))
            {
                AssetDatabase.ImportAsset(firstValidImporterPath, ImportAssetOptions.ForceUpdate);
            }

            // 5. Import and return the new asset
            AssetDatabase.Refresh();
            Texture2D loadedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(fullOutputPath);
            if (loadedTexture == null)
            {
                Debug.LogError($"Failed to save or load packed texture at {fullOutputPath}");
            }
            return loadedTexture;
        }

        private static (int width, int height) GetInputDimensions(List<Texture2D> textures, TextureOutput output)
        {
            // Find the first texture used by this output
            if (output.rChannel.enabled && textures.Count > output.rChannel.textureSource && textures[output.rChannel.textureSource] != null)
                return (textures[output.rChannel.textureSource].width, textures[output.rChannel.textureSource].height);
            if (output.gChannel.enabled && textures.Count > output.gChannel.textureSource && textures[output.gChannel.textureSource] != null)
                return (textures[output.gChannel.textureSource].width, textures[output.gChannel.textureSource].height);
            if (output.bChannel.enabled && textures.Count > output.bChannel.textureSource && textures[output.bChannel.textureSource] != null)
                return (textures[output.bChannel.textureSource].width, textures[output.bChannel.textureSource].height);
            if (output.aChannel.enabled && textures.Count > output.aChannel.textureSource && textures[output.aChannel.textureSource] != null)
                return (textures[output.aChannel.textureSource].width, textures[output.aChannel.textureSource].height);

            // Fallback: Find first non-null texture
            foreach (var tex in textures)
            {
                if (tex != null) return (tex.width, tex.height);
            }

            // Absolute fallback
            return (32, 32);
        }

        private static float GetOutputPixelForChannel(List<Color[]> source, ChannelOutput channel, int currIndex)
        {
            if (channel.enabled == false || channel.textureSource >= source.Count)
                return 0.0f; // Default to black

            Color[] tex = source[channel.textureSource];
            if (currIndex >= tex.Length) // Handle size mismatch
            {
                currIndex = tex.Length - 1;
            }

            Color pixel = tex[currIndex];

            switch (channel.channel)
            {
                case Channel.R: return pixel.r;
                case Channel.G: return pixel.g;
                case Channel.B: return pixel.b;
                case Channel.A: return pixel.a;
            }
            return 0.0f;
        }

        #endregion
    }
}