# GitHub Copilot Instructions – DWD MaterialManager

## Project Overview

**DWD MaterialManager** is a free, open-source Unity toolkit published by **Dicewrench Designs LLC** (Allen White). It provides a **ScriptableObject-based system** for dynamically managing, modifying, and synchronising Material properties in Unity projects at both edit-time and runtime.

- **Package ID:** `com.dicewrenchdesigns.dwdmaterialmanager`
- **Version:** See `package.json` (currently 1.0.0)
- **Minimum Unity Version:** 2019.1
- **Namespace:** `DWD.MaterialManager`
- **Full Documentation:** https://dicewrenchdesigns.com/2024/04/material-manager/
- **License:** Custom MIT variant – redistribution and modification are permitted; selling or including in the Unity Asset Store is prohibited without express permission.

---

## Repository Structure

```
DWD.Toolkit.MaterialManager/
│
├── .github/
│   └── copilot-instructions.md          # This file
│
├── IManageableProperty.cs               # Core interface for all manageable properties
├── BaseManageableMaterialProperty.cs    # Abstract base classes and MaterialPropertyType enum
├── MaterialCollection.cs                # ScriptableObject grouping materials + properties
│
├── ManageableProperties/                # Concrete ScriptableObject property types (open / runtime)
│   ├── ManageableColorProperty.cs
│   ├── ManageableFloatProperty.cs
│   ├── ManageableHDRProperty.cs
│   ├── ManageableKeywordProperty.cs
│   ├── ManageableTextureProperty.cs
│   ├── ManageableVectorProperty.cs
│   └── Editor/                          # Custom Inspectors for manageable properties
│
├── SealedProperties/                    # Inline / serialisable property types (no ScriptableObject)
│   ├── SealedColorProperty.cs
│   ├── SealedFloatProperty.cs
│   ├── SealedHDRProperty.cs
│   ├── SealedKeywordProperty.cs
│   ├── SealedTextureProperty.cs
│   ├── SealedVectorProperty.cs
│   └── Editor/                          # Custom Inspectors for sealed properties
│
├── RandomManageableProperties/          # Property types that generate random values between two endpoints
│   ├── RandomManageableProperty.cs      # Generic abstract base (T firstValue, T secondValue)
│   ├── RandomColorProperty.cs
│   ├── RandomFloatProperty.cs
│   ├── RandomHDRProperty.cs
│   ├── RandomKeywordProperty.cs
│   ├── RandomTextureProperty.cs
│   ├── RandomVectorProperty.cs
│   └── Editor/                          # Custom Inspectors for random properties
│
├── MaterialSyncer/
│   ├── MaterialSyncer.cs                # Sync shader properties across Material / MaterialPropertyBlock combos
│   └── MaterialConverter.cs            # ScriptableObject: convert materials from one shader to another
│
├── TexturePacker/
│   ├── AbstractTexturePackerConfig.cs   # ScriptableObject template for multi-texture channel packing
│   └── TexturePackerConfig.cs          # Concrete packer configuration
│
├── MaterialShaderVariantMaker/
│   ├── MaterialShaderVariantMaker.cs    # ScriptableObject config for shader variant collection generation
│   ├── SearchFolder.cs                  # Helper: folder reference for asset searching
│   └── Editor/
│       ├── ShaderVariantMaker.cs
│       ├── MaterialShaderVariantMakerEditor.cs
│       └── SearchFolderPropertyDrawer.cs
│
├── CustomRenderTextureRandomizer/
│   ├── CustomRenderTextureRandomizer.cs # Runtime: generates randomised CustomRenderTexture outputs
│   └── Editor/
│       └── CustomRenderTextureRandomizerEditor.cs
│
├── Editor/                              # Top-level editor utilities
│   ├── AbstractTexturePackerConfigEditor.cs
│   ├── ChannelOutputPropertyDrawer.cs
│   ├── MaterialCollectionEditor.cs
│   ├── MaterialConverterEditor.cs
│   ├── MaterialConversionPostProcessor.cs  # AssetPostprocessor for automatic material conversion
│   ├── TextureOutputPropertyDrawer.cs
│   ├── TexturePackerConfigEditor.cs
│   └── Resources/                       # Editor-only resources (icons, layouts, etc.)
│
├── _Examples/
│   └── ManagedProperties/
│       └── PropertyUsageExample.cs      # MonoBehaviour showing typical usage
│
├── DWD.MaterialManager.asmdef           # Runtime assembly definition
├── Editor/DWD.MaterialManager.Editor.asmdef  # Editor-only assembly definition
├── package.json                         # UPM package metadata
├── .gitignore                           # Standard Unity .gitignore
├── LICENSE                              # Custom MIT variant
└── README.md                            # Brief overview + documentation link
```

---

## Architecture

### Core Abstraction Layers

#### 1. `IManageableProperty` (interface)

The single contract that all property types implement:

```csharp
public interface IManageableProperty
{
    void TryCacheOriginal(Material m);
    void ApplyPropertyToMaterial(Material m, float intensity = 1.0f);
    void ApplyPropertyToMaterialPropertyBlock(MaterialPropertyBlock block, Material m, float intensity = 1.0f);
    MaterialPropertyType GetMaterialPropertyType();
}
```

- `TryCacheOriginal` – lazily stores the original property value the first time it is applied, enabling lerp/blend via `intensity`.
- `ApplyPropertyToMaterial` – writes the property directly to a `Material`.
- `ApplyPropertyToMaterialPropertyBlock` – writes to a `MaterialPropertyBlock` (preferred for per-renderer overrides at runtime).
- `GetMaterialPropertyType` – returns the `MaterialPropertyType` enum value.

#### 2. Base Classes in `BaseManageableMaterialProperty.cs`

| Class | Inherits | Key Characteristic |
|---|---|---|
| `BaseManageableMaterialProperty` | `ScriptableObject`, `IManageableProperty` | Caches `Shader.PropertyToID` for performance; property name exposed read-only |
| `OpenManageableMaterialProperty` | `BaseManageableMaterialProperty` | `MaterialPropertyName` has a public setter; calling it resets the cached ID |
| `SealedManageableMaterialProperty` | `IManageableProperty` (no `ScriptableObject`) | Property name set only via constructor; serialisable as an inline field |
| `ManageableMaterialProperty<T>` | `OpenManageableMaterialProperty` | Adds generic `PropertyValue` and original-value caching |
| `SealedMaterialProperty<T>` | `SealedManageableMaterialProperty` | Sealed variant with generic `PropertyValue` |

#### 3. Property Type Variants (3 paradigms × 6 types = 18 concrete classes)

| Variant | Base | Usage |
|---|---|---|
| **Manageable** (`ManageableProperties/`) | `ManageableMaterialProperty<T>` | ScriptableObject assets; reusable across materials |
| **Sealed** (`SealedProperties/`) | `SealedMaterialProperty<T>` | Inline `[SerializeField]` fields; property name fixed at construction |
| **Random** (`RandomManageableProperties/`) | `RandomManageableProperty<T>` | Two values (`PropertyValue`, `SecondValue`); call `GetRandomValue()` |

Each variant supports all six property types:

| Type | Unity API | Intensity behaviour |
|---|---|---|
| `Color` | `m.SetColor` | `Color.Lerp(original, target, intensity)` |
| `HDR` | `m.SetColor` | `Color.Lerp` (HDR range) |
| `Float` | `m.SetFloat` | `Mathf.Lerp(original, target, intensity)` |
| `Vector` | `m.SetVector` | `Vector4.Lerp` |
| `Texture` | `m.SetTexture` | Full swap at `intensity == 1`, original otherwise |
| `Keyword` | `m.EnableKeyword` / `DisableKeyword` | Enable at `intensity == 1`, disable otherwise |

#### 4. `MaterialCollection` (ScriptableObject)

Groups an array of `Material`s with an array of `BaseManageableMaterialProperty` instances. Key methods:

```csharp
collection.ApplyAllProperties(float intensity);
collection.ApplyProperty(BaseManageableMaterialProperty prop, float intensity);
collection.ApplyAllPropertiesToBlock(MaterialPropertyBlock block, float intensity);
collection.ApplyPropertyToBlock(MaterialPropertyBlock block, BaseManageableMaterialProperty prop, float intensity);
collection.GetManagedPropertyForParam(int id);   // find a property by shader ID
collection.RecalculateIDs();                     // force re-cache of all shader property IDs
```

Create via: **Assets → Create → DWD → Material Collection**

#### 5. `MaterialSyncer`

A plain `[Serializable]` class (not a ScriptableObject) with static `Sync` overloads:

```csharp
MaterialSyncer.Sync(Material source, Material destination, ShaderPropertyTypePair[] properties);
MaterialSyncer.Sync(Material source, MaterialPropertyBlock destination, ShaderPropertyTypePair[] properties);
MaterialSyncer.Sync(MaterialPropertyBlock source, MaterialPropertyBlock destination, ShaderPropertyTypePair[] properties);
MaterialSyncer.Sync(MaterialPropertyBlock source, Material destination, ShaderPropertyTypePair[] properties);
MaterialSyncer.Sync(MaterialPropertyBlock source, ShaderPropertyTypePair[] properties);  // in-block rename
```

`ShaderPropertyTypePair` holds a `propertyName`, `destinationName`, `ShaderPropertyType`, and optional `AbstractTexturePackerConfig`.

#### 6. `MaterialConverter` (ScriptableObject)

Defines conversion rules between two shaders:

```csharp
converter.SourceShader       // Shader to convert FROM
converter.DestinationShader  // Shader to convert TO
converter.PropertyMap        // ShaderPropertyTypePair[] mapping source → destination names
```

Create via: **Assets → Create → DWD → Material Converter**

Automatic conversion is triggered by `MaterialConversionPostProcessor` (an `AssetPostprocessor`) when materials are imported.

#### 7. Texture Packing (`AbstractTexturePackerConfig`, `TexturePackerConfig`)

ScriptableObjects that define how texture channels from source shader properties are packed into output textures:

- `InputPropertyNames` – ordered list of source shader property names
- `OutputTextures` – array of `TextureOutput` describing which input channel maps to which output channel

Create via: **Assets → Create → DWD → Abstract Texture Packer Config**

#### 8. `MaterialShaderVariantMaker`

ScriptableObject + Editor tool for building `ShaderVariantCollection` assets from materials in specified folders.

#### 9. `CustomRenderTextureRandomizer`

MonoBehaviour that generates randomised `CustomRenderTexture` outputs at runtime.

---

## Design Patterns

| Pattern | Where Used |
|---|---|
| **ScriptableObject data** | All property assets, collections, converters, packer configs |
| **Strategy** | Six property types each implement `IManageableProperty` differently |
| **Template Method** | Base classes define the apply flow; subclasses override type-specific logic |
| **Generics** | `ManageableMaterialProperty<T>` / `SealedMaterialProperty<T>` / `RandomManageableProperty<T>` |
| **Shader ID caching** | `Shader.PropertyToID` cached on first use; reset on name change (performance) |
| **Intensity/lerp blending** | All property types accept `float intensity` (0–1) enabling smooth transitions |
| **AssetPostprocessor** | `MaterialConversionPostProcessor` auto-converts materials on import |

---

## Coding Conventions

- **Copyright header** on every file: `//© Dicewrench Designs LLC <years>` followed by `//Last Owned by: Allen White (allen@dicewrenchdesigns.com)`.
- **Namespace:** `DWD.MaterialManager` for all runtime code.
- **Serialised backing fields** use `_camelCase` prefix; public properties use `PascalCase` with `{ get; }` or `{ get; set; }`.
- **`[SerializeField]` + `[HideInInspector]`** on internal fields to keep Inspectors clean while still serialising.
- **`[CreateAssetMenu(menuName = "DWD/...")]`** on every ScriptableObject intended for direct creation.
- **`[Tooltip("...")]`** on all serialised fields intended to be Inspector-visible.
- Editor-only code lives in the `Editor/` subdirectory of each feature folder and is referenced by the editor assembly definition.
- No external NuGet dependencies; only `UnityEngine`, `UnityEngine.Rendering`, `UnityEngine.Experimental.Rendering`, and `UnityEditor` (conditional).

---

## Adding a New Property Type

1. **Runtime class** – Create in `ManageableProperties/`, inherit `ManageableMaterialProperty<T>`, override `TryCacheOriginal`, `ApplyPropertyToMaterial`, `ApplyPropertyToMaterialPropertyBlock`, and `GetMaterialPropertyType`.
2. **Sealed variant** – Create in `SealedProperties/`, inherit `SealedMaterialProperty<T>`, same overrides, constructor sets the property name.
3. **Random variant** – Create in `RandomManageableProperties/`, inherit `RandomManageableProperty<T>`, implement `GetRandomValue()`.
4. **Editor drawers** – Add a matching `PropertyDrawer` or `CustomEditor` in the corresponding `Editor/` subfolder; reference it from `DWD.MaterialManager.Editor.asmdef`.
5. **MaterialPropertyType enum** – Add the new type to the `MaterialPropertyType` enum in `BaseManageableMaterialProperty.cs`.

---

## Installation

### Via Unity Package Manager (recommended)

1. In Unity, open **Window → Package Manager**.
2. Click the **+** button → **Add package from git URL**.
3. Enter the repository URL.

### Manual

Copy the contents of this repository into your project's `Assets/` folder.

---

## Usage Example

```csharp
using DWD.MaterialManager;
using UnityEngine;

public class PropertyUsageExample : MonoBehaviour
{
    public Material materialToModify;

    // Inline sealed property – property name fixed to "_Color"
    public SealedColorProperty colorProperty = new SealedColorProperty("_Color");

    private void OnValidate()
    {
        if (materialToModify != null)
        {
            // Apply directly to the Material
            colorProperty.ApplyPropertyToMaterial(materialToModify);

            // Or apply to a MaterialPropertyBlock for per-renderer overrides:
            // colorProperty.ApplyPropertyToMaterialPropertyBlock(myBlock, materialToModify);
            // myRenderer.SetPropertyBlock(myBlock);
        }
    }
}
```

More examples are in `_Examples/ManagedProperties/PropertyUsageExample.cs`.

---

## Assembly Definitions

| File | Platform | References |
|---|---|---|
| `DWD.MaterialManager.asmdef` | Any (runtime) | `UnityEngine` |
| `Editor/DWD.MaterialManager.Editor.asmdef` | Editor only | `DWD.MaterialManager` + `UnityEditor` |

---

## No Build / CI System

This is a Unity-native package. There are no Makefiles, npm scripts, or GitHub Actions workflows. Build and test by:

1. Importing the package into a Unity project.
2. Creating ScriptableObject instances via the **Assets → Create → DWD** menus.
3. Using the custom Editor Inspectors to configure properties.
4. Calling `ApplyPropertyToMaterial` or `ApplyAllProperties` from your runtime scripts.
