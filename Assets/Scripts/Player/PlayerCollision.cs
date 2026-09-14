// PlayerCollision.cs — Handles collision detection for the player.
// When the player hits an obstacle, triggers game over.

using UnityEngine;
using EndlessRunner.Core;

namespace EndlessRunner.Player
{
    /// <summary>
    /// Detects when the player collides with obstacles and triggers game over.
    /// Attach this to the player GameObject alongside PlayerController.
    /// </summary>
    public class PlayerCollision : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string obstacleTag = "Obstacle";

        /// <summary>
        /// Whether the player is currently shielded (from Pose Challenge power-up).
        /// When shielded, the first collision is absorbed instead of ending the game.
        /// </summary>
        public bool IsShielded { get; set; }

        private void OnTriggerEnter(Collider other)
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
                return;

            if (other.CompareTag(obstacleTag))
            {
                if (IsShielded)
                {
                    // Shield absorbs the hit
                    IsShielded = false;
                    Debug.Log("Shield absorbed a collision!");

                    // Optionally destroy/disable the obstacle that was hit
                    other.gameObject.SetActive(false);
                    return;
                }

                // Game over
                Debug.Log($"Player hit obstacle: {other.gameObject.name}");
                GameManager.Instance.TriggerGameOver();
            }
        }

        /// <summary>
        /// Reset collision state. Called on game restart.
        /// </summary>
        public void ResetCollision()
        {
            IsShielded = false;
        }
    }
}
