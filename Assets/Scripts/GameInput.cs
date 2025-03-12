using System;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    PlayerInputActions inputActions;
    public event EventHandler OnInterract;
    public event EventHandler OnSlice;
    public event EventHandler OnJump;
    public event Action<KeyCode> OnSelectLevel;

    void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.PlayerMovement.Enable();

        inputActions.PlayerMovement.Interact.performed += Interact_performed;
        inputActions.PlayerMovement.Slice.performed += Slice_performed;
        inputActions.PlayerMovement.Jump.performed += Jump_performed;
        inputActions.PlayerMovement.SelectLevel.performed += SelectLevel_performed;
    }

    private void SelectLevel_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        var name = obj.control.name; 
        OnSelectLevel?.Invoke((KeyCode)System.Enum.Parse(typeof(KeyCode), name));
    }

    private void Slice_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnSlice?.Invoke(this, EventArgs.Empty);
    }

    private void Interact_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnInterract?.Invoke(this, EventArgs.Empty);
    }

    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnJump?.Invoke(this, EventArgs.Empty);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector2 GetMovementVectorNormalized()
    {
        var dir = inputActions.PlayerMovement.Move.ReadValue<Vector2>();
        return dir;
    }
}
