#if invector
using FS_ThirdPerson;
using Invector.vCamera;
using Invector.vCharacterController;
using System.Collections;
using UnityEngine;

namespace FS_ParkourSystem
{

    public class InvectorIntegrationHelper : MonoBehaviour, ICharacter
    {
        public bool verticalJump;
        vThirdPersonInput thirdPersonInput;
        vThirdPersonController thirdPersonController;
        vShooterMeleeInput shooterMeleeInput;
        vMeleeCombatInput meleeCombatInput;

        ParkourController parkourController;
        PlayerController playerController;
        Collider playerCollider;
        Animator animator;

        public bool UseRootMotion { get; set; } = false;

        public Vector3 MoveDir { get { return thirdPersonController.moveDirection; } }

        public bool IsGrounded => thirdPersonController.isGrounded;

        public float Gravity => -15;

        public Animator Animator
        {
            get { return animator == null ? GetComponent<Animator>() : animator; }
            set
            {
                animator = value;
            }
        }

        private void Awake()
        {
            thirdPersonController = GetComponent<vThirdPersonController>();
            thirdPersonInput = GetComponent<vThirdPersonInput>();
            shooterMeleeInput = GetComponent<vShooterMeleeInput>();
            meleeCombatInput = GetComponent<vMeleeCombatInput>();
            playerController = GetComponent<PlayerController>();

            parkourController = GetComponent<ParkourController>();
            playerCollider = GetComponent<Collider>();
            animator = GetComponent<Animator>();
            if (LayerMask.NameToLayer("Ledge") > -1 && !(thirdPersonController.groundLayer == (thirdPersonController.groundLayer | (1 << LayerMask.NameToLayer("Ledge")))))
                thirdPersonController.groundLayer += 1 << LayerMask.NameToLayer("Ledge");
        }

        public void OnEndSystem(SystemBase systemBase)
        {
            systemBase.UnFocusScript();
            systemBase.ExitSystem();
            playerController.ResetState();
            vThirdPersonCamera.instance.selfRigidbody.interpolation = RigidbodyInterpolation.None;
            vThirdPersonCamera.instance.selfRigidbody.useGravity = true;
            thirdPersonController.animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
            thirdPersonController._rigidbody.interpolation = RigidbodyInterpolation.None;
            thirdPersonController._rigidbody.useGravity = true;
            thirdPersonController.enabled = true;
            thirdPersonInput.enabled = true;
            playerCollider.enabled = true;
        }

        public void OnStartSystem(SystemBase systemBase)
        {
            playerController.UnfocusAllSystem();
            systemBase.FocusScript();
            systemBase.EnterSystem();
            playerController.SetSystemState(systemBase.State);

            

            vThirdPersonCamera.instance.selfRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            vThirdPersonCamera.instance.selfRigidbody.useGravity = false;
            thirdPersonController._rigidbody.velocity = Vector3.zero;
            thirdPersonController._rigidbody.useGravity = false;
            //thirdPersonController._rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            thirdPersonController.inputSmooth = Vector3.zero;
            thirdPersonController.isGrounded = false;
            animator.SetBool(vAnimatorParameters.IsGrounded, thirdPersonController.isGrounded);
            animator.SetFloat(vAnimatorParameters.GroundDistance, 0f);
            animator.updateMode = AnimatorUpdateMode.Normal;
            thirdPersonController.enabled = false;
            thirdPersonInput.enabled = false;
            playerCollider.enabled = false;

        }
        private void FixedUpdate()
        {
            if (parkourController.ControlledByParkour)
                thirdPersonInput.CameraInput();
        }

        private void Update()
        {
            if(previuosPrevent != PreventSystem)
            {
                previuosPrevent = PreventSystem;
                PreventAllSystems = PreventSystem;
            }

            if (thirdPersonInput.jumpInput.GetButtonDown())
            {
                StartCoroutine(HandleVerticalJump());
            }
        }

        public IEnumerator HandleVerticalJump()
        {
            yield return new WaitForFixedUpdate();
            if (!verticalJump || !IsGrounded) yield break;
            if (thirdPersonInput.JumpConditions())
                thirdPersonController.Jump(true);
            yield break;
        }
        bool previuosPrevent = false;

        bool PreventSystem => (shooterMeleeInput != null && (shooterMeleeInput.isAimingByInput || shooterMeleeInput.isReloading)) ||
                            thirdPersonController.customAction ||
                            thirdPersonController.isJumping ||
                            thirdPersonController.isRolling ||
                            (meleeCombatInput != null && (meleeCombatInput.isAttacking || meleeCombatInput.isBlocking || meleeCombatInput.isEquipping));

        public bool PreventAllSystems { get; set; } = false;
    }
}
#endif

