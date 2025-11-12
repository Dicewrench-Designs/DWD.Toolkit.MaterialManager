//© Dicewrench Designs LLC 2025
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering; // Added for ShaderPropertyType

namespace DWD.MaterialManager
{
    /// <summary>
    /// <see cref="MaterialConverter"/> is a <see cref="ScriptableObject"/>
    /// that defines the rules for converting a <see cref="Material"/>
    /// from one <see cref="Shader"/> to another.
    /// </summary>
    [CreateAssetMenu(menuName = "DWD/Material Converter")]
    public class MaterialConverter : ScriptableObject
    {
        [Tooltip("The shader that materials will be converted FROM.")]
        [SerializeField]
        private Shader _sourceShader;
        public Shader SourceShader { get { return _sourceShader; } }

        [Tooltip("The shader that materials will be converted TO.")]
        [SerializeField]
        private Shader _destinationShader;
        public Shader DestinationShader { get { return _destinationShader; } }

        [Tooltip("The list of properties to copy from the source shader to the destination shader.")]
        [SerializeField]
        private ShaderPropertyTypePair[] _propertyMap = new ShaderPropertyTypePair[0];
        public ShaderPropertyTypePair[] PropertyMap { get { return _propertyMap; } }
    }
}