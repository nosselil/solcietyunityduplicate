using System;
using System.Reflection;
using UnityEngine;

public class CitrusBorrowDebugTrigger : MonoBehaviour
{
    [Header("Manager")]
    [Tooltip("Leave empty to auto-find a GameObject named 'CitrusClientBorrowManager'")]
    public GameObject managerObject;
    [Tooltip("Name of the component that has BeginBorrow(string,string). Defaults to 'CitrusClientBorrowManager'.")]
    public string managerComponentTypeName = "CitrusClientBorrowManager";

    [Header("Test Params")]
    [Tooltip("Citrus loanAccount public key (string)")]
    public string loanAccount;
    [Tooltip("NFT mint used as collateral (string)")]
    public string nftMint;

    [Header("Start Mode")]
    [Tooltip("Enable to allow manual key trigger in addition to the timer")]
    public bool enableKeyTrigger = false;

    [Header("Controls")]
    [Tooltip("Press this key to trigger a borrow attempt (only if enableKeyTrigger is true)")]
    public KeyCode triggerKey = KeyCode.H;

    [Header("Auto Trigger")]
    [Tooltip("Automatically start the borrow after a delay on scene start")]
    public bool autoTriggerOnStart = true;
    [Tooltip("Delay (seconds) before auto trigger runs")]
    public float autoTriggerDelaySeconds = 1f;

    private Component cachedManagerComponent;
    private MethodInfo beginBorrowMethod;
    private bool inProgress;
    private bool autoTriggered;

    private void Awake()
    {
        if (managerObject == null)
        {
            managerObject = GameObject.Find("CitrusClientBorrowManager");
        }

        TryCacheManager();
    }

    private void Start()
    {
        if (autoTriggerOnStart && !autoTriggered)
        {
            StartCoroutine(AutoTriggerCoroutine());
        }
    }

    private System.Collections.IEnumerator AutoTriggerCoroutine()
    {
        // Countdown logs
        var delay = Mathf.Max(0f, autoTriggerDelaySeconds);
        if (delay > 0f)
        {
            int remaining = Mathf.CeilToInt(delay);
            while (remaining > 0)
            {
                Debug.Log($"[CitrusBorrowDebugTrigger] Starting in {remaining} second{(remaining == 1 ? "" : "s")}…");
                yield return new WaitForSecondsRealtime(1f);
                remaining--;
            }
        }

        if (!EnsureManagerReady())
            yield break;

        if (string.IsNullOrEmpty(loanAccount) || string.IsNullOrEmpty(nftMint))
        {
            Debug.LogWarning("[CitrusBorrowDebugTrigger] AutoTrigger: Set loanAccount and nftMint in the inspector before testing.");
            yield break;
        }

        if (inProgress)
        {
            Debug.Log("[CitrusBorrowDebugTrigger] AutoTrigger: Borrow already in progress; skipping.");
            yield break;
        }

        autoTriggered = true;
        Debug.Log($"[CitrusBorrowDebugTrigger] AutoTrigger now with loanAccount={loanAccount} nftMint={nftMint}");
        try
        {
            inProgress = true;
            beginBorrowMethod.Invoke(cachedManagerComponent, new object[] { loanAccount, nftMint });
        }
        catch (Exception e)
        {
            inProgress = false;
            Debug.LogError("[CitrusBorrowDebugTrigger] AutoTrigger invoke BeginBorrow failed: " + e.Message);
        }
    }

    private void Update()
    {
        if (!enableKeyTrigger) return;

        if (Input.GetKeyDown(triggerKey))
        {
            if (!EnsureManagerReady())
                return;

            if (inProgress)
            {
                Debug.Log("[CitrusBorrowDebugTrigger] Borrow already in progress; ignoring trigger.");
                return;
            }
            if (string.IsNullOrEmpty(loanAccount) || string.IsNullOrEmpty(nftMint))
            {
                Debug.LogWarning("[CitrusBorrowDebugTrigger] Set loanAccount and nftMint in the inspector before testing.");
                return;
            }

            Debug.Log($"[CitrusBorrowDebugTrigger] Triggering borrow with loanAccount={loanAccount} nftMint={nftMint}");
            try
            {
                inProgress = true;
                beginBorrowMethod.Invoke(cachedManagerComponent, new object[] { loanAccount, nftMint });
            }
            catch (Exception e)
            {
                inProgress = false;
                Debug.LogError("[CitrusBorrowDebugTrigger] Failed to invoke BeginBorrow via reflection: " + e.Message);
            }
        }
    }

    private bool EnsureManagerReady()
    {
        if (cachedManagerComponent != null && beginBorrowMethod != null)
            return true;

        if (managerObject == null)
        {
            managerObject = GameObject.Find("CitrusClientBorrowManager");
            if (managerObject == null)
            {
                Debug.LogError("[CitrusBorrowDebugTrigger] Manager GameObject not found. " +
                               "Create one named 'CitrusClientBorrowManager' or assign it explicitly.");
                return false;
            }
        }

        return TryCacheManager();
    }

    private bool TryCacheManager()
    {
        cachedManagerComponent = null;
        beginBorrowMethod = null;

        if (managerObject == null) return false;

        var components = managerObject.GetComponents<MonoBehaviour>();
        foreach (var c in components)
        {
            if (c == null) continue;
            var t = c.GetType();
            if (t.Name == managerComponentTypeName || t.FullName == managerComponentTypeName)
            {
                var m = t.GetMethod("BeginBorrow", BindingFlags.Public | BindingFlags.Instance);
                if (m != null)
                {
                    var pars = m.GetParameters();
                    if (pars.Length == 2 &&
                        pars[0].ParameterType == typeof(string) &&
                        pars[1].ParameterType == typeof(string))
                    {
                        cachedManagerComponent = c;
                        beginBorrowMethod = m;
                        Debug.Log("[CitrusBorrowDebugTrigger] Manager and BeginBorrow method cached.");
                        return true;
                    }
                }
            }
        }

        Debug.LogError($"[CitrusBorrowDebugTrigger] Component '{managerComponentTypeName}' with " +
                       "BeginBorrow(string,string) not found on the assigned GameObject.");
        return false;
    }

    // Optional hooks if you forward events to this script via UnityEvents or SendMessage
    public void OnBorrowStarted() { inProgress = true; }
    public void OnBorrowSignature(string _) { inProgress = false; }
    public void OnBorrowFailed(string _) { inProgress = false; }
}