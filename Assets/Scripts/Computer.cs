using UnityEngine;

public class Computer : MonoBehaviour
{
    [field: SerializeField]
    public int NumPartsNeeded { get; private set; }
    
    public int NumParts { get; private set; }

    public void AddPart()
    {
        NumParts++;
        // TODO: do some particles or something

        if (NumParts >= NumPartsNeeded)
        {
            //TODO: win

            Turret[] turrets = FindObjectsByType<Turret>();
            for (int i = 0; i < turrets.Length; i++)
            {
                turrets[i].Fire();
            }
        }
    }
}
