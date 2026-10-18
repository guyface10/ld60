using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    private PlayerActions inputActions;

    #region Movement

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
    }

    void Update()
    {
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

        if (isNearComputer && Inventory[InventoryIndex]?.Type == ItemData.ItemType.PART)
        {
            computer.AddPart();
            worldItems[InventoryIndex]?.gameObject.SetActive(false);
            worldItems[InventoryIndex] = null;
            Inventory[InventoryIndex] = null;
            UpdateHeldItemSprite();
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
        heldItemTransform.localRotation = Quaternion.Euler(0, 0, angle);

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
    }

    void FixedUpdate()
    {
        rb2d.linearVelocity = movement * speed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {

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

    private void OnDash()
    {
        if (dashCooldownTimer > 0)
            return;

        speed *= dashSpeedMultiplier;
        dashTimer = dashDuration;
        dashing = true;
    }

    private void OnPickup()
    {
        if (itemToPickup == null)
            return;

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
    }

    void OnDisable()
    {
        inputActions.Disable();

        inputActions.Movement.Dash.performed -= _ => OnDash();
        inputActions.Interactions.Pickup.performed -= _ => OnPickup();
        inputActions.Interactions.SwitchItem.performed -= _ => OnSwitchItem();
    }
}
