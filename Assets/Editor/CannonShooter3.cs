using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CannonShooter3))]
public class CannonShooter3Editor : Editor
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

        float cosAngle = Mathf.Cos(angleRadians);
        float velocity = Mathf.Sqrt(gravity * dist.sqrMagnitude / ((2 * cosAngle * cosAngle) * launcherPos.y + dist.magnitude * Mathf.Tan(angleRadians)));

        return velocity;
    }

    /*float CalcAngle(Transform target, Transform center)
    {
        Vector3 target_pos0 = target.position;

        target_pos0.y = target_pos0.y;

        target.LookAt(target_pos0);

        // rotate vector to target into local frame:

        Vector3 target_vec = target.transform.position - center.transform.position;

        target_vec = center.transform.InverseTransformDirection(target_vec);

        float x = target_vec.z;
        float y = target_vec.y;
        float v = shot_vector0.z;
        float g = 9.8f;

        // formula courtesy wikipedia:

        float v2 = v * v;
        float v4 = v2 * v2;
        float x2 = x * x;

        float theta = Mathf.Atan2(v2 - Mathf.Sqrt(v4 - g * (g * x2 + 2 * y * v2)), g * x);

        // adjust aim angle upwards by theta:
        turret.transform.Rotate(new Vector3(-theta * Mathf.Rad2Deg, 0, 0));

        info.text = "Range X = " + x + "  Range Y = " + y + "  theta = " + theta * Mathf.Rad2Deg;
    }*/

    void OnSceneGUI()
    {
        CannonShooter3 connectedObjects = target as CannonShooter3;

        float fps = 60; float gravity = 9.81f;

        Handles.color = Color.cyan;
        int layerMask = LayerMask.NameToLayer("walkable");

      /*  float timeToTarget = connectedObjects.timeToTarget;
        Vector3 center = connectedObjects.transform.position;
        Vector3 dist = connectedObjects.target.transform.position - center;
        float speed = dist.magnitude / timeToTarget;
        float travelDistPerFrame = speed / fps;

        float verticalDist = connectedObjects.target.transform.position.y - center.y;

        float g=gravity, d = dist.magnitude, h = verticalDist
        //float launchAngle = .5 * Mathf.Asin((g*d) ^ 2 - 2 * h) / (v ^ 2 * d));//(2 * gravity * verticalDist) / speed;
        float theta = Mathf.Atan2(v2 - Mathf.Sqrt(v4 - g * (g * x2 + 2 * y * v2)), g * x);

        var right = Vector3.Cross(dist, Vector3.up);
        var dirAngle = Quaternion.AngleAxis(launchAngle, right) * dist;

        Debug.Log("")
        //Debug.Log("d: " + dist + ", angle:" + launchAngle + ", da: " + dirAngle);
        Handles.color = Color.white;
        Handles.DrawLine(center, dirAngle + center);*/

        /*        float velocity = CalculateLaunchVelocity(connectedObjects.target.transform.position, center, angle);

                float numSteps = dist.magnitude / (velocity / fps);
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
                }*/
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
public class CannonShooter3 : MonoBehaviour
{
    [SerializeField, Range(0.2f, 3)]
    internal float timeToTarget = 0;
    [SerializeField]
    internal GameObject target;
}
