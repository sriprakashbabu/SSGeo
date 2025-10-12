using UnityEngine;
using PathCreation;

public class Follower : MonoBehaviour
{
    public PathCreator pathcre;

    [Header("Motion")]
    public float speed = 1f;                 // units per second along the path
    public bool slow = false;                // optional slow mode (50% speed)
    public EndOfPathInstruction endOfPath = EndOfPathInstruction.Loop;

    [Header("Formation")]
    [Tooltip("How far behind to ride (in path distance units). Set > 0 for trailing.")]
    public float distanceOffset = 0f;        // e.g., 5f for a 5m gap
    [Tooltip("Delay before this object starts moving (seconds).")]
    public float startDelay = 0f;

    [Header("Look Ahead")]
    [Tooltip("How far ahead to sample for forward look.")]
    public float lookAhead = 0.1f;

    private float distanceTravelled;         // leader distance baseline
    private float startTime;

    void Awake()
    {
        startTime = Time.time;
        distanceTravelled = 0f;
        // Optional: place at initial offset immediately
        if (pathcre != null)
        {
            Vector3 p = pathcre.path.GetPointAtDistance(-distanceOffset, endOfPath);
            transform.position = p;
            transform.LookAt(pathcre.path.GetPointAtDistance(-distanceOffset + lookAhead, endOfPath));
        }
    }

    void Update()
    {
        if (pathcre == null) return;

        // respect start delay
        float t = Time.time - startTime;
        if (t < startDelay) return;

        // integrate correctly with deltaTime
        float v = slow ? speed * 0.5f : speed;
        distanceTravelled += v * Time.deltaTime;

        float d = distanceTravelled - distanceOffset;

        // position & orientation along the path
        transform.position = pathcre.path.GetPointAtDistance(d, endOfPath);
        transform.LookAt(pathcre.path.GetPointAtDistance(d + lookAhead, endOfPath));
    }
}
