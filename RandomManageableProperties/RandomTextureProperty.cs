//© Dicewrench Designs LLC 2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;

namespace DWD.MaterialManager
{
    public class RandomTextureProperty : RandomManageableProperty<Texture>
    {
        public override MaterialPropertyType GetMaterialPropertyType()
        {
            return MaterialPropertyType.Float;
        }

        public override void TryCacheOriginal(Material m)
        {
            if (m.HasProperty(MaterialPropertyName) && _originalCached == false)
            {
                _originalCached = true;
                _originalValue = m.GetTexture(MaterialPropertyName);
            }
        }

        public override Texture GetRandomValue()
        {
            return UnityEngine.Random.Range(0.0f, 100.0f) > 50.0f ? PropertyValue : SecondValue;
        }

        public override void ApplyPropertyToMaterial(Material m, float intensity = 1)
        {
            TryCacheOriginal(m);
            if (m.HasProperty(MaterialPropertyID))
            {
                Texture output = GetRandomValue();
                if (intensity <= 0)
                    output = _originalValue;
                m.SetTexture(MaterialPropertyID, output);
            }
        }

        public override void ApplyPropertyToMaterialPropertyBlock(MaterialPropertyBlock block, Material m, float intensity = 1.0f)
        {
            TryCacheOriginal(m);
            Texture output = GetRandomValue();
            if (intensity <= 0)
                output = _originalValue;
            block.SetTexture(MaterialPropertyID, output);
        }
    }
}