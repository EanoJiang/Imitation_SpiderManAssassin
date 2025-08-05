using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations.Scriptables;
using UnityEngine.Events;
using MalbersAnimations.Utilities;

namespace MalbersAnimations.Controller
{
    /// <summary>When an animal Enter a Zone this will activate a new State or a new Mode </summary>
    public class Zone : MonoBehaviour, IWayPoint, IDestination
    {
        static Keyframe[] K = { new Keyframe(0, 0), new Keyframe(1, 1) };

        /// <summary>Set the Action Zone to Automatic</summary>
        public bool automatic;
        /// <summary>if Automatic is set to true this will be the time to disable temporarly the Trigger</summary>
        public float AutomaticDisabled = 10f;
        /// <summary>Use the Trigger for Heads only</summary>
        public bool HeadOnly;
        public string HeadName = "Head";


        public ZoneType zoneType = ZoneType.Mode;
        public ModeID modeID;
        public StateID stateID;
        public StanceID stanceID;
        public Action ActionID;
       // protected Mode PreMode;
        public IntReference ID;

        // protected Collider collider;

        /// <summary>Align the Animal entering to the Aling Point</summary>
        public bool Align;

        public Transform AlingPoint;
        public Transform AlingPoint2;
        public float AlignTime = 0.3f;
        public AnimationCurve AlignCurve = new AnimationCurve(K);
        public bool DoubleSided = true;

        public bool AlignPos = true, AlignRot = true, AlignLookAt = false;

        protected MAnimal CurrentAnimal;
        protected List<Collider> animal_Colliders = new List<Collider>();

        public float ActionDelay = 0;
        //public AnimalProxy animalProxy; //This is used to Get all the funtions of any animal that gets to the zone..

        public StatModifier StatModifier;

        public UnityEvent OnEnter = new UnityEvent();
        public UnityEvent OnExit = new UnityEvent();
        public UnityEvent OnZoneActivation = new UnityEvent();


        protected Collider ZoneCollider;
        protected Stats AnimalStats;

        #region AI
        [SerializeField]
        private List<Transform> nextTargets;
        public List<Transform> NextTargets
        {
            get { return nextTargets; }
            set { nextTargets = value; }
        }

        public Transform NextTarget
        {
            get
            {
                if (NextTargets.Count > 0)
                {
                    return NextTargets[UnityEngine.Random.Range(0, NextTargets.Count)];
                }
                return null;
            }
        }

        public WayPointType PointType
        {
            get { return pointType; }
        }


        public float WaitTime
        {
            get { return waitTime.RandomValue; }
        }

        [SerializeField]
        private FloatReference stoppingDistance = new FloatReference( 0.5f);
        public float StoppingDistance
        {
            get { return stoppingDistance; }
            set { stoppingDistance = value; }
        }
        #endregion

        [SerializeField, MinMaxRange(0, 60)]
        private RangedFloat waitTime = new RangedFloat(0, 5);
        public WayPointType pointType = WayPointType.Ground;

        /// <summary>Keep a Track of all the Zones on the Scene </summary>
        public static List<Zone> Zones;
 
        /// <summary>Retuns the ID of the Zone regarding the Type of Zone(State,Stance,Mode) </summary>
        public int GetID
        {
            get
            {
                switch (zoneType)
                {
                    case ZoneType.Mode:
                        return modeID;
                    case ZoneType.State:
                        return stateID;
                    case ZoneType.Stance:
                        return stateID;
                    default:
                        return 0;
                }
            }
        }

        /// <summary>Is the zone a Mode Zone</summary>
        public bool IsMode  { get { return zoneType == ZoneType.Mode; } }
      
        /// <summary>Is the zone a Mode Zone</summary>
        public bool IsState  { get { return zoneType == ZoneType.State; } }

        /// <summary>Is the zone a Mode Zone</summary>
        public bool IsStance { get { return zoneType == ZoneType.Stance; } }




        void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger) return;

            if (!MalbersTools.CollidersLayer(other, LayerMask.GetMask("Animal"))) return;           //Just accept animal layer only
            if (HeadOnly && !other.name.Contains(HeadName)) return;                                 //If is Head Only and no head was found Skip

            MAnimal newAnimal = other.GetComponentInParent<MAnimal>();                              //Get the animal on the entering collider

            if (!newAnimal) return;                                                                 //If there's no animal do nothing

            if (animal_Colliders.Find(coll => coll == other) == null)                               //if the entering collider is not already on the list add it
            {
                animal_Colliders.Add(other);
            }

            if (newAnimal == CurrentAnimal) return;                                    //if the animal is the same do nothing (when entering two animals on the same Zone)
            else
            {
                if (CurrentAnimal)
                {
                    animal_Colliders = new List<Collider>();                            //Clean the colliders
                }

                CurrentAnimal = newAnimal;                                             //Set a new Animal
                AnimalStats = CurrentAnimal.GetComponentInParent<Stats>();
                OnEnter.Invoke();
                ActivateZone();
            }
        }
        void OnTriggerExit(Collider other)
        {
            if (other.isTrigger) return;
            if (HeadOnly && !other.name.Contains(HeadName)) return;         //if is only set to head and there's no head SKIP

            MAnimal existing_animal = other.GetComponentInParent<MAnimal>();

            if (!existing_animal) return;                                            //If there's no animal script found skip all
            if (existing_animal != CurrentAnimal) return;                            //If is another animal exiting the zone SKIP

            if (animal_Colliders.Find(item => item == other))                       //Remove the collider from the list that is exiting the zone.
            {
                animal_Colliders.Remove(other);
            }

            if (animal_Colliders.Count == 0)                                        //When all the collides are removed from the list..
            {
                OnExit.Invoke();                                                   //Invoke On Exit when all animal's colliders has exited the Zone

                ResetStoredAnimal();
            }
        }


        /// <summary>Activate the Zone depending the Zone Type</summary>
        /// <param name="forced"></param>
        public virtual void ActivateZone(bool forced = false)
        {
            CurrentAnimal.IsOnZone = true;


            switch (zoneType)
            {
                case ZoneType.Mode:
                    ActivateModeZone(forced);
                    break;
                case ZoneType.State:
                    ActivateStateZone(); //State Zones does not require to be delay or prepared to be activated
                    break;
                case ZoneType.Stance:
                    ActivateStanceZone(); //State Zones does not require to be delay or prepared to be activated
                    break;
            }

        }

        public virtual void ResetStoredAnimal()
        {
            CurrentAnimal.IsOnZone = false;

            if (zoneType == ZoneType.Mode)
            {
                var PreMode = CurrentAnimal.Mode_Get(modeID);
                if (PreMode != null)
                {
                    PreMode.ResetAbilityIndex();
                    PreMode.GlobalProperties.OnEnter.RemoveListener(OnZONEActive);
                }
            }

            CurrentAnimal = null;

            animal_Colliders = new List<Collider>();                            //Clean the colliders
        }


        /// <summary>Enables the Zone using the State</summary>
        private void ActivateStateZone()
        {
            if (CurrentAnimal.State_TryActivate(stateID))
            {
                StatModifier.ModifyStat(AnimalStats);
                AlignAnimal();
                OnZoneActivation.Invoke();
            }
        }

        /// <summary>Enables the Zone using the Stance</summary>
        private void ActivateStanceZone()
        {
            CurrentAnimal.Stance = stanceID;
            CurrentAnimal.Mode_Stop();
            AlignAnimal();
            OnZoneActivation.Invoke();
        }


        private void ActivateModeZone(bool forced)
        {
            if (modeID == 4 && ActionID != null)
                ID.Value =  ActionID; //Use Action ID Instead

            if (forced || automatic)
            {
                CurrentAnimal.Mode_Activate((int)modeID, ID); //Current animal was empty  ??!?!??!
                OnZoneActivation.Invoke();

                if (automatic)
                {
                    StartCoroutine(ZoneColliderONOFF());
                }
            }
            else
            {  //In Case the Zone is not Automatic
                var PreMode = CurrentAnimal.Mode_Get(modeID);

                if (PreMode != null)
                {
                    PreMode.AbilityIndex = ID;
                    PreMode.GlobalProperties.OnEnter.AddListener(OnZONEActive);
                }
            }
        }


        void OnZONEActive()
        {
            AlignAnimal();
            StatModifier.ModifyStat(AnimalStats);
            OnZoneActivation.Invoke();
        }


        /// <summary>Aligns the Animal to the Align points of the Zone</summary>
        public virtual void AlignAnimal()
        {
            if (Align && AlingPoint)
            {
                IEnumerator ICo = null;

                if (AlignLookAt)
                {
                    ICo = MalbersTools.AlignLookAtTransform(CurrentAnimal.transform, AlingPoint, AlignTime, AlignCurve);                //Align Look At the Zone
                    StartCoroutine(ICo);
                }
                else
                {
                    if (AlignPos)
                    {
                        Vector3 AlingPosition = AlingPoint.position;

                        if (AlingPoint2)                //In case there's a line ... move to the closest point between the two transforms
                        {
                            AlingPosition = MalbersTools.ClosestPointOnLine(AlingPoint.position, AlingPoint2.position, CurrentAnimal.transform.position);
                        }

                        if (DoubleSided)
                        {
                            Vector3 AlingPosOpposite = transform.InverseTransformPoint(AlingPosition);
                            AlingPosOpposite.z *= -1;
                            AlingPosOpposite = transform.TransformPoint(AlingPosOpposite);

                            var Distance1 = Vector3.Distance(CurrentAnimal.transform.position, AlingPosition);
                            var Distance2 = Vector3.Distance(CurrentAnimal.transform.position, AlingPosOpposite);

                            if (Distance2 < Distance1)
                            {
                                StartCoroutine(MalbersTools.AlignTransform_Position(CurrentAnimal.transform, AlingPosOpposite, AlignTime, AlignCurve));
                            }
                            else
                            {
                                StartCoroutine(MalbersTools.AlignTransform_Position(CurrentAnimal.transform, AlingPosition, AlignTime, AlignCurve));
                            }
                        }
                        else
                        {
                            StartCoroutine(MalbersTools.AlignTransform_Position(CurrentAnimal.transform, AlingPosition, AlignTime, AlignCurve));
                        }
                    }
                    if (AlignRot)
                    {

                        Quaternion Side1 = AlingPoint.rotation;
                        Quaternion AnimalRot = CurrentAnimal.transform.rotation;

                        if (DoubleSided)
                        {
                            Quaternion Side2 = AlingPoint.rotation * Quaternion.Euler(0, 180, 0);

                            var Side1Angle = Quaternion.Angle(AnimalRot, Side1);
                            var Side2Angle = Quaternion.Angle(AnimalRot, Side2);

                            StartCoroutine(MalbersTools.AlignTransform_Rotation(CurrentAnimal.transform, Side1Angle < Side2Angle ? Side1 : Side2, AlignTime, AlignCurve));
                        }
                        else
                            StartCoroutine(MalbersTools.AlignTransform_Rotation(CurrentAnimal.transform, Side1, AlignTime, AlignCurve));
                    }
                }
            }
        }

        /// <summary> Destroy the Zone after x Time</summary>
        public virtual void Zone_Destroy(float time)
        {
            if (time == 0)
                Destroy(gameObject);
            else
            {
                Destroy(gameObject, time);
            }
        }

        /// <summary> Enable Disable the Zone COllider for and X time</summary>
        IEnumerator ZoneColliderONOFF() //For Automatic only 
        {
            yield return null;

            if (AutomaticDisabled > 0)
            {
                ZoneCollider.enabled = false;
                CurrentAnimal.ActiveMode.ResetAbilityIndex();       //Reset the Ability Index when Set to automatic and the Collider is off
                yield return new WaitForSeconds(AutomaticDisabled);
                ZoneCollider.enabled = true;
            }
            CurrentAnimal = null;                           //clean animal
            animal_Colliders = new List<Collider>();        //Reset Colliders
            yield return null;
        }

        void OnEnable()
        {
            if (Zones == null) Zones = new List<Zone>();
            ZoneCollider = GetComponent<Collider>();                                   //Get the reference for the collider
            Zones.Add(this);                                                  //Save the the Action Zones on the global Action Zone list
        }
        void OnDisable()
        {
            Zones.Remove(this);                                              //Remove the the Action Zones on the global Action Zone list
        }


        //public virtual void _SetAction(Action id)
        //{
        //    CurrentAnimal.SetAction(id);
        //}

        //public virtual void _SetStance(StanceID id)
        //{
        //    CurrentAnimal.Stance = id;
        //}

        //public virtual void _SetState(StateID id)
        //{
        //    CurrentAnimal.State_Activate(id);
        //}




#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (AlingPoint && AlingPoint2)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(AlingPoint.position, AlingPoint2.position);
                Gizmos.DrawCube(AlingPoint.position, Vector3.one * 0.05f);
                Gizmos.DrawCube(AlingPoint2.position, Vector3.one * 0.05f);

                if (DoubleSided)
                {
                    var AlingPoint1Opp =  transform.InverseTransformPoint(AlingPoint.position);
                    var AlingPoint2Opp =  transform.InverseTransformPoint(AlingPoint2.position);

                    AlingPoint1Opp.z *= -1;
                    AlingPoint2Opp.z *= -1;
                    AlingPoint1Opp = transform.TransformPoint(AlingPoint1Opp);
                    AlingPoint2Opp = transform.TransformPoint(AlingPoint2Opp);

                    Gizmos.DrawLine(AlingPoint1Opp, AlingPoint2Opp);

                    Gizmos.DrawCube(AlingPoint1Opp, Vector3.one * 0.05f);
                    Gizmos.DrawCube(AlingPoint2Opp, Vector3.one * 0.05f);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (EditorAI)
            {
                UnityEditor.Handles.color = Color.red;
                UnityEditor.Handles.DrawWireDisc(transform.position, transform.up, StoppingDistance);

                Gizmos.color = Color.green;
                if (nextTargets != null)
                    foreach (var item in nextTargets)
                    {
                        if (item) Gizmos.DrawLine(transform.position, item.position);
                    }
            }
        }

        //public void Interact()
        //{            ActivateZone();        }
#endif


        [HideInInspector] public bool EditorShowEvents = true;
        [HideInInspector] public bool EditorAI = true;


    }

    public enum ZoneType
    {
        Mode,
        State,
        Stance
    }
}
