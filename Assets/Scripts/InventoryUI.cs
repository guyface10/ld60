using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    private Player player;

    [SerializeField]
    private Color selectedColor;
    private Color unselectedColor;

    [Space]
    [SerializeField]
    private Image primarySlotImage;
    [SerializeField]
    private Image secondarySlotImage;

    [Space]
    [SerializeField]
    private Image primaryIcon;
    [SerializeField]
    private Image secondaryIcon;

    void Awake()
    {
        player = FindAnyObjectByType<Player>();

        unselectedColor = primarySlotImage.color;
    }

    void Update()
    {
        if (player.Inventory[0] == null)
        {
            primaryIcon.color = Color.clear;
        }
        else
        {
            primaryIcon.color = Color.white;
            primaryIcon.sprite = player.Inventory[0].Sprite;
        }

        if (player.Inventory[1] == null)
        {
            secondaryIcon.color = Color.clear;
        }
        else
        {
            secondaryIcon.color = Color.white;
            secondaryIcon.sprite = player.Inventory[1].Sprite;
        }

        if (player.IsPrimarySelected)
        {
            primarySlotImage.color = selectedColor;
            secondarySlotImage.color = unselectedColor;
        }
        else
        {
            primarySlotImage.color = unselectedColor;
            secondarySlotImage.color = selectedColor;
        }
    }
}
