//© Dicewrench Designs LLC 2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;

namespace DWD.MaterialManager
{
    public class RandomColorProperty : RandomManageableProperty<Color>
    {
        public override MaterialPropertyType GetMaterialPropertyType()
        {
            return MaterialPropertyType.Color;
        }

        public override void TryCacheOriginal(Material m)
        {
            if (m.HasProperty(MaterialPropertyName) && _originalCached == false)
            {
                _originalCached = true;
                _originalValue = m.GetColor(MaterialPropertyName);
            }
        }

        public override Color GetRandomValue()
        {
            Color one = PropertyValue;
            Color two = SecondValue;
            float r, g, b, a = 0.0f;
            r = UnityEngine.Random.Range(one.r, two.r);
            g = UnityEngine.Random.Range(one.g, two.g);
            b = UnityEngine.Random.Range(one.b, two.b);
            a = UnityEngine.Random.Range(one.a, two.a);
            return new Color(r, g, b, a);
        }

        public override void ApplyPropertyToMaterial(Material m, float intensity = 1)
        {
            TryCacheOriginal(m);
            if (m.HasProperty(MaterialPropertyID))
            {
                m.SetColor(MaterialPropertyID, Color.Lerp(_originalValue, GetRandomValue(), 1.0f));
            }
        }

        public override void ApplyPropertyToMaterialPropertyBlock(MaterialPropertyBlock block, Material m, float intensity = 1.0f)
        {
            TryCacheOriginal(m);
            block.SetColor(MaterialPropertyID, Color.Lerp(_originalValue, GetRandomValue(), 1.0f));
        }
    }
}