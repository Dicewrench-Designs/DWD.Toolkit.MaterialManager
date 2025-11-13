//© Dicewrench Designs LLC 2025
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.Experimental.Rendering;

namespace DWD.MaterialManager.Editor
{
    [CustomEditor(typeof(AbstractTexturePackerConfig))]
    public class AbstractTexturePackerConfigEditor : UnityEditor.Editor
    {
        private const int _DEFAULT_SIZE = 32;

        private static string _PROP_INPUT = "_inputPropertyNames";
        private static string _PROP_OUTPUT = "_outputTextures";
        public static GUIStyle headerStyle;

        private SerializedProperty _input;
        private SerializedProperty _output;

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

            EditorGUILayout.HelpBox("Use the Abstract Texture Packer in conjunction with the Material Converter to dynamically pack many Textures based on Materials' existing assignments.", MessageType.Info);

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
                EditorGUILayout.HelpBox("Output Suffix should match target Texture Property name in new Shader.", MessageType.Info);
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
        /// from the material, packs a single output texture, saves it, and imports it.
        /// </summary>
        /// <param name="config">The template to use.</param>
        /// <param name="outputToPack">The specific output definition to build.</param>
        /// <param name="sourceMaterial">The material to read textures from (this will be the in-memory copy).</param>
        /// <param name="newAssetName">The base name for the new texture file (e.g., the material name).</param>
        /// <param name="outputDirectory">The valid asset directory path to save the new texture in.</param>
        /// <returns>The asset path of the newly created texture, or null on failure.</returns>
        public static string PackAndImport(
            AbstractTexturePackerConfig config,
            TextureOutput outputToPack,
            Material sourceMaterial, // This is the in-memory _workingMaterial
            string newAssetName,
            string outputDirectory)
        {
            int inputCount = config.InputPropertyNames.Length;
            List<Texture2D> inputTextures = new List<Texture2D>(inputCount);
            List<string> paths = new List<string>(inputCount);
            List<Color[]> originalPixels = new List<Color[]>();

            // 1. Build dynamic Input Texture list and store paths.
            for (int i = 0; i < inputCount; i++)
            {
                string propName = config.InputPropertyNames[i];
                // *** Read from the in-memory material ***
                Texture2D temp = string.IsNullOrEmpty(propName) ? null : sourceMaterial.GetTexture(propName) as Texture2D;
                inputTextures.Add(temp); // Add even if null, so indices match

                if (temp != null)
                    paths.Add(AssetDatabase.GetAssetPath(temp)); // Path is still needed for readability check
                else
                    paths.Add(null);
            }

            // 2. Get Pixels from all input textures
            int firstValidWidth = _DEFAULT_SIZE;
            int firstValidHeight = _DEFAULT_SIZE;

            (int width, int height) = GetInputDimensions(inputTextures, outputToPack);
            firstValidWidth = width;
            firstValidHeight = height;

            for (int i = 0; i < inputCount; i++)
            {
                Texture2D temp = inputTextures[i];
                Color[] pixels; // This is the final, correctly-sized pixel array
                Color[] sourcePixels; // This is the pixel array from the source file
                int sourceWidth, sourceHeight;

                if (temp != null)
                {
                    TextureImporter ti = AssetImporter.GetAtPath(paths[i]) as TextureImporter;

                    sourceWidth = temp.width;
                    sourceHeight = temp.height;

                    try
                    {
                        // Try the fast path first.
                        sourcePixels = temp.GetPixels(0, 0, temp.width, temp.height);
                    }
                    catch (System.Exception e)
                    {
                        if (e.Message.Contains("not readable"))
                        {
                            string path = paths[i];
                            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                            {
                                Debug.LogError($"Cannot read pixels for non-readable texture '{temp.name}' because its path is unknown or invalid: {path}", temp);
                                sourcePixels = Enumerable.Repeat(Color.black, firstValidWidth * firstValidHeight).ToArray();
                                sourceWidth = firstValidWidth;
                                sourceHeight = firstValidHeight;
                            }
                            else
                            {
                                // Read the raw file bytes (e.g., the PNG/JPG data)
                                byte[] fileData = File.ReadAllBytes(path);
                                // Create a new, temporary texture
                                Texture2D readableCopy = new Texture2D(2, 2, temp.format, temp.mipmapCount > 1);
                                // LoadImage makes it readable and resizes it to the file's dimensions
                                ImageConversion.LoadImage(readableCopy, fileData, false);
                                // Now get the pixels from the copy
                                sourcePixels = readableCopy.GetPixels(0, 0, readableCopy.width, readableCopy.height);
                                // Clean up the temporary texture
                                DestroyImmediate(readableCopy);
                            }
                        }
                        else
                        {
                            // Some other error (e.g., corrupted texture)
                            Debug.LogError($"Failed to GetPixels for '{temp.name}'. Error: {e.Message}", temp);
                            sourcePixels = Enumerable.Repeat(Color.black, firstValidWidth * firstValidHeight).ToArray();
                            sourceWidth = firstValidWidth;
                            sourceHeight = firstValidHeight;
                        }
                    }

                    // Now that we have sourcePixels, check if we need to rescale
                    if (sourceWidth != firstValidWidth || sourceHeight != firstValidHeight)
                    {
                        Debug.LogWarning($"Texture '{temp.name}' ({sourceWidth}x{sourceHeight}) does not match first input texture size ({firstValidWidth}x{firstValidHeight}). It will be rescaled (point-sampled).", temp);
                        pixels = GetRescaledPixels(sourcePixels, sourceWidth, sourceHeight, firstValidWidth, firstValidHeight);
                    }
                    else
                    {
                        pixels = sourcePixels; // No rescale needed
                    }
                }
                else
                {
                    // Create black pixels for null texture
                    pixels = Enumerable.Repeat(Color.black, firstValidWidth * firstValidHeight).ToArray();
                }
                originalPixels.Add(pixels);
            }

            // Pack the new texture
            Texture2D newOutput = new Texture2D(firstValidWidth, firstValidHeight, UnityEngine.Experimental.Rendering.DefaultFormat.LDR, TextureCreationFlags.None);
            int pixelCount = firstValidWidth * firstValidHeight;
            Color[] newPixels = new Color[pixelCount];

            for (int i = 0; i < pixelCount; i++)
            {
                float r = GetOutputPixelForChannel(originalPixels, outputToPack.rChannel, i);
                float g = GetOutputPixelForChannel(originalPixels, outputToPack.gChannel, i);
                float b = GetOutputPixelForChannel(originalPixels, outputToPack.bChannel, i);
                float a = GetOutputPixelForChannel(originalPixels, outputToPack.aChannel, i);

                newPixels[i] = new Color(r, g, b, a);
            }

            newOutput.SetPixels(newPixels);
            newOutput.Apply();
            byte[] outputBytes = newOutput.EncodeToPNG();

            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            string fullOutputPath = outputDirectory + Path.DirectorySeparatorChar + newAssetName + ".png";

            try
            {
                string fullSystemPath = Path.GetFullPath(fullOutputPath);
                File.WriteAllBytes(fullSystemPath, outputBytes);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to write packed texture to disk at {fullOutputPath}. Error: {e.Message}");
                return null;
            }

            // Import the new asset. The AssetPostprocessor will pick it up from here.
            // We can remove ForceSynchronousImport as it's no longer needed.
            AssetDatabase.ImportAsset(fullOutputPath, ImportAssetOptions.ForceUpdate);
            return fullOutputPath;
        }

        /// <summary>
        /// Gets rescaled pixels from a source pixel array using point sampling.
        /// </summary>
        private static Color[] GetRescaledPixels(Color[] source, int sourceWidth, int sourceHeight, int targetWidth, int targetHeight)
        {
            Color[] targetPixels = new Color[targetWidth * targetHeight];
            float xScale = (float)sourceWidth / targetWidth;
            float yScale = (float)sourceHeight / targetHeight;

            for (int y = 0; y < targetHeight; y++)
            {
                for (int x = 0; x < targetWidth; x++)
                {
                    int sourceX = Mathf.FloorToInt(x * xScale);
                    int sourceY = Mathf.FloorToInt(y * yScale);
                    targetPixels[y * targetWidth + x] = source[sourceY * sourceWidth + sourceX];
                }
            }
            return targetPixels;
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
            return (_DEFAULT_SIZE, _DEFAULT_SIZE);
        }

        private static float GetOutputPixelForChannel(List<Color[]> source, ChannelOutput channel, int currIndex)
        {
            if (channel.enabled == false || channel.textureSource >= source.Count)
                return 0.0f; // Default to black

            Color[] tex = source[channel.textureSource];
            if (currIndex >= tex.Length) // Handle size mismatch
                currIndex = tex.Length - 1;

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