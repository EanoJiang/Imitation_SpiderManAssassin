using Spiderman.Movement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    public Animator animator;
    public Transform cam;
    public CapsuleCollider capsuleCollider;
    public PlayerMovementControl personController;
    public ClimbController climbController;
    public Rope rope;
    public KeyCode hookInput = KeyCode.F;

    public AvatarTarget avatarTarget = AvatarTarget.RightHand;
    public float startTime;
    public float endTime;

    public float mathTargetPointHeightOffset = 0.1f;
    public float mathTargetPointForwardOffset = 0.3f;


    [Header("Grappling")]
    public bool grappling;
    private bool isInitGrapple;
    public float maxGrappleDistance = 25f;
    public LayerMask grappleLayer;
    private Vector3 grapplePoint;
    public float maxDistance = 100f;
    public float maxSwingDistance = 40;
    public float minDistance = 4f;
    private GameObject grappleGo;
    public GameObject grapplePrefab;


    [Header("Swing")]
    public bool isSwing;
    private bool swing;
    private bool isInitSwing;



    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
	//	if (rope.start == null)
	//	{
            rope.start = animator.GetBoneTransform(HumanBodyBones.RightHand);
     //   }
		
       
    }


    // Update is called once per frame
    void Update()
    {
        DrawGrapplePoint();

		if (!startRotation)
		{
			GrappleHookHandle();

			SwingHandle();
		}


	}
    void SwingHandle()
    {
        if (swing)
        {
            animator.CrossFade("Swing", 0.1f);
            // rope.isDrawRope = false;
            Invoke("CancelRope", 2.6f);
            swing = false;
        }
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Swing"))
        {
           // endPosition = CheckEndPoint(grapplePoint);
            var normalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            if (!isInitSwing)
            {
                startTime = 0f;
                endTime = 0.84f;
                avatarTarget = AvatarTarget.RightFoot;
                isInitSwing = true;
                targetPosition = grapplePoint;
            }
            if (normalizedTime > 0.84f)
            {
                startTime = 0.84f;
                endTime =0.9f;
                avatarTarget = AvatarTarget.Root;
                targetPosition = CheckEndPoint(grapplePoint);
            }

            if (!animator.isMatchingTarget)
            {
                Debug.Log("isMatchingTarget:"+endTime);
                animator.MatchTarget(targetPosition, Quaternion.identity, avatarTarget, new MatchTargetWeightMask(Vector3.one, 0f), startTime, endTime);
			}
			else
			{
				personController.isGrappling = false;
			}
		}
    }
    public Vector3 targetPosition;
    private void GrappleHookHandle()
	{
        if (grappling)
        {
            animator.CrossFade("Grappling Hook", 0.1f);
            // rope.isDrawRope = false;
            Invoke("CancelRope", 1f);
            grappling = false;
        }
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Grappling Hook"))
		{
			var normalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
			if (!isInitGrapple)
			{
				startTime = 0f;
				endTime = 0.64f;
				avatarTarget = AvatarTarget.RightHand;
				isInitGrapple = true;
                targetPosition = grapplePoint;

            }
			if (normalizedTime > 0.64f)
			{
				startTime = 0.64f;
				endTime = 0.7f;
				avatarTarget = AvatarTarget.Root;
                targetPosition = CheckEndPoint(grapplePoint);
			}

			if (!animator.isMatchingTarget)
			{
				animator.MatchTarget(targetPosition, Quaternion.identity, avatarTarget, new MatchTargetWeightMask(Vector3.one, 0f), startTime, endTime);
			}
			else
			{

                personController.isGrappling = false;
			}

		}
	}



    Vector3 CheckEndPoint(Vector3 target)
	{
        RaycastHit hit;

        //检测上面没有障碍物
        if (!Physics.Raycast(grapplePoint-transform.forward*0.1f, Vector3.up, 1f))
		{
            //前方无障碍
            if (!Physics.Raycast(grapplePoint - transform.forward * 0.1f + Vector3.up * 2f, transform.forward,0.5f))
			{
                //向下检测平台的点
                if (Physics.Raycast(grapplePoint - transform.forward * 0.1f + Vector3.up * 2f+transform.forward*0.5f, -Vector3.up,out hit, 2.5f,grappleLayer))
				{

                    Debug.DrawRay(hit.point, Vector3.up, Color.red, 10f);
                    Debug.DrawRay(hit.point + Vector3.up * 0.5f + transform.forward * 0.5f,Vector3.up,Color.green, 10f);
                    var point = new Vector3(hit.point.x, hit.collider.transform.position.y + hit.collider.bounds.size.y,hit.point.z);
                    return point + Vector3.up* mathTargetPointHeightOffset + transform.forward * mathTargetPointForwardOffset;
				}
			}
		}
        return target + Vector3.up * mathTargetPointHeightOffset + transform.forward * mathTargetPointForwardOffset;
	}


	private void FixedUpdate()
	{
        var grapplingInput = Input.GetKey(hookInput);
        if (grapplingInput)
		{
            StartGrapple();

		}

		if (startRotation)
		{
			RotatePlayer(grapplePoint);
		}
	}

   

    void CancelRope()
	{
        rope.isDrawRope = false;
    }
    public bool startRotation;
    void StartGrapple()
	{
        if(isSwing)
		{
            swing = true;
            isInitSwing = false;

        }
		else
		{
            grappling = true;
            isInitGrapple = false;
        }
		if (climbController != null)
		{
			climbController.ResetParam();
		}

		personController.isGrappling = true;
        startRotation = true;

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, grappleLayer))
        {
            grapplePoint = hit.point+Vector3.up*1f;
           
        }
        else
        {
            grapplePoint = cam.position + cam.forward * maxGrappleDistance;
           
        }
        //grapplePoint.y = hit.collider.transform.position.y + hit.collider.bounds.size.y / 2;
        rope.target.position = isSwing ? calculateSwingTarget(grapplePoint) : grapplePoint;
        rope.isDrawRope = true;

    }

    Vector3 calculateSwingTarget(Vector3 target)
	{
        var targetOnPlane = new Vector3(target.x, transform.position.y, target.z);
        var start =new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 dir = (targetOnPlane - start).normalized;
        var result = transform.position+dir*20f+Vector3.up* maxSwingDistance;
        return result;
	}

	private void OnAnimatorIK(int layerIndex)
	{
		if (grappling || swing)
		{
            personController._animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            personController._animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
        }
	}

	void RotatePlayer(Vector3 targetPosition)
	{
        SetRotation();

        var currentDistance = Vector3.Distance(targetPosition, transform.position);

		if (currentDistance > minDistance)
		{
            var angle = Vector3.Angle(-transform.up, (targetPosition - transform.position).normalized);
            animator.SetFloat("Grappling Angle", angle);
        }
    }

    void SetRotation()
	{
        var grapplingPointTemp = new Vector3(grapplePoint.x,transform.position.y,grapplePoint.z);
        var dir = Quaternion.LookRotation(grapplingPointTemp-transform.position, transform.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, dir, 5f * Time.deltaTime * 50);
        if(Quaternion.Angle(transform.rotation, dir) < 0.1f)
		{
            startRotation = false;
		}
    }

    void DrawGrapplePoint()
    {

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, grappleLayer))
        {
            if (grappleGo == null)
            {
                grappleGo = Instantiate<GameObject>(grapplePrefab);

            }
            grappleGo.transform.position = hit.point;
        }


    }
    private void OnDrawGizmos()
    {
        RaycastHit hit;

        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, grappleLayer))
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(cam.position, hit.point);
            Gizmos.DrawSphere(hit.point, 0.5f);
        }
        else
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(cam.position + cam.forward * maxGrappleDistance, 0.5f);

        }
    }
}
