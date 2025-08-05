using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MalbersAnimations.Utilities;
using UnityEngine.Events;
using MalbersAnimations.Events;
using System;
using UnityEngine.AI;

namespace MalbersAnimations.Controller
{
    public class MAnimalAIControl : MonoBehaviour , IAIControl
    {
        [HideInInspector] public bool showevents;

        #region Components References
        private NavMeshAgent agent;                 //The NavMeshAgent
        protected MAnimal animal;                    //The Animal Script
        #endregion


        #region Internal Variables
        /// <summary>The way to know if there no Target Position vector to go to</summary>
        protected static Vector3 NullVector = MalbersTools.NullVector;
        /// <summary>Target Last Position (Useful to know if the Target is moving)</summary>
        protected Vector3 TargetLastPosition = NullVector;
        /// <summary>Stores the Remainin distance to the Target's Position</summary>
        public float RemainingDistance { get; private set; }
        /// <summary>Stores the Remainin distance to the Target's Position</summary>
        protected float DefaultStopDistance;
        /// <summary>Used to Check if you enter once on a OffMeshLink</summary>
        public bool EnterOFFMESH { get; private set; }
        /// <summary>Is the Animal stopped by an external source like Public Function Stop or Mount AI</summary>       
        public bool Stopped { get; private set; }
       
        ///// <summary>Sometimes OffMesh Links can be travelled by flying/Swiming Underwater</summary>         
        //private bool isFreeMovementOffMesh;


        internal IWayPoint NextWayPoint;
        private IEnumerator I_WaitToNextTarget;
        private IEnumerator IFlyOffMesh;
        #endregion

        #region Public Variables
        [SerializeField] protected float stoppingDistance = 0.6f;
        [SerializeField] protected float walkDistance = 1f;
        private int defaultGroundIndex;
        [SerializeField] protected Transform target;                    //The Target
        private Transform nextTarget;
        public bool debug = false;                          //Debuging 
        #endregion

        /// <summary>is the Animal, Flying, swimming ?</summary>
        public bool FreeMove { get; private set; }

        /// <summary>Is the Animal Playing a mode</summary>
        public bool IsOnMode { get; private set; }

        /// <summary>Has the animal Arrived to their current destination</summary>
        public bool HasArrived { get; set; }

        public bool IsFlyingOffMesh { get; private set; }
        public Zone IsZone { get; private set; }
        /// <summary>Is the Target a WayPoint?</summary>
        public MWayPoint IsWayPoint { get; private set; }

        #region Events
        [Space]
            public Vector3Event OnTargetPositionArrived = new Vector3Event();
            public TransformEvent OnTargetArrived = new TransformEvent();
            public UnityEvent OnActionStart = new UnityEvent();
            public UnityEvent OnActionEnd = new UnityEvent();
            public StringEvent OnDebug = new StringEvent();
            #endregion


        #region Properties

     
        /// <summary>Reference of the Nav Mesh Agent</summary>
        public NavMeshAgent Agent
        {
            get
            {
                if (agent == null)
                {
                    agent = GetComponentInChildren<NavMeshAgent>();
                }
                return agent;
            }
        }

        /// <summary>Stopping Distance of the Next Waypoint</summary>
        public float StoppingDistance
        {
            get { return stoppingDistance; }

            set { Agent.stoppingDistance = stoppingDistance = value; }
        }


        /// <summary>is the Target transform moving??</summary>
        public bool TargetisMoving
        {
            get
            {
                if (target != null)
                {
                    return (target.position - TargetLastPosition).magnitude > 0.001f;
                }
                return false;
            }
        }

        /// <summary>The Agent is Active and we are on a NavMesh</summary>
        public bool AgentActive
        {
            get { return Agent.isOnNavMesh && Agent.enabled; }
        }

 
        /// <summary> Is the Animal waiting x time to go to the Next waypoint</summary>
        public bool IsWaiting { get; private set; }

        public Vector3 TargetPosition { get; private set; }
     
        public Transform NextTarget { get; set; }
         

        #endregion

        #region Unity Functions

        void Awake()
        {
            animal = GetComponent<MAnimal>();
        }

        void Start() { StartAgent(); }
        void OnEnable()
        {
            // animal.OnStateChange.AddListener(OnState);           //Listen when the Animations changes..
            animal.OnModeChange.AddListener(OnMode);           //Listen when the Animations changes..
            animal.OnGrounded.AddListener(OnGrounded);           //Listen when the Animations changes..
           // animal.OnStateChange.AddListener(OnStateChanged);           //Listen when the Animations changes..
        }

        void OnDisable()
        {
            //animal.OnStateChange.RemoveListener(OnState);           //Listen when the Animations changes..
            animal.OnModeChange.RemoveListener(OnMode);           //Listen when the Animations changes..
            animal.OnGrounded.RemoveListener(OnGrounded);           //Listen when the Animations changes..
          //  animal.OnStateChange.RemoveListener(OnStateChanged);           //Listen when the Animations changes..
        }

        void Update() { Updating(); }
        #endregion

        protected virtual void StartAgent()
        {
            Agent.updateRotation = false;                                       //The Animal will control the rotation . NOT THE AGENT
            Agent.updatePosition = false;                                       //The Animal will control the  postion . NOT THE AGENT
            DefaultStopDistance = StoppingDistance;                             //Store the Started Stopping Distance
            Agent.stoppingDistance = StoppingDistance;
            SetTarget(target);                                                  //Set the first Target (IMPORTANT)  it also set the next future targets

            IsWaiting = false;

            var locomotion =  animal.GetState(StateEnum.Locomotion);

            if (locomotion != null && locomotion.SpeedSet != null)
            {
                defaultGroundIndex = locomotion.SpeedSet.StartVerticalIndex;
            }
        }

        protected virtual void Updating()
        {
            if (IsFlyingOffMesh) return;                                    //Do nothing while is flying ofmesh

            Agent.nextPosition = agent.transform.position;                  //Update the Agent Position to the Transform position  

            if (FreeMove)
            {
               //Debug.Log("FreeMovement");
                FreeMovement();
            }
            else if (AgentActive)                                               //if we are on a NAV MESH onGround
            {
                if (IsWaiting || IsOnMode)
                {
                   // Debug.Log("isWaiting");
                    return;    //If the Animal is Waiting do nothing . .... he is doing something else... wait until he's finish
                }
                else if (TargetPosition == NullVector)                    //if there's no Position to go to.. Stop the Agent
                {
                   // Debug.Log("Stopped TargetPosition == NullVector");
                    Stop();
                }
                else
                {
                   // Debug.Log("UpdateAgent");
                    UpdateAgent();
                }
            }
            else if (Stopped)
            {
               // Debug.Log("Stopped");

                if (TargetisMoving)
                {
                    Stopped = false; //Means the Target Moved so Try movin again
                    ResumeAgent();
                }
            } 

            if (target)
            {
                if (TargetisMoving)
                {
                    UpdateTargetTransform();
                  //  Debug.Log("Target is moving");
                }

                TargetLastPosition = target.position;
            }
        }

        public virtual void SetTarget(GameObject target)
        {
            SetTarget(target.transform);
        }


        /// <summary>Set the next Target</summary>   
        public virtual void SetTarget(Transform target)
        {
            IsWaiting = false;
            animal.Mode_Interrupt();             //In Case it was making any Mode;
            Stopped = false;
            this.target = target;

           // Debug.Log("targetPosition: " + targetPosition.ToString("F2"));

            if (target == null)
            {
                Stop();
            }
            else
            {
                TargetPosition = target.position;                           //Update the Target Position 

                IsZone = target.GetComponent<Zone>(); 

                IsWayPoint = target.GetComponent<MWayPoint>();
                NextWayPoint = target.GetComponent<IWayPoint>();            //Check if the Next Target has Next Waypoints



                StoppingDistance =  DefaultStopDistance;                    //Reset the Stoppping Distance

                if (NextWayPoint != null)
                {
                    StoppingDistance = NextWayPoint.StoppingDistance;
                    NextTarget = NextWayPoint.NextTarget;
                }
               // StoppingDistance = NextWayPoint != null ? NextWayPoint.StoppingDistance : DefaultStopDistance;  //Set the Next Stopping Distance

                CheckAirTarget();

                Debuging(name + ": is travelling to : " + target.name);

                ResumeAgent();
            }
        }

     

        /// <summary>Set the next Destination Position without having a target</summary>   
        public virtual void SetDestination(Vector3 PositionTarget)
        {
            IsWaiting = false;
            animal.Mode_Interrupt();             //In Case it was making any Mode;
            Stopped = false;

            StoppingDistance = DefaultStopDistance;                    //Reset the Stoppping Distance

            Debuging(name + ": is travelling to : " + PositionTarget);

            IsZone = null;
            IsWayPoint = null;
            NextWayPoint = null;

            if (I_WaitToNextTarget != null) StopCoroutine(I_WaitToNextTarget);  //if there's a coroutine active then stop it

            TargetPosition = PositionTarget;                           //Update the Target Position 
            ResumeAgent();
        }


        /// <summary> Check if the Next Target is a Air Target</summary>
        private void CheckAirTarget()
        {
            if (NextWayPoint != null && NextWayPoint.PointType == WayPointType.Air)    //If the animal can fly, there's a new wayPoint & is on the Air
            {
                 Debuging(name + ": NextTarget is AIR" + NextTarget.name);
                animal.State_Activate(StateEnum.Fly);
                FreeMove = true;
            }
        }


        /// <summary> Stop the Agent on the Animal... also remove the Transform target and the Target Position and Stops the Animal </summary>
        public virtual void Stop()
        {
            if (Agent && Agent.isOnNavMesh) Agent.isStopped = true;

            TargetPosition = NullVector;

            animal.Mode_Interrupt();
            animal.StopMoving();
            animal.MovementAxis = Vector3.zero;
            IsWaiting = false;
            Stopped = true;
        }

        protected virtual void FreeMovement()
        {
            if (IsWaiting) return;
            if (target == null || TargetPosition == NullVector) return; //If we have no were to go then Skip the code

            RemainingDistance = target ? Vector3.Distance(animal.transform.position, target.position) : 0;

            var Direction = (target.position - animal.transform.position).normalized;

            animal.Move(Direction);

           // Debug.DrawRay(animal.transform.position, Direction.normalized, Color.white);

            if (RemainingDistance < StoppingDistance)   //We arrived to our destination
            { 
                Arrive_Destination();
            }
        }

        protected virtual void CheckOffMeshLinks()
        {
            if (Agent.isOnOffMeshLink && !EnterOFFMESH)                         //Check if the Agent is on a OFF MESH LINK
            {
                EnterOFFMESH = true;                                            //Just to avoid entering here again while we are on a OFF MESH LINK
                OffMeshLinkData OMLData = Agent.currentOffMeshLinkData;

                if (OMLData.linkType == OffMeshLinkType.LinkTypeManual)              //Means that it has a OffMesh Link component
                {
                    OffMeshLink CurrentOML = OMLData.offMeshLink;                    //Check if the OffMeshLink is a Manually placed  Link

                    Zone IsOffMeshZone =
                        CurrentOML.GetComponentInParent<Zone>();                     //Search if the OFFMESH IS An ACTION ZONE (EXAMPLE CRAWL)

                    if (IsOffMeshZone)                                               //if the OffmeshLink is a zone and is not making an action
                    {
                        IsOffMeshZone.ActivateZone(true);                            //Activate the Zone
                        return;
                    }


                    var DistanceEnd = (transform.position - CurrentOML.endTransform.position).sqrMagnitude;
                    var DistanceStart = (transform.position - CurrentOML.startTransform.position).sqrMagnitude;


                        //Debug.Log("OMMESH FLY");

                    if (CurrentOML.CompareTag("Fly"))
                    {
                        var FarTransform = DistanceEnd > DistanceStart ? CurrentOML.endTransform : CurrentOML.startTransform;
                        //Debug.Log("OMMESH FLY");

                        FlyOffMesh(FarTransform);
                    }
                    else
                    if (CurrentOML.area == 2)  //2 is Off mesh Jump
                    {
                        var NearTransform = DistanceEnd < DistanceStart ? CurrentOML.endTransform : CurrentOML.startTransform;
                        StartCoroutine(MalbersTools.AlignTransform_Rotation(transform, NearTransform.rotation, 0.15f));         //Aling the Animal to the Link Position
                        animal.State_Activate(2); //2 is Jump State                                                              //if the OffMesh Link is a Jump type
                    }
                }
                else if (OMLData.linkType == OffMeshLinkType.LinkTypeJumpAcross)             //Means that it has a OffMesh Link component
                {
                    animal.State_Activate(2); //2 is Jump State
                }
            }
        }


        /// <summary> Updates the Agents using he animation root motion </summary>
        protected virtual void UpdateAgent()
        {
            if (Agent.pathPending) { return; } //Means is still calculating the path to go
           
           

           RemainingDistance = Agent.remainingDistance;                //Store the remaining distance -- but if navMeshAgent is still looking for a path Keep Moving

            if (RemainingDistance < StoppingDistance)                   //if We Arrive to the Destination
            {
                OnTargetPositionArrived.Invoke(TargetPosition);         //Invoke the Event On Target Position Arrived

                if (target)
                {
                    OnTargetArrived.Invoke(target);                     //Invoke the Event On Target Arrived
                    if (IsWayPoint) IsWayPoint.TargetArrived(this);     //Send that the Animal has Arrived
                }

                TargetPosition = NullVector;                            //Reset the TargetPosition

                agent.isStopped = true;                                 //Stop the Agent

               // Debug.Log("Arrive_Destination");

                Arrive_Destination();
            }
            else
            {
             //  Debug.Log("Moving");
                animal.Move(Agent.desiredVelocity);                                     //Set the Movement to the Animal
              

                if (animal.Grounded)
                {
                    if (walkDistance > RemainingDistance)
                    {
                        animal.CurrentSpeedIndex = 1;
                    }
                    else
                    {
                       animal.CurrentSpeedIndex = defaultGroundIndex;
                    }
                }

                CheckOffMeshLinks();                                        //Jump/Fall behaviour 
            }
        }


        /// <summary>Check when the Animal changes the Grounded State</summary>
        protected virtual void OnGrounded(bool grounded)
        {
            if (grounded)
            {
                Agent.enabled = true;

                ResetFlyingOffMesh();

                EnterOFFMESH = false;
                FreeMove = false;

                if (TargetPosition != NullVector)
                {
                   // Debuging("SetDestination Agent");
                    ResumeAgent();
                }
            }
            else
            {
                Agent.isStopped = true;
                Agent.ResetPath();
                Agent.CompleteOffMeshLink();
                Agent.enabled = false;
                animal.DeltaAngle = 0;
            }
        }

        private void ResetFlyingOffMesh()
        {
            if (IFlyOffMesh != null)
            {
                IsFlyingOffMesh = false;
                StopCoroutine(IFlyOffMesh);
                IFlyOffMesh = null;
            }
        }

        /// <summary>Check the Status of the Next Target</summary>
        private void Arrive_Destination()
        {
            if (NextWayPoint != null)
            {
                if (NextWayPoint.PointType == WayPointType.Ground)
                {
                    FreeMove = false; //if the next waypoing is on the Ground then set the freemovement to false
                }

                if (IsZone && IsZone.IsMode)                     //If the Target is an Moce|Action Zone Start the Animation
                {
                    Debuging(name + ": Is on a Mode Zone");

                    IsZone.ActivateZone(true);

                    if (IsZone.WaitTime > 0)  // Wait time for the Zone which means it can be interrupted
                    {
                        SetNextTarget();
                    }
                    return;
                }
              
                SetNextTarget();  //State Zones and Stance Zones does not need wait for the Zone Animation to be finished
            }
        }

        /// <summary>Resume the Agent component</summary>
        private void ResumeAgent()
        {
            if (!Agent.isOnNavMesh) return;                             //No nothing if we are not on a Nav mesh or the Agent is disabled
            Agent.SetDestination(TargetPosition);                       //If there's a position to go to set it as destination
            Agent.isStopped = false;                                    //Start the Agent again
        }

        /// <summary>Called when the Animal Enter an Action, Attack, Damage or something similar</summary>
        private void OnMode(int ModeID)
        {
            IsOnMode = ModeID != 0;

            if (IsOnMode) animal.StopMoving();     //Means the Animal is making a Mode
            //else
            //{
            //    Debuging(name +": has finished the Mode");

            //    //if (!IsWaiting) //means is not waiting on the coroutine
            //    //{
            //    //    SetNextTarget();
            //    //}
            //}
        }

        /// <summary>Use this for Targets that changes their position</summary>
        protected virtual void UpdateTargetTransform()
        {
            if (target == null) return;             //If there's no target Skip the code
            TargetPosition = target.position;       //Update the Target Position 
            ResumeAgent();
        }


        /// <summary>Set the Target from  on the NextTargets Stored on the Waypoints or Zones</summary>
        protected void SetNextTarget()
        {
            if (I_WaitToNextTarget != null) StopCoroutine(I_WaitToNextTarget); //if there's a coroutine active then stop it

            if (NextTarget == null)
            {
                animal.StopMoving();
                return;
            }
            I_WaitToNextTarget = C_WaitToNextTarget(NextWayPoint.WaitTime, NextTarget);
            StartCoroutine(I_WaitToNextTarget);
        }

        protected void FlyOffMesh(Transform target)
        {
            ResetFlyingOffMesh();

            IFlyOffMesh = C_FlyOffMesh(target);
            StartCoroutine(IFlyOffMesh);
        }


        protected IEnumerator C_WaitToNextTarget(float time, Transform NextTarget)
        {
            if (time > 0)
            {
                IsWaiting = true;
                Debuging(name + ": is waiting " + time.ToString("F2") + " seconds");
                animal.StopMoving();  
                yield return new WaitForSeconds(time);
            }

            SetTarget(NextTarget);

            yield return null;
        }


        internal IEnumerator C_FlyOffMesh(Transform target)
        {
            animal.State_Activate(6); //Set the State to Fly
            IsFlyingOffMesh = true;
            float distance = float.MaxValue;

            while (distance > StoppingDistance)
            {
                animal.Move((target.position - animal.transform.position).normalized);
                distance = Vector3.Distance(animal.transform.position, target.position);
                yield return null;
            }
            animal.ActiveState.AllowExit();
            if (debug) Debug.Log(name+ ": Exit Fly State Off Mesh");
            IsFlyingOffMesh = false;
        }

        protected void Debuging(string Log)
        {
            if (debug) Debug.Log(Log);

            OnDebug.Invoke(Log);
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!debug) return;

            if (Agent == null) { return; }
            if (Agent.path == null) { return; }

            Color lGUIColor = Gizmos.color;

            Gizmos.color = Color.green;
            for (int i = 1; i < Agent.path.corners.Length; i++)
            {
                Gizmos.DrawLine(Agent.path.corners[i - 1], Agent.path.corners[i]);
            }

            Vector3 pos = Agent ? Agent.transform.position : transform.position;

            if (Agent)
            {
                UnityEditor.Handles.color = Color.red;
                UnityEditor.Handles.DrawWireDisc(pos, Vector3.up, StoppingDistance);

                UnityEditor.Handles.color = Color.green;
                UnityEditor.Handles.DrawWireDisc(pos, Vector3.up, walkDistance);
            }
        }
#endif
    }
}