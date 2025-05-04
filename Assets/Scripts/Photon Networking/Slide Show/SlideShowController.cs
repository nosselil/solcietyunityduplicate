using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using Solana.Unity.Metaplex.MplNftPacks.Program;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SlideShowController : NetworkBehaviour
{
    [Networked, HideInInspector, OnChangedRender(nameof(OnControllingPlayerChanged))]
    public PlayerRef ControllingPlayer { get ; set; }    // who may change slides
    
    [Networked, OnChangedRender(nameof(OnSlideIndexChanged))]
    public int SlideIndex { get => default; set { } }    // current slide

    [Networked]
    public bool IsSlideShowActive { get => default; set { } }

    bool initialized = false;

    List<string> slideUrls;                            // thumbnail URLs
    Dictionary<int, Texture2D> slideCache = new();     // cached textures

    [SerializeField]
    Texture2D[] debugSlideTextures;

    [SerializeField]
    Renderer slideRenderer;                             // display target

    [SerializeField]
    SlideShowControllerInteractionArea interactionArea;

    [SerializeField]
    GameObject projectorControlGUI, setupControlGUI, presentationControlGUI, projectorImageGO;

    [SerializeField]
    Button previousSlideButton, nextSlideButton;

    MeshRenderer projectorImageMeshRenderer;

    public override void Spawned()
    {
        if (ControllingPlayer == null)
            ControllingPlayer = PlayerRef.None;

        //ControllingPlayer = PlayerRef.None;             // no controller at start
        slideCache.Add(0, debugSlideTextures[0]);
        slideCache.Add(1, debugSlideTextures[1]);

        slideUrls = new List<string>(2);

        Debug.Log("SLIDE CONTROLLER: Spawned, controlling player is " + ControllingPlayer);

        projectorImageMeshRenderer = projectorImageGO.GetComponent<MeshRenderer>();
        projectorImageGO.SetActive(IsSlideShowActive);

        // Check a networked boolean like IsSlideShowActive to be able to set the visibility of the projectorImageGo correctly
        if (IsSlideShowActive)
            ApplySlideChange();
        
        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (interactionArea.localPlayerInsideInteractionArea && ControllingPlayer == PlayerRef.None)
            {
                Debug.Log("SLIDE CONTROLLER: No one's controlling the project, request controls");
                RequestControlRpc();
            }
        }
        
        // DEBUG: You can claim the projector control just with a button press for now

        //Debug.Log("PROJECTOR: Update initialized");

        /*if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log("PROJECTOR: O pressed, controllingPlayer is " + ControllingPlayer);
            RequestProjectorControls();SLIDE
            
        }*/

        // Debug controls for changing slides in the projector
        if (ControllingPlayer == Runner.LocalPlayer)
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                // TODO: Setting the slide base URL that we can use to download
            }

            /*if (Input.GetKeyDown(KeyCode.A))
                ChangeSlide(-1);
            else if (Input.GetKeyDown(KeyCode.D))
                ChangeSlide(1);*/
        }        
        
    }

    #region General Controls



    #endregion

    void OpenProjectorGUI()
    {
        Debug.Log("SLIDE CONTROLLER: Open projector GUI");
        projectorControlGUI.SetActive(true);
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

    #region SlidePreparation
    public void DownloadSlides()
    {
        // TODO: Actually downloading stuff

        // After initializing the slides, show the presentation controls
        SlideIndex = 0;
        previousSlideButton.interactable = false;
        nextSlideButton.interactable = true;

        ActivateProjectorImageRpc(true);

        ShowPresentationControls();        
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void ActivateProjectorImageRpc(bool enable)
    {
        projectorImageMeshRenderer.enabled = enable;
    }

    void ShowPresentationControls()
    {
        setupControlGUI.SetActive(false);
        presentationControlGUI.SetActive(true);
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

        slideUrls = new List<string>(urls);
        slideCache.Clear();
        DownloadAllThumbnails().Forget();               // start all downloads
        ApplySlideChange();                             // show first slide (index 0)
    }

    public void ChangeSlide(int delta)
    {
        Debug.Log("SLIDE CONTROLLER: Changing slide");

        if (Runner.LocalPlayer != ControllingPlayer) 
            return;

        Debug.Log("SLIDE CONTROLLER: The slide changer is the controlling player, slide Urls count is " + slideUrls.Count);

        int next = Mathf.Clamp(SlideIndex + delta, 0, 1 /*slideUrls.Count - 1*/); //NOTE: 1 is just a debug value for now
        if (next != SlideIndex)
            ChangeSlideIndexRpc(next);

        previousSlideButton.interactable = SlideIndex > 0;
        nextSlideButton.interactable = SlideIndex < 1; // NOTE: Change to slideUrls.Count - 1

        Debug.Log("SLIDE CONTROLLER: Slide changed, SlideIndex is now " + SlideIndex);
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
        // TODO: Using material property blocks for efficiency
        if (slideCache.TryGetValue(SlideIndex, out var tex))
            slideRenderer.material.mainTexture = tex;
    }

    #endregion

    #region Control Requesting

    private void RequestProjectorControls()
    {
        if (ControllingPlayer == PlayerRef.None)
            ControllingPlayer = Runner.LocalPlayer;
        
        Debug.Log("SLIDE CONTROLLER: The controlling player is now " + ControllingPlayer);        
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
        //if (Runner.LocalPlayer == ControllingPlayer)        
        projectorImageGO.SetActive(false);        

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

            projectorImageGO.SetActive(true);
            projectorImageMeshRenderer.enabled = false;
        }
        else
            projectorImageGO.SetActive(false);

    }
    #endregion

    #region Downloading

            private async UniTask DownloadAllThumbnails()
            {
                var tasks = new List<UniTask>(slideUrls.Count);
                for (int i = 0; i < slideUrls.Count; i++)
                    tasks.Add(DownloadAndCache(i, slideUrls[i]));
                await UniTask.WhenAll(tasks);
            }

    private async UniTask DownloadAndCache(int index, string url)
    {
        using var uwr = UnityWebRequestTexture.GetTexture(url);
        await uwr.SendWebRequest().ToUniTask();
        if (uwr.result == UnityWebRequest.Result.Success)
            slideCache[index] = DownloadHandlerTexture.GetContent(uwr);
    }

    public void InitializeDeck(IEnumerable<string> urls)
    {
        SetSlideUrlsRpc(new List<string>(urls).ToArray());
    }

    #endregion

}
