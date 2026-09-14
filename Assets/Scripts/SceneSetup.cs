// SceneSetup.cs — Auto-creates the scene hierarchy for the game.
// Attach this to an empty GameObject in a fresh scene, enter Play Mode,
// and it builds the entire game setup: player, camera, track, managers.

using UnityEngine;
using EndlessRunner.Core;
using EndlessRunner.Input;
using EndlessRunner.Player;
using EndlessRunner.Track;

namespace EndlessRunner
{
    /// <summary>
    /// One-click scene bootstrapper. Creates all necessary GameObjects, components,
    /// and wiring for the game to run. Attach to an empty scene and press Play.
    /// </summary>
    public class SceneSetup : MonoBehaviour
    {
        [Header("Run Setup on Start")]
        [SerializeField] private bool autoSetup = true;

        private void Start()
        {
            if (autoSetup)
            {
                SetupScene();
            }
        }

        /// <summary>
        /// Find a shader that actually works. Unity 6 URP doesn't expose
        /// "Universal Render Pipeline/Lit" to Shader.Find at runtime unless
        /// a material referencing it exists in the project.
        /// </summary>
        private static Shader FindWorkingShader()
        {
            // Try multiple shaders in order of preference
            string[] candidates = {
                "Universal Render Pipeline/Lit",
                "Universal Render Pipeline/Simple Lit",
                "Universal Render Pipeline/Unlit",
                "Standard",
                "Unlit/Color",
                "Sprites/Default"
            };

            foreach (string name in candidates)
            {
                Shader s = Shader.Find(name);
                if (s != null && s.name != "Hidden/InternalErrorShader")
                {
                    Debug.Log($"Using shader: {s.name}");
                    return s;
                }
            }

            // Last resort — grab the shader from an existing renderer in the scene
            Renderer existingRenderer = FindFirstObjectByType<Renderer>();
            if (existingRenderer != null && existingRenderer.sharedMaterial != null)
            {
                Debug.Log($"Using shader from existing renderer: {existingRenderer.sharedMaterial.shader.name}");
                return existingRenderer.sharedMaterial.shader;
            }

            Debug.LogWarning("Could not find any working shader!");
            return Shader.Find("Unlit/Color");
        }

        private static Material CreateMaterial(Color color)
        {
            Shader shader = FindWorkingShader();
            Material mat = new Material(shader);

            // Try to set color via common property names
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);  // URP
            else if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", color);       // Standard/Legacy
            
            mat.color = color; // Fallback for .color property
            return mat;
        }

        [ContextMenu("Setup Scene")]
        public void SetupScene()
        {
            Debug.Log("=== Setting up EndlessRunner scene ===");

            // --- Game Managers ---
            GameObject managers = CreateOrFind("GameManagers");

            // GameManager
            GameManager gm = managers.GetComponent<GameManager>();
            if (gm == null) gm = managers.AddComponent<GameManager>();

            // GameSpeed
            GameSpeed gs = managers.GetComponent<GameSpeed>();
            if (gs == null) gs = managers.AddComponent<GameSpeed>();

            // InputManager + KeyboardInput
            InputManager im = managers.GetComponent<InputManager>();
            if (im == null) im = managers.AddComponent<InputManager>();
            KeyboardInput ki = managers.GetComponent<KeyboardInput>();
            if (ki == null) ki = managers.AddComponent<KeyboardInput>();

            // Wire up InputManager's keyboardInput field via reflection
            SetPrivateField(im, "keyboardInput", ki);

            // --- Player ---
            GameObject player = CreateOrFind("Player");
            player.transform.position = new Vector3(0f, 0.5f, 0f);

            // Ensure "Obstacle" tag exists — if not, we'll use layer-based detection
            try { player.tag = "Player"; } catch { }

            // Player visual: use a primitive capsule
            MeshFilter mf = player.GetComponent<MeshFilter>();
            if (mf == null) mf = player.AddComponent<MeshFilter>();
            MeshRenderer mr = player.GetComponent<MeshRenderer>();
            if (mr == null) mr = player.AddComponent<MeshRenderer>();

            // Grab the capsule mesh from a temp primitive
            GameObject tempCapsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            mf.sharedMesh = tempCapsule.GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(tempCapsule);

            // Player material — green
            mr.material = CreateMaterial(new Color(0.2f, 0.85f, 0.4f));

            // Collider — CapsuleCollider as trigger
            CapsuleCollider cc = player.GetComponent<CapsuleCollider>();
            if (cc == null) cc = player.AddComponent<CapsuleCollider>();
            cc.isTrigger = true;
            cc.height = 2f;
            cc.radius = 0.5f;
            cc.center = Vector3.zero;

            // Rigidbody — kinematic, we handle movement manually
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb == null) rb = player.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            // PlayerController
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc == null) pc = player.AddComponent<PlayerController>();
            SetPrivateField(pc, "playerCollider", cc);

            // PlayerCollision
            PlayerCollision pcol = player.GetComponent<PlayerCollision>();
            if (pcol == null) pcol = player.AddComponent<PlayerCollision>();

            // PlayerAnimator
            PlayerAnimator pa = player.GetComponent<PlayerAnimator>();
            if (pa == null) pa = player.AddComponent<PlayerAnimator>();
            SetPrivateField(pa, "playerController", pc);
            SetPrivateField(pa, "meshRenderer", mr);

            // --- Track ---
            GameObject trackParent = CreateOrFind("TrackManager");
            TrackManager tm = trackParent.GetComponent<TrackManager>();
            if (tm == null) tm = trackParent.AddComponent<TrackManager>();

            // --- Camera ---
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.transform.position = new Vector3(0f, 5f, -8f);
                mainCam.transform.rotation = Quaternion.Euler(20f, 0f, 0f);
                mainCam.fieldOfView = 60f;
                mainCam.clearFlags = CameraClearFlags.SolidColor;
                mainCam.backgroundColor = new Color(0.4f, 0.6f, 0.9f); // Light blue sky
            }

            // --- Directional Light ---
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            bool hasDirectionalLight = false;
            foreach (var l in lights)
            {
                if (l.type == LightType.Directional) { hasDirectionalLight = true; break; }
            }
            if (!hasDirectionalLight)
            {
                GameObject lightObj = new GameObject("Directional Light");
                Light light = lightObj.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = new Color(1f, 0.96f, 0.9f);
                light.intensity = 1.2f;
                lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            // --- Quick Start ---
            var starter = managers.GetComponent<AutoStart>();
            if (starter == null) starter = managers.AddComponent<AutoStart>();

            Debug.Log("=== Scene setup complete! Game auto-starts in 0.5s ===");
            Debug.Log("Controls: Arrow keys or WASD to move, Space/Up to jump, Down/S to duck, R to restart");
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
            }
            else
            {
                Debug.LogWarning($"Could not find field '{fieldName}' on {target.GetType().Name}");
            }
        }

        private GameObject CreateOrFind(string name)
        {
            GameObject obj = GameObject.Find(name);
            if (obj == null) obj = new GameObject(name);
            return obj;
        }
    }

    /// <summary>
    /// Auto-starts the game after a short delay and handles restart.
    /// </summary>
    public class AutoStart : MonoBehaviour
    {
        [SerializeField] private float startDelay = 0.5f;

        private void Start()
        {
            Invoke(nameof(DoStart), startDelay);
        }

        private void DoStart()
        {
            if (GameManager.Instance != null && GameManager.Instance.State == GameState.Menu)
            {
                GameManager.Instance.StartGame();
                Debug.Log("Auto-started game! Use arrow keys / WASD to play.");
            }
        }

        private void Update()
        {
            // R to restart after game over
            if (GameManager.Instance != null &&
                GameManager.Instance.State == GameState.GameOver &&
                UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                var player = FindFirstObjectByType<PlayerController>();
                if (player != null) player.ResetPlayer();
                GameManager.Instance.RestartGame();
            }

            // Escape to quit
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            }
        }
    }
}
