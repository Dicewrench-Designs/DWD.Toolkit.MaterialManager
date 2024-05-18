//© Dicewrench Designs LLC 2019-2024
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


    /// <summary>
    /// <see cref="ManageableMaterialProperty{T}"/> for <see cref="Shader"/> Keywords.  Has
    /// some quirks, view code comments to ensure proper use!
    /// </summary>

    public class ManageableKeywordProperty : ManageableMaterialProperty<bool>, IManageableProperty
    {
        public override void TryCacheOriginal(Material m)
        {
            if (m.HasProperty(MaterialPropertyName) && _originalCached == false)
            {
                _originalCached = true;
                _originalValue = m.GetFloat(MaterialPropertyName) > 1.0f;
            }
        }

        public override void ApplyPropertyToMaterial(Material m, float intensity = 1.0f)
        {
            TryCacheOriginal(m);
            if (m.HasProperty(MaterialPropertyName))
            {
                float orig = _originalValue ? 1.0f : 0.0f;
                m.SetFloat(MaterialPropertyID, Mathf.Lerp(orig, PropertyValue ? 1.0f : 0.0f, intensity));
            }
            bool value = intensity > 0.0f ? PropertyValue : _originalValue;
            if (value == true)
            {
                m.EnableKeyword(MaterialPropertyName.ToUpper());
            }
            else
            { 
                m.DisableKeyword(MaterialPropertyName.ToUpper());
            }
        }

        //Be careful this won't actually set keywords on blocks!
        public override void ApplyPropertyToMaterialPropertyBlock(MaterialPropertyBlock block, Material m, float intensity = 1.0f)
        {
            TryCacheOriginal(m);
            float orig = _originalValue ? 1.0f : 0.0f;
            block.SetFloat(MaterialPropertyID, Mathf.Lerp(orig, PropertyValue ? 1.0f : 0.0f, intensity));
        }

        public override MaterialPropertyType GetMaterialPropertyType()
        {
            return MaterialPropertyType.Texture;
        }
    }
}
