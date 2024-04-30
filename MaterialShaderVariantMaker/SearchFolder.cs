//© Dicewrench Designs LLC 2018-2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;

namespace DWD.MaterialManager
{
   [System.Serializable]
   public class SearchFolder
   {
      public string folder;
      [Tooltip("Check this folder and any child folders?")]
      public bool recursive = false;

      public SearchFolder() { }
      public SearchFolder(string name, bool recursiveSearch = false)
      {
         folder = name;
         recursive = recursiveSearch;
      }
   }
}