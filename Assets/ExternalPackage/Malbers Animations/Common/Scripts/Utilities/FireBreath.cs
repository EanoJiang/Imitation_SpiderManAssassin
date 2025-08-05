using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations
{
    public class FireBreath : MonoBehaviour
    {
        ParticleSystem.EmissionModule emission;
        ParticleSystem.MainModule particles;

        void Start()
        {
            particles = GetComponent<ParticleSystem>().main;
            emission = GetComponent<ParticleSystem>().emission;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(0);
        }

        public void Activate(bool value)
        {
            if (value)
            {
                emission.rateOverTime = new ParticleSystem.MinMaxCurve(500f);
            }
            else
            {
                emission.rateOverTime = new ParticleSystem.MinMaxCurve(0);
            }
        }

        public void FireBreathColor(ColorVar newcolor)
        {
            particles.startColor = new ParticleSystem.MinMaxGradient(newcolor);
        }

        public void FireBreathColor(Color newcolor)
        {
            particles.startColor = new ParticleSystem.MinMaxGradient(newcolor);
        }
    }
}
