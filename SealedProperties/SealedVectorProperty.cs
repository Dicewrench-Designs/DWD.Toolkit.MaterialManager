//© Dicewrench Designs LLC 2019-2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;

namespace DWD.MaterialManager
{
    public class SealedVectorProperty : SealedMaterialProperty<Vector4>, IManageableProperty
    {
        public SealedVectorProperty(string propertyName) : base(propertyName)
        {
            _materialPropertyName = propertyName;
        }
        public override void ApplyPropertyToMaterial(Material m, float intensity = 1.0f)
        {
            TryCacheOriginal(m);
            if (m.HasProperty(MaterialPropertyID))
            {
                m.SetVector(MaterialPropertyID, Vector4.Lerp(_originalValue, PropertyValue, intensity));
            }
        }

        public override void ApplyPropertyToMaterialPropertyBlock(MaterialPropertyBlock block, Material m, float intensity = 1.0f)
        {
            TryCacheOriginal(m);
            block.SetVector(MaterialPropertyID, Vector4.Lerp(_originalValue, PropertyValue, intensity));
        }

        public override MaterialPropertyType GetMaterialPropertyType()
        {
            return MaterialPropertyType.Vector;
        }
    }
}
