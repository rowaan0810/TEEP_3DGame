// TrackManager.cs — Manages the infinite scrolling track.
// Object-pools track segments and recycles them as they pass behind the camera.

using System.Collections.Generic;
using UnityEngine;
using EndlessRunner.Core;

namespace EndlessRunner.Track
{
    /// <summary>
    /// Spawns and recycles track segments to create an infinite scrolling road.
    /// The player stays at Z=0, and the world moves toward them.
    /// </summary>
    public class TrackManager : MonoBehaviour
    {
        [Header("Track Settings")]
        [SerializeField] private GameObject trackSegmentPrefab;
        [SerializeField] private int poolSize = 8;
        [SerializeField] private float segmentLength = 20f;

        [Header("Lane Visual Settings")]
        [SerializeField] private float laneWidth = 2.5f;

        private List<TrackSegment> segments = new List<TrackSegment>();
        private float nextSpawnZ;
        private float cameraPosZ;

        // Cached shader for material creation
        private static Shader cachedShader;

        private void Start()
        {
            cameraPosZ = Camera.main != null ? Camera.main.transform.position.z : 0f;
            InitializeTrack();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameRestart.AddListener(ResetTrack);
            }
        }

        private void InitializeTrack()
        {
            nextSpawnZ = -segmentLength;

            for (int i = 0; i < poolSize; i++)
            {
                SpawnSegment();
            }
        }

        private void SpawnSegment()
        {
            GameObject segObj;

            if (trackSegmentPrefab != null)
            {
                segObj = Instantiate(trackSegmentPrefab, transform);
            }
            else
            {
                segObj = CreatePlaceholderSegment();
            }

            segObj.transform.position = new Vector3(0f, 0f, nextSpawnZ);
            nextSpawnZ += segmentLength;

            TrackSegment segment = segObj.GetComponent<TrackSegment>();
            if (segment == null)
            {
                segment = segObj.AddComponent<TrackSegment>();
            }
            segments.Add(segment);
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
                return;

            for (int i = 0; i < segments.Count; i++)
            {
                if (segments[i].IsBehindCamera(cameraPosZ))
                {
                    RecycleSegment(segments[i]);
                }
            }
        }

        private void RecycleSegment(TrackSegment segment)
        {
            segment.transform.position = new Vector3(0f, 0f, nextSpawnZ);
            nextSpawnZ += segmentLength;
        }

        /// <summary>
        /// Find a shader that works at runtime in Unity 6 URP.
        /// </summary>
        private static Shader GetShader()
        {
            if (cachedShader != null) return cachedShader;

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
                    cachedShader = s;
                    return s;
                }
            }

            // Grab shader from any existing renderer
            Renderer existing = FindFirstObjectByType<Renderer>();
            if (existing != null && existing.sharedMaterial != null)
            {
                cachedShader = existing.sharedMaterial.shader;
                return cachedShader;
            }

            cachedShader = Shader.Find("Unlit/Color");
            return cachedShader;
        }

        private static Material MakeMaterial(Color color)
        {
            Material mat = new Material(GetShader());
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", color);
            mat.color = color;
            return mat;
        }

        private GameObject CreatePlaceholderSegment()
        {
            GameObject segObj = new GameObject("TrackSegment");

            // Main road surface
            GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.transform.SetParent(segObj.transform);
            road.transform.localPosition = new Vector3(0f, -0.05f, segmentLength / 2f);
            road.transform.localScale = new Vector3(laneWidth * 3f + 1f, 0.1f, segmentLength);

            Renderer roadRenderer = road.GetComponent<Renderer>();
            if (roadRenderer != null)
            {
                roadRenderer.material = MakeMaterial(new Color(0.25f, 0.25f, 0.3f));
            }

            // Remove collider from road (not needed for physics)
            Collider roadCollider = road.GetComponent<Collider>();
            if (roadCollider != null) Destroy(roadCollider);

            // Lane divider lines
            CreateLaneLine(segObj.transform, -laneWidth / 2f);
            CreateLaneLine(segObj.transform, laneWidth / 2f);

            // Edge lines
            CreateLaneLine(segObj.transform, -laneWidth * 1.5f, true);
            CreateLaneLine(segObj.transform, laneWidth * 1.5f, true);

            return segObj;
        }

        private void CreateLaneLine(Transform parent, float xPos, bool isEdge = false)
        {
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.transform.SetParent(parent);
            line.transform.localPosition = new Vector3(xPos, 0.01f, segmentLength / 2f);
            line.transform.localScale = new Vector3(isEdge ? 0.15f : 0.08f, 0.02f, segmentLength);

            Renderer lineRenderer = line.GetComponent<Renderer>();
            if (lineRenderer != null)
            {
                Color lineColor = isEdge ? new Color(1f, 0.85f, 0.2f) : Color.white;
                lineRenderer.material = MakeMaterial(lineColor);
            }

            Collider lineCollider = line.GetComponent<Collider>();
            if (lineCollider != null) Destroy(lineCollider);
        }

        public void ResetTrack()
        {
            foreach (var seg in segments)
            {
                if (seg != null) Destroy(seg.gameObject);
            }
            segments.Clear();

            nextSpawnZ = -segmentLength;
            for (int i = 0; i < poolSize; i++)
            {
                SpawnSegment();
            }
        }
    }
}
