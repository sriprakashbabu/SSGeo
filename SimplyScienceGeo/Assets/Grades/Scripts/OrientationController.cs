using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem; // Using the new Input System

public class OrientationController : MonoBehaviour
{
    // --- Import BOTH JavaScript functions from our .jslib file ---
    [DllImport("__Internal")]
    private static extern bool IsMobileDevice();

    [DllImport("__Internal")]
    private static extern void RequestFullscreenAndLockLandscape();

    private bool _isMobile;
    private bool _orientationChangeRequested = false; // Flag to ensure we only run this once

    void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        _isMobile = IsMobileDevice();
#else
        // In the editor, assume it's not a mobile device
        _isMobile = false;
#endif

        if (!_isMobile)
        {
            // If not on mobile, this script has nothing to do.
            // You can optionally disable the button that calls GoLandscape().
            this.enabled = false;
        }
    }

    void Update()
    {
        // If we are on mobile AND we haven't requested the change yet...
        if (_isMobile && !_orientationChangeRequested)
        {
            // Check for the first press from either a mouse or a touch screen
            bool firstPress = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame ||
                              Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

            if (firstPress)
            {
                // Set the flag to true so this code never runs again
                _orientationChangeRequested = true;

                // Call the JavaScript function to go fullscreen and lock landscape
                RequestFullscreenAndLockLandscape();
            }
        }
    }

    // You can keep this public method if you still want a manual button as a backup
    public void GoLandscape()
    {
        if (_isMobile)
        {
            RequestFullscreenAndLockLandscape();
        }
    }
}