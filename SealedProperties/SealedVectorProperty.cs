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
        public override void ApplyPropertyToMaterial(Material m)
        {
            if (m.HasProperty(MaterialPropertyID))
            {
                m.SetVector(MaterialPropertyID, PropertyValue);
            }
        }

        public override void ApplyPropertyToMaterialPropertyBlock(MaterialPropertyBlock block)
        {
            block.SetVector(MaterialPropertyID, PropertyValue);
        }

        public override MaterialPropertyType GetMaterialPropertyType()
        {
            return MaterialPropertyType.Vector;
        }
    }
}
