﻿//© Dicewrench Designs LLC 2019-2023
//Licensed for use in 'Baseball Bash' App
//All Rights Reserved
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;

namespace DWD.MaterialManager
{
    public class ManageableTextureProperty : ManageableMaterialProperty<Texture>, IManageableProperty
    {
        public override void ApplyPropertyToMaterial(Material m)
        {
            if (m.HasProperty(MaterialPropertyID))
            {
                m.SetTexture(MaterialPropertyID, PropertyValue);
            }
        }

        public override void ApplyPropertyToMaterialPropertyBlock(MaterialPropertyBlock block)
        {
            block.SetTexture(MaterialPropertyID, PropertyValue);
        }

        public override MaterialPropertyType GetMaterialPropertyType()
        {
            return MaterialPropertyType.Texture;
        }
    }
}
