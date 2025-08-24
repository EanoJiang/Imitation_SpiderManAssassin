using UnityEngine;


public class FollowTargetController : Controller
{
    [SerializeField] Transform target;
    [SerializeField] float distStop = 1;

    public Transform Target { get => target; set => target = value; }

    void Update()
    {
        if ((transform.position - target.position).magnitude < distStop)
        {
            stickL = Vector2.zero;
            stickR = Vector2.zero;
        }

        Vector3 targetProj = transform.InverseTransformPoint(target.position);
        targetProj.y = 0;
        targetProj.Normalize();

        float angle = Vector3.SignedAngle(Vector3.forward, targetProj, Vector3.up);

        stickL = new Vector2(targetProj.x, targetProj.z);
        stickR = new Vector2(angle > 0 ? 1 : -1, 0);
    }
}
