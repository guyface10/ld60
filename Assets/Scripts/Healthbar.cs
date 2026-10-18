using UnityEngine;

public class Healthbar : MonoBehaviour
{
    [HideInInspector]
    public float maxValue;
    public float Value
    {
        get => maxValue * fillRect.localScale.x;

        set
        {
            float clampedValue = Mathf.Clamp(value, 0, maxValue);
            Vector3 scale = fillRect.localScale;
            scale.x = clampedValue / maxValue;
            fillRect.localScale = scale;
        }
    }
    
    [SerializeField]
    private RectTransform fillRect;
}
