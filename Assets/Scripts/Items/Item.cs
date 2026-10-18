using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class Item : MonoBehaviour
{
    public ItemData itemData;

    private SpriteRenderer spriteRenderer;

    public Rigidbody2D Rb2d { get; private set; }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = itemData.Sprite;

        Rb2d = GetComponent<Rigidbody2D>();
    }

    [ExecuteAlways]

    private void OnValidate()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (itemData != null)
            spriteRenderer.sprite = itemData.Sprite;
    }
}
