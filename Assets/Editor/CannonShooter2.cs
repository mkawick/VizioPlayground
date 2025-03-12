//using System.Drawing;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CustomEditor(typeof(CannonShooter2))]
public class CannonShooter2Editor : Editor
{
    void ApplyGravityToTrajectory(ref Vector3 velocity, float elapsed)
    {
        float gravity = 9.81f;
        velocity.y += -0.5f * gravity * (elapsed * elapsed);
    }

    // Returns the force needed to jump to a goal while countering any wind
    Vector3 JumpForce(Vector3 start, Vector3 goal, Vector3 currentVelocity, Vector3 wind)
    {
        float h = Vector3.Distance(start, goal) * 0.05f; // Get the jump height. greater the distance the higher we need to jump
        h += Mathf.Abs(Vector3.Dot((goal - start).normalized, Vector3.up) * (13f / Physics.gravity.y)); // a little more height is needed when jumping onto things up above.
        Vector3 force = (goal - start) / h - currentVelocity;
        return force - (wind + Physics.gravity) * h * 0.5f; // Intended for ForceMode.VelocityChange with zero drag
    }
   /* private Vector3 CalculateShot()
    {
        Vector3 shotVelocity;
        float height = hoopPosition.y - ballPosition.y;
        float length = hoopPosition.x - ballPosition.x;

        float horizontalVelocity = length / time;
        float verticalVelocity = ((-gravity) * Mathf.Pow(2, time) + 2 * height) / (2 * time);

        Debug.Log(horizontalVelocity + " " + verticalVelocity);
        shotVelocity = new Vector3(horizontalVelocity, verticalVelocity, 0f);

        return shotVelocity;
    }*/

    float CalculateLaunchVelocity(Vector3 target, Vector3 launcherPos, float angleDegrees) 
    {
        float angleRadians = Mathf.Deg2Rad * angleDegrees;
        float gravity = 9.81f;

        var dist = target - launcherPos;

        float displacementY = target.y - launcherPos.y;
        dist.y = 0;

        float cosAngle = Mathf.Cos(angleRadians) ;
        float velocity = Mathf.Sqrt(gravity * dist.sqrMagnitude / ((2* cosAngle * cosAngle) * launcherPos.y + dist.magnitude*Mathf.Tan(angleRadians)));

        return velocity;
    }

    void OnSceneGUI()
    {
        CannonShooter2 connectedObjects = target as CannonShooter2;

        float fps = 60;
        Handles.color = Color.cyan;
        int layerMask = LayerMask.NameToLayer("walkable");

        float angle = connectedObjects.angle;
        Vector3 center = connectedObjects.transform.position;
        Vector3 dist = connectedObjects.target.transform.position - center;
        float velocity = CalculateLaunchVelocity(connectedObjects.target.transform.position, center, angle);

        float numSteps = dist.magnitude / (velocity/fps);
        dist *= velocity / fps;
        Handles.DrawLine(center, dist + center);

        var right = Vector3.Cross(dist, Vector3.up);
        var dirAngle = Quaternion.AngleAxis(angle, right) * dist;

        Handles.color = Color.white;
        Handles.DrawLine(center, dirAngle + center);
        Debug.Log("d: " + dist + ", r:" + right + ", da: " + dirAngle);
        Debug.Log("numSteps: " + numSteps);
        Vector3 trajectory = dirAngle;
        Vector3 direction = trajectory + center;
        Vector3 lastPoint = center;

        for (float i = 1; i < numSteps; i++)
        {
            Handles.DrawLine(lastPoint, direction);
            Handles.SphereHandleCap(0,
                    lastPoint,
                    connectedObjects.transform.rotation * Quaternion.LookRotation(new Vector3(1, 0, 0)),
                    0.2f,
                EventType.Repaint);

            lastPoint = direction;
            trajectory.y -= 9.8f / fps;

            direction = lastPoint + trajectory;

           /* RaycastHit hit;
            bool hits = Physics.Raycast(lastPoint, direction, out hit, direction.sqrMagnitude, layerMask);
            if (hits)
            {
                // Gizmos.DrawSphere(hit.point, 0.2f);
                //Handles.(lastPoint, direction);
                break;
            }*/
        }
        /*
        Vector3 trajectory = connectedObjects.cannon.transform.up;

        Vector3 direction = trajectory * force + center;
        Vector3 lastPoint = center;

        for (float i = 1; i < 20; i++)
        {
            Handles.DrawLine(lastPoint, direction);
            Handles.SphereHandleCap(0,
                    lastPoint,
                    connectedObjects.cannon.transform.rotation * Quaternion.LookRotation(new Vector3(1, 0, 0)),
                    0.2f,
                EventType.Repaint);

            lastPoint = direction;
            trajectory.y -= 9.8f / fps;

            direction = lastPoint + trajectory * force;

            RaycastHit hit;
            bool hits = Physics.Raycast(lastPoint, direction, out hit, direction.sqrMagnitude, layerMask);
            if (hits)
            {
                // Gizmos.DrawSphere(hit.point, 0.2f);
                //Handles.(lastPoint, direction);
                break;
            }
        }*/
    }
}

public class CannonShooter2 : MonoBehaviour
{
    [SerializeField, Range(0.6f, 89)]
    internal float angle = 0;
    [SerializeField]
    internal GameObject target;
}