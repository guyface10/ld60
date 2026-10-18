using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    private PlayerActions inputActions;

    #region Movement

    [SerializeField]
    private float health;
    private float initialHealth;
    [SerializeField]
    private Healthbar healthbar;
    [SerializeField]
    private float healthPickupAmount;

    [Space]
    [SerializeField]
    private float speed;

    [SerializeField]
    private float dashSpeedMultiplier;
    [SerializeField]
    private float dashDuration;
    [SerializeField]
    private float dashCooldown;
    private bool dashing;
    private float dashTimer;
    private float dashCooldownTimer;

    [Space]
    [SerializeField]
    private float knockbackDuration;
    private float knockbackTimer;

    private Rigidbody2D rb2d;
    private Vector2 movement;

    #endregion

    [Space]
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [Space]
    [SerializeField]
    private Transform heldItemTransform;
    [SerializeField]
    private SpriteRenderer heldItemSpriteRenderer;
    private float heldItemRadius;

    private Item itemToPickup;

    [Space]
    [SerializeField]
    private TextMeshProUGUI actionMessage;

    [Space]
    [SerializeField]
    private float dropItemForce;

    public ItemData[] Inventory { get; private set; }
    private Item[] worldItems;
    public bool IsPrimarySelected { get; private set; } = true;
    public int InventoryIndex => IsPrimarySelected ? 0 : 1;

    private Camera cam;

    [Space]
    [SerializeField]
    private Transform crosshairTransform;

    private Computer computer;
    private bool isNearComputer;

    #region Combat

    [Space]
    [SerializeField]
    private float swingArc = 120;
    [SerializeField]
    private float swingDuration = 0.15f;
    [SerializeField]
    private float hitboxPadding = 0.1f;

    private bool swinging;
    private float swingTimer;
    private float attackCooldownTimer;
    private Weapon swingWeapon;
    private readonly System.Collections.Generic.HashSet<Zombie> hitZombies = new();

    #endregion

    [Space]
    [SerializeField]
    private GameObject bobExplosion;

    [Space]
    [SerializeField]
    private float showLoseScreenDelay = 3f;
    [SerializeField]
    private float showWinScreenDelay = 5f;
    private UIManager uiManager;

    public bool Won { get; private set; }

    void Awake()
    {
        inputActions = new PlayerActions();
        rb2d = GetComponent<Rigidbody2D>();

        cam = Camera.main;

        heldItemRadius = heldItemTransform.localPosition.x;
        Inventory = new ItemData[2];
        worldItems = new Item[2];

        actionMessage.text = "";

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;

        healthbar.maxValue = health;
        initialHealth = health;

        uiManager = FindAnyObjectByType<UIManager>();
    }

    void Update()
    {
        if (health <= 0)
            return;

        if (dashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                speed /= dashSpeedMultiplier;
                dashing = false;

                dashCooldownTimer = dashCooldown;
            }
        }
        dashCooldownTimer -= Time.deltaTime;

        movement = inputActions.Movement.Walk.ReadValue<Vector2>();

        AimHeldItem();
        UpdateSwing();

        if (isNearComputer && Inventory[InventoryIndex]?.Type == ItemData.ItemType.PART)
        {
            computer.AddPart();
            worldItems[InventoryIndex]?.gameObject.SetActive(false);
            worldItems[InventoryIndex] = null;
            Inventory[InventoryIndex] = null;
            UpdateHeldItemSprite();

            if (computer.Fixed)
            {
                StartCoroutine(ShowWinScreen());
                Won = true;
            }
        }
    }

    private void AimHeldItem()
    {
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = (Vector2)(mouseWorld - transform.position);
        if (direction.sqrMagnitude < 0.0001f)
            return;

        direction.Normalize();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        heldItemTransform.localPosition = direction * heldItemRadius;
        heldItemTransform.localRotation = Quaternion.Euler(0, 0, angle + GetSwingAngleOffset(angle));

        if (angle >= 90 || angle <= -90)
        {
            spriteRenderer.flipX = true;
            heldItemSpriteRenderer.flipY = true;
        }
        else
        {
            spriteRenderer.flipX = false;
            heldItemSpriteRenderer.flipY = false;
        }

        crosshairTransform.position = new Vector3(mouseWorld.x, mouseWorld.y, crosshairTransform.position.z);

        knockbackTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (knockbackTimer <= 0)
            rb2d.linearVelocity = movement * speed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (health <= 0)
            return;

        Zombie zombie = collision.collider.GetComponent<Zombie>();
        if (zombie != null)
        {
            Vector3 dir = collision.transform.position - transform.position;
            rb2d.linearVelocity = -dir.normalized * zombie.knockback;
            knockbackTimer = knockbackDuration;
            health -= zombie.damage;
            healthbar.Value = health;
            // TODO: make player invulnerable and flash
            
            if (health <= 0)
            {
                GameObject newBobExplosion = Instantiate(bobExplosion);
                newBobExplosion.transform.position = transform.position;
                newBobExplosion.GetComponent<BobExplosion>().Explode();
                spriteRenderer.enabled = false;
                heldItemSpriteRenderer.enabled = false;
                StartCoroutine(ShowLoseScreen());
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Item item = collision.GetComponent<Item>();
        if (item != null)
        {
            itemToPickup = item;
            actionMessage.text = "Press " + inputActions.Interactions.Pickup.GetBindingDisplayString(0) + " to pickup " + itemToPickup.itemData.DisplayName;
        }

        Computer computer = collision.GetComponent<Computer>();
        if (computer != null)
        {
            this.computer = computer;
            isNearComputer = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        Item item = collision.GetComponent<Item>();
        if (item != null)
        {
            actionMessage.text = "";
        }

        Computer computer = collision.GetComponent<Computer>();
        if (computer != null)
        {
            isNearComputer = false;
        }
    }

    private IEnumerator ShowLoseScreen()
    {
        yield return new WaitForSeconds(showLoseScreenDelay);
        uiManager.ShowLoseScreen();
        gameObject.SetActive(false);
    }

    private IEnumerator ShowWinScreen()
    {
        yield return new WaitForSeconds(showWinScreenDelay);
        uiManager.ShowWinScreen();
    }

    private void OnDash()
    {
        if (dashCooldownTimer > 0)
            return;

        speed *= dashSpeedMultiplier;
        dashTimer = dashDuration;
        dashing = true;
    }

    private float GetSwingAngleOffset(float aimAngle)
    {
        if (!swinging)
            return 0;

        float t = 1 - (swingTimer / swingDuration);
        float offset = Mathf.Lerp(swingArc / 2, -swingArc / 2, t);
        bool facingLeft = aimAngle >= 90 || aimAngle <= -90;
        return facingLeft ? -offset : offset;
    }

    private void OnAttack()
    {
        if (health <= 0)
            return;

        if (swinging || attackCooldownTimer > 0)
            return;

        Weapon weapon = worldItems[InventoryIndex]?.GetComponent<Weapon>();
        if (weapon == null || weapon.WeaponData.Type != WeaponData.WeaponType.MELEE)
            return;

        swingWeapon = weapon;
        swinging = true;
        swingTimer = swingDuration;
        attackCooldownTimer = weapon.WeaponData.AttackSpeed > 0 ? 1 / weapon.WeaponData.AttackSpeed : swingDuration;
        hitZombies.Clear();
    }

    private void UpdateSwing()
    {
        attackCooldownTimer -= Time.deltaTime;

        if (!swinging)
            return;

        DamageZombiesInHitbox();

        swingTimer -= Time.deltaTime;
        if (swingTimer <= 0)
            swinging = false;
    }

    private void DamageZombiesInHitbox()
    {
        if (heldItemSpriteRenderer.sprite == null)
            return;

        Bounds bounds = heldItemSpriteRenderer.sprite.bounds;
        Vector2 center = heldItemTransform.TransformPoint(bounds.center);
        Vector2 size = Vector2.Scale(bounds.size, heldItemTransform.lossyScale) + Vector2.one * hitboxPadding;

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, heldItemTransform.eulerAngles.z);
        for (int i = 0; i < hits.Length; i++)
        {
            Zombie zombie = hits[i].GetComponentInParent<Zombie>();
            if (zombie == null || !hitZombies.Add(zombie))
                continue;

            Vector2 direction = ((Vector2)(zombie.transform.position - transform.position)).normalized;
            zombie.TakeDamage(swingWeapon.WeaponData.Damage, direction * swingWeapon.WeaponData.Knockback);
        }
    }

    private void OnPickup()
    {
        if (itemToPickup == null)
            return;

        if (itemToPickup.itemData.Id == "health")
        {
            health += healthPickupAmount;
            health = Mathf.Clamp(health, 0, initialHealth);
            itemToPickup.gameObject.SetActive(false);
            healthbar.Value = health;
            return;
        }

        if (Inventory[InventoryIndex] != null)
        {
            worldItems[InventoryIndex].gameObject.SetActive(true);
            worldItems[InventoryIndex].transform.position = heldItemTransform.position;
            worldItems[InventoryIndex].transform.rotation = heldItemTransform.rotation;
            worldItems[InventoryIndex].Rb2d.linearVelocity = worldItems[InventoryIndex].transform.rotation * Vector3.right * dropItemForce;
        }

        itemToPickup.gameObject.SetActive(false);
        worldItems[InventoryIndex] = itemToPickup;

        Inventory[InventoryIndex] = itemToPickup.itemData;
        UpdateHeldItemSprite();
    }

    private void OnSwitchItem()
    {
        IsPrimarySelected = !IsPrimarySelected;
        swinging = false;
        UpdateHeldItemSprite();
    }

    private void UpdateHeldItemSprite()
    {
        if (Inventory[InventoryIndex] == null)
        {
            heldItemSpriteRenderer.sprite = null;
            return;
        }
        
        heldItemSpriteRenderer.sprite = Inventory[InventoryIndex].Sprite;
    }

    void OnEnable()
    {
        inputActions.Enable();

        inputActions.Movement.Dash.performed += _ => OnDash();
        inputActions.Interactions.Pickup.performed += _ => OnPickup();
        inputActions.Interactions.SwitchItem.performed += _ => OnSwitchItem();
        inputActions.Combat.Attack.performed += _ => OnAttack();
    }

    void OnDisable()
    {
        inputActions.Disable();

        inputActions.Movement.Dash.performed -= _ => OnDash();
        inputActions.Interactions.Pickup.performed -= _ => OnPickup();
        inputActions.Interactions.SwitchItem.performed -= _ => OnSwitchItem();
        inputActions.Combat.Attack.performed -= _ => OnAttack();
    }
}
