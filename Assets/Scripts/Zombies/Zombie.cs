using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Zombie : MonoBehaviour
{
    public float speed;
    public float health;
    public float damage;
    public float separationAmount;
    public float separationForce;

    private Vector2 movement;
    protected Transform playerTransform;

    private Rigidbody2D rb2d;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        playerTransform = FindAnyObjectByType<Player>().transform;

        rb2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        movement = new Vector2();
    }

    void Update()
    {
        if (playerTransform == null)
            return;

        float dirX = playerTransform.position.x - transform.position.x;
        float dirY = playerTransform.position.y - transform.position.y;
        float len = Mathf.Sqrt((dirX * dirX) + (dirY * dirY));

        if (dirX < 0)
            spriteRenderer.flipX = true;
        if (dirX > 0)
            spriteRenderer.flipX = false;

        if (len > 0.0001f)
        {
            movement.x = dirX / len;
            movement.y = dirY / len;
        }
        else
        {
            movement = Vector2.zero;
        }

        Zombie[] zombies = FindObjectsByType<Zombie>();
        for (int i = 0; i < zombies.Length; i++)
        {
            if (zombies[i] == this)
                continue;

            float dist = Vector2.Distance(transform.position, zombies[i].transform.position);
            if (dist <= separationAmount)
            {
                float dx = zombies[i].transform.position.x - transform.position.x;
                float dy = zombies[i].transform.position.y - transform.position.y;
                float l = Mathf.Sqrt((dx * dx) + (dy * dy));
                Vector2 away;
                if (l > 0.0001f)
                    away = new Vector2(dx / l, dy / l);
                else
                    away = Random.insideUnitCircle.normalized;
                movement -= away * separationForce;
            }
        }
    }

    void FixedUpdate()
    {
        rb2d.linearVelocity = movement * speed;
    }
}

