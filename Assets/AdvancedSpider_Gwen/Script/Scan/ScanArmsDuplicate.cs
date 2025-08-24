using System.Collections.Generic;
using UnityEngine;



public class ScanArmsDuplicate : Scan
{
    [SerializeField] bool weightByDist = true;

    [SerializeField] int armCount = 5;
    [SerializeField] float armLenght = 0.8f;
    [SerializeField] int armDuplicateCount = 3;
    [SerializeField] int armDuplicate = 2;
    [SerializeField, Range(0, 180)] int armDuplicateAngle = 54;

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

        for (int i = 0; i < armCount; i++)
        {
            float angle = 360 * i / armCount;

            Arm(transform.position,
                transform.rotation * Quaternion.Euler(0, angle, 0),
                1,
                points,
                angle,
                gizmo);
        }

        return points;
    }


    void Arm(Vector3 pos, Quaternion rot, int duplicateCount, List<(Vector3, Quaternion, float)> points, float angle, bool gizmo)
    {
        if (PhysicsExtension.ArcCast(pos, rot, arcAngle, armLenght, arcResolution, arcLayer, out RaycastHit hit))
        {
            float weight = weightByDist ? 1 - (float)(duplicateCount-1) / armDuplicateCount : 1;

            if (gizmo)
                Gizmos.color = new Color(1, 1, 1, weight);

            if (gizmo && gizmoDrawLink)
                Gizmos.DrawLine(pos, hit.point);

            pos = hit.point;
            rot.MatchUp(hit.normal);

            points.Add((pos, rot * Quaternion.Euler(0, -angle, 0), weight));

            if (gizmo && gizmoDrawPoint)
                Gizmos.DrawSphere(pos, 0.1f);

            if (duplicateCount == armDuplicateCount)
                return;

            for (int i = 0; i < armDuplicate; i++)
            {
                float angleCrt = armDuplicate == 1 ? 0 : armDuplicateAngle * (-0.5f + (float)i / (armDuplicate - 1));

                Arm(pos,
                    rot * Quaternion.Euler(0, angleCrt, 0),
                    duplicateCount + 1,
                    points,
                    angle + angleCrt,
                    gizmo);
            }
        }
    }
}
