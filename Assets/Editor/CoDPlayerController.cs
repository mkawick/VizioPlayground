//using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.InputSystem;

public class CoDPlayerController : MonoBehaviour
{
    PlayerInputActions inputActions;
    [SerializeField] GameObject frontScreen;
    [SerializeField] GameObject CoD;
    [SerializeField] GameObject Camera;
    [SerializeField, Range(1, 50)] 
    float moveSpeed = 3;
    [SerializeField, Range(1, 50)]
    float distCheck = 8;

    [SerializeField] bool hideCod = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputActions = new PlayerInputActions();
        inputActions.PlayerMovement.Enable();
        inputActions.PlayerMovement.Move.performed += Move_performed;
        //inputActions.PlayerMovement.Move.

        //inputActions.PlayerMovement.Interact.performed += Interact_performed;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 codPos = new Vector2(CoD.transform.position.x, CoD.transform.position.z);
        Vector2 playerPos = new Vector2(Camera.transform.position.x, Camera.transform.position.z);
        var dist = (codPos - playerPos).magnitude;

        if (dist <= distCheck)
        {
            CoD.SetActive(true);
            frontScreen.SetActive(false);
        }
        else
        {
            if(hideCod) 
                CoD.SetActive(false);
            frontScreen.SetActive(true);
        }
    }

    private void Move_performed(InputAction.CallbackContext obj)
    {
        PlayerMove(obj.ReadValue<Vector2>());
    }

    public Vector2 PlayerMove(Vector2 dir)
    {

        var forward = transform.forward * -dir.x + transform.right * dir.y;// ; * new Vector3(dir.y, 0, dir.x);

        //var newDir = new Vector3(dir.y, 0, dir.x) * ;
        transform.position += forward * Time.deltaTime * moveSpeed;

        

        //var dir = inputActions.PlayerMovement.Move.ReadValue<Vector2>();
        return dir;
    }


}
