using MAIN.Scripts.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Graphs;
using UnityEditor.SceneManagement;// this using statement needs to be included twice... weird
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;

#endif // UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
[InitializeOnLoad]
class UnityEditorStartup
{
    const string PATH_TO_SCENES_FOLDER = "/Scenes/";
    static UnityEditorStartup()
    {
        BuildPlayerWindow.RegisterBuildPlayerHandler(
            new System.Action<BuildPlayerOptions>(buildPlayerOptions =>
            {
                AssembleAddressables();
                BuildAssetBundles();
                BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(buildPlayerOptions);
            }));
    }

    private static void PreprocessLighting(UnityEngine.SceneManagement.Scene scene)
    {
        // LaunchBakery
    }

    private static void CreateAddressableGroup(UnityEngine.SceneManagement.Scene scene)
    {
        string groupName = scene.name;
        string scenePath = scene.path;
        
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        bool setAsDefault = false;
        bool isReadOnly = false;
        bool hasPostEvent = false;
        var group = settings.FindGroup(groupName);
        if (!group)
        {
            group = settings.CreateGroup(groupName, setAsDefault, isReadOnly, hasPostEvent, null,  typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));
            //EditorBuildSettings.scenes += group;
        }

        var activeSceneAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(scenePath);

        if (UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(activeSceneAsset, out var guid, out var file))
        {
            AddressableAssetGroup g = settings.FindGroup("Default Local Group");
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, g);
            entry.address = groupName; 
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            AssetDatabase.SaveAssets();
            //PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            //UnityEditor.AssetDatabase.Unload
        }

        Debug.Log(group);
    }

    private static List<string> GetPathsOfVaidScenes() 
    {
        var openScenePaths = GetAllScenes().ToList();
        var availableScenePaths = GetListOfAvailableScenes();

        HashSet<string> dict = new HashSet<string>(from x in openScenePaths select x.path);
        foreach (var scene in availableScenePaths)
        {
            dict.Add(scene);
        }
        var merged = dict.ToList();

        return merged;
    }

    public static UnityEngine.SceneManagement.Scene[] GetAllScenes()
    {
        int sceneCount = EditorSceneManager.sceneCount;
        UnityEngine.SceneManagement.Scene[] array = new UnityEngine.SceneManagement.Scene[sceneCount];
        for (int i = 0; i < sceneCount; i++)
        {
            array[i] = EditorSceneManager.GetSceneAt(i);
        }

        return array;
    }

    private static void AssembleAddressables()
    {
        List<string> scenePaths = GetPathsOfVaidScenes();

        var activeScenePath = EditorSceneManager.GetActiveScene().path;
        Dictionary<string, int> variantCount = new Dictionary<string, int>();
        // load all scenes
        OpenSceneMode openSceneMode = OpenSceneMode.Single;
        for (int i = 0; i < scenePaths.Count; i++)
        {
            var scene = EditorSceneManager.OpenScene(scenePaths[i], openSceneMode);
            AddressableLoaderUtility[] addressableLoaders = Object.FindObjectsByType<AddressableLoaderUtility>(FindObjectsSortMode.None);
            if (addressableLoaders.Length > 0)
            {
                CreateAddressableGroup(scene);
                //PreprocessLighting(scene);
                for (int j = 0; j < addressableLoaders.Length; j++)
                {
                    addressableLoaders[j].Setup(scene.name, variantCount);

                    Debug.Log(scene.name + " Bundle added");
                }
            }
            else
            {
                Debug.LogError("Error: " + scene.name + " does not have a scene node with a AddressableLoaderUtility component");
                Debug.LogError("Scene will not have asset bundles");

            }
            //EditorSceneManager.UnloadSceneAsync(scene);
        }

        EditorSceneManager.OpenScene(activeScenePath, openSceneMode);
    }

    private static void BuildAssetBundles()
    {
        AddressableAssetSettingsAccessor addressableAssetSettingsAccessor = new AddressableAssetSettingsAccessor();
        var settings = addressableAssetSettingsAccessor.LoadSettingsObject2();
        AddressableAssetProfileSettings profileSettings = settings.profileSettings;

        string path = AddressableAssetSettingsDefaultObject.DefaultAssetPath;
        string profileId = profileSettings.GetProfileId("Default");

        AddressableAssetSettingsDefaultObject.Settings.activeProfileId = profileId;
        AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
    }
    
    private static List<string> GetListOfAvailableScenes()
    {
        StringBuilder result = new StringBuilder();
        string basePath = Application.dataPath;// ;
        string searchPath = basePath + PATH_TO_SCENES_FOLDER;
        List<string> scenes = new List<string>();
        basePath = basePath.Replace("Assets", "");// this affects the final path below

        AddCodeForDirectory(new DirectoryInfo(searchPath), result);

        void AddCodeForDirectory(DirectoryInfo directoryInfo, StringBuilder result)
        {

            FileInfo[] fileInfoList = directoryInfo.GetFiles();
            for (int i = 0; i < fileInfoList.Length; i++)
            {
                FileInfo fileInfo = fileInfoList[i];
                if (fileInfo.Extension == ".unity")
                {
                    AddCodeForFile(fileInfo, result);
                }
            }
            DirectoryInfo[] subDirectories = directoryInfo.GetDirectories();
            for (int i = 0; i < subDirectories.Length; i++)
            {
                AddCodeForDirectory(subDirectories[i], result);
            }

            void AddCodeForFile(FileInfo fileInfo, StringBuilder result)
            {
                string subPath = fileInfo.FullName.Replace('\\', '/').Replace(basePath, "");
                scenes.Add(subPath);
            }
        }
        return scenes;
    }
}


public class AddressableAssetSettingsAccessor : AddressableAssetSettingsDefaultObject
{
    public string GetGuid()
    {
        AddressableAssetSettingsDefaultObject so;
        EditorBuildSettings.TryGetConfigObject(kDefaultConfigObjectName, out so);
        string guid = typeof(AddressableAssetSettingsDefaultObject).GetField("m_AddressableAssetSettingsGuid", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(so).ToString();
        return guid;
    }
    public AddressableAssetSettings LoadSettingsObject2()
    {
        AddressableAssetSettings settings = new AddressableAssetSettings();

        string guid = GetGuid();
        var path = AssetDatabase.GUIDToAssetPath(guid);
        if (!string.IsNullOrEmpty(path))
        {
            return AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(path);
        }

        return settings;
    }
}



#endif // UNITY_EDITOR
