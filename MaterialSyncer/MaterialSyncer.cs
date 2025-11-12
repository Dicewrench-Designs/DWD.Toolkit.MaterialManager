//© Dicewrench Designs LLC 2024-2025
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;
using UnityEngine.Rendering;

namespace DWD.MaterialManager
{
    /// <summary>
    /// Serializable class for keeping two <see cref="Material"/>s
    /// or <see cref="MaterialPropertyBlock"/>s or any combination
    /// of the two types matched up.
    /// </summary>
    [System.Serializable]
    public class MaterialSyncer
    {
        public ShaderPropertyTypePair[] properties;

        public static void Sync(Material source, Material destination, ShaderPropertyTypePair[] properties)
        {
            int count = properties.Length;
            for (int a = 0; a < count; a++)
            {
                ShaderPropertyTypePair prop = properties[a];
                if (source.HasProperty(prop.propertyName))
                {
                    switch (prop.propertyType)
                    {
                        case ShaderPropertyType.Float:
                            destination.SetFloat(prop.propertyName, source.GetFloat(prop.propertyName)); break;
                        case ShaderPropertyType.Int:
                            destination.SetInt(prop.propertyName, source.GetInt(prop.propertyName)); break;
                        case ShaderPropertyType.Color:
                            destination.SetColor(prop.propertyName, source.GetColor(prop.propertyName)); break;
                        case ShaderPropertyType.Vector:
                            destination.SetVector(prop.propertyName, source.GetVector(prop.propertyName)); break;
                        case ShaderPropertyType.Range:
                            destination.SetFloat(prop.propertyName, source.GetFloat(prop.propertyName)); break;
                        case ShaderPropertyType.Texture:
                            destination.SetTexture(prop.propertyName, source.GetTexture(prop.propertyName)); break;
                    }
                }
            }
        }

        public static void Sync(Material source, MaterialPropertyBlock destination, ShaderPropertyTypePair[] properties)
        {
            int count = properties.Length;
            for (int a = 0; a < count; a++)
            {
                ShaderPropertyTypePair prop = properties[a];
                if (source.HasProperty(prop.propertyName))
                {
                    switch (prop.propertyType)
                    {
                        case ShaderPropertyType.Float:
                            destination.SetFloat(prop.destinationName, source.GetFloat(prop.propertyName)); break;
                        case ShaderPropertyType.Int:
                            destination.SetInt(prop.destinationName, source.GetInt(prop.propertyName)); break;
                        case ShaderPropertyType.Color:
                            destination.SetColor(prop.destinationName, source.GetColor(prop.propertyName)); break;
                        case ShaderPropertyType.Vector:
                            destination.SetVector(prop.destinationName, source.GetVector(prop.propertyName)); break;
                        case ShaderPropertyType.Range:
                            destination.SetFloat(prop.destinationName, source.GetFloat(prop.propertyName)); break;
                        case ShaderPropertyType.Texture:
                            destination.SetTexture(prop.destinationName, source.GetTexture(prop.propertyName)); break;
                    }
                }
            }
        }

        public static void Sync(MaterialPropertyBlock source, MaterialPropertyBlock destination, ShaderPropertyTypePair[] properties)
        {
            int count = properties.Length;
            for (int a = 0; a < count; a++)
            {
                ShaderPropertyTypePair prop = properties[a];
                if (source.HasProperty(prop.propertyName))
                {
                    switch (prop.propertyType)
                    {
                        case ShaderPropertyType.Float:
                            destination.SetFloat(prop.destinationName, source.GetFloat(prop.propertyName)); break;
                        case ShaderPropertyType.Int:
                            destination.SetInt(prop.destinationName, source.GetInt(prop.propertyName)); break;
                        case ShaderPropertyType.Color:
                            destination.SetColor(prop.destinationName, source.GetColor(prop.propertyName)); break;
                        case ShaderPropertyType.Vector:
                            destination.SetVector(prop.destinationName, source.GetVector(prop.propertyName)); break;
                        case ShaderPropertyType.Range:
                            destination.SetFloat(prop.destinationName, source.GetFloat(prop.propertyName)); break;
                        case ShaderPropertyType.Texture:
                            destination.SetTexture(prop.destinationName, source.GetTexture(prop.propertyName)); break;
                    }
                }
            }
        }

        public static void Sync(MaterialPropertyBlock source, Material destination, ShaderPropertyTypePair[] properties)
        {
            int count = properties.Length;
            for (int a = 0; a < count; a++)
            {
                ShaderPropertyTypePair prop = properties[a];
                if (source.HasProperty(prop.propertyName))
                {
                    switch (prop.propertyType)
                    {
                        case ShaderPropertyType.Float:
                            destination.SetFloat(prop.destinationName, source.GetFloat(prop.propertyName)); break;
                        case ShaderPropertyType.Int:
                            destination.SetInt(prop.destinationName, source.GetInt(prop.propertyName)); break;
                        case ShaderPropertyType.Color:
                            destination.SetColor(prop.destinationName, source.GetColor(prop.propertyName)); break;
                        case ShaderPropertyType.Vector:
                            destination.SetVector(prop.destinationName, source.GetVector(prop.propertyName)); break;
                        case ShaderPropertyType.Range:
                            destination.SetFloat(prop.destinationName, source.GetFloat(prop.propertyName)); break;
                        case ShaderPropertyType.Texture:
                            destination.SetTexture(prop.destinationName, source.GetTexture(prop.propertyName)); break;
                    }
                }
            }
        }

        public static void Sync(MaterialPropertyBlock source, ShaderPropertyTypePair[] properties)
        {
            int count = properties.Length;
            for (int a = 0; a < count; a++)
            {
                ShaderPropertyTypePair prop = properties[a];
                if (source.HasProperty(prop.propertyName))
                {
                    switch (prop.propertyType)
                    {
                        case ShaderPropertyType.Float:
                            source.SetFloat(prop.destinationName, source.GetFloat(prop.propertyName)); break;
                        case ShaderPropertyType.Int:
                            source.SetInt(prop.destinationName, source.GetInt(prop.propertyName)); break;
                        case ShaderPropertyType.Color:
                            source.SetColor(prop.destinationName, source.GetColor(prop.propertyName)); break;
                        case ShaderPropertyType.Vector:
                            source.SetVector(prop.destinationName, source.GetVector(prop.propertyName)); break;
                        case ShaderPropertyType.Range:
                            source.SetFloat(prop.destinationName, source.GetFloat(prop.propertyName)); break;
                        case ShaderPropertyType.Texture:
                            source.SetTexture(prop.destinationName, source.GetTexture(prop.propertyName)); break;
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class ShaderPropertyTypePair
    {
        public string propertyName;
        public string destinationName;
        public ShaderPropertyType propertyType = ShaderPropertyType.Float;
        public AbstractTexturePackerConfig packerConfig = null;
    }

}