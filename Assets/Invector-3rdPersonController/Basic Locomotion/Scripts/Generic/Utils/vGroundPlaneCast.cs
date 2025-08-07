using UnityEngine;

public struct vGroundPlaneCast
{
    public Vector3 p1, p2, p3, p4;
    public Vector3 planeNormal;
    public Vector3 planeCenter;
    public Vector3 centerNormal;

    public float planeDistance;
    public bool hasFloor;
    public readonly int planeAngle
    {
        get
        {
            return (int)Vector3.Angle(Vector3.up, planeNormal);
        }
    }
    RaycastHit hit;

    public void CalculatePlane(Transform transform, Vector3 position, Vector3 down, float distance, LayerMask mask, float plane = 0.5f)
    {
        Vector3 basePosition = transform.position;
        float size = plane * 0.5f;
        planeNormal = -down;
        /// p1 --- p2
        /// |      |
        /// |      |
        /// p4 --- p3
        p1 = position + transform.forward * size - transform.right * size;
        p2 = position + transform.forward * size + transform.right * size;
        p3 = position - transform.forward * size + transform.right * size;
        p4 = position - transform.forward * size - transform.right * size;
        //Debug.DrawRay(p1, Vector3.down * distance);
        //Debug.DrawRay(p2, Vector3.down * distance);
        //Debug.DrawRay(p3, Vector3.down * distance);
        //Debug.DrawRay(p4, Vector3.down * distance);

        bool _hasFloor1 = false;
        bool _hasFloor2 = false;
        bool _hasFloor3 = false;
        bool _hasFloor4 = false;
        if (Physics.Raycast(position, down, out hit, distance, mask))
        {
            centerNormal = hit.normal;
            planeCenter = hit.point;
        }
        else planeCenter = transform.position + Vector3.down * distance;
        if (Physics.Raycast(p1, down, out hit, distance, mask)) { p1 = hit.point; _hasFloor1 = true; }
        else p1 = basePosition + Vector3.forward * size + Vector3.left * size;
        if (Physics.Raycast(p2, down, out hit, distance, mask)) { p2 = hit.point; _hasFloor2 = true; }
        else p2 = basePosition + Vector3.forward * size + Vector3.right * size;
        if (Physics.Raycast(p3, down, out hit, distance, mask)) { p3 = hit.point; _hasFloor3 = true; }
        else p3 = basePosition + Vector3.back * size + Vector3.right * size;
        if (Physics.Raycast(p4, down, out hit, distance, mask)) { p4 = hit.point; _hasFloor4 = true; }
        else p4 = basePosition + Vector3.back * size + Vector3.left * size;
        var _planeCenter = (p1 + p2 + p3 + p4) / 4;
        //if (_planeCenter.y > hit.point.y)
        //{
           planeCenter = _planeCenter;
        //}
        planeNormal = (-GetNormal(p1, p2, p3)) + (-GetNormal(p2, p1, p3)) + (-GetNormal(p3, p2, p4)) + (-GetNormal(p4, p3, p1));

        planeDistance = (position - planeCenter).magnitude;
        hasFloor = (_hasFloor1 || _hasFloor2 || _hasFloor3 || _hasFloor4);
       
    }

    readonly Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        // Find vectors corresponding to two of the sides of the triangle.
        Vector3 side1 = b - a;
        Vector3 side2 = c - a;

        // Cross the vectors to get a perpendicular vector, then normalize it.
        return Vector3.Cross(side1, side2).normalized;
    }
}