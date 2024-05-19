//© Dicewrench Designs LLC 2024
//Last Owned by: Allen White (allen@dicewrenchdesigns.com)

using UnityEngine;
using UnityEditor;

namespace DWD.MaterialManager.Editor
{
    [CustomEditor(typeof(CustomRenderTextureRandomizer))]
    public class CustomRenderTextureRandomizerEditor : UnityEditor.Editor
    {
        private static string _PROP_TEX = "_sourceTexture";
        private static string _PROP_RAND = "_numberToRandomize";
        private static string _PROP_SEED = "_seed";
        private static string _PROP_PATH = "_outputPath";
        private static string _PROP_ARRAY = "randomProperties";

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            CustomRenderTextureRandomizer crtr = target as CustomRenderTextureRandomizer;
            if (GUILayout.Button("Generate Random"))
            {               
                if(crtr != null)
                {
                    crtr.GenerateOutputs();
                }
            }
            EditorGUILayout.Space();

            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Source", EditorStyles.boldLabel);

                EditorGUI.indentLevel++;
                SerializedProperty tex = serializedObject.FindProperty(_PROP_TEX);
                EditorGUILayout.PropertyField(tex, true);
                EditorGUI.indentLevel--;
            }
            using(new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Setup", EditorStyles.boldLabel);

                EditorGUI.indentLevel++;
                SerializedProperty rand = serializedObject.FindProperty(_PROP_RAND);
                SerializedProperty seed = serializedObject.FindProperty(_PROP_SEED);
                SerializedProperty path = serializedObject.FindProperty(_PROP_PATH);

                EditorGUILayout.PropertyField(rand, true);
                EditorGUILayout.PropertyField(seed, true);

                if(GUILayout.Button(new GUIContent(path.stringValue, "Set Path")))
                {
                    path.stringValue = EditorUtility.SaveFolderPanel("Set Output Path", path.stringValue, "");
                }
                EditorGUI.indentLevel--;
            }
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Random Properties", EditorStyles.boldLabel);

                EditorGUILayout.Space();

                DrawRandomPropertyArray(crtr);

                EditorGUILayout.Space();

                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField("Add New Random Property", EditorStyles.miniLabel);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button(new GUIContent("+ Float", "Add a Random Float Property"), EditorStyles.miniButton))
                        {
                            AddProperty(crtr, typeof(RandomFloatProperty));
                        }

                        if (GUILayout.Button(new GUIContent("+ Color", "Add a Random Color Property"), EditorStyles.miniButton))
                        {
                            AddProperty(crtr, typeof(RandomColorProperty));
                        }

                        if (GUILayout.Button(new GUIContent("+ HDR", "Add a Random HDR Color Property"), EditorStyles.miniButton))
                        {
                            AddProperty(crtr, typeof(RandomHDRProperty));
                        }

                        if (GUILayout.Button(new GUIContent("+ Vector", "Add a Random Vector Property"), EditorStyles.miniButton))
                        {
                            AddProperty(crtr, typeof(RandomVectorProperty));
                        }

                        if (GUILayout.Button(new GUIContent("+ Texture", "Add a Random Texture Property"), EditorStyles.miniButton))
                        {
                            //AddProperty(crtr, typeof(RandomTextureProperty));
                        }

                        if (GUILayout.Button(new GUIContent("+ Keyword", "Add a Random Keyword Property.  Make sure your property and keyword are named correctly!  i.e. _Emission_Enabled & _EMISSION_ENABLED"), EditorStyles.miniButton))
                        {
                            AddProperty(crtr, typeof(RandomKeywordProperty));
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRandomPropertyArray(CustomRenderTextureRandomizer mc)
        {
            SerializedProperty array = serializedObject.FindProperty(_PROP_ARRAY);

            int count = array.arraySize;

            for (int a = 0; a < count; a++)
            {
                SerializedObject element = new SerializedObject(array.GetArrayElementAtIndex(a).objectReferenceValue);
                element.Update();

                if (element != null)
                {
                    SerializedProperty elementPropertyName = element.FindProperty("_materialPropertyName");

                    using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                    {
                        BaseManageableMaterialProperty bmmp = element.targetObject as BaseManageableMaterialProperty;
                        MaterialPropertyType mpt = bmmp.GetMaterialPropertyType();

                        using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                        {
                            EditorGUILayout.LabelField(mc.randomProperties[a].name, EditorStyles.miniLabel, GUILayout.Width(155.0f));

                            EditorGUI.BeginChangeCheck();
                            EditorGUIUtility.labelWidth = 105.0f;
                            EditorGUILayout.PropertyField(elementPropertyName, new GUIContent("Property Name", "The Shader name of the Property you want to manage."));

                            EditorGUIUtility.labelWidth = 0.0f;
                            if (EditorGUI.EndChangeCheck())
                            {
                                element.ApplyModifiedProperties();
                                bmmp.OnPropertyNameChanged();
                            }

                            if (GUILayout.Button(new GUIContent("-", "Remove this Property"), EditorStyles.miniButton, GUILayout.Width(18.0f), GUILayout.Height(18.0f)))
                            {
                                RemoveProperty(mc, a);
                            }
                        }

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUI.BeginChangeCheck();
                            if (mpt != MaterialPropertyType.HDR)
                            {             
                                if(element != null)
                                    EditorGUILayout.PropertyField(element.FindProperty("_propertyValue"), GUIContent.none, true);
                                if (element != null)
                                    EditorGUILayout.PropertyField(element.FindProperty("_secondValue"), GUIContent.none, true);
                            }
                            else
                            {
                                if (element != null)
                                {
                                    SerializedProperty propVal = element.FindProperty("_propertyValue");
                                    SerializedProperty secVal = element.FindProperty("_secondValue");
                                    propVal.colorValue = EditorGUILayout.ColorField(
                                       GUIContent.none,
                                       propVal.colorValue,
                                       showEyedropper: true,
                                       showAlpha: true,
                                       hdr: true);
                                    secVal.colorValue = EditorGUILayout.ColorField(
                                       GUIContent.none,
                                       secVal.colorValue,
                                       showEyedropper: true,
                                       showAlpha: true,
                                       hdr: true);
                                }
                            }
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (element != null)
                                    element.ApplyModifiedProperties();
                            } 
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create a ManageableMaterialProperty of a given Type and serialize it within
        /// our MaterialCollection
        /// </summary>
        /// <param name="mc"></param>
        /// <param name="t"></param>
        private void AddProperty(CustomRenderTextureRandomizer crtr, System.Type t)
        {
            BaseManageableMaterialProperty temp = ScriptableObject.CreateInstance(t) as BaseManageableMaterialProperty;
            temp.name = t.Name;
            temp.hideFlags = HideFlags.HideInHierarchy;

            AddToArray<BaseManageableMaterialProperty>(ref crtr.randomProperties, temp);

            string path = AssetDatabase.GetAssetPath(crtr);
            AssetDatabase.AddObjectToAsset(temp, path);
            AssetDatabase.ImportAsset(path);

            EditorUtility.SetDirty(crtr);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            Repaint();
        }

        private void RemoveProperty(CustomRenderTextureRandomizer mc, int index)
        {
            BaseManageableMaterialProperty temp = mc.randomProperties[index];
            RemoveFromArray<BaseManageableMaterialProperty>(ref mc.randomProperties, index);

            UnityEngine.Object.DestroyImmediate(temp, true);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(mc));
            AssetDatabase.SaveAssets();

            EditorUtility.SetDirty(mc);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            Repaint();
        }

        /// <summary>
        /// Add a thing to an array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="temp"></param>
        private void AddToArray<T>(ref T[] array, T temp)
        {
            int count;

            if (array == null)
            {
                array = new T[0];
                count = 0;
            }
            else
                count = array.Length;

            int newCount = count + 1;
            T[] newArray = new T[newCount];

            if (newCount == 1)
            {
                newArray[0] = temp;
            }
            else
            {
                for (int a = 0; a < count; a++)
                {
                    newArray[a] = array[a];
                }
                newArray[newCount - 1] = temp;
            }

            array = newArray;
        }

        /// <summary>
        /// Remove a thing from an array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="index"></param>
        private void RemoveFromArray<T>(ref T[] array, int index)
        {
            int count = array.Length;
            int newCount = count - 1;

            int currentIndex = 0;

            T[] newArray = new T[newCount];

            for (int a = 0; a < count; a++)
            {
                if (a != index)
                {
                    newArray[currentIndex] = array[a];
                    currentIndex += 1;
                }
            }

            array = newArray;
        }
    }
}