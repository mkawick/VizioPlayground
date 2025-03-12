//using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;
using TMPro;

public class StartupSimple : MonoBehaviour
{
    [SerializeField] private AssetReference[] loadableScene;
    [SerializeField] private Canvas textCanvas;
    [SerializeField] private float waitTimeToFetchInput = 3;

    private void Input_OnJump(object sender, System.EventArgs e)
    {
        var textObj = textCanvas.GetComponentInChildren<TMP_Text>();
        textObj.color = new Color32(64, 255, 64, 255);

        var input = GetComponent<GameInput>();
        input.OnJump -= Input_OnJump;
        

        var asyncOperationHandle = Addressables.LoadSceneAsync(loadableScene, LoadSceneMode.Additive);
        asyncOperationHandle.Completed += handle =>
        {
            // 
            handle.Result.Scene.GetRootGameObjects().First(x => x.name == "Root").SetActive(false);
            if (textCanvas)
            {
                textCanvas.gameObject.SetActive(false);
            }
        };
        //Addressables.UnloadSceneAsync();
    }

    private void Input_OnSelectLevel(KeyCode key)
    {
        Debug.Log(key);    
    }

    void Start()
    {
        Invoke("GrabInputSystem", waitTimeToFetchInput);

        var textObj = textCanvas.GetComponentInChildren<TMP_Text>();
        textObj.color = new Color32(128, 128, 128, 255);
        textCanvas.gameObject.SetActive(true);
    }

    void GrabInputSystem()
    {
        var textObj = textCanvas.GetComponentInChildren<TMP_Text>();
        textObj.color = new Color32(255, 64, 64, 255);
        
        var input = GetComponent<GameInput>();
        input.OnJump += Input_OnJump;
        input.OnSelectLevel += Input_OnSelectLevel;
    }

}
