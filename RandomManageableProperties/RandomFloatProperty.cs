//© Dicewrench Designs LLC 2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;

namespace DWD.MaterialManager
{
    public class RandomFloatProperty : RandomManageableProperty<float>
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
                _originalValue = m.GetFloat(MaterialPropertyName);
            }
        }

        public override float GetRandomValue()
        {
            return UnityEngine.Random.Range(PropertyValue, SecondValue);
        }

        public override void ApplyPropertyToMaterial(Material m, float intensity = 1)
        {
            TryCacheOriginal(m);
            if (m.HasProperty(MaterialPropertyID))
            {
                m.SetFloat(MaterialPropertyID, Mathf.Lerp(_originalValue, GetRandomValue(), intensity));
            }
        }

        public override void ApplyPropertyToMaterialPropertyBlock(MaterialPropertyBlock block, Material m, float intensity = 1.0f)
        {
            TryCacheOriginal(m);
            block.SetFloat(MaterialPropertyID, Mathf.Lerp(_originalValue, GetRandomValue(), intensity));
        }
    }
}