using System.Collections.Generic;
using UnityEngine;
namespace FS_ThirdPerson
{
    public class JoystickHelper : MonoBehaviour
    {
        public static JoystickHelper i { get; private set; }
        private void Awake()
        {
            i = this;
        }

        Dictionary<string, bool> axisStates = new Dictionary<string, bool>();

        public bool GetAxisDown(string axisName)
        {
            bool currentlyPressed = Mathf.Abs(Input.GetAxisRaw(axisName)) > 0.1f;

            if (!axisStates.ContainsKey(axisName))
                axisStates.Add(axisName, false);

            bool previouslyPressed = axisStates[axisName];
            axisStates[axisName] = currentlyPressed;

            if (!previouslyPressed && currentlyPressed)
                return true;

            return false;
        }
    }
}