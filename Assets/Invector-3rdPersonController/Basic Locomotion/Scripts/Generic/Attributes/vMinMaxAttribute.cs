using UnityEngine;

public class vMinMaxAttribute : PropertyAttribute
{
    public float minLimit = 0;
    public float maxLimit = 1f;
    public vMinMaxAttribute()
    {

    }
    public vMinMaxAttribute(float min, float max)
    {
        minLimit = min;
        maxLimit = max;
    }
}
