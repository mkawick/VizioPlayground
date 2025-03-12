using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;// this using statement needs to be included twice... weird
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;
#endif // UNITY_EDITOR

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PokeSceneManager : MonoBehaviour
{
    PlayerInputActions inputActions;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputActions = new PlayerInputActions();
        inputActions.PlayerMovement.Enable();
        inputActions.PlayerMovement.Jump.performed += Jump_performed;
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
#if UNITY_EDITOR
        int count = UnityEditor.SceneManagement.EditorSceneManager.sceneCount;
        var openScenes = UnityEditor.SceneManagement.EditorSceneManager.GetAllScenes().ToList();
        var scenes = GetListOfAvailableScenes();
        Debug.Log("Num editor scenes: " + UnityEditor.SceneManagement.EditorSceneManager.sceneCount);
        foreach (var openScene in openScenes)
        {
            Debug.Log(openScene.name);
            Debug.Log(openScene.path);
        }
        Debug.Log("Num file scenes: " + scenes.Count);
        foreach (var openScene in scenes)
        {
            Debug.Log(openScene);
        }

        HashSet<string> dict = new HashSet<string>(from x in openScenes select x.path );
        foreach (var scene in scenes)
        {
            dict.Add(scene);
        }
        var merged = dict.ToList();
#endif
    }

    private const string PATH_TO_SCENES_FOLDER = "/Scenes/";
    public List<string> GetListOfAvailableScenes()
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
