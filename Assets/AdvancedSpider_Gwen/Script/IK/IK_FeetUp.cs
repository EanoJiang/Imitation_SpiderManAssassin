using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[ExecuteInEditMode]
public class IK_FeetUp : MonoBehaviour
{
    [SerializeField] Transform[] knees;
    [SerializeField] Transform feet, target;
    [SerializeField] float angleMin = 0, angleMax = 180, angleStep = 5;
    [SerializeField] bool inverseAngle = false;
    List<(float angle, float dist)> anglesDistances = new List<(float, float)>();

    float feetLenght;


    void OnValidate()
    {
        Cache();
    }

    void Awake()
    {
        Cache();
    }

    void LateUpdate()
    {
        MatchTarget();
    }

    void Cache()
    {
        if (!feet || knees.Length < 1)
            return;

        feetLenght = (knees.Last().position - feet.position).magnitude;
        CacheAnglesDistances();
    }


    void CacheAnglesDistances()
    {
        anglesDistances.Clear();

        if (angleStep <= 0)
            return;

        for (float angle = angleMin; angle < angleMax; angle += angleStep)
            anglesDistances.Add((angle, LastKneeDist(angle)));
    }




    void MatchTarget()
    {
        if (!feet || !target)
            return;

        Transform lastKnee = knees.Last();
        Vector3 kneeTarget = target.position + target.up * feetLenght;
        Vector3 toKneeTarget = kneeTarget - transform.position;
        float dstToKneeTarget = toKneeTarget.magnitude;

        // set angle
        SetAngle(FindAngle(dstToKneeTarget));

        // look knee target
        Vector3 right = Vector3.Cross(target.up, toKneeTarget);
        Vector3 up = Vector3.Cross(toKneeTarget, right);
        transform.rotation = Quaternion.LookRotation(toKneeTarget, up);

        // correct leg rotation
        float angle = Vector3.SignedAngle(transform.forward, lastKnee.position - transform.position, transform.right);
        transform.Rotate(-angle, 0, 0);

        // last knee look target
        Vector3 forward = target.position - lastKnee.position;
        knees.Last().rotation = Quaternion.LookRotation(forward, Vector3.Cross(forward, lastKnee.right));
    }

    float LastKneeDist(float angle)
    {
        SetAngle(angle);
        return (transform.position - knees.Last().position).magnitude;
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

