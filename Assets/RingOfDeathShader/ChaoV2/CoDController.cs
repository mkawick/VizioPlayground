using System;
using UnityEngine;
using UnityEngine.InputSystem;
//using static UnityEditor.Experimental.GraphView.Port;

public class CoDController : MonoBehaviour
{
    [SerializeField]
    MeshRenderer mesh;
    [SerializeField]
    Renderer Renderer;
    [SerializeField]
    float initialRadius = 10f;
    [SerializeField, Range(0.01f, 1 )]
    float shrinkSpeed =0.01f;
    [SerializeField, Range(0.01f, 5)]
    float moveSpeed = 0.2f;

    float startTime = 0;
    float ringMoveTime;



    PlayerInputActions inputActions;

    float ringRadius;
    bool ringShrinks = false;
    bool ringMoves = false;
    Vector3 ringCenter = new Vector3();
    Vector3 ringDest = new Vector3();
    //Transform 

    private void Awake()
    {
        
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log(Renderer.material.GetFloat("_Opacity"));
        Debug.Log(Renderer.material.GetFloat("_Speed"));
        Debug.Log(Renderer.material.GetFloat("_RingThickness"));
        Debug.Log(Renderer.material.GetColor("_RingColor"));

        Renderer.material.SetFloat("_Opacity", 0.9f);
        Renderer.material.SetFloat("_Speed", 0.02f);
        Renderer.material.SetFloat("_RingThickness", 0.79f);
        Renderer.material.SetColor ("_RingColor", new Color(0.7f, 0.02f, 0.91f));
        Renderer.material.SetVector("_Tiling", new Vector2(1,1));


        inputActions = new PlayerInputActions();
        inputActions.PlayerMovement.Enable();
        inputActions.PlayerMovement.Jump.performed += Jump_performed;

        startTime = Time.fixedTime;
        ringRadius = -initialRadius;
        transform.position = Vector3.zero + new Vector3(0, ringRadius, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if(startTime > 0 && Time.deltaTime - startTime > 2)
        {
            startTime = -1;
            Renderer.material.SetColor("_RingColor", new Color(0.7f, 1f, 0.91f, 1));
        }

        if (ringShrinks) 
        {
            if(Mathf.Abs(ringRadius) > 2)
            {
                ringRadius += shrinkSpeed * Time.deltaTime;
                transform.position = Vector3.zero + new Vector3(0, ringRadius, 0);
            }
            else
            {
                ringShrinks = false;
                ringMoves = true;
                ringDest = UnityEngine.Random.insideUnitCircle;
                ringMoveTime = 0;
            }
        }

        if (ringMoves)
        {
            var newPos = Vector3.Lerp(ringCenter, ringDest, ringMoveTime);
            ringMoveTime += moveSpeed * Time.deltaTime;
            if(ringMoveTime >= 1)
            {
                ringCenter = ringDest;
                var temp = UnityEngine.Random.insideUnitCircle * 3;
                ringDest.x = temp.x;
                ringDest.z = temp.y;
                ringMoveTime = 0;
            }
            transform.position = newPos;
        }
    }

    private void OnEnable()
    {
        mesh = GetComponentInChildren<MeshRenderer>();
    }


    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        ringShrinks = true;
        ringMoves = false;
        ringCenter = new Vector3(0, ringRadius, 0);
        ringRadius = -initialRadius;
        transform.position = ringCenter;
    }
}
