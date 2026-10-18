using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public enum ItemType
    {
        PART,
        WEAPON,
        CONSUMABLE,
    }

    [field: SerializeField]
    public string Id { get; private set; }
    [field: SerializeField]
    public ItemType Type { get; private set; }
    [field: SerializeField]
    public string DisplayName { get; private set; }
    [field: SerializeField]
    public Sprite Sprite { get; private set; }
    [field: SerializeField]
    public float Weight { get; set; } = 1;
}
