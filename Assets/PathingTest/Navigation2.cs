using System;
using Unity.VisualScripting;
using UnityEditor;
//using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements.Experimental;

public class Navigation2 : MonoBehaviour
{
    PlayerInputActions inputActions;

    [SerializeField] GameObject player;
    [SerializeField] GameObject navRaycastCenter;
    [SerializeField] GameObject needsJumpRaycastCenter;
    [SerializeField] GameObject tooHighRaycastCenter;
    [SerializeField, Range(3, 20)]
    float jumpStrength;
    float agentHeight;
    float agentFowardDistCheck;
    [SerializeField, Range(0.5f, 3)]
    float agentFowardDistCheckMultiplier;

    [SerializeField, Range(1, 30)]
    float stoppingDist;
    NavMeshAgent agent;
    bool isJumping;
    LayerMask mask;
    bool isPaused;
    bool needsGroundRaycast;

    [SerializeField]
    bool slowerFrameRate;

    Vector3 jumpVelocity = Vector3.zero;

    Vector3 debuggingPoint = Vector3.zero;
    float DebuggingAngle = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputActions = new PlayerInputActions();
        inputActions.PlayerMovement.Enable();

        inputActions.PlayerMovement.Jump.performed += Jump_performed;

        isJumping = false;
        isPaused = false;
        needsGroundRaycast = false;

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Bounds bounds = mesh.bounds;
        agentHeight = bounds.max.y *1.1f;
        agentFowardDistCheck = bounds.max.z;

        mask = LayerMask.GetMask("walkable");
        agent = GetComponent<NavMeshAgent>();
        ResumeAgent();
    }

    void Update()
    {
        UpdateWhileJumping();
        CheckToStartJumping();
    }

    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Debug.Log("Jumping");
        Jump(transform.forward);
    }

    void Jump(Vector3 dir, float powerBoost = 1.0f)
    {
        isJumping = true;
        dir.y = 0;
        jumpVelocity = dir.normalized * powerBoost + Vector3.up * jumpStrength;
        PauseAgent();
        isJumping = true;
    }

    void UpdateWhileJumping()
    {
        if (isJumping == false)
            return;

        transform.position += jumpVelocity * Time.deltaTime;
        jumpVelocity += -(Vector3.up * 9.8f * Time.deltaTime);
        if (jumpVelocity.y > 0) { return; }
        needsGroundRaycast = true;

        RaycastHit hitInfo;

        bool hitDown = Physics.Raycast(navRaycastCenter.transform.position, -transform.up, out hitInfo, agentHeight);
        if (hitDown)
        {
            isJumping = false;
            ResumeAgent();
            jumpVelocity = Vector3.zero;
            transform.position = hitInfo.point + new Vector3(0, agentHeight, 0);
        }
    }

    //FindClosestEdge

    float ForwardCheckDistance()
    {
        return agentFowardDistCheck * (agentFowardDistCheckMultiplier + agent.speed / 2) + 1;
    }

    bool FindNearbyJumpAngle()
    {
        RaycastHit hitInfo;
        var originalRotation = transform.rotation;
        float originalAngle = Mathf.Deg2Rad * (-originalRotation.eulerAngles.y + 90);
        float workingAngle = 0;
        for (int angle = 5; angle < 80; angle += 5)
        {
            float angleToRight = originalAngle - Mathf.Deg2Rad * angle;
            float maxDist = 4 + Mathf.Sin(Mathf.Deg2Rad * angle); // angle here only affects length
            Vector3 dir = new Vector3((float)Math.Cos(angleToRight), 0, (float)Math.Sin(angleToRight));

            bool tooHigh = Physics.Raycast(tooHighRaycastCenter.transform.position, dir, out hitInfo, maxDist);
            if (tooHigh)
            {
                float angleToLeft = originalAngle + Mathf.Deg2Rad * angle;
                dir = new Vector3((float)Math.Cos(angleToLeft), 0, (float)Math.Sin(angleToLeft));
                tooHigh = Physics.Raycast(tooHighRaycastCenter.transform.position, dir, out hitInfo, maxDist);
                workingAngle = angleToLeft;
            }
            else
            { 
                workingAngle = angleToRight;
            }
            if (tooHigh == false)
            {
                transform.LookAt(dir + transform.position);
                debuggingPoint = transform.position;
                DebuggingAngle = workingAngle;
                Jump(dir, maxDist * 0.5f);
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        var originalRotation = transform.rotation;

        Handles.color = Color.black;

        var facing = originalRotation.eulerAngles;
        float finalAngle = Mathf.Deg2Rad * (-facing.y + 90);
        Vector3 dir = new Vector3((float)Math.Cos(finalAngle), 0, (float)Math.Sin(finalAngle));
        Handles.DrawLine(transform.position, transform.position + dir * 3, 3);

        for (int angle = 5; angle < 80; angle += 5)
        {
            Handles.color = Color.yellow;

            float angleToRight = finalAngle - Mathf.Deg2Rad * angle;
            dir = new Vector3((float)Math.Cos(angleToRight), 0, (float)Math.Sin(angleToRight));
            var lineLength = dir * (3 + Mathf.Sin(Mathf.Deg2Rad * angle));
            Handles.DrawLine(transform.position, transform.position + lineLength, 3);

            Handles.color = Color.red;

            float angleToLeft = finalAngle + Mathf.Deg2Rad * angle;
            dir = new Vector3((float)Math.Cos(angleToLeft), 0, (float)Math.Sin(angleToLeft));
            lineLength = dir * (3 + Mathf.Sin(Mathf.Deg2Rad * angle));
            Handles.DrawLine(transform.position, transform.position + lineLength, 3);
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(debuggingPoint, 0.3f);
        dir = new Vector3((float)Math.Cos(DebuggingAngle), 0, (float)Math.Sin(DebuggingAngle));

        Gizmos.DrawRay(debuggingPoint, dir);
#endif //UNITY_EDITOR
    }

    private void CheckToStartJumping()
    {
        if (isJumping == false)
        {
            RaycastHit hitInfo;
            float maxDist = agentFowardDistCheck * (agentFowardDistCheckMultiplier + agent.speed / 2);

            bool hitsKnee = Physics.Raycast(needsJumpRaycastCenter.transform.position, transform.forward, out hitInfo, maxDist);
            if(hitsKnee)
            {
                // let's not jump if there is another way to dest

                bool tooHigh = Physics.Raycast(tooHighRaycastCenter.transform.position, transform.forward, out hitInfo, ForwardCheckDistance());
                if (!tooHigh)
                {
                    Jump(transform.forward);
                    Debug.Log("jump");
                }
                else
                {
                    FindNearbyJumpAngle();
                }
            }
            Debug.DrawRay(navRaycastCenter.transform.position, -transform.up, UnityEngine.Color.blue, 0.2f);
            Debug.DrawRay(needsJumpRaycastCenter.transform.position, transform.forward, UnityEngine.Color.green, 0.2f);
            Debug.DrawRay(tooHighRaycastCenter.transform.position, transform.forward, UnityEngine.Color.red, 0.2f);
        }
    }

    void PauseAgent()
    {
        agent.enabled = false;
        isPaused = true;
    }

    void ResumeAgent()
    {
        agent.enabled = true;
        agent.destination = player.transform.position;
        isPaused = false;
    }
}
