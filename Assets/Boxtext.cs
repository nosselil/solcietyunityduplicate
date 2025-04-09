using UnityEngine;

using System.Collections;

using TMPro;


public class TextMeshFader : MonoBehaviour
{
    public GameObject[] textMeshes; // Array of TextMesh GameObjects
    public float fadeDistance = 5f; // Distance within which text fades in
    public float fadeSpeed = 2f; // Speed of fading

    void Update()
    {
        float playerDistance = GetClosestDistance();

        // Determine if the text should fade in or out
        bool isPlayerNear = playerDistance <= fadeDistance;

        // Null guard in case this hasn't been initialized for the current scene
        if (textMeshes == null)
            return;

        // Adjust the alpha of all TextMesh objects
        foreach (GameObject textMeshObj in textMeshes)
        {
            if (textMeshObj.TryGetComponent<TextMesh>(out TextMesh textMesh))
            {
                Color color = textMesh.color;
                float targetAlpha = isPlayerNear ? 1f : 0f; // 1 = visible, 0 = invisible

                // Smoothly fade the alpha using MoveTowards
                color.a = Mathf.MoveTowards(color.a, targetAlpha, fadeSpeed * Time.deltaTime);

                textMesh.color = color;
            }
        }
    }

    // Calculate the closest distance between the player and any text object
    private float GetClosestDistance()
    {
        float closestDistance = float.MaxValue;

        if (textMeshes == null)
            return Mathf.Infinity;

        foreach (GameObject textMeshObj in textMeshes)
        {
            float distance = Vector3.Distance(this.transform.position, textMeshObj.transform.position);
            if (distance < closestDistance)
                closestDistance = distance;
        }

        return closestDistance;
    }
}
