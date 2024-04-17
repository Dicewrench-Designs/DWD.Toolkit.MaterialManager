//© Dicewrench Designs LLC 2019-2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;

namespace DWD.MaterialManager
{
    /// <summary>
    /// An array of <see cref="Material"/>s and <see cref="ManageableMaterialProperty{T}"/> objects that can be used to affect them.
    /// The result is a controller of "pseudo global properties" that impact only selected 
    /// <see cref="Material"/>s.
    /// </summary>
    [CreateAssetMenu(menuName = "DWD/Material Collection")]
    public class MaterialCollection : ScriptableObject
    {
        public BaseManageableMaterialProperty[] managedProperties;

        public Material[] materials;

        public void ApplyProperty(BaseManageableMaterialProperty prop)
        {
            if (materials != null)
            {
                int count = materials.Length;
                for (int a = 0; a < count; a++)
                {
                    prop.ApplyPropertyToMaterial(materials[a]);
                }
            }
        }

        /// <summary>
        /// Applys each Property in order.
        /// </summary>
        public void ApplyAllProperties()
        {
            int count = managedProperties.Length;

            for (int a = 0; a < count; a++)
            {
                ApplyProperty(managedProperties[a]);
            }
        }

        public void RecalculateIDs()
        {
            int count = managedProperties.Length;

            for (int a = 0; a < count; a++)
            {
                BaseManageableMaterialProperty temp = managedProperties[a];
                temp.OnPropertyNameChanged();
            }
        }


        public BaseManageableMaterialProperty GetManagedPropertyForParam(int id)
        {
            int count = managedProperties.Length;
            for (int a = 0; a < count; a++)
            {
                BaseManageableMaterialProperty bmmp = managedProperties[a];
                if (bmmp.MaterialPropertyID == id)
                    return bmmp;
            }
            return null;
        }

    }
}