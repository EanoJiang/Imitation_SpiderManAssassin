using System.Collections.Generic;
using UnityEngine;



public class ScanArms : Scan
{
    [SerializeField] int armCount = 11;
    [SerializeField] float armLenght = 2f;
    [SerializeField] int armPoints = 4;

    [SerializeField] bool weightByDist = false;

    [SerializeField, Range(0, 360)] float arcAngle = 270;
    [SerializeField] int arcResolution = 4;
    [SerializeField] LayerMask arcLayer;

    [SerializeField] bool gizmoDrawPoint = true;
    [SerializeField] bool gizmoDrawLink = true;




    void OnDrawGizmosSelected()
    {
        Scan(true);
    }


    public override List<(Vector3 pos, Quaternion rot, float weight)> Points()
    {
        return Scan(false);
    }




    List<(Vector3, Quaternion, float)> Scan(bool gizmo = false)
    {
        List<(Vector3 pos, Quaternion rot, float weight)> points = new List<(Vector3, Quaternion, float)>();

        float arcRadius = armLenght / armPoints;

        for (int i = 0; i < armCount; i++)
        {
            float angle = 360 * i / armCount;

            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation * Quaternion.Euler(0, angle, 0);

            for (int j = 0; j < armPoints && PhysicsExtension.ArcCast(pos, rot, arcAngle, arcRadius, arcResolution, arcLayer, out RaycastHit hit); j++)
            {
                float weight = weightByDist ? 1 - (float)j / armPoints : 1;

                if (gizmo)
                    Gizmos.color = new Color(1, 1, 1, weight);

                if (gizmo && gizmoDrawLink)
                    Gizmos.DrawLine(pos, hit.point);

                pos = hit.point;
                rot.MatchUp(hit.normal);

                points.Add((pos, rot * Quaternion.Euler(0, -angle, 0), weight));

                if (gizmo && gizmoDrawPoint)
                    Gizmos.DrawSphere(pos, 0.1f);
            }
        }

        return points;
    }
}
