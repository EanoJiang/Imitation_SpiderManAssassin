using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Player3D))]
public class Spaceship : MonoBehaviour
{
    Player3D player3D;
    [SerializeField] Transform body;

    [SerializeField] float angleAcceRotate = 100;
    [SerializeField] float angleAcceMoveSide = 50;
    [SerializeField] float angleFriction = 2.5f;
    float angle = 0;

    [SerializeField] float accelerationFriction = 1;
    float speed = 0;
    float maxSpeedEstimation;
    float speedProgress;

    [SerializeField] float trailTime = 0.25f;
    [SerializeField] float trailProgressStart = 0.5f;
    [SerializeField] TrailRenderer[] trails;

    [SerializeField] bool postPro = true;

    LensDistortion lensDistortion;
    [SerializeField] float lensDistoIntensity = -0.3f;
    float lensDistoIntensityAdd;

    ChromaticAberration chromaticAberration;
    [SerializeField] float chromaticAberrationIntensity = 0.5f;
    float chromAbeIntensityAdd;

    void Awake()
    {
        player3D = GetComponent<Player3D>();

        Volume volume = FindObjectOfType<Volume>();
        volume.profile.TryGet(out lensDistortion);
        volume.profile.TryGet(out chromaticAberration);
        if (lensDistortion) lensDistoIntensityAdd = lensDistortion.intensity.value;
        if (chromaticAberration) chromAbeIntensityAdd = chromaticAberration.intensity.value;

        for (float t = 0; t < 10; t += Time.fixedDeltaTime)
        {
            maxSpeedEstimation += Time.fixedDeltaTime;
            maxSpeedEstimation -= Time.fixedDeltaTime * maxSpeedEstimation * accelerationFriction;
        }
    }


    void Update()
    {
        angle -= (angleAcceMoveSide * player3D.Controller.StickL.x + angleAcceRotate * player3D.Controller.StickR.x) * Time.deltaTime;
        angle -= angle * Time.deltaTime * angleFriction;
        body.localRotation = Quaternion.Euler(0, 0, angle);

        speed += Time.deltaTime * Mathf.Max(player3D.Controller.StickL.y, 0);
        speed -= Time.deltaTime * speed * accelerationFriction;
        speedProgress = speed / maxSpeedEstimation;

        if (postPro)
        {
            if (lensDistortion) lensDistortion.intensity.value = lensDistoIntensity * speedProgress + lensDistoIntensityAdd;
            if (chromaticAberration) chromaticAberration.intensity.value = chromaticAberrationIntensity * speedProgress + chromAbeIntensityAdd;
        }

        foreach (TrailRenderer trail in trails)
            trail.time = trailTime * Mathf.InverseLerp(trailProgressStart, 1, speedProgress);
    }
}
