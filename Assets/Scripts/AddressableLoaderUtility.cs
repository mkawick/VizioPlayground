using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

// possible addition for coloring hierarchy items for prefabs
// https://yarnthen.github.io/yarnthencohosking/how%20to/2018/04/25/Unity-Custom-Hierarchy.html

namespace MAIN.Scripts.Utilities
{
    public class AddressableLoaderUtility : MonoBehaviour
    {
        private Camera _mainCam;
        public static bool isLoadingDone = false;
        [Range(5,40)]
        public int numItemsDeaddressedPerTick = 20;
        private IEnumerator Start()
        {
            _mainCam = Camera.main;
            yield return new WaitForSeconds(0.5f);
            yield return new WaitForEndOfFrame();
            List<AddressableAnchor> anchors = GetComponentsInChildren<AddressableAnchor>().ToList();
            anchors = anchors.OrderBy(x => Vector3.Distance(x.transform.position, _mainCam.transform.position)).ToList();
            for (int index = 0; index < anchors.Count; index++)
            {
                AddressableAnchor addressableAnchor = anchors[index];
                addressableAnchor.LoadAndSpawnObject();
                // give unity a chance to load, keeping the UI active
                if (index % numItemsDeaddressedPerTick == 0)
                {
                    yield return null;
                }
                if (index % (numItemsDeaddressedPerTick * 2) == 0)
                {// sort by the distance and load closer items first
                    anchors = anchors.OrderByDescending(x => x.SpawnComplete).ThenBy(x => Vector3.Distance(x.transform.position, _mainCam.transform.position)).ToList();
                }
                if (index == numItemsDeaddressedPerTick)
                {
                    isLoadingDone = true;
                }
            }

            // remove event systems
            List<EventSystem> eventListeners = GetComponentsInChildren<EventSystem>().ToList();
            for (int index = 0; index < eventListeners.Count; index++)
            {
                eventListeners[index].gameObject.SetActive(false);
            }
        }

#if UNITY_EDITOR 
       // [ContextMenu("Setup Addressables for Children Prefabs")]
        public void Setup(string sceneName, Dictionary<string, int> variantCount)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetGroup assetGroup = settings.FindGroup(sceneName);

            Transform[] allChildren = GetComponentsInChildren<Transform>(false);
            for (var index = 0; index < allChildren.Length; index++)
            {
                Transform child = allChildren[index];
                if (child)
                {
                    Object parentObject = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);

                    if (parentObject)
                    {
                        if (child.GetComponent<Renderer>()) DestroyImmediate(child.GetComponent<Renderer>());
                        if (child.GetComponent<Collider>()) DestroyImmediate(child.GetComponent<Collider>());
                        if (child.GetComponent<Mesh>()) DestroyImmediate(child.GetComponent<Mesh>());
                        string previousName = child.name;
                        string newName = Regex.Replace(previousName, @"\s\(\d*\)", "");
                        child.name = newName;
                        AddressableAnchor anchor = child.GetComponent<AddressableAnchor>();
                        if (!anchor) //anchor = child.gameObject.AddComponent<AddressableAnchor>();
                        {
                            Debug.LogError("Game object is not AddressableAnchor: " + child.name);
                        }
                        else
                        {
                            anchor.Setup(assetGroup, variantCount);
                        }
                    }
                }
            }
        }
#endif
    }
}