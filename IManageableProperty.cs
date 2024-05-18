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
        void TryCacheOriginal(Material m);
        void ApplyPropertyToMaterial(Material m, float intensity = 1.0f);

        void ApplyPropertyToMaterialPropertyBlock(MaterialPropertyBlock block, Material m, float intensity = 1.0f);

        MaterialPropertyType GetMaterialPropertyType();
    }
}