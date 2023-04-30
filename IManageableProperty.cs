//© Dicewrench Designs LLC 2019-2023
//Licensed for use in 'Baseball Bash' App
//All Rights Reserved
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;

namespace DWD.MaterialManager
{
    /// <summary>
    /// Interface for Setting ManageableMaterialProperty Properties...
    /// </summary>
    public interface IManageableProperty
    {
        void ApplyPropertyToMaterial(Material m);

        void ApplyPropertyToMaterialPropertyBlock(MaterialPropertyBlock block);

        MaterialPropertyType GetMaterialPropertyType();
    }
}