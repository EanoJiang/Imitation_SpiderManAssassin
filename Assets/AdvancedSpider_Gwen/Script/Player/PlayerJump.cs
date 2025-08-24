using System.Collections;
using System.Linq;
using UnityEngine;



public class SpiderJump : MonoBehaviour
{
    [SerializeField] Player3D player3D;
    [SerializeField] PlayerPivot playerPivot;
    [SerializeField] FollowTarget followTarget;
    [SerializeField] Spider_Gwen spider;
    [SerializeField] ShakeBody shakeBody;
    [SerializeField] Transform[] targetsJumpDown;
    [SerializeField] Transform[] targetsJumpUp;

    Transform body;
    Transform pivot;
    Controller controller;
    Transform[] targets;
    Transform[] targetsG1;
    Transform[] orbits;
    Vector3 bodyProj;

    enum State { Grounded, Jumping }
    State state = State.Grounded;

    Vector3 velocity;
    float jumpTime;

    [SerializeField] float jumpVelocityY = 20;
    [SerializeField] float jumpPlayerVelocityCoef = 1;

    [SerializeField] Vector3 gravityForce = new Vector3(0, -40f, 0);
    [SerializeField] float acceleration = 10f;
    [SerializeField] float friction = 0.5f;
    [SerializeField] float rotationSpeed = 20f;
    [SerializeField] LayerMask groundLayer;

    [SerializeField] float landingEstimationTime = 1f;

    (Vector3 position, Quaternion rotation, float time) landingEstimation = (Vector3.zero, Quaternion.identity, -1);

    [SerializeField] float targetsJumpTime = 1f;
    [SerializeField] AnimationCurve targetsJumpCurve;

    [SerializeField] int airJumpCount = 1;
    int airJumpCountCrt;
    [SerializeField] float airJumpVelocityY = 20;
    [SerializeField] float airJumpVelocityXZ = 10;

    [SerializeField] AudioClip jumpSound, landingSound;

    void OnDrawGizmosSelected()
    {
        if (state == State.Jumping)
            LandingEstimation(out _, true);
    }

    void Awake()
    {
        Cache();
    }

    void Cache()
    {
        body = shakeBody.Body;
        pivot = playerPivot.Pivot;
        controller = player3D.Controller;
        targets = spider.Targets;
        targetsG1 = spider.TargetsG1;
        orbits = spider.Orbits;

        bodyProj = transform.InverseTransformPoint(body.position);

        airJumpCountCrt = airJumpCount;
    }

    void OnEnable()
    {
        if (state == State.Grounded)
            controller.Y.OnPressDown.AddListener(Jump);

        else if (state == State.Jumping)
        {
            controller.Y.OnPressDown.AddListener(TryAirJump);
            StartCoroutine("Jumping");
        }
    }

    void OnDisable()
    {
        if (state == State.Grounded)
            controller.Y.OnPressDown.RemoveListener(Jump);

        else if (state == State.Jumping)
        {
            controller.Y.OnPressDown.RemoveListener(TryAirJump);
            StopCoroutine("Jumping");
        }
    }

    void Jump()
    {
        controller.Y.OnPressDown.RemoveListener(Jump);
        controller.Y.OnPressDown.AddListener(TryAirJump);

        state = State.Jumping;

        // transform match pivot rotation
        transform.rotation = pivot.rotation;

        // start velocity
        velocity = transform.up * jumpVelocityY + transform.rotation * player3D.Velocity3 * jumpPlayerVelocityCoef;
        jumpTime = 0;

        EnableSideScripts(false);

        landingEstimation = (Vector3.zero, Quaternion.identity, -1);

        // body match velocity
        body.MatchUp(velocity);

        AudioSourceExtension.PlayClipAtPoint(jumpSound, transform.position);

        StartCoroutine("Jumping");
    }

    void JumpEnd()
    {
        AudioSourceExtension.PlayClipAtPoint(landingSound, transform.position);

        // reset body rotation
        body.localRotation = Quaternion.identity;

        landingEstimation = (Vector3.zero, Quaternion.identity, -1);

        EnableSideScripts(true);

        TransferVelocityToPlayer3D();
        velocity = Vector3.zero;
        jumpTime = 0;

        // move targets to orbits
        spider.UpdateOrbits();

        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].position = orbits[i].position;

            if (!targetsG1.Contains(targets[i]))
                (targets[i].position, targets[i].rotation) = spider.EndStep(orbits[i]);
        }

        state = State.Grounded;

        airJumpCountCrt = airJumpCount;
        controller.Y.OnPressDown.RemoveListener(TryAirJump);
        controller.Y.OnPressDown.AddListener(Jump);
    }


    void TryAirJump()
    {
        if (airJumpCountCrt > 0)
        {
            airJumpCountCrt--;
            AirJump();
        }
    }

    void AirJump()
    {
        jumpTime = 0;

        velocity = Vector2.zero;
        velocity.y = airJumpVelocityY;

        if (controller.StickL != Vector2.zero)
        {
            Quaternion rotUp = MathExtension.RotationMatchUp(transform.rotation, Vector3.up);
            velocity += rotUp * controller.StickL3 * airJumpVelocityXZ;
        }

        landingEstimation = (Vector3.zero, Quaternion.identity, -1);

        body.MatchUp(velocity);

        AudioSourceExtension.PlayClipAtPoint(jumpSound, transform.position);
    }


    IEnumerator Jumping()
    {
        while (true)
        {
            jumpTime += Time.deltaTime;

            LandingEstimation();
            ApplyGravity();
            ApplyAcceleration();
            ApplyFriction();
            Rotate();
            CalculateNextPosRot(out Vector3 nextPos, out Quaternion nextRot);

            if (CheckGroundCollision(nextPos, out Vector3 hitPos, out Quaternion hitRot))
            {
                transform.position = hitPos;
                transform.rotation = hitRot;
                break;
            }

            // move
            transform.rotation = nextRot;
            transform.position = nextPos;

            // animation
            if (landingEstimation.time > 0)
            {
                float landingProgress = Mathf.Clamp01(Time.deltaTime / landingEstimation.time);

                // body look landing position
                Vector3 bodyToLandingPos = landingEstimation.position - body.position;
                Quaternion bodyTargetRotation = MathExtension.RotationMatchUp(body.rotation, -bodyToLandingPos);
                body.rotation = Quaternion.Lerp(body.rotation, bodyTargetRotation, landingProgress);
            }

            UpdateTargets(jumpTime);

            yield return new WaitForEndOfFrame();
        }

        JumpEnd();
    }


    bool LandingEstimation() => LandingEstimation(out landingEstimation, false);
    bool LandingEstimation(out (Vector3 position, Quaternion rotation, float time) landingEstimation, bool gizmo = false)
    {
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        Vector3 bodyPos = body.position;
        Vector3 velocity = this.velocity;
        float landingTimeCrt = this.landingEstimation.time;

        for (float t = Time.fixedDeltaTime; t <= landingEstimationTime; t += Time.fixedDeltaTime)
        {
            if (gizmo)
            {
                float alpha = Mathf.Clamp01(1 - t / landingEstimationTime);
                Gizmos.color = new Color(1, 1, 1, alpha);
            }

            ApplyGravity(ref velocity, Time.fixedDeltaTime);
            ApplyAcceleration(ref velocity, rot, Time.fixedDeltaTime);
            ApplyFriction(ref velocity, Time.fixedDeltaTime);
            Rotate(ref velocity, ref rot, Time.fixedDeltaTime);
            CalculateNextPosRot(bodyPos, velocity, rot, landingTimeCrt, Time.fixedDeltaTime, out Vector3 nextPos, out Quaternion nextRot, out Vector3 nextBodyPos);
            landingTimeCrt -= Time.fixedDeltaTime;

            if (CheckGroundCollision(pos, nextPos, rot, out Vector3 hitPos, out Quaternion hitRot, gizmo))
            {
                landingEstimation = (hitPos, hitRot, t);
                return true;
            }

            // move
            bodyPos = nextBodyPos;
            rot = nextRot;
            pos = nextPos;
        }

        landingEstimation = (Vector3.zero, Quaternion.identity, -1);
        return false;
    }

    void ApplyGravity() => ApplyGravity(ref velocity, Time.deltaTime);
    void ApplyGravity(ref Vector3 velocity, float deltaTime) => velocity += gravityForce * deltaTime;

    void ApplyFriction() => ApplyFriction(ref velocity, Time.deltaTime);
    void ApplyFriction(ref Vector3 velocity, float deltaTime) => velocity -= friction * deltaTime * velocity;

    void ApplyAcceleration() => ApplyAcceleration(ref velocity, transform.rotation, Time.deltaTime);
    void ApplyAcceleration(ref Vector3 velocity, Quaternion rot, float deltaTime)
    {
        if (controller.StickL != Vector2.zero)
        {
            Quaternion rotUp = MathExtension.RotationMatchUp(rot, Vector3.up);
            velocity += rotUp * controller.StickL3 * acceleration * deltaTime;
        }
    }

    void Rotate()
    {
        Quaternion rot = transform.rotation;
        Rotate(ref velocity, ref rot, Time.deltaTime);
        transform.rotation = rot;
    } 
    void Rotate(ref Vector3 velocity, ref Quaternion rot, float deltaTime)
    {
        Vector2 stickR = controller.StickR;
        if (stickR.x != 0)
        {
            velocity = Quaternion.Inverse(rot) * velocity;
            rot *= Quaternion.Euler(0, rotationSpeed * deltaTime * stickR.x, 0);
            velocity = rot * velocity;
        }
    }

    void CalculateNextPosRot(out Vector3 nextPos, out Quaternion nextRot) => CalculateNextPosRot(body.position, velocity, transform.rotation, landingEstimation.time, Time.deltaTime, out nextPos, out nextRot, out _);
    void CalculateNextPosRot(Vector3 bodyPos, Vector3 velocity, Quaternion rot, float landingTimeCrt, float deltaTime,
                             out Vector3 nextPos, out Quaternion nextRot, out Vector3 nextBodyPos)
    {
        nextBodyPos = bodyPos + deltaTime * velocity;
        float landingProgress = landingTimeCrt > 0 ? Mathf.Clamp01(deltaTime / landingTimeCrt) : 0;
        nextRot = Quaternion.Lerp(rot, landingEstimation.rotation, landingProgress);
        nextPos = nextBodyPos - nextRot * bodyProj;
    }

    bool CheckGroundCollision(Vector3 nextPos, out Vector3 hitPos, out Quaternion hitRot) => CheckGroundCollision(transform.position, nextPos, transform.rotation, out hitPos, out hitRot);
    bool CheckGroundCollision(Vector3 pos, Vector3 nextPos, Quaternion rot, out Vector3 hitPos, out Quaternion hitRot, bool gizmo = false)
    {
        Vector3 dir = nextPos - pos;
        float dist = dir.magnitude;

        if (Physics.Raycast(pos, dir, out RaycastHit hit, dist, groundLayer))
        {
            if (gizmo)
            {
                Gizmos.DrawLine(pos, hit.point);
                Gizmos.DrawSphere(hit.point, 0.2f);
            }

            hitPos = hit.point;
            hitRot = MathExtension.RotationMatchUp(rot, hit.normal);

            return true;
        }

        if (gizmo)
            Gizmos.DrawLine(pos, nextPos);

        hitPos = Vector3.zero;
        hitRot = Quaternion.identity;

        return false;
    }

    void EnableSideScripts(bool enable)
    {
        player3D    .enabled = enable;
        playerPivot .enabled = enable;
        followTarget.enabled = enable;
        spider      .enabled = enable;
        shakeBody   .enabled = enable;
    }

    void TransferVelocityToPlayer3D()
    {
        velocity = transform.InverseTransformVector(velocity);
        player3D.VelocityNoAdd = new Vector2(velocity.x, velocity.z);
    }


    void UpdateTargets(float jumpTime)
    {
        if (jumpTime >= targetsJumpTime)
            for (int i = 0; i < targets.Length; i++)
                targets[i].position = targetsJumpDown[i].position;

        else
            for (int i = 0; i < targets.Length; i++)
                targets[i].position = Vector3.Lerp(targetsJumpDown[i].position, targetsJumpUp[i].position, targetsJumpCurve.Evaluate(jumpTime / targetsJumpTime));
    }
}
