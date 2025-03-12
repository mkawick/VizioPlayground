using UnityEngine;
using UnityEngine.InputSystem;

public class PositionMover : MonoBehaviour
{
    PlayerInputActions inputActions;
    [SerializeField] Transform[] transportLocations;
    int whichLocation = 0;
    [SerializeField] Transform player;

    public Transform[] Locations => transportLocations;

    void Start()
    {
        inputActions = new PlayerInputActions();
        inputActions.PlayerMovement.Enable();
        inputActions.PlayerMovement.Interact.performed += Interact_performed;    
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            int oldLocationIndex = whichLocation;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                whichLocation--; if (whichLocation < 0)
                { whichLocation = transportLocations.Length - 1; }
            }
            else
            {
                whichLocation++; if (whichLocation >= transportLocations.Length)
                { whichLocation = 0; }
            }
            Next(oldLocationIndex, whichLocation);
        }
    }

    void Next(int oldLocationIndex, int newLocationIndex)
    {
        var oldPos = transportLocations[oldLocationIndex];
        var newPos = transportLocations[newLocationIndex];
        whichLocation = newLocationIndex;
        player.transform.position = newPos.position;
        player.GetComponent<CameraFollower>().Moved();
        var zone = oldPos.GetComponent<VizZone>();
        if (zone)
        {
            zone.ShowSelected(false);
        }
        zone = newPos.GetComponent<VizZone>();
        if (zone)
        {
            zone.ShowSelected(true);
        }
    }
    private void Interact_performed(InputAction.CallbackContext obj)
    {
        /*if (obj.control.IsPressed()) // press down
            return;

        Next(int oldLocationIndex, int newLocationIndex)*/
    }
}
