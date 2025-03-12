using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.Rendering;

public class RotatorTester : MonoBehaviour
{
    PlayerInputActions inputActions;
    public GameObject inputHand;
    public GameObject swingHand;
    public float firstTargetAngle, secondTargetAngle;
    [Range(0.0f, 180)]
    public float offsetAngle;
    [Range(0.0f, 360)]
    public float speed;

    enum LastDirMoved
    {
        nil, l, r
    }
    LastDirMoved lastDirMoved;
    float lastY;
    float thresholdRot = 0;
    bool isRotating;
    float destinationAngle;
    Quaternion swingTargetRotation;
    float targetRot;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputActions = new PlayerInputActions();
        inputActions.PlayerMovement.Enable();
        inputActions.PlayerMovement.Move.performed += Move_performed;
        //inputActions.PlayerMovement.Move.

        lastDirMoved = LastDirMoved.nil;
        lastY = inputHand.transform.localRotation.eulerAngles.y;
        thresholdRot = inputHand.transform.localRotation.eulerAngles.y;
        swingHand.transform.rotation = Quaternion.Euler(0, offsetAngle, 0);

        isRotating = false;
    }

    private void Move_performed(InputAction.CallbackContext obj)
    {
       /* if (isRotating)
            return;*/

        var moveDir = obj.ReadValue<Vector2>();

        
        var jumpAheadAngle = 0;

        var moved = false;
        if (moveDir.x > 0 )
        {
            var rot = inputHand.transform.localRotation.eulerAngles;
            rot.y -= 2;
            inputHand.transform.localRotation = Quaternion.Euler(rot);
            jumpAheadAngle = -1;

            lastDirMoved = LastDirMoved.l;
            print($"<color=#00FFFF>Left</color>");
        }
        else if (moveDir.x < 0)
        {
            var rot = inputHand.transform.localRotation.eulerAngles;
            rot.y += 2;
            inputHand.transform.localRotation = Quaternion.Euler(rot);
            jumpAheadAngle = 1;

            lastDirMoved = LastDirMoved.r;

            print($"<color=#FFFF00>Right</color>");
        }


        if (lastDirMoved != LastDirMoved.nil)
        {
            var currentAngle = swingHand.transform.rotation.eulerAngles.y;

            print($"<color=#00000>currentAngle = {currentAngle}</color>");
            currentAngle += jumpAheadAngle;// tiny move to guarantee direction of rotation
            swingHand.transform.rotation *= Quaternion.Euler(0, jumpAheadAngle, 0);
            print($"<color=#00000>jumped currentAngle = {swingHand.transform.rotation.eulerAngles.y}</color>");


            targetRot = Mathf.Approximately(currentAngle, offsetAngle) ? firstTargetAngle - Mathf.Abs(jumpAheadAngle) : secondTargetAngle - Mathf.Abs(jumpAheadAngle);
            destinationAngle = Mathf.Approximately(currentAngle, offsetAngle) ? firstTargetAngle : offsetAngle;

            print($"<color=#00000>currentAngle = {currentAngle}</color>");

            print($"<color=#00FF00>targetRot = {targetRot}</color>");
            moved = true;
        }
    }

    void Update()
    {
        if (lastDirMoved == LastDirMoved.l)
        {
            swingTargetRotation = swingHand.transform.rotation * Quaternion.Euler(0, -targetRot, 0);
            isRotating = true;
        }
        if (lastDirMoved == LastDirMoved.r)
        {
            swingTargetRotation = swingHand.transform.rotation * Quaternion.Euler(0, targetRot, 0);
            isRotating = true;
        }

        Rotate();
        lastDirMoved = LastDirMoved.nil;
    }

    bool Rotate()
    {
        if (isRotating == false)
            return false;

        swingHand.transform.rotation = Quaternion.RotateTowards(swingHand.transform.rotation, swingTargetRotation, speed * Time.deltaTime);

        var finalAngle = Mathf.Round(swingHand.transform.rotation.eulerAngles.y);
        if(swingHand.transform.rotation == swingTargetRotation)
        {
            print($"<color=#FF0000>Final rot = {finalAngle}</color>");
            isRotating = false;
        }

        return true;
    }
}
