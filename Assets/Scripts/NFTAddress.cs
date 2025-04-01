using UnityEngine;
using System.Collections;

public class NFTAddress : MonoBehaviour
{
    public string nftAddress;
    public Transform targetImage;
    public string nftUri;
    public void MoveToTarget()
    {
        if (targetImage) StartCoroutine(MoveTowardsTarget(targetImage.position, 60f));
        else Debug.LogError("❌ Target image is not assigned!");
    }

    private IEnumerator MoveTowardsTarget(Vector3 target, float speed)
    {
        float time = 0f;
        Vector3 start = transform.position;

        while (time < 1f)
        {
            time += Time.deltaTime * speed; // Speed controls how fast it moves
            transform.position = Vector3.Lerp(start, target, time);
            yield return null;
        }
        transform.position = target; // Ensure exact final position
    }

}