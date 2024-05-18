//© Dicewrench Designs LLC 2019-2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;

namespace DWD.MaterialManager
{
    public class ManageableVectorProperty : ManageableMaterialProperty<Vector4>
    {
        public override void TryCacheOriginal(Material m)
        {
            if (m.HasProperty(MaterialPropertyName) && _originalCached == false)
            {
                _originalCached = true;
                _originalValue = m.GetVector(MaterialPropertyName);
            }
        }

        public override void ApplyPropertyToMaterial(Material m, float intensity = 1.0f)
        {
            if (m.HasProperty(MaterialPropertyID))
            {
                TryCacheOriginal(m);
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
