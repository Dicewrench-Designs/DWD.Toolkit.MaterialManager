//© Dicewrench Designs LLC 2019-2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;

namespace DWD.MaterialManager
{
    public class ManageableColorProperty : ManageableMaterialProperty<Color>
    {
        public override void ApplyPropertyToMaterial(Material m)
        {
            if (m.HasProperty(MaterialPropertyID))
            {
                m.SetColor(MaterialPropertyID, PropertyValue);
            }
        }

        public override void ApplyPropertyToMaterialPropertyBlock(MaterialPropertyBlock block)
        {
            block.SetColor(MaterialPropertyID, PropertyValue);
        }

        public override MaterialPropertyType GetMaterialPropertyType()
        {
            return MaterialPropertyType.Color;
        }
    }
}