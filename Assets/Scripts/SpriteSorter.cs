using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSorter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        spriteRenderer.sortingOrder = -(int)(transform.position.y * 100);
    }
}
