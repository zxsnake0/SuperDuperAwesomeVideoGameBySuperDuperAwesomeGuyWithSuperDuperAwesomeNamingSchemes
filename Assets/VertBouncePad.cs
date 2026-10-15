using UnityEngine;

public class VertBouncePad : MonoBehaviour
{
    [SerializeField] private float bounceForce = 15f; // Adjust strength in the Inspector

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the object colliding with the pad is the player
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            
            if (playerRb != null)
            {
                // Reset Y velocity first so the bounce is consistent regardless of fall speed
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0f);

                // Apply an instant upward impulse force
                playerRb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);

            }
        }
    }
}