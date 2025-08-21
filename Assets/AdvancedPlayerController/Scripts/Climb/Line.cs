using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Line
{
	public Vector3 p1;
	public Vector3 p2;

	public Line()
	{

	}

	public Line(Vector3 p1, Vector3 p2)
	{
		this.p1 = p1;
		this.p2 = p2;
	}

	public bool Cast()
	{
		return Physics.Linecast(p1, p2);
	}

	public bool Cast(out RaycastHit hit)
	{
		return Physics.Linecast(p1, p2, out hit);
	}

	public bool Cast(LayerMask layerMask)
	{
		return Physics.Linecast(p1, p2, layerMask);
	}

	public bool Cast(out RaycastHit hit, LayerMask layerMask)
	{
		return Physics.Linecast(p1, p2, out hit, layerMask);
	}

	public void Draw(float duration = 0, bool draw = true)
	{
		if (draw) Debug.DrawLine(p1, p2, Color.white, duration);
	}

	public void Draw(Color color, float duration = 0, bool draw = true)
	{
		if (draw) Debug.DrawLine(p1, p2, color, duration);
	}
}

