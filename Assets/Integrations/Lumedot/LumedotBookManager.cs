using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class BookData
{
    public string title;
    public string description;
    public string coverImageUrl;
    public AuthorData[] authorCatalogue;
    public AudiobookData[] audiobookCatalogue;
}

[System.Serializable]
public class AuthorData
{
    public string name;
}

[System.Serializable]
public class AudiobookData
{
    public string narrator;
    public ChapterData[] chapterCatalogue;
}

[System.Serializable]
public class ChapterData
{
    public string chapterTitle;
    public string fileUrlMp3;
}

[System.Serializable]
public class ApiResponse
{
    public bool success;
    public BookData[] books;
    public int count;
}

public class LumedotBookManager : MonoBehaviour
{
    [Header("API Settings")]
    [SerializeField] private string serverUrl = "https://solcietyserver.vercel.app";
    
    [Header("Book 1 UI")]
    [SerializeField] private TextMeshProUGUI book1Title;
    [SerializeField] private Image book1Cover;
    [SerializeField] private TextMeshProUGUI book1Description;
    [SerializeField] private TextMeshProUGUI book1Author;
    [SerializeField] private Button book1PlayButton;
    
    [Header("Book 2 UI")]
    [SerializeField] private TextMeshProUGUI book2Title;
    [SerializeField] private Image book2Cover;
    [SerializeField] private TextMeshProUGUI book2Description;
    [SerializeField] private TextMeshProUGUI book2Author;
    [SerializeField] private Button book2PlayButton;
    
    [Header("Book 3 UI")]
    [SerializeField] private TextMeshProUGUI book3Title;
    [SerializeField] private Image book3Cover;
    [SerializeField] private TextMeshProUGUI book3Description;
    [SerializeField] private TextMeshProUGUI book3Author;
    [SerializeField] private Button book3PlayButton;
    
    [Header("3D Cube Materials")]
    [SerializeField] private Material cube1Material;
    [SerializeField] private Material cube2Material;
    [SerializeField] private Material cube3Material;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    
    [Header("Player UI")]
    [SerializeField] private GameObject playerUI;
    [SerializeField] private TextMeshProUGUI playerTitle;
    [SerializeField] private TextMeshProUGUI playerAuthor;
    [SerializeField] private Button playerPlayPauseButton;
    [SerializeField] private TextMeshProUGUI playerPlayPauseText;
    [SerializeField] private TextMeshProUGUI playerTimeText;
    [SerializeField] private Button playerCloseButton;
    
    private List<BookData> books = new List<BookData>();
    private Dictionary<int, AudioClip> audioClips = new Dictionary<int, AudioClip>();
    private int currentlyPlaying = -1;
    private bool isPaused = false;
    private float totalDuration = 0f;
    
    // Chapter tracking
    private int currentChapterIndex = 0;
    private ChapterData[] currentBookChapters;
    private bool isProgressingToNextChapter = false;
    
    void Start()
    {
        // Set up audio source
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Set up button listeners
        if (book1PlayButton != null) book1PlayButton.onClick.AddListener(() => PlayBook(0));
        if (book2PlayButton != null) book2PlayButton.onClick.AddListener(() => PlayBook(1));
        if (book3PlayButton != null) book3PlayButton.onClick.AddListener(() => PlayBook(2));
        
        // Set up player UI button listeners
        if (playerPlayPauseButton != null) playerPlayPauseButton.onClick.AddListener(TogglePlayPause);
        if (playerCloseButton != null) playerCloseButton.onClick.AddListener(ClosePlayer);
        
        // Hide player UI initially
        if (playerUI != null) playerUI.SetActive(false);
        
        // Load books
        StartCoroutine(LoadBooks());
    }
    
    IEnumerator LoadBooks()
    {
        // Debug: Show what serverUrl contains
        Debug.Log($"serverUrl variable contains: {serverUrl}");
        
        // Force use the correct server URL
        string url = serverUrl + "/api/sponsored-books";
        Debug.Log($"Loading books from: {url}");
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                Debug.Log("Received response from API");
                
                try
                {
                    ApiResponse response = JsonUtility.FromJson<ApiResponse>(json);
                    
                    if (response.success && response.books != null)
                    {
                        books.Clear();
                        books.AddRange(response.books);
                        
                        Debug.Log($"Loaded {books.Count} books");
                        DisplayBooks();
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error parsing JSON: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"Error loading books: {request.error}");
            }
        }
    }
    
    void DisplayBooks()
    {
        // Display first 3 books
        for (int i = 0; i < Mathf.Min(3, books.Count); i++)
        {
            DisplayBook(i, books[i]);
        }
    }
    
    void DisplayBook(int index, BookData book)
    {
        TextMeshProUGUI titleText = GetTitleText(index);
        Image coverImage = GetCoverImage(index);
        TextMeshProUGUI descriptionText = GetDescriptionText(index);
        TextMeshProUGUI authorText = GetAuthorText(index);
        Button playButton = GetPlayButton(index);
        
        // Set title
        if (titleText != null)
        {
            titleText.text = book.title;
        }
        
        // Set description (truncate if too long)
        if (descriptionText != null)
        {
            string desc = book.description;
            if (desc.Length > 100)
            {
                desc = desc.Substring(0, 97) + "...";
            }
            descriptionText.text = desc;
        }
        
        // Set author
        if (authorText != null && book.authorCatalogue != null && book.authorCatalogue.Length > 0)
        {
            authorText.text = $"By {book.authorCatalogue[0].name}";
        }
        
        // Load cover image
        if (coverImage != null && !string.IsNullOrEmpty(book.coverImageUrl))
        {
            StartCoroutine(LoadCoverImage(book.coverImageUrl, coverImage));
        }
        
        // Load cover image for 3D cube
        Material cubeMaterial = GetCubeMaterial(index);
        if (cubeMaterial != null && !string.IsNullOrEmpty(book.coverImageUrl))
        {
            StartCoroutine(LoadCubeCoverImage(book.coverImageUrl, cubeMaterial));
        }
        
        // Set up play button
        if (playButton != null)
        {
            bool hasAudio = book.audiobookCatalogue != null && 
                           book.audiobookCatalogue.Length > 0 && 
                           book.audiobookCatalogue[0].chapterCatalogue != null && 
                           book.audiobookCatalogue[0].chapterCatalogue.Length > 0;
            
            playButton.interactable = hasAudio;
            
            if (hasAudio)
            {
                // Preload first chapter only initially
                string audioUrl = book.audiobookCatalogue[0].chapterCatalogue[0].fileUrlMp3;
                StartCoroutine(PreloadAudio(index, audioUrl));
            }
        }
    }
    
    IEnumerator LoadCoverImage(string imageUrl, Image targetImage)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                targetImage.sprite = sprite;
            }
        }
    }
    
    IEnumerator PreloadAudio(int bookIndex, string audioUrl)
    {
        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(audioUrl, AudioType.MPEG))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                audioClips[bookIndex] = clip;
                Debug.Log($"Preloaded audio for book {bookIndex}");
            }
        }
    }
    
    void PlayBook(int bookIndex)
    {
        if (bookIndex >= books.Count)
        {
            Debug.LogWarning($"Book index {bookIndex} out of range");
            return;
        }
        
        // Stop current audio
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        // Set up chapter tracking for this book
        BookData book = books[bookIndex];
        if (book.audiobookCatalogue != null && book.audiobookCatalogue.Length > 0)
        {
            currentBookChapters = book.audiobookCatalogue[0].chapterCatalogue;
            currentChapterIndex = 0;
            isProgressingToNextChapter = false;
        }
        else
        {
            Debug.LogWarning("No chapters available for this book");
            return;
        }
        
        // Play new audio
        if (audioClips.ContainsKey(bookIndex))
        {
            audioSource.clip = audioClips[bookIndex];
            audioSource.Play();
            currentlyPlaying = bookIndex;
            isPaused = false;
            totalDuration = audioSource.clip.length;
            
            // Show player UI
            ShowPlayerUI(books[bookIndex]);
            
            Debug.Log($"Playing: {books[bookIndex].title} - Chapter {currentChapterIndex + 1}");
            UpdatePlayButtons();
        }
        else
        {
            Debug.LogWarning("Audio not loaded for this book");
        }
    }
    
    void UpdatePlayButtons()
    {
        Button[] buttons = { book1PlayButton, book2PlayButton, book3PlayButton };
        
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                TextMeshProUGUI buttonText = buttons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = (i == currentlyPlaying) ? "Playing..." : "Play";
                }
            }
        }
    }
    
    // Helper methods to get UI elements
    TextMeshProUGUI GetTitleText(int index)
    {
        switch (index)
        {
            case 0: return book1Title;
            case 1: return book2Title;
            case 2: return book3Title;
            default: return null;
        }
    }
    
    Image GetCoverImage(int index)
    {
        switch (index)
        {
            case 0: return book1Cover;
            case 1: return book2Cover;
            case 2: return book3Cover;
            default: return null;
        }
    }
    
    TextMeshProUGUI GetDescriptionText(int index)
    {
        switch (index)
        {
            case 0: return book1Description;
            case 1: return book2Description;
            case 2: return book3Description;
            default: return null;
        }
    }
    
    TextMeshProUGUI GetAuthorText(int index)
    {
        switch (index)
        {
            case 0: return book1Author;
            case 1: return book2Author;
            case 2: return book3Author;
            default: return null;
        }
    }
    
    Button GetPlayButton(int index)
    {
        switch (index)
        {
            case 0: return book1PlayButton;
            case 1: return book2PlayButton;
            case 2: return book3PlayButton;
            default: return null;
        }
    }
    
    Material GetCubeMaterial(int index)
    {
        switch (index)
        {
            case 0: return cube1Material;
            case 1: return cube2Material;
            case 2: return cube3Material;
            default: return null;
        }
    }
    
    IEnumerator LoadCubeCoverImage(string imageUrl, Material targetMaterial)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                targetMaterial.mainTexture = texture;
                Debug.Log($"Loaded cube cover image: {imageUrl}");
            }
            else
            {
                Debug.LogError($"Failed to load cube cover image: {request.error}");
            }
        }
    }
    
    // Public methods for external control
    public void StopAudio()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            currentlyPlaying = -1;
            UpdatePlayButtons();
        }
        ClosePlayer();
    }
    
    public void PauseAudio()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            isPaused = true;
        }
        else if (audioSource.clip != null)
        {
            audioSource.UnPause();
            isPaused = false;
        }
        UpdatePlayerUI();
    }
    
    // Player UI methods
    void ShowPlayerUI(BookData book)
    {
        if (playerUI != null)
        {
            playerUI.SetActive(true);
            
            // Set title and author
            if (playerTitle != null) playerTitle.text = book.title;
            if (playerAuthor != null && book.authorCatalogue != null && book.authorCatalogue.Length > 0)
            {
                playerAuthor.text = $"By {book.authorCatalogue[0].name}";
            }
            
            UpdatePlayerUI();
        }
    }
    
    void ClosePlayer()
    {
        if (playerUI != null)
        {
            playerUI.SetActive(false);
        }
        StopAudio();
    }
    
    void TogglePlayPause()
    {
        if (audioSource.clip != null)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
                isPaused = true;
            }
            else
            {
                audioSource.UnPause();
                isPaused = false;
            }
            UpdatePlayerUI();
        }
    }
    
    void UpdatePlayerUI()
    {
        // Update play/pause button text
        if (playerPlayPauseText != null)
        {
            playerPlayPauseText.text = isPaused ? "Play" : "Pause";
        }
        
        // Update time display
        if (playerTimeText != null && audioSource.clip != null)
        {
            string currentTime = FormatTime(audioSource.time);
            string totalTime = FormatTime(totalDuration);
            playerTimeText.text = $"{currentTime} / {totalTime}";
        }
    }
    
    string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    
    void Update()
    {
        // Update player UI time display every frame when playing
        if (audioSource.isPlaying && playerUI != null && playerUI.activeInHierarchy)
        {
            UpdatePlayerUI();
        }
        
        // Auto-progress to next chapter when current chapter finishes
        if (audioSource.clip != null && !audioSource.isPlaying && !isPaused && currentlyPlaying != -1 && !isProgressingToNextChapter)
        {
            if (currentChapterIndex < currentBookChapters.Length - 1)
            {
                // Move to next chapter
                currentChapterIndex++;
                isProgressingToNextChapter = true;
                StartCoroutine(LoadAndPlayNextChapter());
            }
            else
            {
                // Book finished
                currentlyPlaying = -1;
                UpdatePlayButtons();
                ClosePlayer();
            }
        }
    }
    
    IEnumerator LoadAndPlayNextChapter()
    {
        if (currentBookChapters == null || currentChapterIndex >= currentBookChapters.Length)
        {
            isProgressingToNextChapter = false;
            yield break;
        }
        
        ChapterData nextChapter = currentBookChapters[currentChapterIndex];
        string audioUrl = nextChapter.fileUrlMp3;
        
        Debug.Log($"Loading next chapter: {nextChapter.chapterTitle}");
        
        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(audioUrl, AudioType.MPEG))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                audioSource.clip = clip;
                totalDuration = clip.length;
                isPaused = false;
                
                // Small delay to ensure audio is ready
                yield return new WaitForSeconds(0.1f);
                
                audioSource.Play();
                isProgressingToNextChapter = false;
                
                Debug.Log($"Now playing: {nextChapter.chapterTitle}");
                UpdatePlayerUI();
            }
            else
            {
                Debug.LogError($"Failed to load chapter: {request.error}");
                isProgressingToNextChapter = false;
                // Try to continue with next chapter
                if (currentChapterIndex < currentBookChapters.Length - 1)
                {
                    currentChapterIndex++;
                    StartCoroutine(LoadAndPlayNextChapter());
                }
            }
        }
    }
} 