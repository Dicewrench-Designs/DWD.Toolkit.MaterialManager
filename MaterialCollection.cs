//© Dicewrench Designs LLC 2019-2023
//Licensed for use in 'Baseball Bash' App
//All Rights Reserved
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;

//Last owned by: Allen White

namespace DWD.MaterialManager
{
   /// <summary>
   /// An array of Materials and ManageableProperty objects that can be used to affect them.
   /// The result is a controller of "pseudo global properties" that impact only selected 
   /// materials.
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

      public void ApplyAllProperties()
      {
         int count = managedProperties.Length;

         for(int a = 0; a < count; a++)
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
   }
}