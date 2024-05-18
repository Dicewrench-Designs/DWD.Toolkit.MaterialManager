//© Dicewrench Designs LLC 2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;
using System;

namespace DWD.MaterialManager
{
    [System.Serializable]
    public abstract class RandomManageableProperty<T> : ManageableMaterialProperty<T>
    {
        [SerializeField]
        private T _secondValue;
        public T SecondValue
        {
            get { return _secondValue; }
            set { _secondValue = value; }
        }

        public abstract T GetRandomValue();
    }
}