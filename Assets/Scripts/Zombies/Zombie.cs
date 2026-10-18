using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Zombie : MonoBehaviour
{
    public float speed;
    public float health;
    public float damage;
    public float knockback;
    public float separationAmount;
    public float separationForce;

    [Space]
    public GameObject bloodEffectPrefab;
    public GameObject deathEffectPrefab;
    public float knockbackDuration = 0.15f;
    private float knockbackTimer;
    private bool dead;

    private Vector2 movement;
    private Transform playerTransform;
    private Player player;

    private Rigidbody2D rb2d;
    private SpriteRenderer spriteRenderer;

    private bool isInKillZone;
    private bool winDeathScheduled;

    private static readonly List<Zombie> allZombies = new();
    private static readonly Dictionary<Vector2Int, List<Zombie>> grid = new();
    private static int gridFrame = -1;
    private static float cellSize = 1f;

    void Awake()
    {
        playerTransform = FindAnyObjectByType<Player>().transform;
        player = FindAnyObjectByType<Player>();

        rb2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        movement = new Vector2();
    }

    void Update()
    {
        if (player.Won && isInKillZone)
        {
            Die();
        }

        if (player.Won && !winDeathScheduled && IsOnScreen())
        {
            winDeathScheduled = true;
            Invoke(nameof(Die), Random.Range(0.5f, 2f));
        }

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

        ApplySeparation();
    }

    void OnEnable()
    {
        allZombies.Add(this);
    }

    void OnDisable()
    {
        allZombies.Remove(this);
    }

    private void ApplySeparation()
    {
        RebuildGridIfNeeded();

        Vector2 pos = transform.position;
        Vector2Int cell = GetCell(pos);
        float sqrSeparation = separationAmount * separationAmount;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (!grid.TryGetValue(new Vector2Int(cell.x + x, cell.y + y), out List<Zombie> cellZombies))
                    continue;

                for (int i = 0; i < cellZombies.Count; i++)
                {
                    Zombie other = cellZombies[i];
                    if (other == this)
                        continue;

                    Vector2 delta = (Vector2)other.transform.position - pos;
                    float sqrDist = delta.sqrMagnitude;
                    if (sqrDist > sqrSeparation)
                        continue;

                    Vector2 away;
                    if (sqrDist > 0.0001f * 0.0001f)
                        away = delta / Mathf.Sqrt(sqrDist);
                    else
                        away = Random.insideUnitCircle.normalized;
                    movement -= away * separationForce;
                }
            }
        }
    }

    // Spatial hash shared by all zombies, rebuilt once per frame by whichever zombie updates first.
    private static void RebuildGridIfNeeded()
    {
        if (gridFrame == Time.frameCount)
            return;
        gridFrame = Time.frameCount;

        foreach (List<Zombie> cellZombies in grid.Values)
            cellZombies.Clear();

        float maxSeparation = 0.01f;
        for (int i = 0; i < allZombies.Count; i++)
            maxSeparation = Mathf.Max(maxSeparation, allZombies[i].separationAmount);
        cellSize = maxSeparation;

        for (int i = 0; i < allZombies.Count; i++)
        {
            Zombie zombie = allZombies[i];
            Vector2Int cell = GetCell(zombie.transform.position);
            if (!grid.TryGetValue(cell, out List<Zombie> cellZombies))
            {
                cellZombies = new List<Zombie>();
                grid[cell] = cellZombies;
            }
            cellZombies.Add(zombie);
        }
    }

    private static Vector2Int GetCell(Vector2 position)
    {
        return new Vector2Int(Mathf.FloorToInt(position.x / cellSize), Mathf.FloorToInt(position.y / cellSize));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        allZombies.Clear();
        grid.Clear();
        gridFrame = -1;
    }

    void FixedUpdate()
    {
        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            return;
        }

        rb2d.linearVelocity = movement * speed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Zombie Kill Zone"))
        {
            isInKillZone = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Zombie Kill Zone"))
        {
            isInKillZone = false;
        }
    }

    public void TakeDamage(float amount, Vector2 knockback)
    {
        if (dead)
            return;

        health -= amount;

        Vector2 hitDirection = knockback.sqrMagnitude > 0.0001f ? knockback.normalized : Vector2.right;
        SpawnEffect(bloodEffectPrefab, hitDirection);

        if (health <= 0)
        {
            Die();
            return;
        }

        rb2d.linearVelocity = knockback;
        knockbackTimer = knockbackDuration;
    }

    private bool IsOnScreen()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return false;

        Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);
        return viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1;
    }

    private void Die()
    {
        if (dead)
            return;

        dead = true;
        SpawnEffect(deathEffectPrefab, Vector2.right);
        Destroy(gameObject);
    }

    private void SpawnEffect(GameObject prefab, Vector2 direction)
    {
        if (prefab == null)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle) * prefab.transform.rotation;
        GameObject effect = Instantiate(prefab, transform.position, rotation);

        ParticleSystem particleSystem = effect.GetComponent<ParticleSystem>();
        if (particleSystem != null)
            Destroy(effect, particleSystem.main.duration + particleSystem.main.startLifetime.constantMax);
    }
}

