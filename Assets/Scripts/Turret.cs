using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Turret : MonoBehaviour
{
    private Animator animator;

    [SerializeField]
    private string shootAnimationName = "turret-shoot";

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Fire()
    {
        animator.Play(shootAnimationName);
    }
}
