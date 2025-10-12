//using Debug = Utils.Logger;
using UnityEngine;

public class RandomizeAnimation : MonoBehaviour
{
    // Set minimum and maximum speed range
    public float minSpeed = 0.8f;
    public float maxSpeed = 1.5f;

    // Set minimum and maximum offset range
    public float minOffset = 0f;
    public float maxOffset = 1f;

    private Animator animator;

    void Start()
    {
        // Get the Animator component attached to the object
        animator = GetComponent<Animator>();

        // Randomize animation speed
        float randomSpeed = Random.Range(minSpeed, maxSpeed);
        animator.speed = randomSpeed;

        // Randomize animation offset
        float randomOffset = Random.Range(minOffset, maxOffset);
        animator.Play(0, -1, randomOffset);
    }
}

