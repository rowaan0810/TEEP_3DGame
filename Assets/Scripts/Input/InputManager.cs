// InputManager.cs — Manages switching between input modes.
// Holds references to all IGameInput implementations and exposes the active one.

using UnityEngine;

namespace EndlessRunner.Input
{
    public enum InputMode
    {
        Keyboard,
        WebcamPose,
        EasyMode
    }

    /// <summary>
    /// Central input manager. Holds all input implementations and allows
    /// runtime switching between them. PlayerController reads from CurrentInput.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        [Header("Input Implementations")]
        [SerializeField] private KeyboardInput keyboardInput;

        // These will be added in Week 3:
        // [SerializeField] private PoseInput poseInput;
        // [SerializeField] private EasyModeInput easyModeInput;

        [Header("Settings")]
        [SerializeField] private InputMode currentMode = InputMode.Keyboard;

        /// <summary>
        /// The currently active input source. All game systems read from this.
        /// </summary>
        public IGameInput CurrentInput { get; private set; }

        /// <summary>
        /// The current input mode enum value.
        /// </summary>
        public InputMode CurrentMode => currentMode;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // Do NOT call SetMode here — serialized fields may not be wired yet
            // when components are added dynamically via SceneSetup.
        }

        private void Start()
        {
            // Auto-find KeyboardInput if not assigned
            if (keyboardInput == null)
            {
                keyboardInput = GetComponent<KeyboardInput>();
            }
            if (keyboardInput == null)
            {
                keyboardInput = FindFirstObjectByType<KeyboardInput>();
            }

            SetMode(currentMode);

            if (CurrentInput == null)
            {
                Debug.LogWarning("InputManager: No input source found! Adding KeyboardInput.");
                keyboardInput = gameObject.AddComponent<KeyboardInput>();
                SetMode(InputMode.Keyboard);
            }

            Debug.Log($"InputManager ready. Mode: {currentMode}, Input: {CurrentInput?.GetType().Name ?? "NULL"}");
        }

        /// <summary>
        /// Switch input mode at runtime. Called from settings UI.
        /// </summary>
        public void SetMode(InputMode mode)
        {
            currentMode = mode;

            switch (mode)
            {
                case InputMode.Keyboard:
                    CurrentInput = keyboardInput;
                    break;
                case InputMode.WebcamPose:
                    // Will be implemented in Week 3
                    Debug.Log("WebcamPose input not yet implemented, falling back to Keyboard");
                    CurrentInput = keyboardInput;
                    break;
                case InputMode.EasyMode:
                    // Will be implemented in Week 3
                    Debug.Log("EasyMode input not yet implemented, falling back to Keyboard");
                    CurrentInput = keyboardInput;
                    break;
            }
        }

        /// <summary>
        /// Returns the display name for the current input mode.
        /// </summary>
        public string GetModeName()
        {
            return currentMode switch
            {
                InputMode.Keyboard => "Keyboard",
                InputMode.WebcamPose => "Webcam Pose",
                InputMode.EasyMode => "Easy Mode",
                _ => "Unknown"
            };
        }
    }
}
