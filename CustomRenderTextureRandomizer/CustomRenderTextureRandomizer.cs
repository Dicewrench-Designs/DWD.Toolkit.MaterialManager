//© Dicewrench Designs LLC 2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using System.IO;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DWD.MaterialManager
{
    [CreateAssetMenu(menuName = "DWD/Custom Render Texture Randomizer")]
    public class CustomRenderTextureRandomizer : ScriptableObject
    {
        [SerializeField]
        private CustomRenderTexture _sourceTexture;

        [SerializeField]
        private int _numberToRandomize = 1;

        [SerializeField]
        private int _seed = -1;

        [SerializeField]
        private string _outputPath = "";

        public BaseManageableMaterialProperty[] randomProperties = new BaseManageableMaterialProperty[0];

        private int _workingSeed = -1;

        private void OnValidate()
        {
            if (_seed == -1)
                _seed = (int)System.DateTime.Now.Ticks;
        }

        public void GenerateOutputs()
        {
            if (_sourceTexture == null)
                Debug.LogError("Cannot Generate Random Outputs... no CustomRenderTexture assigned!", this);
            if(_sourceTexture.material == null)
                Debug.LogError("Cannot Generate Random Outputs... CustomRenderTexture does not have a Material assigned!", this);

            _workingSeed = _seed;
            UnityEngine.Random.InitState(_workingSeed);

            _nextIndex = -1;

            IncrementOutput();
        }

        private void ApplyPropertiesToMaterial(BaseManageableMaterialProperty[] properties, Material mat, float intensity = 1.0f)
        {
            int count = properties.Length;

            for(int a = 0; a < count; a++)
            {
                BaseManageableMaterialProperty temp = properties[a];
                if(temp != null)
                {
                    temp.ApplyPropertyToMaterial(mat, intensity);
                    _workingSeed++;
                    UnityEngine.Random.InitState(_workingSeed);
                }
            }
        }

        private void IncrementOutput()
        {
#if UNITY_EDITOR
            EditorApplication.delayCall -= IncrementOutput;
            _nextIndex++;
            if (_nextIndex == _numberToRandomize)
                return;

            ApplyPropertiesToMaterial(randomProperties, _sourceTexture.material);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            EditorApplication.delayCall += QueueOutput;
#endif
        }

        private int _nextIndex;
        private void QueueOutput()
        {
#if UNITY_EDITOR
            EditorApplication.delayCall -= QueueOutput;
            EditorApplication.delayCall += GenerateResult;
#endif
        }

        private void GenerateResult()
        {           
#if UNITY_EDITOR
            EditorApplication.delayCall -= GenerateResult;
            _sourceTexture.Update();
            EditorApplication.delayCall += OutputResult;
#endif
        }

        private void OutputResult()
        {
#if UNITY_EDITOR
            EditorApplication.delayCall -= OutputResult;
            int index = _nextIndex;
            int width = _sourceTexture.width;
            int height = _sourceTexture.height;
            int volumeDepth = _sourceTexture.volumeDepth;
            bool flag = GraphicsFormatUtility.IsIEEE754Format(_sourceTexture.graphicsFormat);
            bool flag2 = GraphicsFormatUtility.IsFloatFormat(_sourceTexture.graphicsFormat);
            TextureFormat textureFormat = (flag ? TextureFormat.RGBAFloat : TextureFormat.RGBA32);
            int width2 = width;
            if (_sourceTexture.dimension == TextureDimension.Tex3D)
            {
                width2 = width * volumeDepth;
            }
            else if (_sourceTexture.dimension == TextureDimension.Cube)
            {
                width2 = width * 6;
            }
            Texture2D texture2D = new Texture2D(width2, height, textureFormat, mipChain: false);
            if (_sourceTexture.dimension == TextureDimension.Tex2D)
            {
                Graphics.SetRenderTarget(_sourceTexture);
                texture2D.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
                texture2D.Apply();
            }
            else if (_sourceTexture.dimension == TextureDimension.Tex3D)
            {
                int num = 0;
                for (int i = 0; i < volumeDepth; i++)
                {
                    Graphics.SetRenderTarget(_sourceTexture, 0, CubemapFace.Unknown, i);
                    texture2D.ReadPixels(new Rect(0f, 0f, width, height), num, 0);
                    texture2D.Apply();
                    num += width;
                }
            }
            else
            {
                int num2 = 0;
                for (int j = 0; j < 6; j++)
                {
                    Graphics.SetRenderTarget(_sourceTexture, 0, (CubemapFace)j);
                    texture2D.ReadPixels(new Rect(0f, 0f, width, height), num2, 0);
                    texture2D.Apply();
                    num2 += width;
                }
            }
            byte[] array = null;
            array = ((!flag) ? texture2D.EncodeToPNG() : texture2D.EncodeToEXR(Texture2D.EXRFlags.CompressZIP | (flag2 ? Texture2D.EXRFlags.OutputAsFloat : Texture2D.EXRFlags.None)));
            Object.DestroyImmediate(texture2D);
            string extension = (flag ? "exr" : "png");
            string text = _outputPath + Path.DirectorySeparatorChar + this.name + "_" + index + "." + extension;
            if (!string.IsNullOrEmpty(text))
            {
                File.WriteAllBytes(text, array);
                AssetDatabase.Refresh();
            }
            DestroyImmediate(texture2D);

            EditorApplication.delayCall += IncrementOutput;
#endif
        }
    }
}