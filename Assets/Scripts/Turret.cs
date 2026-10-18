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

        ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < particleSystems.Length; i++)
        {
            particleSystems[i].Play();
        }
    }
}
