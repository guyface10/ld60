using UnityEngine;

public class BobExplosion : MonoBehaviour
{
    [SerializeField]
    private GameObject[] bobParts;

    [Space]
    [SerializeField]
    private float maxUpwardForce;
    [SerializeField]
    private float minUpwardForce;
    [SerializeField]
    private float minAngle;
    [SerializeField]
    private float maxAngle;

    public void Explode()
    {
        for (int i = 0; i < bobParts.Length; i++)
        {
            GameObject part = Instantiate(bobParts[i], transform);

            float randAngle = Random.Range(minAngle, maxAngle);
            float radians = randAngle* Mathf.Deg2Rad;
            Vector2 dir = new(Mathf.Cos(radians), Mathf.Sin(radians));   

            part.GetComponent<Rigidbody2D>().linearVelocity = dir * Random.Range(minUpwardForce, maxUpwardForce);
        }
    }
}
