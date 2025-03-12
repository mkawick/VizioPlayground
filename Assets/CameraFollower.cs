using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    Transform originalCameraPosition;
    Vector3 offsetPosition;
    [SerializeField] bool follow = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalCameraPosition = Camera.main.transform;
        offsetPosition = originalCameraPosition.position - transform.position;
    }

    public void Moved()
    {
        if (follow)
        {
            Camera.main.transform.position = transform.position + offsetPosition;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
