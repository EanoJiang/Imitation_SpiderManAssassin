using UnityEngine;

/// <summary>Malbers Aislated classes to be used on other Scripts </summary>
namespace MalbersAnimations.Controller
{
    [System.Serializable]
    public class MPivots
    {
        /// <summary>Name of the Pivot</summary>
        public string name = "Pivot";
        public Vector3 position = Vector3.up;
        public float multiplier = 1;
        public int interval = 1;

        [HideInInspector] public bool EditorModify = false;
        /// <summary>Hit to Store all the RayCast Values </summary>
        public RaycastHit hit;

        public MPivots(string name, Vector3 pos, float mult)
        {
            this.name = name;
            position = pos;
            multiplier = mult;
        }

        /// <summary>Returns the World position of the Pivot </summary>
        public Vector3 World(Transform t)
        {
            return t.TransformPoint(position);
        }
    }
}
