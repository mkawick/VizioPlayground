//using System.Drawing;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CustomEditor(typeof(CannonShooter))]
public class CannonShooterEditor : Editor
{
    void OnSceneGUI()
    {
        CannonShooter connectedObjects = target as CannonShooter;

        float fps = 60;
        Handles.color = Color.yellow;
        int layerMask = LayerMask.NameToLayer("walkable");

        float force = connectedObjects.force;
        Vector3 center = connectedObjects.cannon.transform.position;
        Vector3 trajectory = connectedObjects.cannon.transform.up;

        Vector3 direction = trajectory * force + center;
        Vector3 lastPoint = center;

        for (float i = 1; i < 20; i++)
        {
            Handles.DrawLine(lastPoint, direction);
            Handles.SphereHandleCap(0,
                    lastPoint ,
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
        }
    }
}

public class CannonShooter : MonoBehaviour
{
    [SerializeField, Range(0.6f, 40)]
    internal float force;
    [SerializeField]
    internal GameObject cannon;
}