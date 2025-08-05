using UnityEngine;
using System.Collections;
using MalbersAnimations.Utilities;
using System.Collections.Generic;

namespace MalbersAnimations.Controller
{
    /// <summary>Simple Script to make damage anything with a stat</summary>
    public class MAttackTrigger : MonoBehaviour, IMHitLayer
    {
        /// <summary>ID of the Trigger. This is called on the Animator to wakes up the Trigger</summary>
        [Tooltip("ID of the Trigger. This is called on the Animator to wakes up the Trigger")]
        public int index = 1;

        [Tooltip("If the Target has a rigidbody then it will push it with that force")]
        public float PushForce = 10;

        [Tooltip("Collider used for the Interaction")]
        public Collider Trigger;
        
        /// <summary>When the Attack Trigger is Enabled, Affect your stat</summary>
        [/*Header("Stats "), */Tooltip("When the Attack Trigger is Enabled, Affect your stat")]
        public StatModifier SelfStatEnter;

        /// <summary>When the Attack Trigger is Disabled, Affect your stat</summary>
        [Tooltip("When the Attack Trigger is Disabled, Affect your stat")]
        public StatModifier SelfStatExit;


        /// <summary>When the Attack Trigger Enters an enemy, Affect his stats</summary>
        [Tooltip("When the Attack Trigger Enters an enemy, Affect his stats")]
        public StatModifier EnemyStatEnter;

        /// <summary>When the Attack Trigger Exits an enemy, Affect his stats</summary>
        [Tooltip("When the Attack Trigger Exits an enemy, Affect his stats")]
        public StatModifier EnemyStatExit;

        public bool debug = true;
        public Color DebugColor = new Color(1, 0.25f, 0, 0.15f);
        
        /// <summary>Enemy that can be Damaged </summary>
        private IMDamage enemy;
        ///// <summary>YourSelf that can be Damaged </summary>
        //private IMDamage  self;
        private IMHitLayer HitMaskOwner;

        [SerializeField] private LayerMask hitLayer = ~0;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;
        public LayerMask HitLayer
        {
            get { return hitLayer; }
            set { hitLayer = value; }
        }

        public QueryTriggerInteraction TriggerInteraction
        {
            get { return triggerInteraction; }
            set { triggerInteraction = value; }
        }

        protected Stats SelfStats;
        Transform CurrentAnimal;
        private Transform owner;
        protected List<Transform> AlreadyHitted = new List<Transform>();

        public Transform Owner
        {
            get { return owner; }
            set
            {
                owner = value;

                HitMaskOwner = Owner.GetComponentInChildren<IMHitLayer>();

                if (HitMaskOwner != null)
                {
                    HitLayer = HitMaskOwner.HitLayer;
                    TriggerInteraction = HitMaskOwner.TriggerInteraction;
                }

                SelfStats = Owner.GetComponentInParent<Stats>();
            }
        }

        void Awake()
        {
            Owner = transform.root; //Set which is the owner of this AttackTrigger

            if (!Trigger)
            {
                Trigger = GetComponentInChildren<Collider>(); ;
                Trigger.isTrigger = true;
            }
        }

        void OnEnable()
        {
            SelfStatEnter.ModifyStat(SelfStats);
            Trigger.enabled = true;
            AlreadyHitted = new List<Transform>();
        }
 

        void OnDisable()
        {
            SelfStatExit.ModifyStat(SelfStats);
            Trigger.enabled = false;
            AlreadyHitted = new List<Transform>();
        }

        void OnTriggerEnter(Collider other)
        {
            var Root = other.transform.root;
            if (other.isTrigger && TriggerInteraction == QueryTriggerInteraction.Ignore) return;                                                    //just collapse when is a collider what we are hitting
            if (!MalbersTools.Layer_in_LayerMask(other.gameObject.layer, HitLayer)) return;  //Just hit what is on the HitMask Layer

            if (Root == Owner.transform) return;                                            //Don't hit yourself;

            if (!AlreadyHitted.Find(item => item == Root))                                 //if the entering collider is not already on the list add it
            {
                AlreadyHitted.Add(Root);
            }

            if (Root == CurrentAnimal) return;                                    //if the animal is the same, do nothing we already done the logic below
            else
            {
                if (CurrentAnimal)
                {
                    AlreadyHitted = new List<Transform>();                            //Clean the colliders if you had a previus animal
                }

                CurrentAnimal = Root;                                              //Is a new Animal that is enetering the Attack Trigger

                var TargetPos = Root.position;

                var mesh = Root.GetComponentInChildren<Renderer>();
                if (mesh != null) TargetPos = mesh.bounds.center;                           //Get the mesh Bounds Center 

                Vector3 direction = (Owner.position - TargetPos).normalized;                //Calculate the direction of the attack

                var  interactable = other.transform.GetComponent<IInteractable>();           //Get the Animal on the Other collider

                if (interactable != null) interactable.Interact();

                enemy = Root.GetComponentInChildren<IMDamage>();                             //Get the Animal on the Other collider

                if (enemy != null)                                                          //if the other does'nt have the Damagable Interface dont send the Damagable stuff
                {
                    enemy.HitDirection = direction;
                    enemy.Damage();
                    EnemyStatEnter.ModifyStat(Root.GetComponentInChildren<Stats>());            //Affect Stats
                }
                else if (other.attachedRigidbody && PushForce != 0)        //If the other has a riggid body and it can be pushed
                {
                    other.attachedRigidbody.AddForce(-direction * PushForce, ForceMode.VelocityChange);
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.isTrigger) return;
            var Root = other.transform.root;

            if (Root != CurrentAnimal) return;                      //If is another animal exiting the zone SKIP

            if (AlreadyHitted.Find(item => item == Root))          //Remove the collider from the list that is exiting the zone.
            {
                AlreadyHitted.Remove(Root);
            }

            if (AlreadyHitted.Count == 0)                                        //When all the collides are removed from the list..
            {
                enemy = Root.GetComponentInChildren<IMDamage>();                     //Get the Animal on the Other collider

                if (enemy != null)                                                  //if the other does'nt have the animal script skip
                {
                  //  if (self == enemy) return;                                      //Don't Hit yourself
                    EnemyStatExit.ModifyStat(Root.GetComponentInChildren<Stats>());
                }
                CurrentAnimal = null;
            }
        }

        public virtual void SetOwner(Transform owner) { Owner = owner; }


#if UNITY_EDITOR
        void Reset()
        {
            SetOwner(transform.root);

            EnemyStatEnter = new StatModifier()
            {
                ID = MalbersTools.GetInstance<StatID>("Health"),
                modify = StatOption.SubstractValue,
                Value = new Scriptables.FloatReference() { UseConstant = true, ConstantValue = 10 },
            };

            Trigger = GetComponent<Collider>();
            if (!Trigger)  Trigger = gameObject.AddComponent<BoxCollider>();
            Trigger.isTrigger = true;
            Owner = transform.root; //Set which is the owner of this AttackTrigger
            this.enabled = false;
        }


        void OnDrawGizmos()
        {
            MalbersTools.DrawTriggers(transform, Trigger, DebugColor);
        }
#endif
    }
}

