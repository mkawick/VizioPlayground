using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class RaycastGun : MonoBehaviour
{
    PlayerInputActions inputActions;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] GameObject barrel;
    MeshRenderer renderer;
    Vector3 rendererSize;
    LayerMask layerToSearch;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Physics.queriesHitBackfaces = true;

        inputActions = new PlayerInputActions();
        inputActions.PlayerMovement.Enable();
        inputActions.PlayerMovement.Move.performed += Move_performed;
        inputActions.PlayerMovement.Interact.performed += PressE_performed;
        inputActions.PlayerMovement.Jump.performed += Jump_performed;
        inputActions.PlayerMovement.Slice.performed += PressF_performed;
       // inputActions.PlayerMovement.SelectLevel.performed += PressF_performed;

        renderer = barrel.GetComponent<MeshRenderer>();
        rendererSize = renderer.localBounds.max;
        layerToSearch = LayerMask.GetMask("Visio");
    }

    /*  private void SelectLevel_performed(InputAction.CallbackContext obj)
      {
          var name = obj.control.name;
          var key = (KeyCode)System.Enum.Parse(typeof(KeyCode), name);
          //var key =  (int)obj.ReadValue<float>();
          if (key == (KeyCode)1)
          {
              PerformOverlapSphereHit();
          }
          else
          {
              PerformRaycastHit();
          }
      }*/

    private void PressF_performed(InputAction.CallbackContext obj)
    {
        PerformOverlapSphereHit();    
    }
    private void PressE_performed(InputAction.CallbackContext obj)
    {
        //PerformOverlapSphereHit();
        PerformRaycastHit();
    }

    void PerformRaycastHit()
    {
        var startPosition = barrel.transform.position + (rendererSize.y * 0.5f * barrel.transform.up);

        RaycastHit hitInfo; float length;
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Collide;
        if (Physics.Raycast(startPosition, barrel.transform.up, out hitInfo, 30, layerToSearch, queryTriggerInteraction))
        {
            length = hitInfo.distance;
            Vector3 connectToCenterOfHitCollider = startPosition - hitInfo.transform.position;// yes, the order works for this dot product

            bool weAreOutside = Vector3.Dot(barrel.transform.up, connectToCenterOfHitCollider) > 0;
            if (weAreOutside) { Debug.Log($"HIT: {this.name} is inside a collider"); }
            else { Debug.Log($"HIT: {this.name} is outside a collider"); }

        }
        else
        {
            length = 1;
            Debug.Log($"{this.name} did not hit a collider");
        }
        lineRenderer.SetPosition(0, startPosition);
        startPosition += barrel.transform.up * length;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, startPosition);

    }

    void PerformOverlapSphereHit()
    {
        GameObject go = GetNearestObjectOnLayer(layerToSearch);
        var startPosition = barrel.transform.position + (rendererSize.y * 0.5f * barrel.transform.up);
        var endPosition = go.transform.position;
        var dist = (endPosition - startPosition).magnitude;

        lineRenderer.SetPosition(0, startPosition);
        startPosition += barrel.transform.up * dist;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, startPosition);

    }

    public GameObject GetNearestObjectOnLayer(int layerMask)
    {
        float searchRadius = 10;
        Collider[] colliders = Physics.OverlapSphere(transform.position, searchRadius, layerMask);
        float closestDistance = Mathf.Infinity;

        GameObject nearestObject = null;
        foreach (Collider collider in colliders)
        {
            float distance = Vector3.Distance(transform.position, collider.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestObject = collider.gameObject;
            }
        }
        return nearestObject;
    }

    private void Move_performed(InputAction.CallbackContext obj)
    {
        var moveDir = obj.ReadValue<Vector2>();
        transform.position += new Vector3(moveDir.x, moveDir.y, 0);
    }

    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Debug.Log("Jumping");
        //Jump(transform.forward);
    }

        // Update is called once per frame
    void Update()
    {
        
    }
}
