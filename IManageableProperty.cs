//© Dicewrench Designs LLC 2019-2024
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