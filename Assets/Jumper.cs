using Sirenix.Utilities;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Terresquall;

public class Jumper : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float jumpImpulse;
    [SerializeField] GameObject joystickCanvas;
    [SerializeField, Range(0.2f, 16)]
    float moveSpeed = 3;

    float verticalVelocity = 0;

    [SerializeField, Range(0,4)]
    float gravityMultiplier = 1;

    Int32 layerMask;
    bool shouldCheckForGround = false;

    Rigidbody playerRb;
    float playerHeight = 0;
    bool jumpPressed;

    void Start()
    {
        layerMask = LayerMask.GetMask("walkable");
        playerRb = GetComponentInChildren<Rigidbody>();
        var mesh = GetComponentInChildren<MeshRenderer>();
        if (mesh == null)
        {
            var skinnedMesh = GetComponentInChildren<SkinnedMeshRenderer>();
            playerHeight = skinnedMesh.bounds.size.y;
        }
        else
        {
            playerHeight = mesh.bounds.size.y;
        }
        jumpPressed = false;
        if (joystickCanvas)
            joystickCanvas.SetActive(true);
    }

    void Update()
    {
        
        RaycastSwarm();

        if(Input.GetKeyUp(KeyCode.Keypad0))
        {
            Application.targetFrameRate = 10;
        }
        if (Input.GetKeyUp(KeyCode.Keypad1))
        {
            Application.targetFrameRate = 60;
        }
    }

    void CheckGroundDuringFall()
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
            verticalVelocity -= 9.8f * Time.deltaTime * gravityMultiplier;
            if (transform.position.y < 0)
            {
                transform.position = new Vector3(transform.position.x, 0, transform.position.z);
                verticalVelocity = 0;
            }
            if (verticalVelocity < 0)
            {
                shouldCheckForGround = true;
                //tripleSpeed = true;
            }
        }
    }

    private void GroundCheckStartFalling()
    {
        var raycastPos = transform.position + new Vector3(0, playerHeight * 0.5f, 0);
        // ramps and such
        RaycastHit hit;
        bool hits = Physics.SphereCast(raycastPos, 0.1f, -transform.up, out hit, playerHeight, layerMask);
        if (hits)
        {
            var zeroedHitPoint = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            // the hit point is rarely directly beneath the character, so it will tend to drift over time
            var verticalDist = Mathf.Abs(transform.position.y - hit.point.y);
            if (verticalDist > 0.5) // start falling
            {
                verticalVelocity = -0.1f;
                shouldCheckForGround = true;
            }
            else if (verticalDist > 0.03f)// ground
            {
                transform.position = zeroedHitPoint;
            }
        }
        else
        {
            verticalVelocity = -0.1f;
            shouldCheckForGround = true;
        }
    }

    void FixedUpdate()
    {
        Jump();

        Fall();
        CheckGroundDuringFall();

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        x += VirtualJoystick.GetAxis("Horizontal", 16);
        z += VirtualJoystick.GetAxis("Vertical", 16);

        if (x != 0 || z != 0)
        {
            var cameraDir = Camera.main.transform;
            var forward = new Vector3( cameraDir.forward.x, 0, cameraDir.forward.z );
            Vector3 dir = x * cameraDir.right + z * forward;
            transform.rotation = Quaternion.LookRotation(dir);
            HandleWalls(dir);
        }

        if (verticalVelocity == 0)
        {
            GroundCheckStartFalling();
        }
        if (jumpPressed)
        {
            Debug.Log("Jumping");
            verticalVelocity += jumpImpulse;
            jumpPressed = false;
        }
    }
    public void OnJumpButton()
    {
        jumpPressed = true;
    }

    private void HandleWalls(Vector3 moveDir)
    {
        float castDist = 0.3f;
        var forwardDir = playerRb.transform.forward;
        var raycastPos = playerRb.transform.position - forwardDir * castDist; // slightly behind
        var rot = playerRb.transform.rotation;

        float potentialMoveDist = Time.deltaTime * moveSpeed;

        RaycastHit hitInfo;
        bool hits = Physics.SphereCast(raycastPos, 0.1f, forwardDir, out hitInfo, castDist * 2, layerMask); // a huge distance
        if (hits)
        {
            var intersectingRay = hitInfo.point - raycastPos;// todo : this ray is probably not aligned with the player's feet
            float hitDist = (intersectingRay).magnitude;

            float angle = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(Vector3.up, hitInfo.normal));
            /*if (angle <= 45)
            {
                transform.position = hitInfo.point;
                verticalVelocity = 0;
                shouldCheckForGround = false;
                return;
            }*/
            if(angle >45)
            {
                return;// walls stop you if the angle is too much
            }

            if (hitDist < castDist * 2)
            {
                var minusHitDir = moveDir - hitInfo.normal * potentialMoveDist;
                var remainingMoverDir = minusHitDir * potentialMoveDist;// reproject orthagonally
                bool doesHitPerpendicular = Physics.SphereCast(raycastPos, 0.1f, remainingMoverDir, out hitInfo, castDist * 2, layerMask); // a huge distance
                if (doesHitPerpendicular)
                {
                    /* intersectingRay = hit.point - raycastPos;// todo : this ray is probably not aligned with the player's feet
                     hitDist = (intersectingRay).magnitude;
                     if (hitDist < castDist * 2)
                     {
                         minusHitDir = raycastPos - hit.point;// actual distance from our center to wall
                         hitDist = minusHitDir.magnitude;
                     }*/
                    return;
                }
            }
            if (hitDist > 0)
            {
                transform.position += moveDir * ((hitDist > potentialMoveDist) ? potentialMoveDist : hitDist);
            }
        }
        else
        {
            transform.position += moveDir * potentialMoveDist;
        }
    }

    private void Jump()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            jumpPressed = true;
        }
    }

    void RaycastSwarm()
    {
        var pos = playerRb.transform.position;
        var rot = playerRb.transform.rotation;
        var dir = playerRb.transform.forward;

        float radius = 0.2f;
        Vector3 lower = pos - new Vector3(0, radius, 0);
        Vector3 upper = pos + new Vector3(0, radius, 0);
        Vector3 left  = pos - playerRb.transform.right * radius;
        Vector3 right = pos + playerRb.transform.right * radius;

        Debug.DrawLine(pos, pos + dir, Color.red);
        Debug.DrawLine(lower, dir + lower, Color.gray);
        Debug.DrawLine(upper, dir + upper, Color.white);

        Debug.DrawLine(left, dir + left, Color.blue);
        Debug.DrawLine(right, dir + right, Color.blue);

        var center = transform.position;
        var dirDown = center - transform.up * 4;

        Debug.DrawLine(center, dirDown, Color.green);
    }

    private void OnDrawGizmos()
    {
        float radius = 0.2f;
        var center = transform.position;
        var dirDown = center - transform.up * 4;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(dirDown, radius);
    }

}

