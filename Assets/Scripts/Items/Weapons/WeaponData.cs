using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    public enum WeaponType
    {
        MELEE,
        RANGED,
    }

    [field: SerializeField]
    public WeaponType Type { get; private set; }
    [field: SerializeField]
    public float Damage { get; private set; }
    [field: SerializeField]
    public float AttackSpeed { get; private set; }
    [field: SerializeField]
    public float Knockback { get; private set; }
}

