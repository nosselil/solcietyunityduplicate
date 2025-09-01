using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Fusion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PixelCrushers.DialogueSystem;
using Solana.Unity.Metaplex.MplNftPacks.Program;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static System.Net.WebRequestMethods;

public class SlideShowController : NetworkBehaviour
{
    class SlideEntry
    {
        public string Url;
        public Texture2D Texture;
    }


    private bool anySlideFailed = false;

    [Networked, HideInInspector, OnChangedRender(nameof(OnControllingPlayerChanged))]
    public PlayerRef ControllingPlayer { get ; set; }    // who may change slides
    
    [Networked, OnChangedRender(nameof(OnSlideIndexChanged))]
    public int SlideIndex { get => default; set { } }    // current slide

    [Networked]
    public bool IsSlideShowActive { get => default; set { } }

    bool initialized = false;

    bool isAuthComplete = false;
    string[] slideIds;
    bool slideIdsReady = false;
    bool[] thumbUrlsReady;

    [Networked, Capacity(24), OnChangedRender(nameof(OnSlideUrlsChanged))] // Sets the fixed capacity of the collection
    NetworkArray<NetworkString<_256>> SlideUrls { get; } = MakeInitializer(new NetworkString<_256>[] 
    { /*"https://lh7-us.googleusercontent.com/docsdf/AFQj2d4DPSTQSxou8jtnhqzfd-0MvdYDgZ4Zg-yHdAEbcmFErEYjD2eIOhbppGnZLKD6iY6Mdp9dNqIUBA7jrIQ5DXwZtdNfg0o-VDycAk8Kp-CNFt5xPwfhHHiYnCEX3iZbywvixheWWJQ6RZaTvOL_xB-SPYZMkpRkEwnqbBlnRNHGxeAt=s800",
      "https://lh7-us.googleusercontent.com/docsdf/AFQj2d5zaTzBgTuC_1aqeEoaxnjVuUNB9fH9jSfDl_8C1eomaL3dRKypGPlBNjFnmsMKiYTygtAWpliqbwPqOYP-f99IRMN-hB0UQ5nKqKUzsCOvBWoA8Y-kvysTzRMxU5cPRvj8hZxByvjrFHHbH8iqGF_BBs9AGB9rbwO_kkSpor0W-iDV=s800"*/});    

    bool[] slideUrlsReady;

    int slideCount = 0; // A local variable to keep track of the total slides

    /*List<string> slideUrls;                            // thumbnail URLs
    Dictionary<int, Texture2D> slideCache = new();     // cached textures*/

    /*[SerializeField]
    Texture2D[] slideTextures;*/

    Dictionary<int, SlideEntry> slides = new();

    [SerializeField]
    Renderer slideRenderer;                             // display target

    [SerializeField]
    SlideShowControllerInteractionArea interactionArea;

    [SerializeField]
    GameObject projectorControlGUI, setupControlGUI, setupInfoText, presentationControlGUI, projectorImageGO, notificationCanvasGO;

    [SerializeField]
    Button previousSlideButton, nextSlideButton;

    [SerializeField]
    TMP_InputField slideDownloadUrlInputField;

    [SerializeField]
    TextMeshProUGUI notificationText;

    MeshRenderer projectorImageMeshRenderer;
    
    [SerializeField]
    Usable usable;

    //string apiKey = "AIzaSyB7MNydTlHyTURQbufpZzJAe2wf0SHln0U"; // TODO: Secure this

    public void OnSlideUrlsChanged()
    {
        //Debug.Log("SLIDE DEBUG: Slide urls 2 contains " + SlideUrls[2].ToString());        
    }


    public override void Spawned()
    {
        if (ControllingPlayer == null)
            ControllingPlayer = PlayerRef.None;

        //ControllingPlayer = PlayerRef.None;             // no controller at start
        //slideCache.Add(0, slideTextures[0]);
        //slideCache.Add(1, slideTextures[1]);

        //slideUrls = new List<string>(2);

        Debug.Log("SLIDE CONTROLLER: Spawned, controlling player is " + ControllingPlayer);

        projectorImageMeshRenderer = projectorImageGO.GetComponent<MeshRenderer>();
        projectorImageGO.SetActive(IsSlideShowActive);

        // Check a networked boolean like IsSlideShowActive to be able to set the visibility of the projectorImageGo correctly
        if (IsSlideShowActive)
        {
            Debug.Log("SLIDE SHOW: Active already, need to download slide URLs: " + SlideUrls.ToArray().ToString());
            DownloadSlides();
            //ApplySlideChange();
        }
        
        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
            return;

        /*if (Input.GetKeyDown(KeyCode.F))
        {
            //UpdateProjectorImageTexture(default);
            //string randomString = UnityEngine.Random.Range(0, 100000).ToString("D5");
            //            "D5" pads with leading zeros so the length is always 5

            // Commit it to the networked array
            //SlideUrls.Set(2, randomString);
            //RequestProjectorControls();
        }*/
    }

    #region General Controls



    #endregion

    void OpenProjectorGUI()
    {
        Debug.Log("SLIDE CONTROLLER: Open projector GUI");
        projectorControlGUI.SetActive(true);
        setupInfoText.transform.parent.gameObject.SetActive(true);
        setupControlGUI.SetActive(true);
        presentationControlGUI.SetActive(false);
    }

    public void CloseProjectorControls()
    {
        if (ControllingPlayer == Runner.LocalPlayer)
        {
            ReleaseControlRpc();
            projectorControlGUI.SetActive(false);
            projectorImageGO.SetActive(false);
            // NOTE: Is it possible that in some cases, the controls are not released?
        }
    }

    #region Google Api callbacks

    public void OnAuthSuccess()
    {
        Debug.Log("Auth succeeded - continuing download");
        isAuthComplete = true;
    }

    public void OnSlidesListed(string json)
    {
        try
        {
            slideIds = JsonConvert.DeserializeObject<string[]>(json);
            slideIdsReady = true;

            // Prepare parallel arrays for URLs and ready-flags
            //slideUrls.Clear();
            //slideUrlsReady = new bool[slideIds.Length];

            Debug.Log($"OnSlidesListed: received {slideIds.Length} slide IDs");
        }
        catch (Exception e)
        {
            Debug.LogError("OnSlidesListed: JSON parse failed: " + e);
        }
    }

    public void OnThumbnailUrlReceived(string message)
    {
        // expect "pageId|https://..."
        int pipe = message.IndexOf('|');
        if (pipe < 0)
        {
            Debug.LogError("OnThumbnailUrlReceived: bad message format");
            return;
        }

        string pageId = message.Substring(0, pipe);
        string url = message.Substring(pipe + 1);

        // TODO: We could just supply the index as a parameter I suppose
        // find the slide index that matches this pageId
        int idx = Array.IndexOf(slideIds, pageId);
        if (idx < 0)
        {
            Debug.LogError($"OnThumbnailUrlReceived: unknown pageId {pageId}");
            return;
        }

        SlideUrls.Set(idx, url);
        slideUrlsReady[idx] = true;

        Debug.Log($"OnThumbnailUrlReceived: slide {idx} URL ready");
    }

    #endregion

    #region Slide Preparation

    public void PrepareSlideDeck()
    {
        
        string shareUrl =
            //"https://docs.google.com/presentation/d/1mal-rfHMSLX-l2p2Gvog8OZcm7vFHELusi5vO_jcW-Y/edit?usp=sharing"; // MOCK
            slideDownloadUrlInputField.text;

        //"https://docs.google.com/presentation/d/1TZ0A1z2Am7RQpWzS4bDYEWcZ0Ke0Z16UP3UNcSVgIpw/edit?usp=sharing";
        //"https://docs.google.com/presentation/d/1mal-rfHMSLX-l2p2Gvog8OZcm7vFHELusi5vO_jcW-Y/edit?usp=sharing"; //; //https://docs.google.com/presentation/d/1mal-rfHMSLX-l2p2Gvog8OZcm7vFHELusi5vO_jcW-Y/edit?usp=sharing
        try
        {
            string presentationId = ExtractPresentationId(shareUrl);            
            StartCoroutine(FetchingSlides(presentationId));
            setupInfoText.transform.parent.gameObject.SetActive(false);
            notificationCanvasGO.SetActive(true);
            notificationText.text = "Preparing slide show...";
        }
        catch (Exception e)
        {
            notificationCanvasGO.SetActive(true);
            notificationText.text = "There was an error downloading your slideshow. Please make sure your share URL begins with the format https://docs.google.com/presentation/d/{PRESENTATION_ID}, and is set to public.";
            CancelInvoke("HideNotificationCanvas");
            Invoke("HideNotificationCanvas", 3.0f);

            Debug.Log("There was an error downloading your slideshow. Please make sure your share URL begins with the format https://docs.google.com/presentation/d/{PRESENTATION_ID}");
        }        
    }    

    private string ExtractPresentationId(string shareUrl)
    {
        if (string.IsNullOrEmpty(shareUrl))
            throw new ArgumentException(nameof(shareUrl) + " cannot be null or empty");

        var match = Regex.Match(shareUrl, @"/d/([^/]+)");
        if (!match.Success)
            throw new ArgumentException("Invalid Google Slides URL", nameof(shareUrl));

        return match.Groups[1].Value;
    }

    private IEnumerator FetchingSlides(string presentationId)
    {
        int total;

        // MOCK IMPLEMENTATION FOR TESTING                
        /*total = 1; //slideIds.Length;        
        slideUrlsReady = new bool[total];

        SlideUrls.Set(0, "https://lh7-us.googleusercontent.com/docsdf/AFQj2d6tE19qYy4y6_C_ZzCfDucWujme9BzHdFOkAXFOsyQs4qNox50Sv01tUfwWnwLUq88S3hbNSmRzCNmk7wwSlrGCm-wbEWAJCg4pjhKUCObyu1mN3ISScMUNDA4c4aodFAt_-_Kt967FBOOkNkFWUiIpW5jch0rMyDcL2NPy5unJAd89=s800");
        //SlideUrls.Set(1, "https://lh7-us.googleusercontent.com/docsdf/AFQj2d5Hr72PC8LreuLS-BPxHCd5an0kN43ysxGwit8B4SYKWQGSzdOUmcpWHjnoOlFQbEQkkvOLu2_XMtT4DhsyvBg9IzKKO4StnpRrX2PYpQc7a4-bCrlRz4yNAR3zK_0fka3eKJjQ8ZDQYoTQGXy_gB4y_7V23UO9cVFkBrsqE7BxwsVb=s800");
        //SlideUrls.Set(2, "https://lh7-us.googleusercontent.com/docsdf/AFQj2d5ZxPLz-5zfAXNWb8X3n-K-Mr4b505Zc7DucRaC1RHKk6h8JCbCSwq3WklJSZk7GG1DE47q8oL8dQkgW9Wg0Zgh9scJKHPcOuGk0QIzGQ0OawMgGMiXu-mR-C8gop_-za4JO-PumKRqi8CP8gnxpx0C7ovbCNyVbygAk68xxhudX1SX=s800");

        ActivateSlideShowRpc();
        ShowPresentationControls();
        DownloadSlidesRpc();

        yield break; //*/
        // END OF MOCK IMPLEMENTATION


        // 1) kick off OAuth
        GoogleApiController.Login();

        // 2) wait for auth
        yield return new WaitUntil(() => isAuthComplete);

        Debug.Log("SLIDE CONTROLLER: Proceed to fetching slide ids for presentation id " + presentationId);
        // 3) request slide IDs
        slideIdsReady = false;
        GoogleApiController.ListSlides(presentationId);
        yield return new WaitUntil(() => slideIdsReady);

        Debug.Log("SLIDE CONTROLLER: Slides for presentation id " + presentationId + " are ready");

        total = slideIds.Length;
        SlideUrls.Clear();
        slideUrlsReady = new bool[total];

        // 3. Kick off per-slide URL fetch with exponential back-off 
        for (int i = 0; i < total; i++)
            StartCoroutine(FetchThumbnailUrlWithRetry(i, slideIds[i], presentationId));

        // 4. Wait until every URL slot is filled
        yield return new WaitUntil(() =>
        {
            for (int i = 0; i < total; i++)
                if (!slideUrlsReady[i]) return false;
            Debug.Log("SLIDE CONTROLLER: All thumbnail URLs resolved — start downloading textures.");
            return true;
        });

        // ActivateSlideShowRpc();        
        ShowPresentationControls();

        // 6) broadcast the URLs to all clients

        // MOCK IMPLEMENTATION
        //slideUrls = new string[1];
        //slideUrls[0] = https://lh7-us.googleusercontent.com/docsdf/AFQj2d78wI_Yr6G_kYRIMAagSKqinGWEHNuqwdrT02vB-5am46_ZsmCy0Hd5hVp3t0FNPy_ycJ2ezPuyjaZgjjrtY9bmM7hn2QTY1masFr63kMjrQa21BL3UVwNLDv3ZbTJ2sdRIaOtbj3nSbVBNfD5VD2UKDvLHfR-UIe-r0AAO8wnJ5Gb9=s800"        
        //slideUrls[1] = "https://lh7-us.googleusercontent.com/docsdf/AFQj2d5zaTzBgTuC_1aqeEoaxnjVuUNB9fH9jSfDl_8C1eomaL3dRKypGPlBNjFnmsMKiYTygtAWpliqbwPqOYP-f99IRMN-hB0UQ5nKqKUzsCOvBWoA8Y-kvysTzRMxU5cPRvj8hZxByvjrFHHbH8iqGF_BBs9AGB9rbwO_kkSpor0W-iDV=s800";*/
        
        
        DownloadSlidesRpc();

        yield break;
    }

    private IEnumerator FetchThumbnailUrlWithRetry(int index, string pageId, string presId)
    {
        int delay = 1;               // seconds: 1,2,4,8,16,32
        const int maxDelay = 32;
        const float responseTimeout = 10f;   // secs to wait for a JS reply

        while (!slideUrlsReady[index])
        {
            /* ask JS for the URL */
            GoogleApiController.GetThumbnailUrl(presId, pageId);

            /* wait up to responseTimeout for JS to call OnThumbnailUrlReceived */
            float t = 0f;
            while (t < responseTimeout && !slideUrlsReady[index])
            {
                yield return null;
                t += Time.deltaTime;
            }

            if (slideUrlsReady[index]) break;   // success

            Debug.LogError($"Slide {index}: thumbnail URL not returned, retrying in {delay}s");
            yield return new WaitForSeconds(delay);
            delay = Mathf.Min(delay * 2, maxDelay);   // exponential back-off
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void DownloadSlidesRpc()
    {
        // Download slides from the given list of urls
        //string shareUrl = "https://docs.google.com/presentation/d/1mal-rfHMSLX-l2p2Gvog8OZcm7vFHELusi5vO_jcW-Y/edit?usp=sharing"; // slideDownloadUrlInputField.text;
        //string presentationId = ExtractPresentationId(shareUrl);        

        DownloadSlides();
        
    }

    private void DownloadSlides()
    {
        notificationText.text = "Downloading slides...";
        StartCoroutine(DownloadingSlides());
    }


    private IEnumerator DownloadingSlides()
    {
        anySlideFailed = false;            // reset for this run

        // pull only populated slots
        string[] urls = SlideUrls
            .Where(ns => ns.Length > 0)
            .Select(ns => (string)ns)
            .ToArray();

        int total = urls.Length;
        slideCount = total;

        // reset slide map
        slides.Clear();
        for (int i = 0; i < total; i++)
            slides[i] = new SlideEntry { Url = urls[i], Texture = null };

        Debug.Log($"Start downloading {total} slide textures ...");

        // bounded parallelism (<= 5 simultaneous requests)
        const int maxConcurrent = 5;
        var queue = new Queue<int>(Enumerable.Range(0, total));
        int running = 0;

        float spacing = 3f; // default

#if UNITY_WEBGL
        string url = Application.absoluteURL;
        var queryParams = System.Web.HttpUtility.ParseQueryString(new Uri(url).Query);
        string spacingParam = queryParams.Get("SlideDownloadSpacing");
        if (float.TryParse(spacingParam, out float parsedSpacing))        
            spacing = parsedSpacing;
#endif
        Debug.Log("SLIDES: Download spacing time is " + spacing);

        while (queue.Count > 0 || running > 0)
        {
            while (queue.Count > 0 && running < maxConcurrent)
            {
                yield return new WaitForSeconds(spacing);

                int idx = queue.Dequeue();
                Debug.Log($"Initiate download for slide {slides[idx].Url} at {DateTime.Now:HH:mm:ss}");

                running++;
                StartCoroutine(DownloadSingleSlideWithRetry(
                    idx,
                    slides[idx].Url,
                    () => running--           // frees the slot
                ));
            }
            yield return null;               // wait one frame
        }

        // -----------------------------------------------------
        //  decide what to do after all coroutines completed
        // -----------------------------------------------------
        if (anySlideFailed)
        {
            projectorControlGUI.SetActive(true);
            setupControlGUI.SetActive(true);
            setupInfoText.transform.parent.gameObject.SetActive(true);
            presentationControlGUI.SetActive(false);
            notificationText.text = "Slides could not be downloaded. Please try again later.";
            CancelInvoke("HideNotificationCanvas");
            Invoke("HideNotificationCanvas", 3.0f);
            yield break;                     // do not launch the slide show
        }

        Debug.Log("All slide textures downloaded.");
        notificationCanvasGO.SetActive(false);
        ActivateSlideShow();
        ApplySlideChange();
    }

    private IEnumerator DownloadSingleSlideWithRetry(int index, string url, Action onComplete)
    {
        const int initialDelay = 4;
        const int maxDelay = 16;
        const int maxRetries = 4;
        const int requestTimeout = 15; // seconds

        int delay = initialDelay;
        int attempt = 0;

        while (true)
        {
            attempt++;
            Debug.Log($"Slide {index}: attempt {attempt} -> {url}");

            using (var uwr = UnityWebRequestTexture.GetTexture(url))
            {
                uwr.timeout = requestTimeout;
                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    slides[index].Texture = DownloadHandlerTexture.GetContent(uwr);
                    Debug.Log($"Slide {index} downloaded on attempt {attempt}");
                    break;                           // success
                }

                Debug.LogError(
                    $"Slide {index} failed (attempt {attempt}): {uwr.error} - " +
                    $"retrying in {delay}s");

                if (attempt >= maxRetries)
                {
                    // mark global failure flag and stop retrying
                    anySlideFailed = true;
                    Debug.LogError($"Slide {index} exceeded max retries, giving up.");
                    break;
                }

                yield return new WaitForSeconds(delay);
                delay = Mathf.Min(delay * 2, maxDelay);
            }
        }

        onComplete?.Invoke();                        // free the concurrency slot
    }


    //[Rpc(RpcSources.All, RpcTargets.All)]
    private void ActivateSlideShow() //bool enable
    {
        Debug.Log("SLIDE CONTROLLER: Activate slide show RPC");
        SlideIndex = 0;
        IsSlideShowActive = true; // enable
        projectorImageMeshRenderer.enabled = true;  // bool enable
    }

    void ShowPresentationControls()
    {
        setupControlGUI.SetActive(false);
        presentationControlGUI.SetActive(true);

        // After initializing the slides, show the presentation controls        
        previousSlideButton.interactable = false;
        nextSlideButton.interactable = true;
    }

    #endregion


    #region Slide Changing

    // Only the controlling player can set the slide URLs
    public void SetSlideUrls(IEnumerable<string> urls)
    {
        if (ControllingPlayer != Runner.LocalPlayer) 
            return;

        SetSlideUrlsRpc(new List<string>(urls).ToArray());
    }

    // This is called so that all players can update their local copies of the slide URL list
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void SetSlideUrlsRpc(string[] urls, RpcInfo info = default)
    {
        if (info.Source != ControllingPlayer) 
            return;

        //slideUrls = new List<string>(urls);
        //slideCache.Clear();
        //DownloadAllThumbnails().Forget();               // start all downloads
        ApplySlideChange();                             // show first slide (index 0)
    }

    public void ChangeSlide(int delta)
    {
        Debug.Log("SLIDE CONTROLLER: Changing slide");

        if (Runner.LocalPlayer != ControllingPlayer) 
            return;

        Debug.Log("SLIDE CONTROLLER: The slide changer is the controlling player, slide Urls count is " + slides.Count);

        int next = Mathf.Clamp(SlideIndex + delta, 0, slides.Count); //NOTE: 1 is just a debug value for now
        if (next != SlideIndex)
            ChangeSlideIndexRpc(next);

        previousSlideButton.interactable = SlideIndex > 0;
        nextSlideButton.interactable = SlideIndex < slideCount - 1;

        Debug.Log("SLIDE CONTROLLER: Slide changed, SlideIndex is now " + SlideIndex + " / " + (slideCount - 1));
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void ChangeSlideIndexRpc(int newIndex)
    {
        SlideIndex = newIndex;
        OnSlideIndexChanged();
    }

    private void OnSlideIndexChanged()
    {
        Debug.Log("SLIDE CONTROLLER: On Slide index changed");
        ApplySlideChange();                             // apply slide on index change
    }

    void ApplySlideChange()
    {
        // Use our new slides map instead of slideCache
        if (slides.TryGetValue(SlideIndex, out var entry) && entry.Texture != null)        
            UpdateProjectorImageTexture(entry.Texture);   
        
    }

    void UpdateProjectorImageTexture(Texture2D entryTexture)
    {
        // Optional: use a MaterialPropertyBlock for better batching
        var mpb = new MaterialPropertyBlock();
        slideRenderer.GetPropertyBlock(mpb);
        mpb.SetTexture("_MainTex", entryTexture);
        slideRenderer.SetPropertyBlock(mpb);
    }

    #endregion

    #region Control Requesting

    public void RequestProjectorControls()
    {
        Debug.Log("SLIDE CONTROLLER: Request projector controls, start");
        
        if (interactionArea.localPlayerInsideInteractionArea && ControllingPlayer == PlayerRef.None)
        {
            Debug.Log("SLIDE CONTROLLER: No one's controlling the projector, request controls");
            RequestControlRpc();
        }

        /*if (ControllingPlayer == PlayerRef.None)
            ControllingPlayer = Runner.LocalPlayer;*/

        //Debug.Log("SLIDE CONTROLLER: The controlling player is now " + ControllingPlayer);        
    }

    /*private void ReleaseProjectorControls()
    {
        if (ControllingPlayer == Runner.LocalPlayer) // If we're controlling the project, release control
            ControllingPlayer = PlayerRef.None; // NOTE: Can this cause issue if two players claim the controls at around the same time? But then again, since this is a networked variable, there should be
                                                // Sync resolution
                                                //ReleaseControlRpc();
    }*/

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RequestControlRpc(RpcInfo info = default)
    {
        if (ControllingPlayer == PlayerRef.None)
        {
            ControllingPlayer = info.Source;                // grant control to the calling player
            Debug.Log("SLIDE CONTROLLER: Player " + info.Source.PlayerId + " claimed control of the projector");

            if (Runner.LocalPlayer == info.Source)
                OpenProjectorGUI();
            
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void ReleaseControlRpc(RpcInfo info = default)
    {        
        projectorImageGO.SetActive(false);
        IsSlideShowActive = false;

        ControllingPlayer = PlayerRef.None;        
        Debug.Log("SLIDE CONTROLLER: Projector control released");
    }

    public void OnControllingPlayerChanged()
    {
        Debug.Log("SLIDE CONTROLLER: On controlling player changed to " + ControllingPlayer);

        if (ControllingPlayer != PlayerRef.None)
        {
            Debug.Log("SLIDE CONTROLLER: Controlling player is not none, local player is " + Runner.LocalPlayer);

            if (ControllingPlayer == Runner.LocalPlayer)
                OpenProjectorGUI();

            usable.enabled = false;            
            projectorImageGO.SetActive(true);
            projectorImageMeshRenderer.enabled = false;
        }
        else
        {
            projectorImageGO.SetActive(false);            
            usable.enabled = true; // Other players can interact with the monitor again
        }

    }
    #endregion

    #region Downloading

    public void InitializeDeck(IEnumerable<string> urls)
    {
        SetSlideUrlsRpc(new List<string>(urls).ToArray());
    }

    #endregion

    #region notifications

    // Called via invoke
    private void HideNotificationCanvas() 
    {
        notificationCanvasGO.SetActive(false);
    }

    #endregion


#if UNITY_EDITOR
    public void MockOnSlidesListed()
    {
        slideIds = new string[] { "p", "g353f3de4406_0_0", "g353f6112c34_0_0" };
        slideIdsReady = true;
    }

    public void MockOnThumbnailUrlReceived()
    {
        // expect "pageId|https://..."
        /*int pipe = message.IndexOf('|');
        if (pipe < 0)
        {
            Debug.LogError("OnThumbnailUrlReceived: bad message format");
            return;
        }

        string pageId = message.Substring(0, pipe);
        string url = message.Substring(pipe + 1);

        // TODO: We could just supply the index as a parameter I suppose
        // find the slide index that matches this pageId
        int idx = Array.IndexOf(slideIds, pageId);
        if (idx < 0)
        {
            Debug.LogWarning($"OnThumbnailUrlReceived: unknown pageId {pageId}");
            return;
        }*/

        SlideUrls.Set(0, "https://lh7-us.googleusercontent.com/docsdf/AFQj2d4gOBU4dRxF5fTyJCrzpvvsK42LW7Y4aPwl9IbZFcgh3LFOvQrczZKw252e-sHSG9ExAtQ4PTvHONhOgqtwdkxYc3HeFez5rgLMhc34XG0nEQgxzy4oy6ABlm37qRtM_5gGFz-na39159hBXiP28hZnF732nqkp2x7TFSaKfDQ9Ev9Y=s800");
        slideUrlsReady[0] = true;

        SlideUrls.Set(1, "https://lh7-us.googleusercontent.com/docsdf/AFQj2d5AgrECykhyF5YKWABQvjSY3S5MIP8V6elgmixgQT03Chn6cWUqw_wmH6G_DlaoDXutAkdvCArgAVx08bp4H-BnUpE8bm-qgngTG6_-0nwLj54OzzIZqzRYjH46gY62cw-FjL3CPl61F7tblbb23u5chqc-vktQi6AqQHa93RTWjyyA=s800");
        slideUrlsReady[1] = true;

        SlideUrls.Set(2, "https://lh7-us.googleusercontent.com/docsdf/AFQj2d6W8PRFClHJ2NxjZvUSBJ4XmF1TmHGzlpmBRy8SP-LJz8JUUEBPzJaDBOz0UX9Z1XacVp_lWNp8LUOBHT6EZ2biTzvCFCmlZ66l4WVIMqT1kGqwViO7Gt_KeHFz7KOU-UhKTEfFuXlzTMoS_Su_mVg1I0gQMs1IYZh-jn1m3eRFSefk=s800");
        slideUrlsReady[2] = true;

        Debug.Log($"MOCK: OnThumbnailUrlReceived: slides 0 to 2: URLs ready");
    }
#endif

}
