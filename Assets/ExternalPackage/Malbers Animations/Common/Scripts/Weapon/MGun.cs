using UnityEngine;
using UnityEngine.Events;
using MalbersAnimations.Events;

using System.Collections;

namespace MalbersAnimations.Weapons
{
    public class MGun : MWeapon, IGun
    {
        public int Ammo;                             //Total of Ammo for this weapon
        public int ammoInChamber;                    //Total of Ammo in the Chamber
        public int ClipSize;                         //Max Capacity of the Ammo in once hit
        public bool isAutomatic = false;             //Press Fire one or continues 
        //public float fireRate = 0.5f;
        public GameObject bulletHole;
        public float BulletHoleTime = 10;
        public Vector3Event OnFire;
        public UnityEvent OnReload;
        public TransformEvent OnHit;
        public BoolEvent OnAiming;
        protected bool isAiming;

        public int TotalAmmo
        {
            get { return Ammo; }
            set { Ammo = value; }
        }

        public int AmmoInChamber
        {
            get { return ammoInChamber; }
            set { ammoInChamber = value; }
        }

        public bool IsAutomatic
        {
            get { return isAutomatic; }
            set { isAutomatic = value; }
        }

        public bool IsAiming
        {
            get { return isAiming; }
            set
            {
                if (isAiming != value)
                {
                    isAiming = value;
                    OnAiming.Invoke(IsAiming);
                }
            }
        }

        //public float FireRate
        //{
        //    get { return fireRate; }
        //    set { fireRate = value; }
        //}

        /// <summary>
        /// Set the total Ammo (Refill when you got some ammo)
        /// </summary>
        /// <param name="value"></param>
        public void SetTotalAmmo(int value)
        {
            Ammo = value;
        }

        void Awake()
        {
            InitializeWeapon();
        }

        public virtual void ReduceAmmo(int amount)
        {
            ammoInChamber -= amount;
        }

        public virtual void FireProyectile(RaycastHit AimRay)
        {
            float percent = Random.Range(0, 1);

            Vector3 AimDirection = (AimRay.point - transform.position).normalized;
            ReduceAmmo(1);

            OnFire.Invoke(AimDirection);

            if (AimRay.transform)                                                                                       //If the AIM Ray hit something 
            {
                var interactable = AimRay.transform.GetComponent<IInteractable>();
                interactable?.Interact();

                AffectStat.Value = Random.Range(MinDamage, MaxDamage);
                AffectStat.ModifyStat(AimRay.transform.GetComponentInParent<Stats>());

                Damager.SetDamage(AimRay.normal, AimRay.transform);               

                if (AimRay.rigidbody)                                                                                       //If the thing we hit has a rigidbody
                {
                    AimRay.rigidbody.AddForceAtPosition(AimDirection * Mathf.Lerp(MinForce, MaxForce, percent), AimRay.point);                                //Apply the force to it
                }

                BulletHole(AimRay);

                OnHit.Invoke(AimRay.transform);  //Invoke OnHitSomething Event
            }
        }

        public virtual bool Reload()
        {
            if (Ammo == 0) return false;
            int AmmoSpent = ClipSize - ammoInChamber;                         //Ammo Spent from the Chamber
            if (AmmoSpent == 0) return false;                                   //If there's no ammo spent exit

            int AmmoLeft = TotalAmmo - AmmoSpent;                               //Ammo Remaining

            if (AmmoLeft >= 0)                                                  //If is there any Ammo 
            {
                ammoInChamber += AmmoSpent;
                TotalAmmo = AmmoLeft;
            }
            else
            {
                ammoInChamber += TotalAmmo;                                          //Set in the Chamber the remaining ammo  
                TotalAmmo = 0;                                                  //Empty the Total Ammo
            }
            OnReload.Invoke();
            return true;
        }

        public virtual void BulletHole(RaycastHit hit)
        {
            if (!bulletHole) return;

            //For Good Instantiation on the object (Scaled or not)
            Vector3 NewScale = hit.transform.lossyScale;

            NewScale.x = 1f / Mathf.Max(NewScale.x, 0.0001f);
            NewScale.y = 1f / Mathf.Max(NewScale.y, 0.0001f);
            NewScale.z = 1f / Mathf.Max(NewScale.z, 0.0001f);

            GameObject Hlper = new GameObject();
            Hlper.transform.parent = hit.collider.transform;
            Hlper.transform.localScale = NewScale;
            Hlper.transform.localPosition = hit.collider.transform.InverseTransformPoint(hit.point);
            Hlper.transform.localRotation = Quaternion.identity;


            GameObject hole = Instantiate(bulletHole);

            hole.transform.parent = Hlper.transform;
            hole.transform.localScale = Vector3.one;
            hole.transform.localPosition = Vector3.zero;
            hole.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            hole.transform.localRotation = hole.transform.localRotation * Quaternion.Euler(0, Random.Range(-90, 90), 0);


            Destroy(Hlper, BulletHoleTime);
        }

        public override void ResetWeapon()  { }
    }
}