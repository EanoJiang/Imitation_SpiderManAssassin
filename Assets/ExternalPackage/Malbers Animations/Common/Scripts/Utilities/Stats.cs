using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations.Scriptables;
using UnityEngine.Events;
using MalbersAnimations.Events;
using System;

namespace MalbersAnimations
{
    public class Stats : MonoBehaviour
    {
        /// <summary>List of Stats</summary>
        public List<Stat> stats = new List<Stat>();
        /// <summary>List of Stats Converted to Dictionary</summary>
        public Dictionary<int, Stat> stats_D;

        public Stat PinnedStat;

        private void Start()
        {
            StopAllCoroutines();

            stats_D = new Dictionary<int, Stat>();

            foreach (var stat in stats)
            {
                stat.InitializeStat(this);
                stats_D.Add(stat.ID, stat);
            }
        }


        private void OnDisable() { StopAllCoroutines(); }
   

        public virtual void _UpdateStats()
        {
            foreach (var stat in stats)
                stat.UpdateStat();
        }

        public virtual void _EnableStat(StatID iD)
        {
            PinnedStat = GetStat(iD);

            if (PinnedStat != null)
            {
                PinnedStat.Active = true;
            }
        }

        public virtual void _DisableStat(StatID iD)
        {
            PinnedStat = GetStat(iD);

            if (PinnedStat != null)
            {
                PinnedStat.Active = false;
            }
        }


        public virtual void DegenerateOff(StatID ID)
        {
            GetStat(ID);
            if (PinnedStat != null)
            {
                PinnedStat.Degenerate = false;
            }
        }

        public virtual void DegenerateOn(StatID ID)
        {
            GetStat(ID);
            if (PinnedStat != null)
            {
                PinnedStat.Degenerate = true;
            }
        }

        public virtual void _PinStat(string name) { GetStat(name); }

        public virtual void _PinStat(int ID) { GetStat(ID); }

        public virtual void _PinStat(StatID ID) { GetStat(ID); }

        public virtual Stat GetStat(string name)
        {
            PinnedStat = stats.Find(item => item.Name == name);
            return PinnedStat;
        }

        public virtual Stat GetStat(int ID)
        {
            PinnedStat = null;
            if (stats_D.TryGetValue(ID, out PinnedStat))
            {
                return PinnedStat;
            }
            return PinnedStat;
        }

        public virtual Stat GetStat(IntVar ID)
        {
            return GetStat(ID.Value);
        }

        public virtual Stat GetStat(StatID ID)
        {
            return GetStat(ID.ID);
        }



        /// <summary>Modify Stat Value instantly (Add/Remove to the Value)</summary>
        public virtual void _PinStatModifyValue(float value)
        {
            if (PinnedStat != null)
                PinnedStat.Modify(value);
            else Debug.Log("There's no Pinned Stat");
        }

        /// <summary>Modify Stat Value instantly (Add/Remove to the Value)</summary>
        public virtual void _PinStatModifyValue(FloatVar value)
        {
            if (PinnedStat != null)
                PinnedStat.Modify(value.Value);
            else Debug.Log("There's no Pinned Stat");
        }

        /// <summary>Modify Stat Value in a X time period(Add/Remove to the Value)</summary>
        public virtual void _PinStatModifyValue(float value, float time)
        {
            if (PinnedStat != null)
                PinnedStat.Modify(value, time);
            else Debug.Log("There's no Pinned Stat");
        }

        /// <summary>Modify Stat Value in 1 second period(Add/Remove to the Value)</summary>
        public virtual void _PinStatModifyValue1Sec(float value)
        {
            if (PinnedStat != null)
                PinnedStat.Modify(value, 1);
            else Debug.Log("There's no Pinned Stat");
        }

        /// <summary>Set  Stat Value to a fixed Value</summary>
        public virtual void _PinStatSetValue(float value)
        {
            if (PinnedStat != null)
                PinnedStat.Value = value;
            else Debug.Log("There's no Pinned Stat");
        }


        /// <summary>Modify the Pinned Stat MAX Value (Add or remove to the Max Value) </summary>
        public virtual void _PinStatModifyMaxValue(float value)
        {
            if (PinnedStat != null)
                PinnedStat.ModifyMAX(value);
            else Debug.Log("There's no Pinned Stat");
        }

        /// <summary>Set the Pinned Stat MAX Value </summary>
        public virtual void _PinStatSetMaxValue(float value)
        {
            if (PinnedStat != null)
                PinnedStat.MaxValue = value;
            else Debug.Log("There's no Pinned Stat");
        }

        /// <summary> Enable/Disable the Pinned Stat Regeneration Rate </summary>
        public virtual void _PinStatModifyRegenerationRate(float value)
        {
            if (PinnedStat != null)
                PinnedStat.ModifyRegenerationRate(value);
            else
                Debug.Log("There's no Active Stat or the Stat you are trying to modify does not exist");
        }


        /// <summary> Enable/Disable the Pinned Stat Degeneration </summary>
        public virtual void _PinStatDegenerate(bool value)
        {
            if (PinnedStat != null)
            {
                PinnedStat.Degenerate = value;
            }
            else
            {
                Debug.Log("There's no Active Stat or the Stat you are trying to modify does not exist");
            }
        }


        /// <summary>Enable/Disable the Pinned Stat Regeneration </summary>
        public virtual void _PinStatRegenerate(bool value)
        {
            if (PinnedStat != null)
            {
                PinnedStat.Regenerate = value;
            }
            else
                Debug.Log("There's no Active Stat or the Stat you are trying to modify does not exist");
        }

        /// <summary> Enable/Disable the Pinned Stat</summary>
        public virtual void _PinStatEnable(bool value)
        {
            if (PinnedStat != null)
            {
                PinnedStat.Active = value;
            }
            else
                Debug.Log("There's no Active Stat or the Stat you are trying to modify does not exist");
        }

        /// <summary>Modify the Pinned Stat value with a 'new Value',  'ticks' times , every 'timeBetweenTicks' seconds</summary>
        public virtual void _PinStatModifyValue(float newValue, int ticks, float timeBetweenTicks)
        {
            if (PinnedStat != null)
            {
                PinnedStat.Modify(newValue, ticks, timeBetweenTicks);
            }
            else
                Debug.Log("There's no Active Stat or the Stat you are trying to modify does not exist");
        }

        /// <summary> Clean the Pinned Stat from All Regeneration/Degeneration and Modify Tick Values </summary>
        public virtual void _PinStatCLEAN()
        {
            if (PinnedStat != null)
            {
                PinnedStat.CleanRoutines();
            }
            else
                Debug.Log("There's no Active Stat or the Stat you are trying to modify does not exist");
        }

    }


    ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    ///STAT CLASS
    ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    [Serializable]
    public class Stat
    {
        #region Variables
        ///// <summary> Name of the Stat </summary>
        //public string name;

        /// <summary> Is the Stat Active? </summary>
        [SerializeField] private bool active = true;
        /// <summary> ID of the Stat</summary>
        public StatID ID;
        /// <summary> Value and Default/RestoreValue of the Stat</summary>
        [SerializeField] private FloatReference value = new FloatReference(0);
        /// <summary> Restore Value of the Stat  </summary>
        [SerializeField] private FloatReference maxValue = new FloatReference(100);
        /// <summary>Min Value of the Stat</summary>
        [SerializeField] private FloatReference minValue = new FloatReference(0);
        /// <summary>Can the Stat regenerate overtime</summary>
        [SerializeField] private bool regenerate = false;
        /// <summary>Regeneration Rate. Change the Speed of the Regeneration</summary>
        public FloatReference RegenRate;
        /// <summary>Regeneration Rate. When the value is modified this will increase or decrease it over time.</summary>
        public FloatReference RegenWaitTime;
        /// <summary>Can the Stat degenerate overtime</summary>
        [SerializeField] private bool degenerate = false;
        /// <summary>Degeneration Rate. Change the Speed of the Degeneration</summary>
        public FloatReference DegenRate;
        public FloatReference InmuneTime;
        public ResetTo resetTo = ResetTo.MaxVlaue;

        private bool isBelow = false;
        private bool isAbove = false;
        private float StoredTime = 0;
        #endregion

        #region Events
        public bool ShowEvents = false;
        public UnityEvent OnStatFull = new UnityEvent();
        public UnityEvent OnStatEmpty = new UnityEvent();
        public float Below;
        public float Above;
        public UnityEvent OnStatBelow = new UnityEvent();
        public UnityEvent OnStatAbove = new UnityEvent();
        public FloatEvent OnValueChangeNormalized = new FloatEvent();
        public FloatEvent OnValueChange = new FloatEvent();
        public BoolEvent OnDegenereate = new BoolEvent();
        #endregion

        #region Properties
        /// <summary>Is the Stat Enabled? when Disable no modification can be done. All current modification can't be stopped</summary>
        public bool Active
        {
            get { return active; }
            set
            {
                active = value;
                if (value)
                    StartRegeneration(); //If the Stat was activated start the regeneration
                else
                    StopRegeneration();
            }
        }


        public string Name
        {
            get
            {
                if (ID != null)
                {
                    return ID.name;
                }
                return string.Empty;
            }
        }

        /// <summary> Current value of the Stat</summary>
        public float Value
        {
            get { return value; }
            set
            {
                if (!Active) return;                //If the  Stat is not Active do nothing

                if (this.value.Value != value)      //Check the code below only if the value has changed
                {
                    SetValue(value);
                }
            }
        }

        private void SetValue(float value)
        {
            this.value.Value = value;

            if (this.value <= minValue.Value)
            {
                this.value = minValue.Value;
                OnStatEmpty.Invoke();   //if the Value is 0 invoke Empty Stat
            }
            else if (this.value >= maxValue.Value)
            {
                this.value = maxValue.Value;
                OnStatFull.Invoke();    //if the Value is 0 invoke Empty Stat
            }


            OnValueChangeNormalized.Invoke(this.value / MaxValue);
            OnValueChange.Invoke(this.value);

            if (this.value > Above && !isAbove)
            {
                OnStatAbove.Invoke();
                isAbove = true;
                isBelow = false;
            }
            else if (this.value < Below && !isBelow)
            {
                OnStatBelow.Invoke();
                isBelow = true;
                isAbove = false;
            }
        }

        /// <summary>Maximum Value of the Stat</summary>
        public float MaxValue
        {
            get { return maxValue; }
            set { maxValue.Value = value; }
        }

        /// <summary>Minimun Value of the Stat </summary>
        public float MinValue
        {
            get { return minValue; }
            set { minValue.Value = value; }
        }
        
        /// <summary>Can the Stat Regenerate over time</summary>
        public bool Regenerate
        {
            get { return regenerate; }
            set
            {
                regenerate = value;
                Regenerate_LastValue = regenerate;           //In case Regenerate is changed 
                StartRegeneration();
            }
        }

        /// <summary> Can the Stat Degenerate over time </summary>
        public bool Degenerate
        {
            get { return degenerate; }
            set
            {
                if (degenerate != value)        //If the Values are diferent then...
                {
                    degenerate = value;
                    OnDegenereate.Invoke(value);

                    if (degenerate)
                    {
                        regenerate = false;     //Do not Regenerate if we are Degenerating
                        StartDegeneration();
                        StopRegeneration();
                    }
                    else
                    {
                        regenerate = Regenerate_LastValue;   //If we are no longer Degenerating Start Regenerating again
                        StopDegeneration();
                        StartRegeneration();
                    }
                }
            }
        }

        #endregion


        bool Regenerate_LastValue;
        internal void InitializeStat(MonoBehaviour holder)
        {
            isAbove = isBelow = false;
            Coroutine = holder;

            if (value.Value > Above) isAbove = true;        //This means that The Stat Value is over the Above value
            else if (value.Value < Below) isBelow = true;   //This means that The Stat Value is under the Below value

            Regenerate_LastValue = Regenerate;

            if (MaxValue < Value)
            {
                MaxValue = Value;
            }

            I_Regeneration = null;
            I_Degeneration = null;
            I_ModifyPerTicks = null;

            //        Debug.Log(Name + " MAX: " + MaxValue + "Val: " + Value);

            OnValueChangeNormalized.Invoke(Value / MaxValue);
            OnValueChange.Invoke(Value);

            if (Active)
            {
                StartRegeneration();
            }
        }


        /// <summary>Adds or remove to the Stat Value </summary>
        public virtual void Modify(float newValue)
        {
            if (TryModifyValue())
            {
                Value += newValue;
                StartRegeneration();
            }
        }

        public virtual void UpdateStat()
        {
            SetValue(value);
            StartRegeneration();
        }


        /// <summary>Adds or remove to the Stat Value</summary>
        public virtual void Modify(float newValue, float time)
        {
            if (TryModifyValue())
            {
                StopSlowModification();
                I_ModifySlow = C_SmoothChangeValue(newValue, time);
                Coroutine.StartCoroutine(I_ModifySlow);
            }
        }

        /// <summary>
        /// Modify the Stat value with a 'new Value',  'ticks' times , every 'timeBetweenTicks' seconds
        /// </summary>
        public virtual void Modify(float newValue, int ticks, float timeBetweenTicks)
        {
            if (!Active) return;            //Ignore if the Stat is Disable

            if (I_ModifyPerTicks != null)
                Coroutine.StopCoroutine(I_ModifyPerTicks);

            I_ModifyPerTicks = C_ModifyTicksValue(newValue, ticks, timeBetweenTicks);
            Coroutine.StartCoroutine(I_ModifyPerTicks);
        }

        /// <summary> Add or Remove Value the 'MaxValue' of the Stat </summary>
        public virtual void ModifyMAX(float newValue)
        {
            if (!Active) return;            //Ignore if the Stat is Disable

            MaxValue += newValue;
            StartRegeneration();
        }


        /// <summary>Add or Remove Rate to the Regeneration Rate</summary>
        public virtual void ModifyRegenerationRate(float newValue)
        {
            if (!Active) return;            //Ignore if the Stat is Disable

            RegenRate.Value += newValue;
            StartRegeneration();
        }

        public virtual void SetRegenerationWait(float newValue)
        {
            if (!Active) return;            //Ignore if the Stat is Disable

            RegenWaitTime.Value = newValue;

            if (RegenWaitTime < 0) RegenWaitTime.Value = 0;
        }

        /// <summary>Set a new Regeneration Rate</summary>
        public virtual void SetRegenerationRate(float newValue)
        {
            if (!Active) return;            //Ignore if the Stat is Disable
            RegenRate.Value = newValue;
        }

        /// <summary> Reset the Stat to the Default Min or Max Value</summary>
        public virtual void Reset()
        {
            Value = resetTo == ResetTo.MaxVlaue ? Value = MaxValue : Value = MinValue;
        }

        /// <summary>True if the Stat is active and is not on its imnune time</summary>
        protected virtual bool TryModifyValue()
        {
            if (!Active) return false;            //Ignore if the Stat is Disable

            if (InmuneTime > 0)
            {
                if (Time.time - StoredTime < InmuneTime) return false;
                else StoredTime = Time.time;
            }
            return true;
        }

        /// <summary>Clean all Coroutines</summary>
        internal void CleanRoutines()
        {
            StopDegeneration();
            StopRegeneration();
            StopTickDamage();
            StopSlowModification();
        }


        public virtual void RegenerateOverTime(float time)
        {
            if (time <= 0)
            {
                StartRegeneration();
            }
            else
            {
                Coroutine.StartCoroutine(C_RegenerateOverTime(time));
            }
        } 

        protected virtual void StartRegeneration()
        {
            if (RegenRate == 0 || !Regenerate) return;            //Means if there's no Regeneration
            StopRegeneration();

            I_Regeneration = C_Regenerate();
            Coroutine.StartCoroutine(I_Regeneration);
        }


        protected virtual void StartDegeneration()
        {
            if (DegenRate == 0) return;                       //Means there's no Degeneration
            StopDegeneration();
            I_Degeneration = C_Degenerate();
            Coroutine.StartCoroutine(I_Degeneration);
        }

        protected virtual void StopRegeneration()
        {
            if (I_Regeneration != null)
                Coroutine.StopCoroutine(I_Regeneration);            //If there was a regenation active .... interrupt it
            I_Regeneration = null;
        }

        protected virtual void StopDegeneration()
        {
            if (I_Degeneration != null)
                Coroutine.StopCoroutine(I_Degeneration);            //if it was ALREADY Degenerating.. stop
            I_Degeneration = null;
        }

        protected virtual void StopTickDamage()
        {
            if (I_ModifyPerTicks != null)
                Coroutine.StopCoroutine(I_ModifyPerTicks);   //if it was ALREADY Degenerating.. stop
            I_ModifyPerTicks = null;
        }

        protected virtual void StopSlowModification()
        {
            if (I_ModifySlow != null)
                Coroutine.StopCoroutine(I_ModifySlow);       //If there was a regenation active .... interrupt it
            I_ModifySlow = null;
        }

        #region Coroutines
        private MonoBehaviour Coroutine;        //I need this to use coroutines in this class because it does not inherit from Monobehaviour
        private IEnumerator I_Regeneration;
        private IEnumerator I_Degeneration;
        private IEnumerator I_ModifyPerTicks;
        private IEnumerator I_ModifySlow;


        protected IEnumerator C_RegenerateOverTime(float time)
        {
            float ReachValue = RegenRate > 0 ? MaxValue : MinValue;                                //Set to the default or 0
            bool Positive = RegenRate > 0;                                                          //Is the Regeneration Positive?
            float currentTime = Time.time;

            while (Value != ReachValue || currentTime > time )
            {
                Value += (RegenRate * Time.deltaTime);

                if (Positive && Value > MaxValue)
                {
                    Value = MaxValue;
                }
                else if (!Positive && Value < 0)
                {
                    Value = MinValue;
                }
                currentTime += Time.deltaTime;

                yield return null;
            }
            yield return null;
        }


        protected IEnumerator C_Regenerate()
        {
            if (RegenWaitTime > 0)
                yield return new WaitForSeconds(RegenWaitTime);          //Wait a time to regenerate

            while (Regenerate && Value < MaxValue)
            {
                Value += (RegenRate * Time.deltaTime);
                yield return null;
            }
            yield return null;
        }

        protected IEnumerator C_Degenerate()
        {
            while (Degenerate && Value > MinValue)
            {
                Value -= (DegenRate * Time.deltaTime);
                yield return null;
            }
            yield return null;
        }

        protected IEnumerator C_ModifyTicksValue(float value, int Ticks, float time)
        {
            var WaitForTicks = new WaitForSeconds(time);

            for (int i = 0; i < Ticks; i++)
            {
                Value += value;
                if (Value <= MinValue)
                {
                    Value = MinValue;
                    break;
                }
                yield return WaitForTicks;
            }

            yield return null;

            StartRegeneration();
        }

        protected IEnumerator C_SmoothChangeValue(float newvalue, float time)
        {
            StopRegeneration();

            Debug.Log(newvalue);

            float currentTime = 0;
            float currentValue = Value;
            newvalue = Value + newvalue;


            while (currentTime <= time)
            {

                Value = Mathf.Lerp(currentValue, newvalue, currentTime / time);
                currentTime += Time.deltaTime;


                yield return null;
            }
            Value = newvalue;

            yield return null;
            StartRegeneration();
        }
        #endregion

        public enum ResetTo
        {
            MinValue,
            MaxVlaue
        }
    }
}