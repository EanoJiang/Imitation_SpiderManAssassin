using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace FS_GrapplingSystem
{
    public class Rope
    {
        [Tooltip("Specifies the thickness of the rope used for the grappling hook.")]
        public float ropeRadius = .025f;

        [Tooltip("The material applied to the rope for visual appearance.")]
        public Material ropeMaterial;

        [Tooltip("The speed at which the rope is thrown towards the target point.")]
        public float ropeThrowSpeed = 4f;

        [Tooltip("Determines the number of segments used to render the rope, affecting its smoothness.")]
        public int ropeResolution = 200;

        [Tooltip("The radius of the spiral motion when the rope is thrown.")]
        public float spiralRadius = 0.1f;

        [Tooltip("Controls the frequency of the spiral effect in the rope's motion.")]
        public float spiralFrequency = 5f;

        LineRenderer lineRenderer;

        private float speed = 0;
        private Vector3 startPoint;
        private Vector3 endPoint;
        private Vector3[] ropePositions;
        private bool isThrowingRope = false;
        private bool ropeReachedEndpoint = false;
        private float ropeLength;
        private float currentRopeLength;
        private Transform hookObject;

        Vector3 hookObjectDefaultPos;
        Quaternion hookObjectDefaultRot;
        Transform hookObjectDefaultParent;

        public Rope(float ropeRadius, Material ropeMaterial, float ropeThrowSpeed, int ropeResolution, float spiralRadius, float spiralFrequency, Transform transform, Transform hookObject)
        {
            this.ropeRadius = ropeRadius;
            this.ropeMaterial = ropeMaterial;
            this.ropeThrowSpeed = ropeThrowSpeed;
            this.ropeResolution = ropeResolution;
            this.spiralRadius = spiralRadius;
            this.spiralFrequency = spiralFrequency;
            this.hookObject = hookObject;
            if (hookObject != null)
            {
                hookObjectDefaultPos = hookObject.localPosition;
                hookObjectDefaultRot = hookObject.localRotation;
                hookObjectDefaultParent = hookObject.parent;
            }
            SetLineRenderer(transform);
        }

        public void ThrowRope(Transform start, Vector3 end)
        {
            if (hookObject != null)
                hookObject.parent = null;
            speed = ropeThrowSpeed * Vector3.Distance(start.position, end);
            startPoint = start.position;
            endPoint = end;
            ropeLength = Vector3.Distance(start.position, end);
            currentRopeLength = 0f;
            isThrowingRope = true;
            ropeReachedEndpoint = false;

            lineRenderer.positionCount = ropeResolution;
            ropePositions = new Vector3[ropeResolution];
            AnimateRope(start);
        }
        public void UpdateRopePull(Vector3 start)
        {
            lineRenderer.SetPosition(0, start);
        }
        public void RetractRope()
        {
            if (hookObject != null)
            {
                hookObject.parent = hookObjectDefaultParent;
                hookObject.localPosition = hookObjectDefaultPos;
                hookObject.localRotation = hookObjectDefaultRot;
            }
            isThrowingRope = false;
            ropeReachedEndpoint = false;
            lineRenderer.positionCount = 0;
        }
        void UpdateRopePositions()
        {
            for (int i = 0; i < ropeResolution; i++)
            {
                lineRenderer.SetPosition(i, ropePositions[i]);
            }
            if(hookObject != null)
                hookObject.position = ropePositions.Last();
        }
        async void AnimateRope(Transform start)
        {
            while (currentRopeLength < ropeLength && isThrowingRope)
            {
                startPoint = start.position;
                Vector3 ropeDirection = (endPoint - startPoint).normalized;
                Vector3 up = Vector3.up;
                Vector3 right = Vector3.Cross(ropeDirection, up).normalized;
                if (right == Vector3.zero)
                {
                    right = Vector3.Cross(ropeDirection, Vector3.forward).normalized;
                }
                currentRopeLength += speed * Time.deltaTime;
                currentRopeLength = Mathf.Min(currentRopeLength, ropeLength);
                float t = currentRopeLength / ropeLength;

                for (int i = 0; i < ropeResolution; i++)
                {
                    float segmentT = (float)i / (ropeResolution - 1);
                    Vector3 segmentPosition = Vector3.Lerp(startPoint, startPoint + ropeDirection * currentRopeLength, segmentT);

                    if (segmentT < 0.3f)
                    {
                        float spiralT = segmentT / 0.3f;
                        float spiralAngle = spiralT * spiralFrequency * Mathf.PI * 2;
                        Vector3 spiralOffset = right * Mathf.Cos(spiralAngle) + up * Mathf.Sin(spiralAngle);
                        float spiralScale = Mathf.Sin(spiralT * Mathf.PI);
                        segmentPosition += spiralOffset * spiralRadius * spiralScale * (1 - t);
                    }
                    else
                    {
                        float sinWave = Mathf.Sin((segmentT - 0.3f) * Mathf.PI / 0.7f) * (1 - t) * 0.3f;
                        segmentPosition += right * sinWave;
                    }

                    ropePositions[i] = segmentPosition;
                }

                UpdateRopePositions();

                if (currentRopeLength >= ropeLength)
                {
                    ropeReachedEndpoint = true;
                    StartPullRope();
                    break;
                }
                await Task.Yield();
            }
        }
        public bool HasRopeReachedEndpoint()
        {
            return ropeReachedEndpoint;
        }
        void StartPullRope()
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, endPoint);
        }

        public void SetLineRenderer(Transform transform)
        {
            var lineRendererObject = new GameObject("Rope");
            lineRendererObject.transform.parent = transform.parent;
            lineRenderer = lineRendererObject.AddComponent<LineRenderer>();
            lineRenderer.material = ropeMaterial;
            lineRenderer.numCapVertices = 1;
            lineRenderer.startWidth = ropeRadius;
            lineRenderer.endWidth = ropeRadius;
            lineRenderer.positionCount = 0;
        }
    }
}
