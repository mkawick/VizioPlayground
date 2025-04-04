using UnityEngine;

public class MovingHideableDuck : MonoBehaviour
{
    int whichLocation = 0;
    [SerializeField] public PositionMover positionMover;
    [SerializeField] public KeyCode keyDown = KeyCode.RightArrow;

    void Update()
    {
        if (Input.GetKeyUp(keyDown))
        {
            int oldLocationIndex = whichLocation;
            whichLocation--; if (whichLocation < 0)
            { whichLocation = positionMover.transportLocations.Length - 1; }
            Next(oldLocationIndex, whichLocation);
        }
    }
    void Next(int oldLocationIndex, int newLocationIndex)
    {
        var oldPos = positionMover.transportLocations[oldLocationIndex];
        var newPos = positionMover.transportLocations[newLocationIndex];
        whichLocation = newLocationIndex;
        transform.position = newPos.position;
    }
}
