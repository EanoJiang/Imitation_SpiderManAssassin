using Spiderman.Movement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbController : MonoBehaviour
{
    public bool autoClimb = true;
    public CharacterController _characterController;
    public PlayerMovementControl _personController;
    [Range(0, 180)]
    public float minSurfaceAngle = 30, maxSurfaceAngle = 160;
   
    public Transform handTarget;
    [Tooltip("墙的tag")]
    public List<string> draggableTags = new List<string>() { "FreeClimb" };
    [Tooltip("可攀爬的墙面的层")]
    public LayerMask draggableWall;
    [Tooltip("障碍层")]
    public LayerMask obstacle;

    [Tooltip("移动地面层")]
    public LayerMask groundLayer;

    public float climbEnterSpeed = 5f;

    public UnityEngine.Events.UnityEvent onEnterClimb, onExitClimb;

    public float climbEnterMaxDistance = 1f;

    public float lastPointDistanceH = 0.4f;

    public float lastPointDistanceVUp = 0.2f;
    public float lastPointDistanceVDown = 1.25f;

    public float offsetHandTarget = -0.2f;

    public float offsetBase = 0.35f;


    public float climbUpMinThickness = 0.3f;

    public float climbUpMinSpace = 0.5f;

    public float climbJumpDistance = 2f;
    public float climbUpHeight = 2f;
    public float climbJumpDepth = 2f;
    [Tooltip("IK偏移")]
    public Vector3 offsetHandPositionL, offsetHandPositionR;

    public string animatorStateHierarchy = "Base Layer.FreeClimb";


    public DragInfo dragInfo;
    protected DragInfo jumDragInfo;

    protected float horizontal, vertical;

    
    public bool debugRays;
    public bool debugClimbMovement = true;
    public bool debugClimbUp;
    public bool debugClimbJump;
    public bool debugBaseRotation;
    public bool debugHandIK;

    protected RaycastHit hit;
    public bool canMoveClimb;
    public float oldInput = 0.1f;
    public bool inAlingClimb;
    public bool inClimbEnter;
    public bool inClimbJump;
    public bool inClimbUp;
    protected bool climbEnterGrounded, climbEnterAir;
    protected Vector3 upPoint;
    protected Vector3 jumpPoint;
    protected Quaternion jumpRotation;
    protected Vector2 input;
    protected float ikWeight;



    Vector3 lHandPos;
    Vector3 rHandPos;
    Vector3 targetPositionL;
    Vector3 targetPositionR;
    Vector2 lastInput;
    Vector3 handTargetPosition
    {
        get
        {
            return transform.TransformPoint(handTarget.localPosition.x, handTarget.localPosition.y, 0);
        }
    }

    void Start()
    {
        dragInfo = new DragInfo();
        jumDragInfo = new DragInfo();
        
        // 如果没有手动设置 CharacterController，自动获取
        if (_characterController == null)
        {
            _characterController = GetComponent<CharacterController>();
        }
    }

    // Update is called once per frame
    void Update()
    {

		input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		ClimbHandle();
		ClimbJumpHandle();
		ClimbUpHandle();

	}


	private void FixedUpdate ()
	{
		if (!canMoveClimb) return;
		climbEnterGrounded = (_personController._animator.GetCurrentAnimatorStateInfo(0).IsName(animatorStateHierarchy + ".EnterClimbGrounded"));
		if (dragInfo.inDrag && (canMoveClimb) && !inClimbUp && !inClimbJump && !climbEnterGrounded)
		{
			ApplyClimbMovement();
		}


	}

    [System.Serializable]
    public class DragInfo
    {
        public bool canGo;
        public bool inDrag;
        public Vector3 position
        {
            get
            {
                if (collider != null && collider.transform.parent) return collider.transform.parent.TransformPoint(localPosition);
                return localPosition;
            }
            set
            {
                if (collider != null && collider.transform.parent) localPosition = collider.transform.parent.InverseTransformPoint(value);
                else localPosition = value;
            }
        }
        public Vector3 normal;
        private Vector3 localPosition;
        public Collider collider;
    }

    void ClimbHandle()
	{
		if (!dragInfo.inDrag)
		{
            if (Physics.Raycast(handTargetPosition, transform.forward, out hit, climbEnterMaxDistance, draggableWall))
            {
                if (IsValidPoint(hit.normal, hit.collider.transform.gameObject.tag))
                {
                    if (debugRays) Debug.DrawRay(handTargetPosition, transform.forward * climbEnterMaxDistance, Color.green);
                    dragInfo.canGo = true;
                    dragInfo.normal = hit.normal;
                    dragInfo.collider = hit.collider;
                    dragInfo.position = hit.point;
                }
            }
            else
            {
                if (debugRays) Debug.DrawRay(handTargetPosition, transform.forward * climbEnterMaxDistance, Color.red);
                dragInfo.canGo = false;
            }
        }
		if (dragInfo.canGo && !inClimbEnter && Physics.SphereCast(handTargetPosition + transform.forward * -_characterController.radius * 0.5f, _characterController.radius * 0.5f, transform.forward, out hit, climbEnterMaxDistance, draggableWall))
		{
			dragInfo.collider = hit.collider;
			var hitPointLocal = transform.InverseTransformPoint(hit.point);
			hitPointLocal.y = handTarget.localPosition.y;
			hitPointLocal.x = handTarget.localPosition.x;

			dragInfo.position = transform.TransformPoint(hitPointLocal);

			if (Input.GetKeyDown(KeyCode.Space) && dragInfo.inDrag && input.magnitude == 0 && Time.time > (oldInput + 0.5f))
				ExitClimb();
			else if (dragInfo.canGo && (Input.GetKeyDown(KeyCode.Space) || input.y > 0.1f||(!_personController._characterIsOnGround&& input.magnitude == 0)) && !dragInfo.inDrag && Time.time > (oldInput + 2f))
				EnterClimb();
		}
		ClimbMovement();
	}

    void EnterClimb()
	{
        oldInput = Time.time;
        //_rigidbody.isKinematic = true;
        _characterController.enabled = false;
        _personController._isOnClimb = true;
        _personController.enabled = false;
        RaycastHit hit;
        var dragPosition = new Vector3(dragInfo.position.x, transform.position.y, dragInfo.position.z) + transform.forward * -_characterController.radius;
        var castObstacleUp = Physics.Raycast(dragPosition + transform.up * _characterController.height, transform.up, _characterController.height * 0.5f, obstacle);
        var castDragableWallForward = Physics.Raycast(dragPosition + transform.up * (_characterController.height * climbUpHeight), transform.forward, out hit, 1f, draggableWall) && draggableTags.Contains(hit.collider.gameObject.tag);
        var climbUpConditions = _personController._characterIsOnGround && !castObstacleUp && castDragableWallForward;
        Debug.Log($"climbUpConditions : {climbUpConditions},_personController._characterIsOnGround:{_personController._characterIsOnGround},castObstacleUp:{castObstacleUp},castDragableWallForward:{castDragableWallForward}");

        _personController._animator.SetBool("IsGrounded", true);

        _personController._animator.CrossFadeInFixedTime(climbUpConditions ? "EnterClimbGrounded" : "EnterClimbAir", 0.2f);

        if (dragInfo.collider && dragInfo.collider.transform.parent && transform.parent != dragInfo.collider.transform.parent && !dragInfo.collider.transform.parent.gameObject.isStatic)
            transform.parent = dragInfo.collider.transform.parent;

		if (!climbUpConditions)
		{
			StartCoroutine(EnterClimbAlignment());
		}
		else
		{
			transform.position = (dragInfo.position - transform.rotation * handTarget.localPosition);
			dragInfo.inDrag = true;
		}

        Debug.Log("Enter Climb");
    }

    //调整角色开始攀爬时，角色朝向于墙壁对齐。并调整角色爬墙是与墙壁的距离
    IEnumerator EnterClimbAlignment()
	{
        inClimbEnter = true;
        dragInfo.inDrag = true;
        var _position = transform.position;
        var _rotation = transform.rotation;
        var _targetRotation = Quaternion.LookRotation(-dragInfo.normal);
        var _targetPosition = (dragInfo.position - transform.rotation * handTarget.localPosition);

        var _transition = 0f;
        Debug.DrawLine(handTargetPosition, dragInfo.position, Color.red, 10f);
        Debug.DrawLine(transform.position, dragInfo.position, Color.red, 10f);
        while (_transition < 1)
        {
            _transition += Time.deltaTime * climbEnterSpeed;

            transform.rotation = Quaternion.Lerp(_rotation, _targetRotation, _transition);
            _targetPosition = (dragInfo.position - transform.rotation * handTarget.localPosition);
            transform.position = _targetPosition;
            yield return null;
        }
        _targetPosition = (dragInfo.position - transform.rotation * handTarget.localPosition);
        transform.position = _targetPosition;
        inClimbEnter = false;
        //yield return null;
	}

    //退出时重置参数
    public void ResetParam()
	{
        dragInfo.inDrag = false;
        dragInfo.canGo = false;
        inClimbJump = false;
        _characterController.enabled = true;
        _personController._isOnClimb = false;
        _personController.enabled = true;
        inClimbUp = false;
    }
    public void ExitClimb()
    {

        oldInput = Time.time;
      
        //_personController.isJumping = false;
        if (!inClimbUp)
        {
            bool nextGround = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.5f, groundLayer);
            var charAngle = Vector3.Angle(transform.forward, Vector3.up);
            if (charAngle < 80)
            {
                var dir = transform.forward;
                dir.y = 0;

                var postion = dragInfo.position + dir.normalized * -_characterController.radius + Vector3.down * _characterController.height;
                transform.position = postion;
            }
			//if (nextGround)
			//{
   //             _personController._animator.SetTrigger("ClimbExitGrounded");
   //         }else
			//{
   //             _personController._animator.SetTrigger("ClimbExitAir");
   //         }
            _personController._animator.CrossFadeInFixedTime(nextGround ? "ExitGrounded" : "ExitAir", 0.2f);
        }

        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        ResetParam();

        if (transform.parent != null && dragInfo.collider && dragInfo.collider.transform.parent && transform.parent == dragInfo.collider.transform.parent) transform.parent = null;
        Debug.Log("Exit Climb");
    }

    void ClimbMovement()
	{
        if (!dragInfo.inDrag) return;
        if (dragInfo.collider && dragInfo.collider.transform.parent && transform.parent != dragInfo.collider.transform.parent && !dragInfo.collider.transform.parent.gameObject.isStatic) transform.parent = dragInfo.collider.transform.parent;
        horizontal = input.x;
        vertical = input.y;
        canMoveClimb = CheckCanMoveClimb();
        dragInfo.canGo = canMoveClimb;
        Debug.Log("canMoveClimb");
        if (canMoveClimb)
        {
 
            _personController._animator.SetFloat("AxisX", horizontal, 0.2f, Time.deltaTime);
            _personController._animator.SetFloat("AxisY", vertical, 0.2f, Time.deltaTime);
        }
        else if (!inAlingClimb && !inClimbJump)
        {
            _personController._animator.SetFloat("AxisX", 0, 0.2f, Time.deltaTime);
            _personController._animator.SetFloat("AxisY", 0, 0.2f, Time.deltaTime);
        }

        if (input.y < 0 && Physics.Raycast(transform.position + transform.up * (_characterController.height * 0.5f), -transform.up, _characterController.height, groundLayer))
        {
            Debug.Log("检测到地面");
            ExitClimb();
        }
    }

    bool CheckCanMoveClimb()
	{
        if (input.magnitude > 0.1f)
        {
            lastInput = input;
        }
        var h = lastInput.x > 0 ? 1 * lastPointDistanceH : lastInput.x < 0 ? -1 * lastPointDistanceH : 0;
        var v = lastInput.y > 0 ? 1 * lastPointDistanceVUp : lastInput.y < 0 ? -1 * lastPointDistanceVDown : 0;
        var centerCharacter = handTargetPosition + transform.up * offsetHandTarget;
        var targetPosNormalized = centerCharacter + (transform.right * h) + (transform.up * v);
        var targetPos = centerCharacter + (transform.right * lastInput.x) + (transform.up * lastInput.y);
        var castDir = (targetPosNormalized - handTargetPosition + (transform.forward * -0.5f)).normalized;
        var castDirCapsule = (targetPos - handTargetPosition + (transform.forward * -0.5f)).normalized;

        if (CheckCapsule(_characterController, castDirCapsule, out hit, obstacle) && !draggableTags.Contains(hit.collider.gameObject.tag))
        {
            return false;
        }

        if (inClimbJump || inClimbUp) return false;
        Line climbLine = new Line(centerCharacter, targetPosNormalized);
        climbLine.Draw(Color.green, draw: debugRays && debugClimbMovement);
        if (Physics.Linecast(climbLine.p1, climbLine.p2, out hit, draggableWall))
        {
            if (IsValidPoint(hit.normal, hit.collider.transform.gameObject.tag))
            {
                dragInfo.collider = hit.collider;
                dragInfo.normal = hit.normal;
                return true;
            }
        }

        climbLine.p1 = climbLine.p2;
        climbLine.p2 = climbLine.p1 + transform.forward *_characterController.radius * 2f;
        climbLine.Draw(Color.yellow, draw: debugRays && debugClimbMovement);
        if (Physics.Linecast(climbLine.p1, climbLine.p2, out hit, draggableWall))
        {
            if (IsValidPoint(hit.normal, hit.collider.transform.gameObject.tag))
            {
                dragInfo.collider = hit.collider;
                dragInfo.normal = hit.normal;
                return true;
            }
        }

        climbLine.p1 += transform.forward * _characterController.radius * 0.5f;
        climbLine.p2 += (transform.right * (_characterController.radius + lastPointDistanceH) * -input.x) + (transform.up * -v) + transform.forward * _characterController.radius;
        climbLine.Draw(Color.red, draw: debugRays && debugClimbMovement);
        if (Physics.Linecast(climbLine.p1, climbLine.p2, out hit, draggableWall))
        {
            if (IsValidPoint(hit.normal, hit.collider.transform.gameObject.tag))
            {
                dragInfo.normal = hit.normal;
                dragInfo.collider = hit.collider;
                return true;
            }
        }
        return false;
	}
    void ClimbJumpHandle()
    {
        if (_personController.enabled || !_personController._animator || !dragInfo.inDrag || inClimbUp || inClimbEnter) return;
        if (Input.GetKeyDown(KeyCode.Space) && !inClimbJump && input.magnitude > 0 && !_personController._animator.GetCurrentAnimatorStateInfo(0).IsName(animatorStateHierarchy + ".ClimbJump"))
        {
            var angleBetweenCharacterAndCamera = Vector3.Angle(transform.right, Camera.main.transform.right);
            var rightDirection = angleBetweenCharacterAndCamera > 60 ? Camera.main.transform.right : transform.right;
            var pos360 = handTargetPosition + (transform.forward * -0.5f) + (rightDirection * climbJumpDistance * horizontal) + (Vector3.up * climbJumpDistance * vertical);
            if (debugRays && debugClimbJump)
            {
                Debug.DrawLine(handTargetPosition + (transform.forward * -0.05f), pos360, Color.red, 1f);
                Debug.DrawRay(pos360, transform.forward * climbJumpDepth, Color.red, 1f);
            }

            float casts = 0f;
            for (int i = 0; casts < 1f; i++)
            {
                var radius = _characterController.radius / 0.45f;

                var dir = (rightDirection * input.x + transform.up * input.y).normalized;
                for (float a = 0; a < 1; a += 0.2f)
                {
                    var p = transform.position + transform.up * _characterController.height * casts;
                    p = p + rightDirection * ((-_characterController.radius) + (radius * a));

                    if (Physics.Raycast(p, dir.normalized, out hit, climbJumpDistance, obstacle))
                    {
                        if (!(draggableWall == (draggableWall | (1 << hit.collider.gameObject.layer))) || !draggableTags.Contains(hit.collider.gameObject.tag))
                        {
                            if (debugRays && debugClimbJump) Debug.DrawRay(p, dir.normalized * climbJumpDistance, Color.red, 0.4f);
                            return;
                        }
                        else if (debugRays && debugClimbJump) Debug.DrawRay(p, dir.normalized * climbJumpDistance, Color.yellow, 0.4f);
                    }
                    else if (debugRays && debugClimbJump) Debug.DrawRay(p, dir.normalized * climbJumpDistance, Color.green, 0.4f);
                }
                casts += 0.1f;
            }

            if (Physics.Linecast(handTargetPosition + (transform.forward * -0.5f), pos360, out hit, draggableWall))
            {
                if (IsValidPoint(hit.normal, hit.collider.transform.gameObject.tag))
                {
                    var dir = pos360 - handTargetPosition;
                    var angle = Vector3.Angle(transform.up, dir);
                    angle = angle * (input.x > 0 ? 1 : input.x < 0 ? -1 : 1);
                    jumpRotation = Quaternion.LookRotation(-hit.normal);


                    jumDragInfo.collider = hit.collider;
                    jumDragInfo.normal = hit.normal;
                    jumDragInfo.position = hit.point;
                    dragInfo.collider = hit.collider;
                    dragInfo.position = hit.point;
                    ClimbJump();
                }
            }
            else if (Physics.Raycast(pos360, transform.forward, out hit, climbJumpDepth, draggableWall))
            {
                if (IsValidPoint(hit.normal, hit.collider.transform.gameObject.tag))
                {
                    var dir = pos360 - handTargetPosition;
                    var angle = Vector3.Angle(transform.up, dir);
                    angle = angle * (input.x > 0 ? 1 : input.x < 0 ? -1 : 1);
                    jumpRotation = Quaternion.LookRotation(-hit.normal);
                    jumDragInfo.collider = hit.collider;
                    jumDragInfo.normal = hit.normal;
                    jumDragInfo.position = hit.point;
                    dragInfo.collider = hit.collider;
                    dragInfo.position = hit.point;
                    ClimbJump();
                }
            }
        }

        //TO Do 在Jump动画播放到0.8时，设置inClimbJump为false
        if (Input.GetKeyUp(KeyCode.Space))
		{
            inClimbJump = false;
		}    
    }



	void ClimbJump()
	{
        inClimbJump = true;
        _personController._animator.SetFloat("AxisX", input.x);
        _personController._animator.SetFloat("AxisY", input.y);
        _personController._animator.CrossFadeInFixedTime("ClimbJump", 0.2f);
      //  if (jumpClimbStaminaCost > 0) TP_Input.cc.ReduceStamina(jumpClimbStaminaCost, false);
    }
    void ClimbUpHandle()
	{
        if (inClimbJump || _personController.enabled || !_personController._animator || !dragInfo.inDrag) return;
       // Debug.Log(_personController._animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
        if (inClimbUp && !inAlingClimb)
        {

           // Debug.Log(_personController._animator.GetCurrentAnimatorClipInfo(0)[0].clip);
            if (_personController._animator.GetCurrentAnimatorStateInfo(0).IsName(animatorStateHierarchy + ".ClimbUpWall"))
            {
                if (!_personController._animator.IsInTransition(0))
                    _personController._animator.MatchTarget(upPoint + Vector3.up * 0.1f, Quaternion.Euler(0, transform.eulerAngles.y, 0), AvatarTarget.RightHand, 
                        new MatchTargetWeightMask(new Vector3(0, 1, 1), 1), 0.1f, 0.4f);
               // Debug.Log(_personController._animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
                var normalizedTime = _personController._animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                if (normalizedTime > 0.45f)
				{
                    _personController._animator.MatchTarget(upPoint + Vector3.up * 0.1f+transform.forward * 0.2f, Quaternion.Euler(0, transform.eulerAngles.y, 0), 
                        AvatarTarget.Root, new MatchTargetWeightMask(new Vector3(0, 1, 1), 1), 0.4f, 0.8f);
                }
                if (normalizedTime > .9f) ExitClimb();
            }
            return;
        }
        CheckClimbUp();
    }

    void CheckClimbUp(bool ignoreInput = false)
	{
      //  Debug.Log("CheckClimbUp");
      
        var climbUpConditions = autoClimb ? vertical > 0f : Input.GetKeyDown(KeyCode.E)/*climbEdgeInput.GetButtonDown()*/;

        if (!canMoveClimb && !inClimbUp && (climbUpConditions || ignoreInput))
        {
            var dir = transform.forward;

            var startPoint = dragInfo.position + transform.forward * -0.1f;
            var endPoint = startPoint + Vector3.up * (_characterController.height * 0.25f);
            var obstructionPoint = endPoint + dir.normalized * (climbUpMinSpace + 0.1f);
            var thicknessPoint = endPoint + dir.normalized * (climbUpMinThickness + 0.1f);
            var climbPoint = thicknessPoint + -transform.up * (_characterController.height * 0.5f);

            if (!Physics.Linecast(startPoint, endPoint, obstacle))
            {
                //向上检测障碍
                if (debugRays && debugClimbUp) Debug.DrawLine(startPoint, endPoint, Color.green, 2f);
                if (!Physics.Linecast(endPoint, obstructionPoint, obstacle))
                {
                    if (debugRays && debugClimbUp) Debug.DrawLine(endPoint, obstructionPoint, Color.green, 2f);
                    //向前检测地面
                    if (Physics.Linecast(thicknessPoint, climbPoint, out hit, groundLayer))
                    {
                        if (debugRays && debugClimbUp) Debug.DrawLine(thicknessPoint, climbPoint, Color.green, 2f);
                        var angle = Vector3.Angle(Vector3.up, hit.normal);
                        var localUpPoint = transform.InverseTransformPoint(hit.point + (angle > 25 ? Vector3.up * _characterController.radius : Vector3.zero) + dir * -(climbUpMinThickness * 0.5f));
                        localUpPoint.z = _characterController.radius;
                        upPoint = transform.TransformPoint(localUpPoint);
                        //向上检测障碍
                        if (Physics.Raycast(hit.point + Vector3.up * -0.05f, Vector3.up, out hit, _characterController.height, obstacle))
                        {
                            if (hit.distance > _characterController.height * 0.5f)
                            {
                                ClimbUp();
                            }
                            else
                            {
                                if (debugRays && debugClimbUp) Debug.DrawLine(upPoint, hit.point, Color.red, 2f);
                            }
                        }
                        else ClimbUp();
                    }
                    else if (debugRays && debugClimbUp) Debug.DrawLine(thicknessPoint, climbPoint, Color.red, 2f);
                }
                else if (debugRays && debugClimbUp) Debug.DrawLine(endPoint, obstructionPoint, Color.red, 2f);
            }
            else if (debugRays && debugClimbUp) Debug.DrawLine(startPoint, endPoint, Color.red, 2f);
        }
    }

    void ClimbUp()
    {
        Debug.Log("ClimbUp");
        StartCoroutine(AlignClimb());
        inClimbUp = true;
    }
    //让角色对齐墙面
    IEnumerator AlignClimb()
    {
        inAlingClimb = true;
        var transition = 0f;
        var dir = transform.forward;
        dir.y = 0;
        var angle = Vector3.Angle(Vector3.up, transform.forward);

        var targetRotation = Quaternion.LookRotation(-dragInfo.normal);
        var targetPosition = ((dragInfo.position + dir * -_characterController.radius + Vector3.up * 0.1f) - transform.rotation * handTarget.localPosition);

        _personController._animator.SetFloat("AxisY", 1f);
        while (transition < 1 && Vector3.Distance(targetRotation.eulerAngles, transform.rotation.eulerAngles) > 0.2f && angle < 60)
        {
            _personController._animator.SetFloat("AxisY", 1f);
            transition += Time.deltaTime * 0.5f;
            targetPosition = ((dragInfo.position + dir * -_characterController.radius) - transform.rotation * handTarget.localPosition);
            transform.position = Vector3.Slerp(transform.position, targetPosition, transition);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, transition);
            yield return null;
        }
        _personController._animator.CrossFadeInFixedTime("ClimbUpWall", 0.1f);
        inAlingClimb = false;
    }

    //旋转
    void ApplyClimbMovement()
    {
        if (!canMoveClimb)
            return;
        ///Apply Rotation
        CalculateMovementRotation();

		var root = transform.InverseTransformPoint(_personController._animator.rootPosition);

		var position = (dragInfo.position - transform.rotation * handTarget.localPosition) + (transform.right * root.x + transform.up * root.y);
		Debug.DrawLine(transform.position, dragInfo.position);
		if (input.magnitude > 0.1f)
			transform.position = Vector3.Lerp(transform.position, position, 1f);
	}

    //根据移动方向，检测墙面转向
    void CalculateMovementRotation(bool ignoreLerp = false)
    {
        var h = lastInput.x;
        var v = lastInput.y;
        var characterBase = transform.position + transform.up * (_characterController.radius + offsetBase);
        var directionPoint = characterBase + transform.right * (h * lastPointDistanceH) + transform.up * (v * lastPointDistanceVUp);

        RaycastHit rotationHit;
        Line centerLine = new Line(characterBase, directionPoint);
        centerLine.Draw(Color.cyan,10f, draw: debugRays && debugBaseRotation);
        var hasBasePoint = CheckBasePoint(out rotationHit);

        var basePoint = rotationHit.point;
        if (Physics.Linecast(centerLine.p1, centerLine.p2, out rotationHit, draggableWall) && draggableTags.Contains(rotationHit.collider.gameObject.tag))
        {
            
            RotateTo(-rotationHit.normal, hasBasePoint ? basePoint : rotationHit.point, ignoreLerp);
            return;
        }

        centerLine.p1 = centerLine.p2;
        centerLine.p2 += transform.forward * (climbEnterMaxDistance);
        centerLine.Draw(Color.yellow, draw: debugRays && debugBaseRotation);

        if (Physics.Linecast(centerLine.p1, centerLine.p2, out rotationHit, draggableWall) && draggableTags.Contains(rotationHit.collider.gameObject.tag))
        {
         
            RotateTo(-rotationHit.normal, hasBasePoint ? basePoint : rotationHit.point, ignoreLerp);
            return;
        }
        centerLine.p1 += transform.forward * _characterController.radius * 0.5f;
        centerLine.p2 += (transform.right * ((_characterController.radius + lastPointDistanceH) * -input.x)) + (transform.up * lastPointDistanceVUp * -v) + transform.forward * _characterController.radius;
        centerLine.Draw(Color.red, draw: debugRays && debugBaseRotation);

        if (Physics.Linecast(centerLine.p1, centerLine.p2, out rotationHit, draggableWall) && draggableTags.Contains(rotationHit.collider.gameObject.tag))
        {
            RotateTo(-rotationHit.normal, hasBasePoint ? basePoint : rotationHit.point, ignoreLerp);
            return; 
        }
    }
    bool CheckBasePoint(out RaycastHit baseHit)
    {
        var forward = new Vector3(transform.forward.x, 0, transform.forward.z);
        var characterBase = transform.position + transform.up * (_characterController.radius + offsetBase) - forward * (_characterController.radius * 2);

        var targetPoint = transform.position + forward * (1 + _characterController.radius);
        Line baseLine = new Line(characterBase, targetPoint);

        if (Physics.Linecast(baseLine.p1, baseLine.p2, out baseHit, draggableWall) && draggableTags.Contains(baseHit.collider.gameObject.tag))
        {
            baseLine.Draw(Color.blue, draw: debugRays && debugBaseRotation);
            return true;
        }
        baseLine.Draw(Color.magenta, draw: debugRays);
        baseLine.p1 = baseLine.p2;
        baseLine.p2 = baseLine.p1 + forward + Vector3.up;

        if (Physics.Linecast(baseLine.p1, baseLine.p2, out baseHit, draggableWall) && draggableTags.Contains(baseHit.collider.gameObject.tag))
        {
            baseLine.Draw(Color.blue, draw: debugRays && debugBaseRotation);
            return true;
        }
        baseLine.Draw(Color.magenta, draw: debugRays);
        baseLine.p2 = baseLine.p1 + forward + Vector3.down;

        if (Physics.Linecast(baseLine.p1, baseLine.p2, out baseHit, draggableWall) && draggableTags.Contains(baseHit.collider.gameObject.tag))
        {
            baseLine.Draw(Color.blue, draw: debugRays && debugBaseRotation);
            return true;
        }
        baseLine.Draw(Color.magenta, draw: debugRays && debugBaseRotation);
        return false;
    }
    
    void RotateTo(Vector3 direction, Vector3 point, bool ignoreLerp = false)
    {
        if (input.magnitude < 0.1f) return;
      
        var referenceDirection = point - dragInfo.position;
        if (debugRays && debugBaseRotation) Debug.DrawLine(point, dragInfo.position, Color.blue);
        var resultDirection = Quaternion.AngleAxis(-90, transform.right) * referenceDirection;
        var eulerX = Quaternion.LookRotation(resultDirection).eulerAngles.x;
        var baseRotation = Quaternion.LookRotation(direction);
        var resultRotation = Quaternion.Euler(eulerX, baseRotation.eulerAngles.y, transform.eulerAngles.z);
       // Debug.Log("Rotation"+ resultRotation.eulerAngles);
       transform.rotation = Quaternion.Lerp(transform.rotation, resultRotation, (_personController._animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1) * 0.2f);
    }


    bool IsValidPoint(Vector3 normal, string tag)
    {
        if (!draggableTags.Contains(tag)) return false;

        var angle = Vector3.Angle(Vector3.up, normal);

        if (angle >= minSurfaceAngle && angle <= maxSurfaceAngle)
            return true;
        return false;
    }

    public bool CheckCapsule(CharacterController capsule, Vector3 dir, out RaycastHit hit, LayerMask mask, bool drawGizmos = false)
    {
        var pCenter = capsule.transform.position;
        var p1 = pCenter + capsule.transform.up * ((capsule.height * 0.5f) - capsule.radius);
        var p2 = pCenter - capsule.transform.up * ((capsule.height * 0.5f) - capsule.radius);

        if (drawGizmos)
            Gizmos.color = Color.green;
        var check = false;
        if (Physics.CapsuleCast(p1, p2, 0.2f, dir, out hit, capsule.radius * 0.5f, mask))
        {
            if (drawGizmos)
                Gizmos.color = Color.red;
            check = true;
        }
        if (drawGizmos)
        {
            Gizmos.DrawWireSphere(p1, capsule.radius);
            Gizmos.DrawWireSphere(p2, capsule.radius);
        }

        return check;
    }

	private void OnAnimatorIK(int layerIndex)
	{
        if (_personController.enabled || inClimbJump || inClimbUp || !dragInfo.inDrag) { ikWeight = 0; return; }
        ikWeight = Mathf.Lerp(ikWeight, 1f, 2f * Time.deltaTime);
        if (ikWeight > 0)
        {
            var lRoot = transform.InverseTransformPoint(_personController._animator.GetBoneTransform(HumanBodyBones.LeftHand).position);
            var rRoot = transform.InverseTransformPoint(_personController._animator.GetBoneTransform(HumanBodyBones.RightHand).position);
            RaycastHit hit2;

            if (Physics.Raycast(_personController._animator.GetBoneTransform(HumanBodyBones.LeftHand).position + transform.forward * -0.5f + transform.up * -0.2f, transform.forward, out hit2, 1f, draggableWall))
            {
                targetPositionL = transform.InverseTransformPoint(hit2.point);
                if (debugRays && debugHandIK) Debug.DrawLine(_personController._animator.GetBoneTransform(HumanBodyBones.LeftHand).position + transform.forward * -0.5f + transform.up * -0.2f, hit2.point, Color.green);
            }
            else
            {
                var center = transform.TransformPoint(0, lRoot.y, 0);
                var target = rRoot;
                if (Physics.Raycast(center, transform.forward, out hit2, 1f, draggableWall))
                {
                    target = transform.InverseTransformPoint(hit2.point);
                }
                target.x = 0;
                targetPositionL = Vector3.Lerp(targetPositionL, target, 5f * Time.deltaTime);
                if (debugRays && debugHandIK) Debug.DrawRay(_personController._animator.GetBoneTransform(HumanBodyBones.LeftHand).position + transform.forward * -0.5f + transform.up * -0.2f, transform.forward, Color.red);
            }

            if (Physics.Raycast(_personController._animator.GetBoneTransform(HumanBodyBones.RightHand).position + transform.forward * -0.5f + transform.up * -0.2f, transform.forward, out hit2, 1f, draggableWall))
            {
                targetPositionR = transform.InverseTransformPoint(hit2.point);
                if (debugRays && debugHandIK) Debug.DrawLine(_personController._animator.GetBoneTransform(HumanBodyBones.RightHand).position + transform.forward * -0.5f + transform.up * -0.2f, hit2.point, Color.green);
            }
            else
            {
                var center = transform.TransformPoint(0, rRoot.y, 0);
                var target = lRoot;
                if (Physics.Raycast(center, transform.forward, out hit2, 1f, draggableWall))
                    target = transform.InverseTransformPoint(hit2.point);

                target.x = 0;
                targetPositionR = Vector3.Lerp(targetPositionR, target, 5f * Time.deltaTime);
                if (debugRays && debugHandIK) Debug.DrawRay(_personController._animator.GetBoneTransform(HumanBodyBones.RightHand).position + transform.forward * -0.5f + transform.up * -0.2f, transform.forward, Color.red);
            }
            var leftHandPosition = transform.position + transform.right * targetPositionL.x + transform.up * lRoot.y + transform.forward * targetPositionL.z;
            var rightHandPosition = transform.position + transform.right * targetPositionR.x + transform.up * rRoot.y + transform.forward * targetPositionR.z;
            lHandPos = transform.forward * offsetHandPositionL.z + transform.right * offsetHandPositionL.x + transform.up * offsetHandPositionL.y;// Vector3.Lerp(lHandPos, transform.forward * offsetHandPositionL.z + transform.right * offsetHandPositionL.x + transform.up * offsetHandPositionL.y, 2 * Time.deltaTime);
            rHandPos = transform.forward * offsetHandPositionR.z + transform.right * offsetHandPositionR.x + transform.up * offsetHandPositionR.y;// Vector3.Lerp(rHandPos, transform.forward * offsetHandPositionR.z + transform.right * offsetHandPositionR.x + transform.up * offsetHandPositionR.y, 2 * Time.deltaTime);
            leftHandPosition += lHandPos;
            rightHandPosition += rHandPos;

            _personController._animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, ikWeight);
            _personController._animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);

            _personController._animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandPosition);
            _personController._animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandPosition);
        }
        else
        {
            _personController._animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            _personController._animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
        }
    }
}
