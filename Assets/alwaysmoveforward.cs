using UnityEngine;

public class AlwaysMoveForward : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of movement
    public Vector3 moveDirection = Vector3.forward; // Direction to move in

    private Rigidbody myRigidbody;

    void Start()
    {
        // Get the Rigidbody component if it exists
        myRigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Normalize the move direction to ensure consistent speed
        Vector3 normalizedDirection = moveDirection.normalized;

        if (myRigidbody != null)
        {
            // Move the object using Rigidbody velocity
            myRigidbody.linearVelocity = normalizedDirection * moveSpeed;
        }
        else
        {
            // Move the object using Transform (fallback if no Rigidbody)
            transform.position += normalizedDirection * moveSpeed * Time.deltaTime;
        }
    }
}