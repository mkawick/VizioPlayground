using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif

namespace MAIN.Scripts.Utilities
{
    public class AddressableAnchor : MonoBehaviour
    {
        private GameObject _spawn = null;
        public bool SpawnComplete { get; set; }
        public void LoadAndSpawnObject()
        {
            if (_spawn == null)
            {
                Addressables.LoadAssetAsync<GameObject>(gameObject.name).Completed += OnCompleted;
            }
        }

        private void OnCompleted(AsyncOperationHandle<GameObject> obj)
        {
            GameObject originalObj = obj.Result;
            //var prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(originalObj);
            _spawn = Instantiate(originalObj, transform);
            _spawn.transform.localPosition = Vector3.zero;
            _spawn.transform.localScale = Vector3.one;
            SpawnComplete = true;
        }

        //----------------------------------------------------------------
#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
        public class ReadOnlyDrawer : PropertyDrawer
        {
            public override float GetPropertyHeight(SerializedProperty property,
                                                    GUIContent label)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            public override void OnGUI(Rect position,
                                       SerializedProperty property,
                                       GUIContent label)
            {
                GUI.enabled = false;
                EditorGUI.PropertyField(position, property, label, true);
                GUI.enabled = true;
            }
        }
     
        public bool HasTooManyComponentsAnchor()
        {
            return GetComponents(typeof(Component)).Length > 2 && GetComponent<AddressableAnchor>() != null;
        }
        public bool IsOverrideAnchor()
        {
            return PrefabUtility.HasPrefabInstanceAnyOverrides(gameObject, false);
        }

        [Header("problem"), ShowIf("HasTooManyComponentsAnchor"), GUIColor(1, 0, 1), ReadOnly]
        public string errorTooManyComponents = "Anchors only on types of more than 2 components, remove other components";
        [Header("problem"), ShowIf("IsOverrideAnchor"), GUIColor(1, 1, 0), ReadOnly]
        public string errorIsPrefabOverride = "overrides cannot be made into addressables";

        //----------------------------------------------------------------

        [ContextMenu("Setup Addressable Anchor")]
        public void Setup()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup assetGroup = settings.FindGroup("Default Local Group");
            Dictionary<string, int> variantCount = new Dictionary<string, int>();// not used
            Setup(assetGroup, variantCount);
        }

        private void SetupAddressableEntry(bool isVariant, string pathToObject, Dictionary<string, int> variantCount, ref Object parentObject, AddressableAssetEntry entry, AddressableAssetSettings settings)
        {
         /*   if (isVariant) // create prefab variant here possibly
            {
                string newName = CreateVariantName(pathToObject, variantCount);

                gameObject.name = parentObject.name;
                entry.address = gameObject.name;
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            }
            else*/
            {
                gameObject.name = parentObject.name;
                entry.address = gameObject.name;
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            }
        }

        public void Setup(ScriptableObject groupInDisguise, Dictionary<string, int> variantCount)
        {
            bool allPrefabsIncluded = false;
            bool isOverride = PrefabUtility.HasPrefabInstanceAnyOverrides(gameObject, allPrefabsIncluded);
            
            if(isOverride) {
                Debug.LogError("Game object is overridden in scene; not added as addressable" + gameObject.name);
                return;
            }
            AddressableAssetGroup assetGroup = groupInDisguise as AddressableAssetGroup;
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

            Object parentObject = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            string pathToObject = AssetDatabase.GetAssetPath(parentObject);

            string guid = AssetDatabase.AssetPathToGUID(pathToObject);
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, assetGroup);

            SetupAddressableEntry(isOverride, pathToObject, variantCount, ref parentObject, entry, settings);

            AssetDatabase.SaveAssets();
            PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            RemoveChildren();
        }

      /*  string CreateVariantName(string pathToObject, Dictionary<string, int> variantCount)
        {
            string dir = System.IO.Path.GetDirectoryName(pathToObject);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(pathToObject);
            string fileExt = System.IO.Path.GetExtension(pathToObject);

            int num = 0;
            if (variantCount.ContainsKey(pathToObject))
            {
                num = variantCount[pathToObject];
                num++;
            }

            string variantId = num.ToString("000");
            variantCount[pathToObject] = num;
            string encoding = "VAR" + variantId + fileExt;
            return dir + "/" + fileName + encoding;
        }*/

        [ContextMenu("RemoveChildren")]
        public void RemoveChildren()
        {
            List<Transform> tempList = transform.Cast<Transform>().ToList();
            foreach (Transform o in tempList)
            {
                if (o.gameObject.name != gameObject.name)
                {
                    DestroyImmediate(o.gameObject);
                }
            }
        }

        [ContextMenu("ExplorePrefabOverrides")]
        public void ExplorePrefabOverrides()
        {
            Debug.Log("name = " + gameObject.name);
            bool allPrefabsIncluded = false;
            bool isOverride = PrefabUtility.HasPrefabInstanceAnyOverrides(gameObject, allPrefabsIncluded);
            Debug.Log("isOverride = " + isOverride);
            Object parentObject = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            Debug.Log("parent obj = " + parentObject);

            string pathToParent = AssetDatabase.GetAssetPath(parentObject);
            Debug.Log("parent path = " + pathToParent);
        }
#endif
    }
}


        /*public void Setup(ScriptableObject groupInDisguise)
        {
            AddressableAssetGroup g = groupInDisguise as AddressableAssetGroup;
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

            Object parentObject = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            string pathToObject = AssetDatabase.GetAssetPath(parentObject);

            string guid = AssetDatabase.AssetPathToGUID(pathToObject);
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, g);
            //entry.SetLabel(sceneName, true, true);
            gameObject.name = parentObject.name;
            entry.address = gameObject.name;
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            AssetDatabase.SaveAssets();
            PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            RemoveChildren();
        }*/

        /*   private void SaveMatsAndShaders(AddressableAssetSettings settings)
           {
               Object parentObject = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
               string pathToObject = AssetDatabase.GetAssetPath(parentObject);
               Material[] mats = parentObject.GetComponentsInChildren<Material>();
               Shader[] shaders = parentObject.GetComponentsInChildren<Shader>();
               Mesh[] meshes = parentObject.GetComponentsInChildren<Mesh>();

               if (mats.Length > 0)
               {
                   string materialsGroupName = "Materials";
                   var materialsGroup = settings.FindGroup(materialsGroupName);
               }
               var shadersGroup = settings.FindGroup(shadersGroupName);
               var meshesGroup = settings.FindGroup(meshesGroupName);

           }*/