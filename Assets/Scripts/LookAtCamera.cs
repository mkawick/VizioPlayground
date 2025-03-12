using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField]
    private enum Mode
    {
        LookAt, LookAtInv, CameraForward, CameraForwardInv
    }

    [SerializeField]
    Mode mode;
    // Update is called once per frame
    void LateUpdate()
    {
        switch (mode)
        {
            case Mode.LookAtInv:
                transform.LookAt(Camera.main.transform);
                break;
            case Mode.LookAt:
                Vector3 dirFromCamera = transform.position - Camera.main.transform.position;
                transform.LookAt(Camera.main.transform.position + dirFromCamera);
                break;
            case Mode.CameraForward:
                transform.forward = -Camera.main.transform.forward;
                break;
            case Mode.CameraForwardInv:
                transform.forward = Camera.main.transform.forward;
                break;
        }
    }
}
