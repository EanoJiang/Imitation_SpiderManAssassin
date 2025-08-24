using UnityEngine;


[DefaultExecutionOrder(0)]
public class Player3D : MonoBehaviour
{
    [SerializeField] Controller controller;
    [SerializeField] float accelerationForward = 40;
    [SerializeField] float accelerationBack = 40;
    [SerializeField] float accelerationSide = 40;
    [SerializeField] float friction = 5;
    [SerializeField] Vector2 addVelocity = Vector2.zero;
    Vector2 velocityNoAdd = Vector2.zero;
    Vector2 velocity = Vector2.zero;
    float speed = 0;
    float maxSpeedEstimation;
    float speedProgress;

    [SerializeField] float rotationSpeed = 90;

    [SerializeField, Range(0, 360)] float arcAngle = 270;
    [SerializeField] int arcResolution = 6;
    [SerializeField] LayerMask arcLayer;
    [SerializeField] Transform arcTransformRotation;

    public Controller Controller { get => controller; }
    public Vector2 VelocityNoAdd { get => VelocityNoAdd;
        set {
            velocityNoAdd = value;
            UpdateVeclocity();
        }
    }
    public Vector2 Velocity { get => velocity; }
    public Vector3 Velocity3 { get => new Vector3(velocity.x, 0, velocity.y); }
    public float Speed { get => speed; }
    public float SpeedProgress { get => speedProgress; }


    void OnValidate()
    {
        EstimateMaxSpeed();
    }

    void Awake()
    {
        EstimateMaxSpeed();
    }


    void OnDisable()
    {
        velocityNoAdd = Vector3.zero;
        UpdateVeclocity();
    }

    void Update()
    {
        ApplyVelocity();
        Rotate();
    }

    void FixedUpdate()
    {
        ApplyAcceleration();
        ApplyFriction();
        UpdateVeclocity();
    }

    void EstimateMaxSpeed()
    {
        // forward
        float v = 0, s;

        for (float t = 0; t < 10; t += Time.fixedDeltaTime)
        {
            v += Time.fixedDeltaTime * accelerationForward;
            v -= Time.fixedDeltaTime * friction * v;
        }
        v += addVelocity.y;
        s = Mathf.Abs(v);

        maxSpeedEstimation = s;

        // back
        v = 0;

        for (float t = 0; t < 10; t += Time.fixedDeltaTime)
        {
            v -= Time.fixedDeltaTime * accelerationBack;
            v -= Time.fixedDeltaTime * friction * v;
        }
        v += addVelocity.y;
        s = Mathf.Abs(v);

        maxSpeedEstimation = Mathf.Max(maxSpeedEstimation, s);

        // side
        v = 0;

        for (float t = 0; t < 10; t += Time.fixedDeltaTime)
        {
            v += Time.fixedDeltaTime * accelerationSide;
            v -= Time.fixedDeltaTime * friction * v;
        }
        v += addVelocity.x * (addVelocity.x > 0 == v > 0 ? 1 : -1);
        s = Mathf.Abs(v);

        maxSpeedEstimation = Mathf.Max(maxSpeedEstimation, s);
    }

    void ApplyAcceleration()
    {
        if (!controller)
            return;

        Vector2 stickL = controller.StickL;

        if (stickL != Vector2.zero)
            velocityNoAdd += Time.fixedDeltaTime * new Vector2(accelerationSide, stickL.y > 0 ? accelerationForward : accelerationBack) * stickL;
    }

    void ApplyFriction()
    {
        velocityNoAdd -= Time.fixedDeltaTime * friction * velocityNoAdd;
    }

    void UpdateVeclocity()
    {
        velocity = velocityNoAdd + addVelocity;
        UpdateSpeed();
    }

    void UpdateSpeed()
    {
        speed = velocity.magnitude;
        speedProgress = Mathf.Clamp01(speed / maxSpeedEstimation);
    }

    void ApplyVelocity()
    {
        if (velocity == Vector2.zero)
            return;

        float arcRadius = speed * Time.deltaTime;
        Vector3 worldVelocity = arcTransformRotation.TransformVector(Velocity3);

        if (PhysicsExtension.ArcCast(transform.position, Quaternion.LookRotation(worldVelocity, arcTransformRotation.up), arcAngle, arcRadius, arcResolution, arcLayer, out RaycastHit hit))
        {
            transform.position = hit.point;
            transform.MatchUp(hit.normal);
        }
    }

    void Rotate()
    {
        if (!controller)
            return;

        Vector2 stickR = controller.StickR;

        if (stickR.x != 0)
            transform.Rotate(0, rotationSpeed * Time.deltaTime * stickR.x, 0);
    }
}
