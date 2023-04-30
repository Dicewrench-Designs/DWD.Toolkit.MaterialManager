﻿//© Dicewrench Designs LLC 2019-2022
//Licensed for use in 'Baseball Bash' App
//All Rights Reserved
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;

namespace DWD.MaterialManager
{
    //Bit of a gotchya on this one.... So we CAN set the keyword this way and it works fine
    //but if you also have a toggle drawing the thing it won't set itself and you have no way
    //of knowing the prop just based on the MaterialPropertyDrawer tag.  And we can't dig
    //through the MaterialEditor because we might be doing this at runtime.... SO, we need to
    //make some assumptions about the string formatting. 

    //IF YOU WANT THIS TO WORK YOU GOTTA:
    // 1. Set the name of the Property to the name of the toggle property, i.e. "_Emission_Enabled"
    // 2. The Shader KEYWORD must be named the same but ALL CAPS i.e. "_EMISSION_ENABLED"

    public class SealedKeywordProperty : SealedMaterialProperty<bool>
    {
        public SealedKeywordProperty(string propertyName) : base(propertyName)
        {
            _materialPropertyName = propertyName;
        }

        public override void ApplyPropertyToMaterial(Material m)
        {
            if (m.HasProperty(_materialPropertyName))
            {
                m.SetFloat(MaterialPropertyID, PropertyValue ? 1.0f : 0.0f);
                if (PropertyValue == true)
                {
                    m.EnableKeyword(_materialPropertyName.ToUpper());
                }
                else
                    m.DisableKeyword(_materialPropertyName.ToUpper());
            }
        }

        //Be careful this won't work on MaterialPropertyBlocks with actual keywords!
        public override void ApplyPropertyToMaterialPropertyBlock(MaterialPropertyBlock block)
        {
            block.SetFloat(MaterialPropertyID, PropertyValue ? 1.0f : 0.0f);           
        }

        public override MaterialPropertyType GetMaterialPropertyType()
        {
            return MaterialPropertyType.Texture;
        }
    }
}
