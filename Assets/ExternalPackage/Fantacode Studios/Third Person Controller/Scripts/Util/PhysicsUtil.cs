using System.Collections.Generic;
using UnityEngine;
namespace FS_ThirdPerson
{
    public class PhysicsUtil
    {
        public static bool ThreeRayCasts(Vector3 origin, Vector3 dir, float spacing, Vector3 right, out List<RaycastHit> hitInfo,
            float distance, LayerMask layer, bool allHit = true, bool debugDraw = false)
        {

            bool centerHitFound = Physics.Raycast(origin, dir,
                out RaycastHit centerHit, distance, layer);

            bool leftHitFound = Physics.Raycast(origin - right * spacing, dir,
                out RaycastHit leftHit, distance, layer);

            bool rightHitFound = Physics.Raycast(origin + right * spacing, dir,
                out RaycastHit rightHit, distance, layer);

            hitInfo = new List<RaycastHit>() { centerHit, leftHit, rightHit };

            bool hitFound = (allHit) ? centerHitFound && leftHitFound && rightHitFound :
                centerHitFound || leftHitFound || rightHitFound;

            if (debugDraw && hitFound)
            {
                Debug.DrawLine(origin, centerHit.point, (centerHitFound) ? Color.red : Color.white);
                Debug.DrawLine(origin - right * spacing, leftHit.point, (leftHitFound) ? Color.red : Color.white);
                Debug.DrawLine(origin + right * spacing, rightHit.point, (rightHitFound) ? Color.red : Color.white);
            }

            return hitFound;
        }

        public static Vector3 ValidatePosition(RaycastHit hit, float padding, LayerMask obstacleLayer)
        {
            Vector3 targetPos = Vector3.zero;
            if (hit.transform != null)
            {
                bool spaceFrontFound = Physics.Raycast(hit.point + hit.transform.forward * padding + Vector3.up * 0.2f, Vector3.down, out RaycastHit spaceFrontHit, 0.3f, obstacleLayer);
                bool spaceBackFound = Physics.Raycast(hit.point - hit.transform.forward * padding + Vector3.up * 0.2f, Vector3.down, out RaycastHit spaceBackHit, 0.3f, obstacleLayer);
                bool spaceRightFound = Physics.Raycast(hit.point + hit.transform.right * padding + Vector3.up * 0.2f, Vector3.down, out RaycastHit spaceRightHit, 0.3f, obstacleLayer);
                bool spaceLeftFound = Physics.Raycast(hit.point - hit.transform.right * padding + Vector3.up * 0.2f, Vector3.down, out RaycastHit spaceLeftHit, 0.3f, obstacleLayer);

                if (!spaceRightFound && !spaceLeftFound)
                    targetPos.x = hit.collider.bounds.center.x;
                else if (!spaceRightFound)
                    targetPos = hit.point - (hit.transform.right * padding);
                else if (!spaceLeftFound)
                    targetPos = hit.point + (hit.transform.right * padding);

                if (!spaceFrontFound && !spaceBackFound)
                    targetPos.z = hit.collider.bounds.center.z;
                else if (!spaceFrontFound)
                    targetPos = hit.point - hit.transform.forward * padding;
                else if (!spaceBackFound)
                    targetPos = hit.point + hit.transform.forward * padding;

                if (hit.transform != null)
                {
                    Debug.DrawLine(hit.point + hit.transform.forward * padding + Vector3.up * 0.2f, hit.point + hit.transform.forward * padding, Color.black);
                    //if (hit.transform != null)
                    Debug.DrawLine(hit.point - hit.transform.forward * padding + Vector3.up * 0.2f, hit.point - hit.transform.forward * padding, Color.black);
                    //if (hit.transform != null)
                    Debug.DrawLine(hit.point + hit.transform.right * padding + Vector3.up * 0.2f, hit.point + hit.transform.right * padding, Color.black);
                    //if (hit.transform != null)
                    Debug.DrawLine(hit.point - hit.transform.right * padding + Vector3.up * 0.2f, hit.point - hit.transform.right * padding, Color.black);
                }
            }
            return targetPos;
        }
    }
}