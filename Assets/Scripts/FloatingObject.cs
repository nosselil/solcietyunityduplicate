using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    public float amplitude = 0.5f; // How far up and down
    public float speed = 1f;       // How fast

    private float startY;

    void Start()
    {
        startY = transform.position.y;
    }

    void Update()
    {
        Vector3 pos = transform.position;
        pos.y = startY + Mathf.Sin(Time.time * speed) * amplitude;
        transform.position = pos;
    }
} 