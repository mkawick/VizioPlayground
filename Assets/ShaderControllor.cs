using TMPro;
using UnityEngine;

public class ShaderControllor : MonoBehaviour
{
    public Material[] shaders;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GrabInputSystem();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GrabInputSystem()
    {
        var input = GetComponent<GameInput>();
        input.OnSelectLevel += Input_OnSelectShader;
    }

    private void Input_OnSelectShader(KeyCode key)
    {
        Debug.Log(key);
        int x = (int)key;
        if(x < 0 || x >= shaders.Length)
        {
            Debug.Log("shader DNE");
        }

        GetComponentInChildren<MeshRenderer>().sharedMaterial = shaders[x];
    }
}
