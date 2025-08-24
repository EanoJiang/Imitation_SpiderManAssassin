using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[ExecuteInEditMode]
public class IK : MonoBehaviour
{
    [SerializeField] Transform[] knees;
    [SerializeField] Transform feet, target, dir;
    [SerializeField] float angleMin = 0, angleMax = 180, angleStep = 5;
    [SerializeField] bool inverseAngle = false;
    List<(float angle, float dist)> anglesDistances = new List<(float, float)>();



    void OnValidate()
    {
        CacheAnglesDistances();
    }

    void Awake()
    {
        CacheAnglesDistances();
    }

    void LateUpdate()
    {
        MatchTarget();
    }


    void CacheAnglesDistances()
    {
        if (!feet)
            return;

        anglesDistances.Clear();

        if (angleStep <= 0)
            return;

        for (float angle = angleMin; angle < angleMax; angle += angleStep)
            anglesDistances.Add((angle, FeetDist(angle)));
    }



    void MatchTarget()
    {
        if (!feet || !target || !dir)
            return;

        Vector3 toTarget = target.position - transform.position;
        float dstToTarget = toTarget.magnitude;

        // set angle
        SetAngle(FindAngle(dstToTarget));

        // look target
        Vector3 up = Vector3.Cross(toTarget, dir.right);
        transform.rotation = Quaternion.LookRotation(toTarget, up);

        // correct leg rotation
        float angle = Vector3.SignedAngle(transform.forward, feet.position - transform.position, transform.right);
        transform.Rotate(-angle, 0, 0);
    }

    float FeetDist(float angle)
    {
        SetAngle(angle);
        return (transform.position - feet.position).magnitude;
    }

    void SetAngle(float angle)
    {
        if (inverseAngle)
            angle *= -1;

        foreach (Transform knee in knees)
            knee.localRotation = Quaternion.Euler(angle, 0, 0);
    }

    float FindAngle(float dist)
    {
        if (anglesDistances.Count == 0)
            return 0;

        if (dist >= anglesDistances[0].dist)
            return anglesDistances[0].angle;

        for (int i = 1; i < anglesDistances.Count; i++)
        {
            (float angle, float dist) e1 = anglesDistances[i];

            if (e1.dist < dist)
            {
                (float angle, float dist) e2 = anglesDistances[i-1];
                float progress = Mathf.InverseLerp(e1.dist, e2.dist, dist);
                return Mathf.Lerp(e1.angle, e2.angle, progress);
            }
        }

        return anglesDistances.Last().angle;
    }
}