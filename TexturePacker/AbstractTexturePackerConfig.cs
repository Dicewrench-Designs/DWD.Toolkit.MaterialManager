//© Dicewrench Designs LLC 2025
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;

namespace DWD.MaterialManager
{
    /// <summary>
    /// <see cref="AbstractTexturePackerConfig"/> is a <see cref="ScriptableObject"/>
    /// that serves as a *template* for packing textures from a source <see cref="Material"/>'s
    /// properties in a dynamic processor like the <see cref="MaterialConverter"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "DWD/Abstract Texture Packer Config")]
    public class AbstractTexturePackerConfig : ScriptableObject
    {
        [Tooltip("The list of *shader property names* to be used as inputs. The order here matches the 'Texture Source' index in the outputs.")]
        [SerializeField]
        private string[] _inputPropertyNames = new string[0];
        public string[] InputPropertyNames { get { return _inputPropertyNames; } }

        [SerializeField]
        private TextureOutput[] _outputTextures = new TextureOutput[0];
        public TextureOutput[] OutputTextures { get { return _outputTextures; } }
    }
}