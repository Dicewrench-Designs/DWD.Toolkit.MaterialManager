//© Dicewrench Designs LLC 2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;

namespace DWD.MaterialManager
{
    public class RandomVectorProperty : RandomManageableProperty<Vector4>
    {
        public override MaterialPropertyType GetMaterialPropertyType()
        {
            return MaterialPropertyType.Vector;
        }

        public override void TryCacheOriginal(Material m)
        {
            if (m.HasProperty(MaterialPropertyName) && _originalCached == false)
            {
                _originalCached = true;
                _originalValue = m.GetVector(MaterialPropertyName);
            }
        }

        public override Vector4 GetRandomValue()
        {
            Vector4 one = PropertyValue;
            Vector4 two = SecondValue;
            float r, g, b, a = 0.0f;
            r = UnityEngine.Random.Range(one.x, two.x);
            g = UnityEngine.Random.Range(one.y, two.y);
            b = UnityEngine.Random.Range(one.z, two.z);
            a = UnityEngine.Random.Range(one.w, two.w);
            return new Vector4(r, g, b, a);
        }

        public override void ApplyPropertyToMaterial(Material m, float intensity = 1)
        {
            TryCacheOriginal(m);
            if (m.HasProperty(MaterialPropertyID))
            {
                m.SetVector(MaterialPropertyID, Vector4.Lerp(_originalValue, GetRandomValue(), 1.0f));
            }
        }

        public override void ApplyPropertyToMaterialPropertyBlock(MaterialPropertyBlock block, Material m, float intensity = 1.0f)
        {
            TryCacheOriginal(m);
            block.SetVector(MaterialPropertyID, Vector4.Lerp(_originalValue, GetRandomValue(), 1.0f));
        }
    }
}