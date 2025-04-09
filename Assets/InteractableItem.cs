using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class InteractableItem : MonoBehaviour
{
    public float interactionDistance = 2f;
    public string interactionText = "Press E to Enter Gallery"; // Updated text
    public TextMeshProUGUI interactionDisplay;
    private Transform player;
    private bool canInteract = false;
    public string sceneToLoad = "Gallery"; // Hardcoded scene name (can be made public if needed)

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform; // Safe null check
        if (interactionDisplay != null)
        {
            interactionDisplay.gameObject.SetActive(false);
        }
        else
        {
          //  Debug.LogError("Interaction Display TextMeshProUGUI is not assigned!");
        }

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("Scene to load is not set! Make sure 'Gallery' is in Build Settings.");
        }
    }

    void Update()
    {
        if (LocalChatWindowController.Instance == null || LocalChatWindowController.Instance.IsChatWindowActive)        
            return;

        if (player == null)
        {            
            player = GameObject.FindGameObjectWithTag("Player")?.transform; // Safe null check // TODO: Refactor later on, will work for now
            Debug.LogError("Player not found! Make sure the player has the tag 'Player'.");
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= interactionDistance)
        {
            if (!canInteract)
            {
                canInteract = true;
                if (interactionDisplay != null)
                {
                    interactionDisplay.text = interactionText;
                    interactionDisplay.gameObject.SetActive(true);
                }
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                Interact();
            }
        }
        else
        {
            if (canInteract)
            {
                canInteract = false;
                if (interactionDisplay != null)
                {

                    interactionDisplay.gameObject.SetActive(false);
                }
            }
        }
    }

    void Interact()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("Scene to load is not set! Make sure 'Gallery' is in Build Settings. " +gameObject.name);
            return;
        }

        Debug.Log("Interacting... Loading scene: " + sceneToLoad + gameObject.name);
        SceneManager.LoadSceneAsync(sceneToLoad); // Asynchronous loading
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}