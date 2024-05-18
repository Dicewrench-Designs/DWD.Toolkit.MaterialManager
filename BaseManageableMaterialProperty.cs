//© Dicewrench Designs LLC 2019-2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;

namespace DWD.MaterialManager
{
    /// <summary>
    /// Base <see cref="ScriptableObject"/> for ManageableMaterialProperties
    /// </summary>
    public abstract class BaseManageableMaterialProperty : ScriptableObject, IManageableProperty
    {
        //cache the shader ID for our property
        //so things are faster when we actually set 
        //stuff...
        protected int _materialPropertyID = -1;
        public int MaterialPropertyID
        {
            get
            {
                if (_materialPropertyID == -1)
                    _materialPropertyID = Shader.PropertyToID(_materialPropertyName);
                return _materialPropertyID;
            }
        }

        //for dev facing data use string name
        [SerializeField]
        [HideInInspector]
        protected string _materialPropertyName = "_SomeProperty";
        public string MaterialPropertyName { get { return _materialPropertyName; } }

        public void OnPropertyNameChanged()
        {
            _materialPropertyID = -1;
        }

        public virtual void TryCacheOriginal(Material m) { }

        public virtual void ApplyPropertyToMaterial(Material m, float intensity = 1.0f) { return; }

        public virtual void ApplyPropertyToMaterialPropertyBlock(MaterialPropertyBlock block, Material m, float intensity = 1.0f) { return; }

        public virtual MaterialPropertyType GetMaterialPropertyType() { return MaterialPropertyType.Color; }

        public virtual void Reset() { return; }
    }

    /// <summary>
    /// "Open" version of <see cref="BaseManageableMaterialProperty"/>
    /// where developers can set MaterialPropertyName.
    /// </summary>
    public abstract class OpenManageableMaterialProperty : BaseManageableMaterialProperty
    {
        public new string MaterialPropertyName
        {
            get { return _materialPropertyName; }
            set
            {
                if (_materialPropertyName != value)
                {
                    _materialPropertyName = value;
                    //when we change value update our shader id
                    OnPropertyNameChanged();
                }
            }
        }
    }

    /// <summary>
    /// "Sealed" version of <see cref="IManageableProperty"/>
    /// where MaterialPropertyName is only set via the Constructor.
    /// Because this is explicitly inlined we don't need to inherit
    /// <see cref="ScriptableObject"/>.
    /// </summary>
    [System.Serializable]
    public abstract class SealedManageableMaterialProperty : IManageableProperty
    {
        public SealedManageableMaterialProperty(string propertyName)
        {
            _materialPropertyName = propertyName;
        }

        public int MaterialPropertyID
        {
            get
            {
                return Shader.PropertyToID(_materialPropertyName);
            }
        }

        public int ResetID()
        {
            return MaterialPropertyID;
        }

        //for dev facing data use string name
        [SerializeField]
        [HideInInspector]
        protected string _materialPropertyName = "_SomeProperty";

        public virtual void TryCacheOriginal(Material m) { }

        public virtual void ApplyPropertyToMaterial(Material m, float intensity) { return; }

        public virtual void ApplyPropertyToMaterialPropertyBlock(MaterialPropertyBlock block, Material m, float intensity) { return; }

        public virtual MaterialPropertyType GetMaterialPropertyType() { return MaterialPropertyType.Color; }
    }

    /// <summary>
    /// Base Class for ManageableMaterialProperties of different Types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ManageableMaterialProperty<T> : OpenManageableMaterialProperty
    {
        [SerializeField]
        [HideInInspector]
        protected T _propertyValue;
        public T PropertyValue
        {
            get { return _propertyValue; }
            set { _propertyValue = value; }
        }

        protected bool _originalCached = false;
        protected T _originalValue;
    }

    /// <summary>
    /// Base Class for SealedMaterialProperties of different Types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SealedMaterialProperty<T> : SealedManageableMaterialProperty
    {
        public SealedMaterialProperty(string propertyName) : base(propertyName)
        {
            _materialPropertyName = propertyName;
        }

        [SerializeField]
        [HideInInspector]
        private T _propertyValue;
        public T PropertyValue
        {
            get { return _propertyValue; }
            set { _propertyValue = value; }
        }

        protected bool _originalCached = false;
        protected T _originalValue;
    }

    public enum MaterialPropertyType
    {
        Color,
        HDR,
        Float,
        Texture,
        Vector,
        Keyword
    }
}