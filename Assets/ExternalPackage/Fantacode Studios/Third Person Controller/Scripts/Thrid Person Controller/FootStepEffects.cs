using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace FS_ThirdPerson
{
    public class FootStepEffects : MonoBehaviour
    {
        [SerializeField] List<AudioClip> footStepSounds;
        [SerializeField] List<GameObject> footStepParticles;

        [SerializeField] List<OverrideStepEffects> overrideStepEffects;
        [SerializeField] StepEffectsOverrideType overrideType;

        public LayerMask groundLayer = 1;

        [SerializeField] List<SystemState> soundIgnoreStates;
        [SerializeField] List<SystemState> particleIgnoreStates;

        [SerializeField] bool adjustVolumeBasedOnSpeed = true;
        [SerializeField] float minVolume = 0.2f;

        public List<OverrideStepEffects> OverrideStepEffects => overrideStepEffects;
        public StepEffectsOverrideType OverrideType => overrideType;

        PlayerController playerController;
        Animator animator;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            animator = GetComponent<Animator>();
        }

        private void Start()
        {
            int footTriggerLayer = LayerMask.NameToLayer("FootTrigger");

            for (int i = 0; i < 32; i++)
            {
                Physics.IgnoreLayerCollision(footTriggerLayer, i, !IsLayerInLayerMask(i, groundLayer));
            }

            if (overrideStepEffects != null)
            {
                overrideStepEffects.ForEach(x => x.MaterialName = x.MaterialName.ToLower());
            }
        }

        public void OnFootLand(Transform footTransform, FloorStepData floorData = null)
        {
            var sounds = footStepSounds;
            var particleEffects = footStepParticles;

            if (floorData != null && overrideStepEffects != null)
            {
                //Debug.Log(floorData.materialName + " - " + floorData.textureName + " - " + floorData.tag);

                var overrideEffect = GetOverrideEffect(floorData);
                if (overrideEffect != null)
                {
                    if (overrideEffect.ovverideFootStepSounds)
                        sounds = overrideEffect.footStepSounds;

                    if (overrideEffect.overrideFootStepParticles)
                        particleEffects = overrideEffect.footStepParticles;
                }
            }

            if(sounds != null && sounds.Count > 0)
            {
                if (playerController != null && !soundIgnoreStates.Contains(playerController.FocusedSystemState))
                {
                    float moveAmount = 1;
                    if(playerController.CurrentSystemState == SystemState.Locomotion && animator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion"))
                        moveAmount = animator.GetFloat(AnimatorParameters.moveAmount) / 1.5f;
                    PlaySfx(sounds[Random.Range(0, sounds.Count)], moveAmount);
                }
                else if (playerController == null)
                    PlaySfx(sounds[Random.Range(0, sounds.Count)]);

            }

            if (particleEffects != null && particleEffects.Count > 0)
            {
                if(playerController != null && !particleIgnoreStates.Contains(playerController.FocusedSystemState))
                    SpawnParticle(particleEffects[Random.Range(0, particleEffects.Count)], footTransform);
                else if(playerController == null)
                    SpawnParticle(particleEffects[Random.Range(0, particleEffects.Count)], footTransform);
            }
        }

        void PlaySfx(AudioClip clip, float volume = 1)
        {
            GameObject sfx = new GameObject();
            sfx.transform.position = transform.position;
            var audioSource = sfx.AddComponent<AudioSource>();
            audioSource.clip = clip;

            if (adjustVolumeBasedOnSpeed)
                audioSource.volume = Mathf.Clamp(volume, minVolume, audioSource.volume);

            audioSource.Play();
            Destroy(sfx, 1.5f);
        }

        void SpawnParticle(GameObject particleEffect, Transform footTransform)
        {
            var particleObj = Instantiate(particleEffect, footTransform.position, footTransform.rotation);
            Destroy(particleObj, 2f);
        }

        OverrideStepEffects GetOverrideEffect(FloorStepData floorData)
        {
            if (overrideType == StepEffectsOverrideType.MaterialName)
                return overrideStepEffects.FirstOrDefault(x => x.MaterialName == floorData.materialName);
            else if (overrideType == StepEffectsOverrideType.TextureName)
                return overrideStepEffects.FirstOrDefault(x => x.TextureName == floorData.textureName);
            else
                return overrideStepEffects.FirstOrDefault(x => x.Tag == floorData.tag);
        }

        bool IsLayerInLayerMask(int layer, LayerMask layerMask)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }

        public LayerMask GroundLayer => groundLayer;
    }

    [System.Serializable]
    public class OverrideStepEffects
    {
        public string tag;
        public Material materialName;
        public Texture textureName;

        public bool ovverideFootStepSounds;
        public bool overrideFootStepParticles;

        public List<AudioClip> footStepSounds;
        public List<GameObject> footStepParticles;

        public string Tag => tag;
        public string MaterialName { get { return materialName.name; } set { materialName.name = value; } } 
        public string TextureName { get { return textureName.name; } set { textureName.name = value; } }
       
    }

    public enum StepEffectsOverrideType { MaterialName, TextureName, Tag }
}