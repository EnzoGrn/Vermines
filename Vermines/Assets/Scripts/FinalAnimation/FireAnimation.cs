using UnityEngine;

public class FireAnimation : MonoBehaviour
{
    private Animator _animator;

    void Start()
    {
        _animator = GetComponent<Animator>();

        if (_animator == null)
        {
            Debug.LogError("Animator component not found on the GameObject.");
            return;
        }

        _animator.Play("Fire", 0, Random.value); // Play à une phase aléatoire (0.0 à 1.0)
    }
}
