using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Rope : MonoBehaviour
{

    [Header("Settings")]
    public int quality = 200; 
    public float damper = 14; 
    public float strength = 800;
    public float velocity = 15;
    public float waveCount = 3; 
    public float waveHeight = 1;
    public AnimationCurve affectCurve;
    public float originVelocity = 15f;

    private LineRenderer lr;
    private Vector3 currentGrapplePosition;

    public Transform start;
    public Transform target;
    public bool isDrawRope;

    private float value;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();

    }

    //Called after Update
    private void LateUpdate()
    {
        DrawRope();
    }

	private void Start()
	{
        currentGrapplePosition = start.position;

    }

	void DrawRope()
    {
		if (!isDrawRope)
		{
            currentGrapplePosition = start.position;
            velocity = originVelocity;
            value = 1f;
            if (lr.positionCount > 0)
              lr.positionCount = 0;
            return;
        }

       
        if (lr.positionCount == 0)
		{
			lr.positionCount = quality + 1;
		}

		Vector3 up = Quaternion.LookRotation((target.position - start.position).normalized) * Vector3.up;


        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, target.position, Time.deltaTime * 8f);
        
        RopeUpdate();
        //RopeUpdate的简易方法：waveHeight根据时间慢慢变成0，曲线随时间变平
        //if (value > 0f)
        //    value -= velocity * Time.deltaTime;
        //else
        //    value = 0f;
        for (int i = 0; i < quality + 1; i++)
        {
            float delta = i / (float)quality;
            //把waveHeight*value理解成曲线随时间变平
            Vector3 offset = up * waveHeight * value * Mathf.Sin(delta * waveCount * Mathf.PI) * affectCurve.Evaluate(delta);

            lr.SetPosition(i, Vector3.Lerp(start.position, currentGrapplePosition, delta) + offset);
        }
    }

    void RopeUpdate()
	{
        var direction = 0 - value >= 0 ? 1f : -1f;
        var force = Mathf.Abs(0 - value) * strength;
        velocity += (force * direction - velocity * damper) * Time.deltaTime;
        value += velocity * Time.deltaTime;
    }
}
