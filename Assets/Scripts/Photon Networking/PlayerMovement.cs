using Fusion;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    // Horizontal movement and jump settings
    public float PlayerSpeed = 2f;
    public float JumpForce = 5f;
    public float GravityValue = -9.81f;

    // Define the ground level; if player's y <= GroundLevel, they're considered grounded.
    public float GroundLevel = -12f;

    // Internal state for vertical velocity and jump input flag
    private Vector3 _velocity;
    private bool _jumpPressed;

    // Detect jump input in Update (called every frame)
    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            _jumpPressed = true;
        }
    }

    // FixedUpdateNetwork is called on the StateAuthority and used to update simulation consistently.
    public override void FixedUpdateNetwork()
    {
        // Check if the player is grounded based on the y position.
        bool isGrounded = transform.position.y <= GroundLevel;

        if (isGrounded)
        {
            //Debug.Log("Player is grounded");
            // Reset vertical velocity when grounded.
            _velocity.y = 0f;

            // Optional: Snap the player to the ground level if they've fallen below.
            if (transform.position.y < GroundLevel)
            {
                Vector3 pos = transform.position;
                pos.y = GroundLevel;
                transform.position = pos;
            }
        }

        // Read horizontal movement from Input axes.
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Calculate the horizontal movement vector.
        Vector3 move = new Vector3(horizontal, 0, vertical) * Runner.DeltaTime * PlayerSpeed;

        // If jump was pressed and the player is grounded, set the vertical velocity.
        if (_jumpPressed && isGrounded)
        {
            _velocity.y = JumpForce;
        }

        // Apply gravity to the vertical velocity.
        _velocity.y += GravityValue * Runner.DeltaTime;

        // Update the player's position using both horizontal movement and vertical velocity.
        transform.position += move + _velocity * Runner.DeltaTime;

        // If there's horizontal movement, rotate the player to face that direction.
        Vector3 horizontalMove = new Vector3(move.x, 0, move.z);        

        // Reset jump input after processing.
        _jumpPressed = false;
    }
}
