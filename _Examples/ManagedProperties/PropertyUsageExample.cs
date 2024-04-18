//© Dicewrench Designs LLC 2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using DWD.MaterialManager;
using UnityEngine;

public class PropertyUsageExample : MonoBehaviour
{
    [Header("Material to Change OnValidate")]
    public Material materialToModify;

    [Header("Properties to Modify")]
    [Tooltip("Sealed Color Property for '_Color' ")]
    public SealedColorProperty colorProperty = new SealedColorProperty("_Color");

    private void OnValidate()
    {        
        if(materialToModify != null)
        {
            //Applies changes straight to the Material!            
            colorProperty.ApplyPropertyToMaterial(materialToModify);

            //You could also use ApplyPropertyToMaterialPropertyBlock 
            //and provide a MPB and then apply that to a Renderer...
            //
            //colorProperty.ApplyPropertyToMaterialPropertyBlock(myBlock);;
            //myRenderer.SetPropertyBlock(myBlock);
            //
        }
    }
}
