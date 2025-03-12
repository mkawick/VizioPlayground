//using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using System.Linq;
using TMPro;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class Startup : MonoBehaviour
{
    [SerializeField] private AssetReference[] loadableScene;
    [SerializeField] private Canvas textCanvas;
    [SerializeField] private float waitTimeToFetchInput = 3;
    Scene loadedScene;
    int lastnum;
    int unloadScene = 5;
    private AsyncOperationHandle<SceneInstance> sceneHandle;

    private void Input_OnJump(object sender, System.EventArgs e)
    {
        //var textObj = textCanvas.GetComponentInChildren<TMP_Text>();
        //textObj.color = new Color32(64, 255, 64, 255);

        var input = GetComponent<GameInput>();
        input.OnJump -= Input_OnJump;
        

        //
    }

    void UnloadLastScene(int id)
    {
        if (id != unloadScene)
        {
            if (sceneHandle.IsValid() && sceneHandle.Result.Scene.isLoaded)
            {
                Addressables.UnloadSceneAsync(sceneHandle, UnloadSceneOptions.None);
            }
        }
    }

    void LoadNextScene(int id)
    {
        sceneHandle = Addressables.LoadSceneAsync(loadableScene[id - 1], LoadSceneMode.Additive);
        sceneHandle.Completed += handle =>
        {
            loadedScene = handle.Result.Scene;

            var inspect = loadedScene.GetRootGameObjects();
            loadedScene.GetRootGameObjects().First(x => x.name == "Root").SetActive(false);
            if (textCanvas)
            {
                textCanvas.gameObject.SetActive(false);
            }
        };

    }

    private void Input_OnSelectLevel(KeyCode key)
    {
        Debug.Log(key);    
        if (lastnum != unloadScene)
        {
            UnloadLastScene(lastnum);            
        }
        if (key == (KeyCode)unloadScene)
        {
            lastnum = unloadScene;
            if (textCanvas)
            {
                textCanvas.gameObject.SetActive(true);
            }
            return;
        }

        if (key == (KeyCode)1)
            lastnum = 1;
        else if(key == (KeyCode)2)
            lastnum = 2;
        else if(key == (KeyCode)3)
            lastnum = 3;
        else if (key == (KeyCode)4)
            lastnum = 4;

        LoadNextScene(lastnum);
    }

    void Start()
    {
        Invoke("GrabInputSystem", waitTimeToFetchInput);

        var textObj = textCanvas.GetComponentInChildren<TMP_Text>();
        textObj.color = new Color32(128, 128, 128, 255);
        textCanvas.gameObject.SetActive(true);
        lastnum = unloadScene;
    }

    void GrabInputSystem()
    {
        var textObj = textCanvas.GetComponentInChildren<TMP_Text>();
        textObj.color = new Color32(255, 64, 64, 255);
        
        var input = GetComponent<GameInput>();
        input.OnSelectLevel += Input_OnSelectLevel;
    }

}
