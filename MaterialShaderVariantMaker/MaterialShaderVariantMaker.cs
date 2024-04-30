//© Dicewrench Designs LLC 2018-2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;

namespace DWD.MaterialManager
{
   [CreateAssetMenu(menuName = "DWD/Material Shader Variant Maker")]
   public class MaterialShaderVariantMaker : ScriptableObject
   {
      public ShaderVariantCollection targetShaderVariantCollection;

      public Shader[] targetShaders = new Shader[0];

      public SearchFolder[] searchFolders = new SearchFolder[0];

      public Material[] sourceMaterials = new Material[0];
   }
}