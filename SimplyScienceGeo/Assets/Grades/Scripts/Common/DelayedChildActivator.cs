using UnityEngine;
using System.Collections; // Required for Coroutines

public class DelayedChildActivator : MonoBehaviour
{
    // A public variable to set the delay time in seconds via the Inspector
    public float activationDelay = 0.2f;

    // A public reference to the specific child GameObject you want to activate
    public GameObject childToActivate;

    void OnEnable()
    {
        // When the parent object becomes active, start the delayed activation routine
        if (childToActivate != null)
        {
            // Optionally, ensure the child starts as disabled before the delay starts
            childToActivate.SetActive(false);
            StartCoroutine(ActivateChildAfterDelay());
        }
        else
        {
            Debug.LogError("Child GameObject reference not set in the Inspector!");
        }
    }

    private IEnumerator ActivateChildAfterDelay()
    {
        // Wait for the specified amount of time (set in the Inspector)
        yield return new WaitForSeconds(activationDelay);

        // After the delay, activate the child GameObject
        // Check 'isActiveAndEnabled' to ensure the parent is still active
        if (this.isActiveAndEnabled && childToActivate != null)
        {
            childToActivate.SetActive(true);
        }
    }

    void OnDisable()
    {
        // Stop the coroutine if the object is disabled before the delay finishes
        StopAllCoroutines();
    }
}
