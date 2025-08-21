using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace FS_ParkourSystem
{
    public partial class ClimbPoint : MonoBehaviour
    {
        public bool useManualOptions = false;
        [SerializeField] bool mountPoint;
        public float handSpacing;
        public bool hasOwner { get; set; }

        List<Neighbour> neighbours = new List<Neighbour>();

        private void Awake()
        {
            this.enabled = false;
        }

        public Neighbour GetNeighbour(Vector2 direction)
        {
            Neighbour neighbour = null;

            //if (direction.y != 0)
            neighbour = neighbours.FirstOrDefault(n => n.direction.y == direction.y && n.direction.x == direction.x);

            //if (direction.x != 0 && neighbour == null)
            //    neighbour = neighbours.FirstOrDefault(n => n.direction.x == direction.x);

            return neighbour;
        }

        public void CreateConnection(ClimbPoint point, Vector2 direction, ConnectionType connectionType,
            bool isBothWay = true)
        {
            if (neighbours.Count(n => n.point == point && n.direction == direction) > 0)
                return;

            var newNeigbour = new Neighbour()
            {
                point = point,
                direction = direction,
                connectionType = connectionType,
                isBothWay = isBothWay
            };
            neighbours.Add(newNeigbour);
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {

            if ((Camera.current.transform.position - transform.position).sqrMagnitude < 225f)
            {
                Gizmos.color = new Vector4(1, 1, 0f, 0.6f);
                Gizmos.DrawSphere(transform.position, 0.02f);
                Gizmos.color = Color.green;
                DrawArrow(transform.position, transform.rotation * new Vector3(0, 0, 0.15f));
            }
        }
        public static void DrawArrow(Vector3 pos, Vector3 dir)
        {
            Gizmos.DrawRay(pos, dir);

            Gizmos.DrawRay(pos + dir, Quaternion.Euler(0, 45, 0) * -(dir / 5));
            Gizmos.DrawRay(pos + dir, Quaternion.Euler(0, -45, 0) * -(dir / 5));
        }
#endif

        public List<Neighbour> Neighbours => neighbours;
        public bool MountPoint { get { return mountPoint; } set { mountPoint = value; } }
    }

    [Serializable]
    public class Neighbour
    {
        public ClimbPoint point;
        public Vector2 direction;
        public ConnectionType connectionType;
        public bool isBothWay = true;
    }

    public enum ConnectionType { Move, Jump, None }
}