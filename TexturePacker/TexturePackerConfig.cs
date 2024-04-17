//© Dicewrench Designs LLC 2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEditor;
using UnityEngine;

namespace DWD.MaterialManager
{
    /// <summary>
    /// <see cref="TexturePackerConfig"/> is a <see cref="ScriptableObject"/>
    /// that serves as a template for outputting new <see cref="Texture"/>s.
    /// </summary>
    [CreateAssetMenu(menuName = "DWD/Texture Packer Config")]
    public class TexturePackerConfig : ScriptableObject
    {
        [SerializeField]
        private Texture2D[] _inputTextures = new Texture2D[0];
        public Texture2D[] InputTextures { get { return _inputTextures; } }
        [SerializeField]
        private TextureOutput[] _outputTextures = new TextureOutput[0];
        public TextureOutput[] OutputTextures { get { return _outputTextures; } }

        [SerializeField]
        [HideInInspector]
        private string _outputPath = "Assets/";
        public string OutputPath { get { return _outputPath; } }
    }


    /// <summary>
    /// An enum to define Texture Channel Outputs
    /// </summary>
    public enum Channel
    {
        R, G, B, A
    }

    /// <summary>
    /// A map for output for a single Texture Channel
    /// </summary>
    [System.Serializable]
    public class ChannelOutput
    {
        public bool enabled;
        public int textureSource;
        public Channel channel;
    }

    [System.Serializable]
    public class TextureOutput
    {
        public string outputSuffix = "_Texture";

        public ChannelOutput rChannel;
        public ChannelOutput gChannel;
        public ChannelOutput bChannel;
        public ChannelOutput aChannel;
    }

}