// PlayerAnimator.cs — Drives animation states based on player movement.
// For Week 1 this uses placeholder visual changes; later it will drive a proper Animator.

using UnityEngine;

namespace EndlessRunner.Player
{
    /// <summary>
    /// Manages player visual states based on the PlayerController's current state.
    /// In Week 1 this does simple visual changes (scale, color).
    /// In Week 5 this will drive a proper Animator controller with Mixamo animations.
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private MeshRenderer meshRenderer;

        [Header("Visual Feedback Colors")]
        [SerializeField] private Color runColor = new Color(0.2f, 0.8f, 0.3f);    // Green while running
        [SerializeField] private Color jumpColor = new Color(0.3f, 0.6f, 1.0f);   // Blue while jumping
        [SerializeField] private Color duckColor = new Color(1.0f, 0.8f, 0.2f);   // Yellow while ducking
        [SerializeField] private Color fallColor = new Color(0.9f, 0.4f, 0.3f);   // Red while falling

        private Material playerMaterial;

        private void Start()
        {
            if (meshRenderer != null)
            {
                // Create instance material so we don't modify the shared one
                playerMaterial = new Material(meshRenderer.material);
                meshRenderer.material = playerMaterial;
                playerMaterial.color = runColor;
            }
        }

        private void Update()
        {
            if (playerController == null || playerMaterial == null) return;

            // Update color based on state (placeholder for proper animation)
            Color targetColor;

            if (playerController.IsDucking)
                targetColor = duckColor;
            else if (playerController.IsJumping)
                targetColor = jumpColor;
            else if (playerController.IsFalling)
                targetColor = fallColor;
            else
                targetColor = runColor;

            playerMaterial.color = Color.Lerp(playerMaterial.color, targetColor, Time.deltaTime * 10f);
        }

        private void OnDestroy()
        {
            // Clean up instance material
            if (playerMaterial != null)
                Destroy(playerMaterial);
        }
    }
}
