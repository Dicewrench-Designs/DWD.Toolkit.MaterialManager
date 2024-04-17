//© Dicewrench Designs LLC 2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace DWD.MaterialManager.Editor
{
    [CustomEditor(typeof(TexturePackerConfig))]
    public class TexturePackerConfigEditor : UnityEditor.Editor
    {
        private static string _PROP_INPUT = "_inputTextures";
        private static string _PROP_OUTPUT = "_outputTextures";
        private static string _PROP_PATH = "_outputPath";

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

        public override void OnInspectorGUI()
        {
            TryStyles();

            serializedObject.Update();

            SerializedProperty input = serializedObject.FindProperty(_PROP_INPUT);
            SerializedProperty output = serializedObject.FindProperty(_PROP_OUTPUT);
            SerializedProperty path = serializedObject.FindProperty(_PROP_PATH);

            using(new EditorGUILayout.VerticalScope(GUI.skin.box)) 
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

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                string pathLabel = "Path : " + path.stringValue;
                pathLabel = pathLabel.Substring(0, Mathf.Min(32, pathLabel.Length)) + "...";
                if (GUILayout.Button(new GUIContent(pathLabel, path.stringValue), EditorStyles.miniButtonLeft, GUILayout.ExpandWidth(true)))
                {
                    path.stringValue = EditorUtility.SaveFolderPanel("Choose Save Folder", path.stringValue, "");
                }
                EditorGUI.BeginDisabledGroup(output.arraySize == 0);
                if(GUILayout.Button(new GUIContent("Pack", "Output new Textures"), EditorStyles.miniButtonRight, GUILayout.ExpandWidth(true)))
                {
                    serializedObject.ApplyModifiedProperties();
                    PackTextures(serializedObject.targetObject as TexturePackerConfig);
                }
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.Space();

            using(new EditorGUILayout.HorizontalScope())
            {
                using(new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField("Texture Inputs", EditorStyles.centeredGreyMiniLabel);
                    EditorGUILayout.Space();
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(input, true);
                    EditorGUI.indentLevel--;

                    EditorGUILayout.Space();

                    if(_messages != null && _messages.Count > 0)
                    {
                        EditorGUILayout.LabelField("Validation Messages");
                        EditorGUI.indentLevel++;
                        int count = _messages.Count;
                        for (int a = 0; a < count; a++)
                            _messages[a].Draw();
                        EditorGUI.indentLevel--;
                    }
                }

                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField("Texture Outputs", EditorStyles.centeredGreyMiniLabel);
                    EditorGUILayout.Space();
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(output, true);
                    EditorGUI.indentLevel--;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void PackTextures(TexturePackerConfig config)
        {
            int inputCount = config.InputTextures.Length;
            int count = config.OutputTextures.Length;
            Debug.Log("Packing " + count + " new Textures to ; " + config.OutputPath);

            bool[] readWriteOriginal = new bool[inputCount];
            for(int a = 0; a < inputCount; a++)
            {
                Texture2D temp = config.InputTextures[a];
                string path = AssetDatabase.GetAssetPath(temp);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                readWriteOriginal[a] = importer.isReadable;
                importer.isReadable = true;
                importer.SaveAndReimport();
            }
            AssetDatabase.Refresh();

            //Loop over the outputs
            for(int a = 0; a < count; a++)
            {
                TextureOutput output = config.OutputTextures[a];
                int width = GetInputWidthForOutput(config, output);
                int height = GetInputHeightForOutput(config, output);
                Texture2D newOutput = new Texture2D(width, height);
                int pixelCount = width * height;
                Color[] pixels = new Color[pixelCount];

                for(int b = 0; b < height; b++)
                {
                    for(int c = 0; c < width; c++)
                    {
                        float x = GetOutputPixelForChannel(config, output.rChannel, c, b);
                        float y = GetOutputPixelForChannel(config, output.gChannel, c, b);
                        float z = GetOutputPixelForChannel(config, output.bChannel, c, b);
                        float w = GetOutputPixelForChannel(config, output.aChannel, c, b);

                        int currentIndex = (b * width) + c;
                        pixels[currentIndex] = new Color(x, y, z, w);
                    }
                }

                newOutput.SetPixels(pixels);
                newOutput.Apply();
                byte[] outputBytes = newOutput.EncodeToPNG();
                string outputPath = config.OutputPath + "/" + config.name + output.outputSuffix + ".png";
                File.WriteAllBytes(outputPath, outputBytes);
            }

            for (int a = 0; a < inputCount; a++)
            {
                Texture2D temp = config.InputTextures[a];
                string path = AssetDatabase.GetAssetPath(temp);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                importer.isReadable = readWriteOriginal[a];
                importer.SaveAndReimport();
            }
            AssetDatabase.Refresh();
        }

        private bool TextureOutputUsesIndex(TextureOutput output, int index)
        {
            if ((output.rChannel.enabled && output.rChannel.textureSource == index) ||
                (output.gChannel.enabled && output.gChannel.textureSource == index) ||
                (output.bChannel.enabled && output.bChannel.textureSource == index) ||
                (output.aChannel.enabled && output.aChannel.textureSource == index))
                return true;
            else
                return false;
        }

        private List<Texture2D> GetInputTexturesInUse(TexturePackerConfig config, TextureOutput output)
        {
            List<Texture2D> texture2Ds = new List<Texture2D>();

            int count = config.InputTextures.Length;
            for(int a = 0; a < count; a++)
            {
                if(TextureOutputUsesIndex(output, a))
                    texture2Ds.Add(config.InputTextures[a]);
            }

            return texture2Ds;
        }

        private int GetInputWidthForOutput(TexturePackerConfig config, TextureOutput output)
        {
            List<Texture2D> texture2Ds = GetInputTexturesInUse(config, output);
            return texture2Ds[0].width;
        }

        private int GetInputHeightForOutput(TexturePackerConfig config, TextureOutput output)
        {
            List<Texture2D> texture2Ds = GetInputTexturesInUse(config, output);
            return texture2Ds[0].height;
        }

        private float GetOutputPixelForChannel(TexturePackerConfig config, ChannelOutput channel, int x, int y)
        {
            Texture2D source = config.InputTextures[channel.textureSource];
            Color pixel = source.GetPixel(x, y);
            if (channel.enabled == false)
                return 0.0f;

            switch (channel.channel)
            {
                case Channel.R:
                    return pixel.r;
                case Channel.G:
                    return pixel.g;
                case Channel.B:
                    return pixel.b;
                case Channel.A:
                    return pixel.a;           
            }
            return 0.0f;
        }

        [System.Serializable]
        private class ValidationMessage
        {
            public string message;
            public UnityEngine.Object context;
            public MessageType type;

            public ValidationMessage(string message, UnityEngine.Object context, MessageType type)
            {
                this.message = message;
                this.context = context;
                this.type = type;
            }

            public void Draw()
            {
                EditorGUILayout.HelpBox(message, type);
            }
        }

        private List<ValidationMessage> _messages = new List<ValidationMessage>();

        private void OnEnable()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            _messages.Clear();
            TexturePackerConfig config = serializedObject.targetObject as TexturePackerConfig;

            int inputCount = config.InputTextures.Length;

            TextureArrayIsUniformSize(config.InputTextures);

            int outputCount = config.OutputTextures.Length;
            for(int a = 0; a < outputCount; a++)
            {
                TextureOutputsHaveSameName(config.OutputTextures, a);
            }
        }

        private bool TextureArrayIsUniformSize(Texture2D[] array)
        {
            int x = -1; int y = -1;

            int count = array.Length;
            for(int a = 0; a < count; a++)
            {
                Texture2D temp = array[a];
                if(a == 0)
                {
                    x = temp.width; y = temp.height;
                }
                else
                {
                    if (x != temp.width || y != temp.height)
                        _messages.Add(new ValidationMessage(
                            temp.name + " size does not match.  Textures need to be the same size.", temp, MessageType.Error));
                }
            }

            return false;
        }

        private void TextureOutputsHaveSameName(TextureOutput[] array, int index)
        {
            string target = array[index].outputSuffix;
            int outputCount = array.Length;
            for (int a = 0; a < outputCount; a++)
            {
                if (a != index)
                {
                    if (target == array[a].outputSuffix)
                        _messages.Add(new ValidationMessage(
                            "Texture Output " + index + " has the same Output Suffix as Texture Output " + a + ".  This will overwrite data!",
                            null,
                            MessageType.Warning));
                }
            }
        }
    }
}