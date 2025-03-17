using Sirenix.Utilities;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Jumper : MonoBehaviour
{
    PlayerInputActions inputActions;
    [SerializeField] Transform player;
    [SerializeField] float jumpImpulse;
    [SerializeField, Range(0.2f, 8)]
    float moveSpeed = 3;

    float verticalVelocity = 0;

    [SerializeField, Range(0,4)]
    float gravityMultiplier = 1;

    Int32 layerMask;
    bool shouldCheckForGround = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        layerMask = LayerMask.GetMask("walkable");
      /*  inputActions = new PlayerInputActions();
        inputActions.PlayerMovement.Enable();
        inputActions.PlayerMovement.Interact.performed += Interact_performed;
        inputActions.PlayerMovement.Move.performed += Move_performed;
        inputActions.PlayerMovement.Jump.performed += Jump_performed;
        inputActions.PlayerMovement.Slice.performed += PressF_performed;
        inputActions.PlayerMovement.SelectLevel.performed += PressE_performed;
        */
    }

    void Update()
    {
        CheckKeys();
        Fall();
        CheckGround();
    }

    void CheckGround()
    {
        if (shouldCheckForGround == false)
            return;

        if (verticalVelocity != 0)
        {
            if(Physics.Raycast(transform.position, new Vector3(0, -1, 0), out RaycastHit hitInfo, -verticalVelocity * 1.2f, layerMask ))
            {
                //float dist = (transform.position - hitInfo.point).magnitude;
                //if(-dist < verticalVelocity)
                {
                    float angle = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(Vector3.up, hitInfo.normal));
                    Debug.Log($"angle = {angle}");
                    if (angle <= 45)
                    {
                        transform.position = hitInfo.point;
                        verticalVelocity = 0;
                        shouldCheckForGround = false;
                    }
                }
            }
        }
    }

    private void Fall()
    {
        if (verticalVelocity != 0)
        {
            transform.position += new Vector3(0, verticalVelocity, 0);
            verticalVelocity -= 4.9f * Time.deltaTime * gravityMultiplier;
            if (transform.position.y < 0)
            {
                transform.position = new Vector3(transform.position.x, 0, transform.position.z);
                verticalVelocity = 0;
            }
            if (verticalVelocity < 0)
                shouldCheckForGround = true;
        }
    }

    private void CheckKeys()
    {
        var dirs = Camera.main.transform;
        Vector3 dir = new Vector3();

        if(Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Jumping");
            verticalVelocity += jumpImpulse;

        }
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            dir = dirs.forward;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            dir = -dirs.right;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            dir = -dirs.forward;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            dir = dirs.right;
        }

        if (dir.magnitude != 0)
        {
            dir *= moveSpeed * Time.deltaTime;
            this.transform.position += new Vector3(dir.x, 0, dir.z);
        }
    }

    private void Interact_performed(InputAction.CallbackContext obj)
    {
        /*if (obj.control.IsPressed()) // press down
            return;

        Next(int oldLocationIndex, int newLocationIndex)*/
    }

    private void Move_performed(InputAction.CallbackContext obj)
    {
          var dir = obj.ReadValue<Vector2>();
          var forward = transform.forward * -dir.x + transform.right * dir.y;// ; * new Vector3(dir.y, 0, dir.x);

          //var newDir = new Vector3(dir.y, 0, dir.x) * ;
          transform.position += forward * Time.deltaTime * moveSpeed;


    }

    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Debug.Log("Jumping");
        verticalVelocity += jumpImpulse;// * gravityMultiplier;
        //Jump(transform.forward);
    }
    private void PressF_performed(InputAction.CallbackContext obj)
    {
        //PerformOverlapSphereHit();
    }
    private void PressE_performed(InputAction.CallbackContext obj)
    {
        //PerformOverlapSphereHit();
       // PerformRaycastHit();
    }

    // Update is called once per frame

    void OnCollisionEnter(Collision collision)
    {
        verticalVelocity = 0;
        Debug.Log($"Collided {collision.body.name}");
        //isGrounded = true;
    }
}

