//© Dicewrench Designs LLC 2019-2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;

namespace DWD.MaterialManager
{
    public class ManageableColorProperty : ManageableMaterialProperty<Color>
    {
        public override void TryCacheOriginal(Material m)
        {
            if (m.HasProperty(MaterialPropertyName) && _originalCached == false)
            {
                _originalCached = true;
                _originalValue = m.GetColor(MaterialPropertyName);
            }
        }
        public override void ApplyPropertyToMaterial(Material m, float intensity = 1.0f)
        {
            TryCacheOriginal(m);
            if (m.HasProperty(MaterialPropertyID))
            {
                m.SetColor(MaterialPropertyID, Color.Lerp(_originalValue, PropertyValue, intensity));
            }
        }

        public override void ApplyPropertyToMaterialPropertyBlock(MaterialPropertyBlock block, Material m, float intensity = 1.0f)
        {
            TryCacheOriginal(m);
            block.SetColor(MaterialPropertyID, Color.Lerp(_originalValue, PropertyValue, intensity));
        }

        public override MaterialPropertyType GetMaterialPropertyType()
        {
            return MaterialPropertyType.Color;
        }
    }
}