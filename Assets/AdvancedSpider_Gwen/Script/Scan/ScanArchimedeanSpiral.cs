using System.Collections.Generic;
using UnityEngine;

public class ScanArchimedeanSpiral : Scan
{
    [SerializeField] bool weightByDist = true;

    [SerializeField] float radius = 5;
    [SerializeField] int nbPoints = 100;
    [SerializeField] float nbTurns = 5;
    [SerializeField] float progressPow = 0.5f;

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

    List<(Vector3 pos, Quaternion rot, float weight)> Scan(bool gizmo = false)
    {
        List<(Vector3 pos, Quaternion rot, float weight)> points = new List<(Vector3, Quaternion, float)>();

        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        Vector2 A = Vector2.zero, B, AB = Vector2.right;

        for (int i = 1; i <= nbPoints; i++)
        {
            float progress = (float)i / nbPoints;
            progress = Mathf.Pow(progress, progressPow);

            float o = Mathf.PI * 2 * nbTurns * progress;
            float r = radius * progress;
            B = new Vector2(Mathf.Cos(o) * r, Mathf.Sin(o) * r);

            float angle = Vector2.SignedAngle(AB, B-A);
            rot *= Quaternion.Euler(0, angle, 0);

            AB = B - A;

            if (PhysicsExtension.ArcCast(pos, rot, arcAngle, AB.magnitude, arcResolution, arcLayer, out RaycastHit hit))
            {
                float weight = weightByDist ? 1 - progress : 1;

                if (gizmo)
                    Gizmos.color = new Color(0.2f, 0.8f, 0.2f, weight);

                if (gizmo && gizmoDrawLink)
                    Gizmos.DrawLine(pos, hit.point);

                pos = hit.point;
                rot.MatchUp(hit.normal);

                points.Add((pos, rot * Quaternion.Euler(0, -angle, 0), weight));

                if (gizmo && gizmoDrawPoint)
                    Gizmos.DrawSphere(pos, 0.1f);
            }
            else break;

            A = B;
        }

        return points;
    }
}
