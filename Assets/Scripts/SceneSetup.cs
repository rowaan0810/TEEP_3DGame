// SceneSetup.cs — Auto-creates the scene hierarchy for the game.
// Builds: player, camera, track, obstacle spawner, UI (HUD, menus).

using UnityEngine;
using UnityEngine.UI;
using EndlessRunner.Core;
using EndlessRunner.Input;
using EndlessRunner.Player;
using EndlessRunner.Track;
using EndlessRunner.Obstacles;
using EndlessRunner.UI;

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
        /// Find a shader that actually works in Unity 6 URP at runtime.
        /// </summary>
        public static Shader FindWorkingShader()
        {
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
                    return s;
            }

            Renderer existingRenderer = FindFirstObjectByType<Renderer>();
            if (existingRenderer != null && existingRenderer.sharedMaterial != null)
                return existingRenderer.sharedMaterial.shader;

            return Shader.Find("Unlit/Color");
        }

        public static Material CreateMaterial(Color color)
        {
            Shader shader = FindWorkingShader();
            Material mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            mat.color = color;
            return mat;
        }

        [ContextMenu("Setup Scene")]
        public void SetupScene()
        {
            Debug.Log("=== Setting up EndlessRunner scene ===");

            // --- Game Managers ---
            GameObject managers = CreateOrFind("GameManagers");

            GameManager gm = EnsureComponent<GameManager>(managers);
            GameSpeed gs = EnsureComponent<GameSpeed>(managers);
            InputManager im = EnsureComponent<InputManager>(managers);
            KeyboardInput ki = EnsureComponent<KeyboardInput>(managers);

            SetPrivateField(im, "keyboardInput", ki);

            // --- Player ---
            GameObject player = CreateOrFind("Player");
            player.transform.position = new Vector3(0f, 0.5f, 0f);
            try { player.tag = "Player"; } catch { }

            MeshFilter mf = EnsureComponent<MeshFilter>(player);
            MeshRenderer mr = EnsureComponent<MeshRenderer>(player);

            GameObject tempCapsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            mf.sharedMesh = tempCapsule.GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(tempCapsule);

            mr.material = CreateMaterial(new Color(0.2f, 0.85f, 0.4f));

            CapsuleCollider cc = EnsureComponent<CapsuleCollider>(player);
            cc.isTrigger = true;
            cc.height = 2f;
            cc.radius = 0.5f;
            cc.center = Vector3.zero;

            Rigidbody rb = EnsureComponent<Rigidbody>(player);
            rb.isKinematic = true;
            rb.useGravity = false;

            PlayerController pc = EnsureComponent<PlayerController>(player);
            SetPrivateField(pc, "playerCollider", cc);

            PlayerCollision pcol = EnsureComponent<PlayerCollision>(player);
            PlayerAnimator pa = EnsureComponent<PlayerAnimator>(player);
            SetPrivateField(pa, "playerController", pc);
            SetPrivateField(pa, "meshRenderer", mr);

            // --- Track ---
            GameObject trackParent = CreateOrFind("TrackManager");
            TrackManager tm = EnsureComponent<TrackManager>(trackParent);

            // --- Obstacle Spawner ---
            GameObject spawnerObj = CreateOrFind("ObstacleSpawner");
            ObstacleSpawner spawner = EnsureComponent<ObstacleSpawner>(spawnerObj);

            // --- Ensure "Obstacle" tag exists ---
            // The tag must be created in Unity Editor: Edit > Project Settings > Tags
            // For now, PlayerCollision also checks by component if tag is missing.

            // --- UI ---
            GameObject uiObj = CreateOrFind("UIManager");
            EnsureComponent<GameHUD>(uiObj);
            EnsureComponent<GameOverScreen>(uiObj);
            EnsureComponent<MainMenu>(uiObj);

            // --- Camera ---
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.transform.position = new Vector3(0f, 5f, -8f);
                mainCam.transform.rotation = Quaternion.Euler(20f, 0f, 0f);
                mainCam.fieldOfView = 60f;
                mainCam.clearFlags = CameraClearFlags.SolidColor;
                mainCam.backgroundColor = new Color(0.4f, 0.6f, 0.9f);
            }

            // --- Lighting ---
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            bool hasDirectional = false;
            foreach (var l in lights)
                if (l.type == LightType.Directional) { hasDirectional = true; break; }
            if (!hasDirectional)
            {
                GameObject lightObj = new GameObject("Directional Light");
                Light light = lightObj.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = new Color(1f, 0.96f, 0.9f);
                light.intensity = 1.2f;
                lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            // --- Event System for UI ---
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            Debug.Log("=== Scene setup complete! Main menu will appear. ===");
        }

        private static T EnsureComponent<T>(GameObject obj) where T : Component
        {
            T comp = obj.GetComponent<T>();
            if (comp == null) comp = obj.AddComponent<T>();
            return comp;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                field.SetValue(target, value);
        }

        private GameObject CreateOrFind(string name)
        {
            GameObject obj = GameObject.Find(name);
            if (obj == null) obj = new GameObject(name);
            return obj;
        }
    }
}
