//© Dicewrench Designs LLC 2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;

namespace DWD.MaterialManager
{
    public class RandomKeywordProperty : RandomManageableProperty<bool>
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
                _originalValue = m.IsKeywordEnabled(MaterialPropertyName);
            }
        }

        public override bool GetRandomValue()
        {
            return UnityEngine.Random.Range(0.0f, 100.0f) > 50.0f;
        }

        public override void ApplyPropertyToMaterial(Material m, float intensity = 1)
        {
            TryCacheOriginal(m);
            if (m.HasProperty(MaterialPropertyID))
            {
                bool enable = GetRandomValue();
                if (intensity <= 0)
                    enable = _originalValue;

                if (enable)
                    m.EnableKeyword(MaterialPropertyName.ToUpper());
                else
                    m.DisableKeyword(MaterialPropertyName.ToUpper());
            }
        }

        public override void ApplyPropertyToMaterialPropertyBlock(MaterialPropertyBlock block, Material m, float intensity = 1.0f)
        {
            TryCacheOriginal(m);
            float orig = _originalValue ? 1.0f : 0.0f;
            block.SetFloat(MaterialPropertyID, Mathf.Lerp(orig, PropertyValue ? 1.0f : 0.0f, intensity));
        }
    }
}