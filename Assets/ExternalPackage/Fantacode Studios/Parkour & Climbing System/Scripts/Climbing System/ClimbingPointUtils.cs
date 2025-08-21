using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace FS_ParkourSystem
{

    public static class ClimbingPointUtils
    {
        private static List<ClimbPoint> pointsOnASingleLedge;
        private static int pointCount;
        public static int totalPointCount;
#if UNITY_EDITOR
        public async static void BakePoints(GameObject obj, float distanceBetweenPoints, bool bothSide, float progress)
        {
            distanceBetweenPoints = Mathf.Max(0.05f, distanceBetweenPoints);
            pointsOnASingleLedge = new List<ClimbPoint>();
            var curRot = obj.transform.eulerAngles;
            obj.transform.eulerAngles = Vector3.zero;


            float xLength = Mathf.Abs(obj.GetComponent<MeshRenderer>().bounds.min.x - obj.GetComponent<MeshRenderer>().bounds.max.x);
            float yLength = Mathf.Abs(obj.GetComponent<MeshRenderer>().bounds.min.y - obj.GetComponent<MeshRenderer>().bounds.max.y);
            float zLength = Mathf.Abs(obj.GetComponent<MeshRenderer>().bounds.min.z - obj.GetComponent<MeshRenderer>().bounds.max.z);

            float x = obj.GetComponent<MeshRenderer>().bounds.center.x;
            float y = obj.GetComponent<MeshRenderer>().bounds.center.y;
            float z = obj.GetComponent<MeshRenderer>().bounds.center.z;



            var objSize = new Vector3(xLength, yLength, zLength);

            

            Vector3 startPoint = Vector3.zero;
            float firstPoint = 0;
            pointCount = 0;
            bool hasSpaceForPoint = false;

            if (xLength > yLength && xLength > zLength)
            {
                if (xLength > distanceBetweenPoints * 1.5f)
                {
                    firstPoint = x + (xLength - distanceBetweenPoints) / 2;
                    hasSpaceForPoint = true;
                }
                else
                    firstPoint = x;
                startPoint = new Vector3(firstPoint, y, z);
                CreatePoint(obj, bothSide, startPoint,objSize);
                if (hasSpaceForPoint)
                    for (float i = 1; distanceBetweenPoints * i < xLength - distanceBetweenPoints / 2; i++)
                    {
                        EditorUtility.DisplayProgressBar("Generating Points", $"Creating climbpoints on ledge - {obj.name}", ((distanceBetweenPoints * i) / (xLength - distanceBetweenPoints / 2)) * (progress == 0 ? 1f : .5f) + progress);
                        if (totalPointCount == 500)
                        {
                            await Task.Delay((int)(Time.deltaTime * 1000.0f));
                            totalPointCount = 0;
                        }
                        CreatePoint(obj, bothSide, new Vector3(firstPoint - distanceBetweenPoints * i, y, z), objSize);
                    }
                EditorUtility.ClearProgressBar();
            }
            else if (yLength > xLength && yLength > zLength)
            {
                if (yLength > distanceBetweenPoints * 1.5f)
                {
                    firstPoint = y + (yLength - distanceBetweenPoints) / 2;
                    hasSpaceForPoint = true;
                }
                else
                    firstPoint = y;
                startPoint = new Vector3(x, firstPoint, z);
                CreatePoint(obj, bothSide, startPoint, objSize);
                if (hasSpaceForPoint)
                    for (float i = 1; distanceBetweenPoints * i < yLength - distanceBetweenPoints / 2; i++)
                    {
                        EditorUtility.DisplayProgressBar("Generating Points", $"Creating climbpoints on ledge - {obj.name}", ((distanceBetweenPoints * i) / (yLength - distanceBetweenPoints / 2)) * (progress == 0 ? 1f : .5f) + progress);
                        if (totalPointCount == 500)
                        {
                            await Task.Delay((int)(Time.deltaTime * 1000.0f));
                            totalPointCount = 0;
                        }
                        CreatePoint(obj, bothSide, new Vector3(x, firstPoint - distanceBetweenPoints * i, z), objSize);
                    }
                EditorUtility.ClearProgressBar();
            }
            else
            {
                if (zLength > distanceBetweenPoints * 1.5f)
                {
                    firstPoint = z + (zLength - distanceBetweenPoints) / 2;
                    hasSpaceForPoint = true;
                }
                else
                    firstPoint = z;
                startPoint = new Vector3(x, y, firstPoint);
                CreatePoint(obj, bothSide, startPoint, objSize, obj.transform.right);
                if (hasSpaceForPoint)
                    for (float i = 1; distanceBetweenPoints * i < zLength - distanceBetweenPoints / 2; i++)
                    {
                        EditorUtility.DisplayProgressBar("Generating Points", $"Creating climbpoints on ledge - {obj.name}", ((distanceBetweenPoints * i) / (zLength - distanceBetweenPoints / 2)) * (progress == 0 ? 1f : .5f) + progress);
                        if (totalPointCount == 500)
                        {
                            await Task.Delay((int)(Time.deltaTime * 1000.0f));
                            totalPointCount = 0;
                        }

                        CreatePoint(obj, bothSide, new Vector3(x, y, firstPoint - distanceBetweenPoints * i), objSize, obj.transform.right);
                    }
                EditorUtility.ClearProgressBar();
            }
            obj.transform.eulerAngles = curRot;
        }

        public async static void ClearPoints(List<GameObject> objs)
        {
            ClimbingPointUtils.totalPointCount = 0;

            foreach (var obj in objs)
            {
                var points = obj.GetComponentsInChildren<ClimbPoint>().ToList();
                for (int i = 0; i < points.Count; i++)
                {
                    if (points[i].gameObject != obj)
                    {
                        EditorUtility.DisplayProgressBar("Clearing Points", $"Clearing climbpoints on the ledge - {obj.name}", (float)i / points.Count);
                        if (ClimbingPointUtils.totalPointCount == 1000)
                        {
                            await Task.Delay((int)(Time.deltaTime * 1000.0f));
                            ClimbingPointUtils.totalPointCount = 0;
                        }
                        Undo.DestroyObjectImmediate(points[i].gameObject);
                        ClimbingPointUtils.totalPointCount++;
                    }
                }
            }
            EditorUtility.ClearProgressBar();
        }


        static void CreatePoint(GameObject parent, bool bothSides, Vector3 pos, Vector3 objSize = new Vector3(), Vector3 dir = new Vector3())
        {
            pointCount++;
            totalPointCount++;
            var pointObj = new GameObject();
            pointObj.name = $"Climb Point {pointCount}";
            pointObj.transform.SetParent(parent.transform);
            pointObj.transform.position = pos;
            if (dir != Vector3.zero)
                pointObj.transform.forward = dir;
            var forward = pointObj.transform.forward;
            var up = pointObj.transform.up;
            pointObj.transform.position += new Vector3(forward.x * objSize.x/2, forward.y * objSize.y/2, forward.z * objSize.z/2);
            pointObj.transform.position += new Vector3(up.x * objSize.x/2, up.y * objSize.y/2, up.z * objSize.z/2);
            var climbPoint = pointObj.AddComponent<ClimbPoint>();
            climbPoint.enabled = false;
            if (bothSides)
            {
                pointCount++;
                totalPointCount++;
                pointObj = new GameObject();
                pointObj.name = $"Climb Point {pointCount}";
                pointObj.transform.SetParent(parent.transform);
                pointObj.transform.position = pos;
                if (dir != Vector3.zero)
                    pointObj.transform.forward = dir;
                forward = -pointObj.transform.forward;
                pointObj.transform.forward = -pointObj.transform.forward;
                up = pointObj.transform.up;
                pointObj.transform.position += new Vector3(forward.x * objSize.x / 2, forward.y * objSize.y / 2, forward.z * objSize.z / 2);
                pointObj.transform.position += new Vector3(up.x * objSize.x / 2, up.y * objSize.y / 2, up.z * objSize.z / 2);
                climbPoint = pointObj.AddComponent<ClimbPoint>();
                climbPoint.enabled = false;
            }
            //SetConnectionsOnLedge(climbPoint);
            Undo.RegisterCreatedObjectUndo(pointObj, "New point created");
        }

        static void SetConnectionsOnLedge(ClimbPoint climbPoint)
        {
            if (pointsOnASingleLedge.Count > 0)
            {
                var p1 = pointsOnASingleLedge.Last().gameObject.transform.position;
                var p2 = climbPoint.gameObject.transform.position;
                var x = GetDirection(p1.x, p2.x);
                var y = GetDirection(p2.y, p1.y);
                var direction = new Vector2(x, y);
                pointsOnASingleLedge.Last().CreateConnection(climbPoint, direction, ConnectionType.Move);
            }
            pointsOnASingleLedge.Add(climbPoint);
        }

        static float GetDirection(float p1, float p2)
        {
            if (p1 - p2 == 0)
                return 0;
            return p1 - p2 > 0 ? 1 : -1;
        }

        public static void ConnectTwoPoints(ConnectionType connectionType)
        {
            var objects = Selection.gameObjects.ToList();
            if (objects.Count == 2)
            {
                try
                {
                    CreateConnection(objects[0], objects[1], connectionType);
                    CreateConnection(objects[1], objects[0], connectionType);
                }
                catch { }
            }
        }

        static void CreateConnection(GameObject point1, GameObject point2, ConnectionType connectionType)
        {
            var p1 = point1.GetComponent<ClimbPoint>().transform;
            var p2 = point2.GetComponent<ClimbPoint>().transform;
            var x = GetDirection(p1.position.x, p2.position.x);
            var y = GetDirection(p2.position.y, p1.position.y);

            Vector3 point2orgin = (p2.position - p1.position);

            var projectedP2 = Vector3.ProjectOnPlane(point2orgin, p1.forward);

            float angle = Mathf.Abs(Vector3.SignedAngle(projectedP2, p1.right, p1.forward));
            if (x != 0)
                if ((angle < 22.5f || angle > 67.5f) && (angle < 112.5 || angle > 157.5) && y != 0)
                    x = 0;

            var direction = new Vector2(x, y);

            Undo.RegisterCompleteObjectUndo(point1.GetComponent<ClimbPoint>(), "Connection Created");
            point1.GetComponent<ClimbPoint>().CreateConnection(point2.GetComponent<ClimbPoint>(), direction, connectionType, false);
        }
#endif
    }
}

