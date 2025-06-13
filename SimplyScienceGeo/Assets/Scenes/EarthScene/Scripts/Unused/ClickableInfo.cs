using UnityEngine;
using System.Collections; // Required for Coroutines

[RequireComponent(typeof(Collider))]
public class ClickableInfo : MonoBehaviour
{
    [Header("Feature Information")]
    [Tooltip("The information text to display when this feature is double-clicked.")]
    [TextArea(3, 10)]
    public string informationToShow;

    [Header("Pulsing Effect")]
    [Tooltip("Enable a pulsing effect when this feature is selected by a double-click.")]
    public bool usePulseEffect = true;
    [Tooltip("The color the feature will pulse towards.")]
    public Color pulseTargetColor = new Color(0.8f, 0.8f, 0.5f, 1f); // A slightly desaturated yellow
    [Tooltip("How many full pulse cycles (original -> target -> original) per second.")]
    public float pulseCyclesPerSecond = 0.5f;

    // For double-click detection
    private float _instanceLastClickTime;
    private const float DOUBLE_CLICK_THRESHOLD = 0.3f; // Time in seconds for a double click

    private GlobeInfoUIManager _globeInfoUIManager;
    private Collider _thisCollider;
    private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;
    private Color _originalBaseColor;
    private Coroutine _pulseCoroutine;
    private bool _isPulsing = false;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    void Awake()
    {
        _thisCollider = GetComponent<Collider>();
        _renderer = GetComponent<Renderer>();

        _instanceLastClickTime = -DOUBLE_CLICK_THRESHOLD; // Ensures first click isn't a double-click

        if (_renderer != null)
        {
            _propertyBlock = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(_propertyBlock);

            if (_renderer.sharedMaterial.HasProperty(BaseColorId))
            {
                Color currentColorInBlock = _propertyBlock.GetColor(BaseColorId);
                if (!_propertyBlock.isEmpty && (currentColorInBlock.r != 0 || currentColorInBlock.g != 0 || currentColorInBlock.b != 0 || currentColorInBlock.a != 0))
                {
                    _originalBaseColor = currentColorInBlock;
                }
                else
                {
                    _originalBaseColor = _renderer.sharedMaterial.GetColor(BaseColorId);
                    _propertyBlock.SetColor(BaseColorId, _originalBaseColor);
                    _renderer.SetPropertyBlock(_propertyBlock);
                }
            }
            else
            {
                _originalBaseColor = Color.white;
                if (usePulseEffect) Debug.LogWarning($"ClickableInfo on '{gameObject.name}': Renderer's material does not have '_BaseColor'. Pulse may not work as expected.", this);
            }
        }
        else
        {
            if (usePulseEffect) Debug.LogWarning($"ClickableInfo on '{gameObject.name}': No Renderer found. Pulse effect will be disabled.", this);
            usePulseEffect = false;
        }
    }

    void Start()
    {
        _globeInfoUIManager = FindObjectOfType<GlobeInfoUIManager>();
        if (_globeInfoUIManager == null)
        {
            Debug.LogWarning($"ClickableInfo on '{gameObject.name}': Could not find GlobeInfoUIManager. Info display will not work.", this);
        }
    }

    public bool CanBeClicked()
    {
        return _thisCollider.enabled && this.enabled;
    }

    /// <summary>
    /// Called by GlobalClickHandler on each click to this feature.
    /// Starts pulsing on selection, handles double-click for info/relief.
    /// </summary>
    public void SelectAndPulse()
    {
        if (!CanBeClicked()) return;

        // Check for double-click using the time of the PREVIOUS click on this instance
        bool isDoubleClick = (Time.time - _instanceLastClickTime < DOUBLE_CLICK_THRESHOLD);

        // --- Handle Pulsing ---
        // If this feature is clicked (meaning it's selected or being re-confirmed),
        // it should pulse if usePulseEffect is true.
        if (usePulseEffect && _renderer != null)
        {
            if (!_isPulsing) // If not currently pulsing, start it.
            {
                // Stop any old one just in case _isPulsing was somehow out of sync.
                if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);
                _pulseCoroutine = StartCoroutine(PulseRoutine());
                // _isPulsing is set to true at the start of PulseRoutine.
                Debug.Log($"ClickableInfo on '{gameObject.name}': Pulse STARTED (was not pulsing).", this);
            }
            // else: If it's already pulsing, let it continue. A click doesn't necessarily need to restart it unless desired.
        }

        // --- Handle Double-Click Specific Actions (Info, Relief Activation) ---
        if (isDoubleClick)
        {
            Debug.Log($"ClickableInfo: DOUBLE-CLICK confirmed for {gameObject.name}!", this);

            // Display Information
            if (_globeInfoUIManager != null)
            {
                if (!string.IsNullOrEmpty(informationToShow))
                {
                    _globeInfoUIManager.DisplayInformation(informationToShow);
                }
                else
                {
                    _globeInfoUIManager.DisplayInformation($"{gameObject.name}: No specific details available.");
                }
            }
            else
            {
                Debug.LogWarning($"ClickableInfo on '{gameObject.name}': Double-clicked, GlobeInfoUIManager missing.", this);
            }

            // Activate Detail View
            var relief = GetComponent<ReliefFeatureActivator>();
            if (relief) relief.ActivateDetail();

            // After a successful double-click, reset _instanceLastClickTime
            // so the next interaction requires two fresh clicks for a double-click.
            _instanceLastClickTime = -DOUBLE_CLICK_THRESHOLD;
        }
        else // Not a double click (it's a single click, or the first click of a potential pair)
        {
            Debug.Log($"ClickableInfo: Single click (or first part of double-click) on '{gameObject.name}'. Pulse active/started. Setting up for potential next click.", this);
            // Store the time of THIS click to compare against for the NEXT click to this same feature.
            _instanceLastClickTime = Time.time;
        }
    }

    // ClickableInfo.cs  – only 1 small change inside DeselectAndReset()

    public void DeselectAndReset()
    {
        /* 1️⃣ stop the pulse if we had one */
        if (_pulseCoroutine != null)
        {
            StopCoroutine(_pulseCoroutine);
            _pulseCoroutine = null;
        }

        /* 2️⃣ ***NEW CONDITION: reset colour only when pulse was used *** */
        if (usePulseEffect && _renderer != null && _propertyBlock != null)
        {
            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(BaseColorId, _originalBaseColor);
            _renderer.SetPropertyBlock(_propertyBlock);
        }

        _isPulsing = false;
    }


    private IEnumerator PulseRoutine()
    {
        _isPulsing = true; // Mark that pulsing has started
        float speed = pulseCyclesPerSecond <= 0 ? 1f : pulseCyclesPerSecond; // Ensure speed is positive

        while (_isPulsing) // Loop continues as long as _isPulsing is true
        {
            if (_renderer == null || _propertyBlock == null) // Safety check
            {
                _isPulsing = false; // Stop if renderer is gone
                yield break;
            }

            // Calculate lerpFactor for sine wave pulse
            float lerpFactor = (Mathf.Sin(Time.time * speed * 2f * Mathf.PI) + 1f) * 0.5f;
            Color currentColor = Color.Lerp(_originalBaseColor, pulseTargetColor, lerpFactor);

            _renderer.GetPropertyBlock(_propertyBlock); // Get the current block
            _propertyBlock.SetColor(BaseColorId, currentColor);
            _renderer.SetPropertyBlock(_propertyBlock); // Apply the change

            yield return null; // Wait for the next frame
        }

        // When _isPulsing becomes false and loop exits, ensure original color is restored
        // (though DeselectAndReset also does this, this is a safeguard if coroutine exits differently)
        if (_renderer != null && _propertyBlock != null && _renderer.sharedMaterial.HasProperty(BaseColorId))
        {
             _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(BaseColorId, _originalBaseColor);
            _renderer.SetPropertyBlock(_propertyBlock);
        }
        // Debug.Log($"ClickableInfo on '{gameObject.name}': PulseRoutine ended.", this);
    }

    void OnDestroy()
    {
        // Ensure coroutine is stopped if object is destroyed
        if (_pulseCoroutine != null)
        {
            StopCoroutine(_pulseCoroutine);
            _pulseCoroutine = null;
        }
        _isPulsing = false;
    }
}