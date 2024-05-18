//© Dicewrench Designs LLC 2019-2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;

namespace DWD.MaterialManager
{
    public class ManageableTextureProperty : ManageableMaterialProperty<Texture>, IManageableProperty
    {
        public override void TryCacheOriginal(Material m)
        {
            if (m.HasProperty(MaterialPropertyName) && _originalCached == false)
            {
                _originalCached = true;
                _originalValue = m.GetTexture(MaterialPropertyName);
            }
        }

        public override void ApplyPropertyToMaterial(Material m, float intensity = 1.0f)
        {
            if (m.HasProperty(MaterialPropertyID))
            {
                TryCacheOriginal(m);
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
