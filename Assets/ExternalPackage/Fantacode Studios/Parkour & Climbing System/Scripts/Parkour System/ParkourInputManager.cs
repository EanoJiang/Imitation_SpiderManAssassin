using System;
using UnityEngine;

namespace FS_ParkourSystem
{
    public partial class ParkourInputManager : MonoBehaviour
    {
        [Header("Keys")]
        [SerializeField] KeyCode jumpKey = KeyCode.Space;
        [SerializeField] KeyCode dropKey = KeyCode.E;
        [SerializeField] KeyCode jumpFromHangKey = KeyCode.Q;


        [Header("Buttons")]
        [SerializeField] string jumpButton;
        [SerializeField] string dropButton;
        [SerializeField] string jumpFromHangButton;

        public bool Jump { get; set; }
        public bool JumpKeyDown { get; set; }
        public bool Drop { get; set; }
        public bool JumpFromHang { get; set; }

#if inputsystem
        ParkourInputAction input;
        private void OnEnable()
        {
            input = new ParkourInputAction();
            input.Enable();
        }
        private void OnDisable()
        {
            input.Disable();
        }
#endif

        private void Update()
        {
            //Jump
            HandleJump();

            //JumpKeyDown
            HandleJumpKeyDown();

            //Drop
            HandleDrop();

            //JumpFromHang
            HandleJumpFromHang();
        }

        void HandleJump()
        {
#if inputsystem
            Jump = input.Parkour.Jump.inProgress;
#else
            Jump = Input.GetKey(jumpKey) || (String.IsNullOrEmpty(jumpButton) ? false : Input.GetButton(jumpButton));
#endif

        }

        void HandleJumpKeyDown()
        {

#if inputsystem
            JumpKeyDown = input.Parkour.Jump.WasPressedThisFrame();
#else
            JumpKeyDown = Input.GetKeyDown(jumpKey) || (String.IsNullOrEmpty(jumpButton) ? false : Input.GetButtonDown(jumpButton));
#endif

        }

        void HandleDrop()
        {
#if inputsystem
            Drop = input.Parkour.Drop.inProgress;
#else
            Drop = Input.GetKey(dropKey) || (String.IsNullOrEmpty(dropButton) ? false : Input.GetButton(dropButton));
#endif
        }

        void HandleJumpFromHang()
        {
#if inputsystem
            JumpFromHang = input.Parkour.JumpFromHang.inProgress;
#else
            JumpFromHang = Input.GetKey(jumpFromHangKey) || (String.IsNullOrEmpty(jumpFromHangButton) ? false : Input.GetButton(jumpFromHangButton));
#endif
        }


    }
}
