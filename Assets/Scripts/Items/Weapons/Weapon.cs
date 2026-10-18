using UnityEngine;

public class Weapon : MonoBehaviour
{
    [field: SerializeField]
    public WeaponData WeaponData { get; private set; }
}
