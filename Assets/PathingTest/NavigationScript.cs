using System.Drawing;
using Unity.VisualScripting;
//using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;

public class NavigationScript : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] GameObject navRaycastCenter;
    [SerializeField] GameObject needsJumpRaycastCenter;
    [SerializeField] GameObject tooHighRaycastCenter;
    [SerializeField, Range(3, 30)]
    float jumpStrength;
    //[SerializeField, Range(3, 50)]
    float agentHeight;
    float agentFowardDistCheck;

    [SerializeField, Range(1, 30)]
    float stoppingDist;
    NavMeshAgent agent;
    bool isJumping;
    LayerMask mask;
    bool isPaused;

    [SerializeField]
    bool slowerFrameRate;

    Vector3 jumpVelocity = Vector3.zero;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isJumping = false;
        isPaused = false;
     //   RaycastHit hitInfo;
     //   float maxDist = 60;
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Bounds bounds = mesh.bounds;
        agentHeight = bounds.max.y;
        agentFowardDistCheck = bounds.max.z;
        
        mask = LayerMask.GetMask("walkable");
        agent = GetComponent<NavMeshAgent>();
        /*   bool hits = Physics.Raycast(navRaycastCenter.transform.position, transform.forward, out hitInfo, maxDist, mask);
           if (hits)
           {
               hitInfo.point = Vector3.zero;
           }*/

        if (slowerFrameRate)
        {
            Debug.Log("*** I have limited the frame rate ***");
            Application.targetFrameRate = 10;
        }
    }

    private void CheckToStartJumping()
    {
        if (isJumping == false)
        {
            RaycastHit hitInfo;
            float maxDist = agentFowardDistCheck * 1.1f;// + agent.speed * 2;

            bool hits = Physics.Raycast(navRaycastCenter.transform.position, transform.forward, out hitInfo, maxDist);
            if (hits)
            {
                bool hitsKnee = Physics.Raycast(needsJumpRaycastCenter.transform.position, transform.forward, out hitInfo, maxDist);
                if (hitsKnee)
                {
                    Jump();
                }
            }
            Debug.DrawRay(navRaycastCenter.transform.position, transform.forward, UnityEngine.Color.blue, 0.2f);
            Debug.DrawRay(needsJumpRaycastCenter.transform.position, transform.forward, UnityEngine.Color.green, 0.2f);
            Debug.DrawRay(tooHighRaycastCenter.transform.position, transform.forward, UnityEngine.Color.green, 0.2f);
        }
    }

    bool ShouldStop()
    {
        //if (agent.isActiveAndEnabled == false) return false;
        if (agent.remainingDistance != 0 && agent.remainingDistance< stoppingDist)
            return true;
        return false;
    }

    void Update()
    {
        //if(agent.isActiveAndEnabled == false) return;
        //agent.

        if (ShouldStop())
        {
            if(agent.isStopped == false)
            {
                agent.isStopped = true;
            }
        }
        if(agent.isStopped)
        {
            return;
        }
        CheckToStartJumping();
        if (isJumping)
        {
            UpdateWhileJumping();
        }
        else
        {
            if (isPaused == false)//agent.velocity != Vector3.zero)//agent.isStopped == false)
            {
                agent.destination = player.transform.position;
            }
        }
    }

    // https://discussions.unity.com/t/navmesh-agent-pause/56142/3
     void pause()
     {
         //lastAgentVelocity = agent.velocity;
         //lastAgentPath = agent.path;
         agent.velocity = Vector3.zero;
         agent.ResetPath();
        isPaused = true;
     }

     void resume()
     {
        // agent.velocity = lastAgentVelocity;
        // agent.SetPath(lastAgentPath);
        agent.destination = player.transform.position;
        isPaused = false;
    }

    void UpdateWhileJumping()
    {
        if (isJumping == false)
            return;

        transform.position += jumpVelocity * Time.deltaTime;
        jumpVelocity += -(Vector3.up * 9.8f * Time.deltaTime);
        if(jumpVelocity.y > 0) { return; }

        RaycastHit hitInfo;
      //  float maxDist = 0.2f;

        bool hitDown = Physics.Raycast(navRaycastCenter.transform.position, -transform.up, out hitInfo, agentHeight);
        if (hitDown)// == false)
        {
            
       /* }
        else //if (isJumping)
        {*/
            isJumping = false;
            //agent.enabled = true;
            resume();
            jumpVelocity = Vector3.zero;
            transform.position = hitInfo.point + new Vector3(0, agentHeight, 0);
        }

    }

    void Jump()
    {
        isJumping = true;
        var dir = //(player.transform.position - agent.destination).normalized;
        agent.transform.forward;
        //dir.y = 0;
        dir *= agent.speed;
        jumpVelocity = dir.normalized + Vector3.up * jumpStrength;
        //agent.enabled = false;
        pause();
    }
}
