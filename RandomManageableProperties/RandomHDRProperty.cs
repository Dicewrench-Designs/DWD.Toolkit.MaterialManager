//© Dicewrench Designs LLC 2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

namespace DWD.MaterialManager
{
    public class RandomHDRProperty : RandomColorProperty
    {
        public override MaterialPropertyType GetMaterialPropertyType()
        {
            return MaterialPropertyType.HDR;
        }
    }
}