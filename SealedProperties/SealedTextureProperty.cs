//© Dicewrench Designs LLC 2019-2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;

namespace DWD.MaterialManager
{
    public class SealedTextureProperty : SealedMaterialProperty<Texture>, IManageableProperty
    {
        public override void TryCacheOriginal(Material m)
        {
            if (m.HasProperty(MaterialPropertyID) && _originalCached == false)
            {
                _originalCached = true;
                _originalValue = m.GetTexture(MaterialPropertyID);
            }
        }

        public SealedTextureProperty(string propertyName) : base(propertyName)
        {
            _materialPropertyName = propertyName;
        }

        public override void ApplyPropertyToMaterial(Material m, float intensity = 1.0f)
        {
            TryCacheOriginal(m);
            if (m.HasProperty(MaterialPropertyID))
            {
                m.SetTexture(MaterialPropertyID, intensity == 1.0f ? PropertyValue : _originalValue);
            }
        }

        public override void ApplyPropertyToMaterialPropertyBlock(MaterialPropertyBlock block, Material m, float intensity = 1.0f)
        {
            TryCacheOriginal(m);
            block.SetTexture(MaterialPropertyID, intensity == 1.0f ? PropertyValue : _originalValue);
        }

        public override MaterialPropertyType GetMaterialPropertyType()
        {
            return MaterialPropertyType.Texture;
        }
    }
}
