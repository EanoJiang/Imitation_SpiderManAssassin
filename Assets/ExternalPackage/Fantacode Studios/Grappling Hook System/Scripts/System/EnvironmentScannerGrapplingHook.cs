using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS_ThirdPerson
{
    public partial class EnvironmentScanner : MonoBehaviour
    {
        [field: SerializeField] public LayerMask GrapplingLedgeLayer { get; set; } = 1;

        //Vector3 boxHalfExtents = new Vector3(0.75f, 0.75f, 0.01f);
        Vector3 spaceCheckExtendTop = new Vector3(0.4f, 1f, 0.4f);
        Vector3 pathCheckExtend = new Vector3(0.05f, .1f, 0.01f);
        Vector3 spaceCheckExtendFront = new Vector3(0.05f, .4f, 0.05f);

        float capsuleRadius = 1.5f; 
        float minimumHookAngle = 5;

        public GrapplingData GetGrapplingLedgeData(float maxDistance, float minDistance, Transform ropeHoldPoint, bool debug)
        {
            List<Vector3> detectedHookPoints = new List<Vector3>();
            Vector3 direction = Camera.main.transform.forward + transform.up * .25f;
            //direction = Vector3.up;
            var checkOrigin = transform.position + transform.up * .75f + direction.normalized * capsuleRadius * 2;
            bool hasLedge = false;
            //Vector3 halfExtends = boxHalfExtents;
            float radius = capsuleRadius;
            float coveredDistance = 0f;
            float totalCastLength = 0f;
            float singleCastDist = 5f;


            Vector3 hookPos = Vector3.zero;
            Vector3 dir = Vector3.zero;
            float distancToHookPos = 0;
            var disToOrigin = Vector3.Distance(checkOrigin, ropeHoldPoint.position);

            for (float i = 0; totalCastLength < maxDistance - disToOrigin; i++)
            {
                var castLength = (i + 1) * singleCastDist + radius * 2;
                

                var hits = Physics.SphereCastAll(
                   checkOrigin + direction.normalized * coveredDistance,
                   radius,
                   direction,
                   castLength,
                   GrapplingLedgeLayer);

                hasLedge = hits.Length > 0;

                //GizmosExtend.drawSphereCast(
                //   checkOrigin + direction.normalized * coveredDistance,
                //   radius,
                //   direction,
                //   castLength,
                //   Color.red);

                if (hasLedge)
                {
                    foreach (var hit in hits)
                    {
                        if (hit.point == Vector3.zero)
                            continue;
                        RaycastHit heightHit;
                        var heightHitting = Physics.SphereCast(hit.point + Vector3.up, .1f, Vector3.down, out heightHit, 1.5f);
                        //GizmosExtend.drawSphereCast(hit.point + Vector3.up, .1f, Vector3.down, 1.5f, Color.blue);
                        hasLedge = heightHitting;

                        if (heightHitting)
                        {
                            hookPos = heightHit.point;
                            var o = transform.position + transform.up * .6f;
                            var distance = (o - hookPos).magnitude;
                            var hasPath = !Physics.BoxCast(
                                    o,
                                    pathCheckExtend,
                                    (hookPos - o).normalized,
                                    out RaycastHit pathHit,
                                    Quaternion.LookRotation(hookPos),
                                    distance - .5f,
                                    ObstacleLayer
                                );
                            hasLedge = hasPath;
                            if (debug)
                            {
                                BoxCastDebug.DrawBoxCastBox(
                                       o,
                                       pathCheckExtend,
                                       Quaternion.LookRotation(hookPos),
                                       (hookPos - o).normalized,
                                       distance - .5f,
                                       Color.green
                                   );
                            }
                        }

                        var hp = hookPos;
                        hp.y = transform.position.y;
                        dir = (hp - transform.position).normalized;



                        if (hasLedge && dir != Vector3.zero)
                        {
                            Vector3 origin = hookPos - dir * .1f + Vector3.up * .1f;
                            float offset = 0.25f;
                            Vector3 rightOffset = Vector3.Cross(Vector3.up, dir).normalized * offset;
                            Vector3 leftOffset = -rightOffset;

                            // Original BoxCast
                            bool isHitFront = Physics.CheckBox(origin, spaceCheckExtendFront, Quaternion.LookRotation(dir));

                            // Right BoxCast
                            Vector3 rightOrigin = origin + rightOffset;
                            bool isHitRight = Physics.CheckBox(rightOrigin, spaceCheckExtendFront, Quaternion.LookRotation(dir));

                            // Left BoxCast
                            Vector3 leftOrigin = origin + leftOffset;
                            bool isHitLeft = Physics.CheckBox(leftOrigin, spaceCheckExtendFront, Quaternion.LookRotation(dir));
                            dir.y = 0;
                            if (debug)
                            {
                                BoxCastDebug.DrawBoxCastBox(origin, spaceCheckExtendFront, Quaternion.LookRotation(dir), dir, 0f, Color.blue);
                                BoxCastDebug.DrawBoxCastBox(rightOrigin, spaceCheckExtendFront, Quaternion.LookRotation(dir), dir, 0f, Color.blue);
                                BoxCastDebug.DrawBoxCastBox(leftOrigin, spaceCheckExtendFront, Quaternion.LookRotation(dir), dir, 0f, Color.blue);
                            }
                            hasLedge = !isHitFront || (!isHitLeft && !isHitRight);

                            if (hasLedge)
                            {
                                origin = hookPos + Vector3.up * (spaceCheckExtendTop.y + .2f);
                                distancToHookPos = (ropeHoldPoint.position - hookPos).magnitude;
                                hasLedge = !Physics.CheckBox(origin, spaceCheckExtendTop, Quaternion.LookRotation(dir)) && distancToHookPos > minDistance && distancToHookPos <= maxDistance;
                                dir.y = 0;
                                if (debug)
                                    BoxCastDebug.DrawBoxCastBox(origin, spaceCheckExtendTop, Quaternion.LookRotation(dir), dir, 0f, hasLedge? Color.yellow:Color.red);
                                if (hasLedge)
                                {
                                    detectedHookPoints.Add(hookPos);
                                }
                            }
                        }
                    }
                }
                coveredDistance += castLength - radius;
                totalCastLength += castLength + 2 * radius;
               
                radius += capsuleRadius * 1.5f;
            }
            if (detectedHookPoints.Count > 0)
            {
                hookPos = GetAccurateHookPosition(checkOrigin, direction, detectedHookPoints);
                var hp = hookPos;
                distancToHookPos = (ropeHoldPoint.position - hookPos).magnitude;
                hp.y = transform.position.y;
                dir = (hp - transform.position).normalized;
            }


            GrapplingData data = new GrapplingData()
            {
                hasLedge = detectedHookPoints.Count > 0,
                hookPosition = hookPos,
                forwardDirection = dir,
                directionToHook = (hookPos - transform.position).normalized,
                distance = distancToHookPos
            };
           

            return data;
        }


        Vector3 GetAccurateHookPosition(Vector3 origin,Vector3 direction, List<Vector3> hookPositions)
        {
            Vector3 hookPos = hookPositions.First();
            float previousAngle = 400;
            Vector3 bestPos = hookPos;
            foreach (var hp in hookPositions)
            {
                var dir = (hp - origin).normalized;
                var angle = Vector3.Angle(direction.normalized,dir);
                if (angle < minimumHookAngle)
                    return hp;
                else if (angle < previousAngle)
                    bestPos = hp;
                previousAngle = angle;
            }

            return bestPos;
        }
    }

    public class GrapplingData
    {
        public Vector3 hookPosition;
        public bool hasLedge;
        public Vector3 directionToHook;
        public Vector3 forwardDirection;
        public float distance;
    }
}