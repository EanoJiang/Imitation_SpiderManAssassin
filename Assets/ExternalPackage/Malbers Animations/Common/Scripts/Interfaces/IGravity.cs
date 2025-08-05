using UnityEngine;
namespace MalbersAnimations
{

    public interface IGravity
    {
        Vector3 GravityDirection { get; set; }
        Vector3 UpVector { get; }
    }
}