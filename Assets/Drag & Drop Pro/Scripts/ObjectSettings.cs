using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ObjectEvents))]
public class ObjectSettings : MonoBehaviour
{
    [HideInInspector]
    public int OriginalSiblingIndex;

  //  [HideInInspector]
    public Transform OriginalParent;
    [HideInInspector]
    public Vector2 FirstAnchoredPosition;

    // DDM Game Object
    DragDropManager DDM;
    // Rect Transform Component of this object
    RectTransform thisRT;
    // Parent of this object
    Transform thisParent;

    // Vector3 variables
    Vector3 CurrentPos;
    [HideInInspector]
    public Vector3 FirstPos;
    [HideInInspector]
    public Vector3 FirstScale;
    //

    // Bools
    bool PDown = false;
    bool CheckStatus = false;
    [HideInInspector]
    public bool DefaultPanel = false;
    [HideInInspector]
    public bool Dropped = false;
    [HideInInspector]
    public bool OnReturning = false;
    //

    // Used for dragging modes
    Vector2 mouseDifference;

    // Customization Tools
    public string Id;   // the Id of this object

    [Tooltip("Allow user to control this object")]
    public bool UserControl = true;

    [Header("Return Object Smoothly (DragDrop Failed)")]
    [Tooltip("Return Object to its first Position Smoothly When Drag & Drop Failed")]
    public bool ReturnSmoothly = false;
    [Tooltip("Returning speed")]
    [Range(0.1f, 2.0f)]
    public float ReturnSpeed = 1.0f;

    [Header("Scale Object (Dragging)")]
    [Tooltip("Scale Object When dragging gets begun")]
    public bool ScaleOnDrag = false;
    [Tooltip("Object Scale")]
    public Vector3 DragScale = new Vector3(1.0f, 1.0f, 1.0f);

    [Header("Stay Object on dropped position")]
    [Tooltip("Keep Object on dropped position When it gets dropped successfully")]
    public bool StayDroppedPos = false;

    [Header("Scale Object (Dropped successfully)")]
    [Tooltip("Scale Object When it gets dropped successfully")]
    public bool ScaleOnDropped = false;
    [Tooltip("Object Scale")]
    public Vector3 DropScale = new Vector3(1.0f, 1.0f, 1.0f);

    [Header("Locking Object")]
    [Tooltip("Lock Object When it gets dropped successfully")]
    public bool LockObject = false;

    [Header("Return Object (Dropped successfully)")]
    [Tooltip("Return Object to its first position when it gets dropped successfully")]
    public bool ReturnObject = false;

    [Header("Smooth Replacement")]
    [Tooltip("Replace Object smoothly when it gets dropped successfully")]
    public bool ReplaceSmoothly = false;
    [Range(0.1f, 2.0f)]
    public float ReplacementSpeed = 1.0f;

    [Header("Allow to switch Objects")]
    [Tooltip("Allow to switch Objects between panels")]
    public bool SwitchObjects = false;
    [Tooltip("Move Object smoothly When it is switching")]
    public bool MoveSmoothly = false;
    [Range(0.1f, 2.0f)]
    public float MovementSpeed = 1.0f;

    [Header("Filter Panels")]
    [Tooltip("Allow using Filter Panels tool")]
    public bool FilterPanels = false;
    [Tooltip("The Ids of the panels that object is allowed to drop on them")]
    public string[] AllowedPanels;

    [Header("Events Management")]
    public UnityEvent OnBeginDragging;
    public UnityEvent OnDragDropFailed;
    public UnityEvent OnDroppedSuccessfully;
    public UnityEvent OnReplaced;



    void Awake()
    {
        thisRT = GetComponent<RectTransform>();
        DDM = FindFirstObjectByType<DragDropManager>();
        if (DDM == null)
        {
            Debug.LogError("NftManager.instance is null");
        }



      //  OriginalParent = NftManager.instance.GalleryContent_transform;



          OriginalParent = thisRT.parent;
        FirstAnchoredPosition = thisRT.anchoredPosition;
        FirstPos = thisRT.position;
        FirstScale = thisRT.localScale;
        // NEW: store the sibling index
        OriginalSiblingIndex = thisRT.GetSiblingIndex();

        DDM = FindFirstObjectByType<DragDropManager>();
        if (DDM == null)
        {
            Debug.LogError("DragDropManager not found! Ensure it is in the scene.");
        }

        //    DDM.AllObjects.Clear();
        DDM.AllObjects.Add(GetComponent<ObjectSettings>());
    }


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void PreserveEvents()
    {
        new ObjectSettings();
        // Create a new GameObject and add ObjectSettings as a component
    //    var go = new GameObject("ObjectSettingsHolder");
    //    go.AddComponent<ObjectSettings>();
    }
    private void Start()
    {
      
       // DDM.AllPanels.Add(GetComponent<ObjectSettings>());
    }

    void Update()
    {
        if (PDown)
        {
            Vector3 MousePos = GetMousePos();
            if (DDM.DraggingModes == DragDropManager.DragModes.ChangeToMousePos)
            {
                thisRT.position = new Vector3(MousePos.x, MousePos.y, thisRT.position.z);
            }
            else
            {
                thisRT.position = new Vector3(MousePos.x + mouseDifference.x, MousePos.y + mouseDifference.y, thisRT.position.z);
            }
        }
    }

    public void PointerDown(string state, PanelSettings AIPanel)
    {
        if ((state == "User" && UserControl && AIDragDrop.ReservedObject != Id) || state.StartsWith("AI"))
        {
            PointerDownActions(state, AIPanel);
        }
    }

    void PointerDownActions(string state, PanelSettings AIPanel)
    {

     
        

        if (Dropped)
        {
            // Setup customization tools of the panel
            int index = DDM.IndexOfObjectId(Id);
            if (index != -1)
            {
                if (DDM.AllPanels[index].LockObject != PanelSettings.ObjectLockStates.LockObject)
                {
                    if (!LockObject || DDM.AllPanels[index].LockObject == PanelSettings.ObjectLockStates.DoNotLockObject)
                    {
                        BeginDragging(state, AIPanel);
                    }
                }
            }
        }
        else
        {
            BeginDragging(state, AIPanel);
        }
    }

    void BeginDragging(string state, PanelSettings AIPanel)
    {
        Debug.LogError("BeginDragging");

        if (WalletManager.instance == null)
        {
            Debug.LogError("WalletManager is null pointer up");
        }

        ScrollRect sr = WalletManager.instance.scrollrect;
        if (sr != null)
            sr.enabled = false;



        CurrentPos = thisRT.position;

        if (ScaleOnDrag)
        { // Setup ScaleOnDrag tool
            thisRT.localScale = DragScale;
        }

        thisRT.SetParent(DDM.SecondCanvas.GetComponent<RectTransform>());

        if (state == "User")
        {
            if (DDM.DraggingModes == DragDropManager.DragModes.DoNotChange)
            {
                mouseDifference = thisRT.position - GetMousePos();
            }
            PDown = true;
        }
        else
        {
            // Setup AI system
            if (state == "AI1")
            {
                // Instant movement
                thisRT.position = new Vector3(AIPanel.GetComponent<RectTransform>().position.x, AIPanel.GetComponent<RectTransform>().position.y, thisRT.position.z);
            }
            DDM.SmoothMoveStarter(state, thisRT, AIPanel.GetComponent<RectTransform>().position, DDM.GetComponent<AIDragDrop>().MovementSpeed);
        }

        // Events Management
        if (OnBeginDragging != null)
        {
            try
            {
                OnBeginDragging.Invoke();
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error invoking OnBeginDragging: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("OnBeginDragging is NULL");
        }


    }

    Vector3 GetMousePos()
    {
        Vector3 screenPoint;
        if (DDM.TargetPlatform == DragDropManager.Platforms.PC)
        {
            // for PC
            screenPoint = Mouse.current.position.ReadValue();
        }
        else
        {
            // for Mobile
            screenPoint = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0].screenPosition;
        }
        if (DDM.FirstCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            // ScreenSpaceCamera & WorldSpace support
            screenPoint.z = thisRT.position.z - DDM.FirstCanvas.worldCamera.transform.position.z;
            screenPoint = DDM.FirstCanvas.worldCamera.ScreenToWorldPoint(screenPoint);
        }
        return screenPoint;
    }

    public void PointerUp(string state)
    {

      
        if (WalletManager.instance == null)
        {
            Debug.LogError("WalletManager is null pointer up");
        }



        if ((state == "User" && UserControl && AIDragDrop.ReservedObject != Id) || state.StartsWith("AI"))
        {
            ScrollRect sr = WalletManager.instance.scrollrect;
            if (sr != null)
                sr.enabled = true;


            PointerUpActions(state);
        }
    }

    void PointerUpActions(string state)
    {
        if (Dropped)
        {
            // Setup customization tools of the panel
            int index = DDM.IndexOfObjectId(Id);
            if (index != -1)
            {
                if (DDM.AllPanels[index].LockObject != PanelSettings.ObjectLockStates.LockObject)
                {
                    if (!LockObject || DDM.AllPanels[index].LockObject == PanelSettings.ObjectLockStates.DoNotLockObject)
                    {
                        CheckObjectPos(state);
                    }
                }
            }
        }
        else
        {
            CheckObjectPos(state);
        }
    }

    void CheckObjectPos(string state)
    {
        PDown = false;
        CheckStatus = false;
        PanelSettings targetPanel = null;
        for (int i = 0; i < DDM.AllPanels.Count; i++)
        {
            // Check if the object is on any panel
            if (RectTransformUtility.RectangleContainsScreenPoint(DDM.AllPanels[i].GetComponent<RectTransform>(), thisRT.position))
            {
                // Check if the panel is blocked
                if (DDM.AllPanels[i].Ignore ||
                   (FilterPanels && !Array.Exists(AllowedPanels, panel => panel == DDM.AllPanels[i].Id)) ||
                   (state == "User" && DDM.AllPanels[i].Id == AIDragDrop.ReservedPanel))
                    break;

                targetPanel = DDM.AllPanels[i];
               
                PanelDropTools(i);
                break;
            }
        }

        if (!CheckStatus)
        { // Drag & drop failed
            if (!ReturnSmoothly)
            {
                // Instead of resetting world position, you might reset the anchored position when going back to the scroll view.
                // (If the object isn’t dropped on a valid panel, we want it to return to its original layout spot.)
                thisRT.anchoredPosition = FirstAnchoredPosition;
                if (DDM.SaveStates)
                {
                    PlayerPrefs.SetFloat(Id + "X", CurrentPos.x);
                    PlayerPrefs.SetFloat(Id + "Y", CurrentPos.y);
                }
            }
            else
            {
                DDM.SmoothMoveStarter("User", thisRT, CurrentPos, ReturnSpeed);
            }
            thisRT.localScale = FirstScale;
          

            if (OnDragDropFailed != null)
            {
                try
                {
                    OnDragDropFailed.Invoke();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Error invoking OnDragDropFailed: " + ex.Message);
                }
            }
            else
            {
                Debug.LogError("OnDragDropFailed is NULL");
            }

        }

        // Re-parenting based on where the object is dropped:
        if (targetPanel != null && targetPanel.Id == "panel2")
        {
            // Dropped on the designated drop target.
            thisRT.SetParent(targetPanel.transform);
        }
        else
        {
            Debug.LogError("EXECUETING HERE1");
            // Return to the scroll view's content.
            thisRT.SetParent(OriginalParent, false);
            // Restore its sibling order.
            Debug.LogError("EXECUETING HERE2");
            thisRT.SetSiblingIndex(OriginalSiblingIndex);
            // Optionally, you can reapply the scale if needed.
            Debug.LogError("EXECUETING HERE4");
            thisRT.localScale = FirstScale;
            Debug.LogError("EXECUETING HERE5");
            // Force the layout group to update.
            if (OriginalParent == null) Debug.LogError(gameObject.name);
            Debug.LogError("EXECUETING HERE6");
            LayoutRebuilder.ForceRebuildLayoutImmediate(OriginalParent.GetComponent<RectTransform>());
        }

    }


    void PanelDropTools(int i)
    {


       


        if (DDM.AllPanels[i].ObjectId != "" && DDM.AllPanels[i].ObjectId != Id)
        {
            if (DragDropManager.GetObjectById(DDM.AllPanels[i].ObjectId).OnReturning)
            {
                return;
            }
            // Setup customization tools of the panel
            if (DDM.AllPanels[i].ObjectReplacement != PanelSettings.ObjectReplace.NotAllowed)
            {
                CheckStatus = true;
                DropActions(i);
            }
        }
        else
        {
            CheckStatus = true;
            DropActions(i);
        }
    }

    void DropActions(int i)
    {
        bool SwitchStatus = false;

        // Setup ReturnObject tool
        ReturnObjectTool();

        thisRT.localScale = FirstScale;

        if (!StayDroppedPos && !ReturnObject && DDM.AllPanels[i].ObjectPosition != PanelSettings.ObjectPosStates.DroppedPosition)
        {
            thisRT.position = new Vector3(DDM.AllPanels[i].GetComponent<RectTransform>().position.x, DDM.AllPanels[i].GetComponent<RectTransform>().position.y, thisRT.position.z);
        }

        // Save last position of this object
        if (DDM.SaveStates)
        {
            PlayerPrefs.SetFloat(Id + "X", thisRT.position.x);
            PlayerPrefs.SetFloat(Id + "Y", thisRT.position.y);
        }

        // Check if there is another object on target panel
        if (DDM.AllPanels[i].ObjectId != "" && DDM.AllPanels[i].ObjectId != Id)
        {
            if (DDM.AllPanels[i].ObjectReplacement == PanelSettings.ObjectReplace.Allowed)
            {
                // Check if objects should not switch between their panels. So this object will replace with second object
                if (!SwitchObjects && !DefaultPanel && !DragDropManager.GetObjectById(DDM.AllPanels[i].ObjectId).DefaultPanel)
                {
                    ObjectsReplacement(i);
                }
                else
                {
                    // Objects will switch between their panels (if this object was on any panel)
                    if (ObjectsSwitching(i))
                    {
                        SwitchStatus = true;
                    }
                    else
                    {
                        // This object was not on any panel. So this object will be replaced with second object
                        ObjectsReplacement(i);
                    }
                }
            }
        }

        int index = DDM.IndexOfObjectId(Id);
        // Setup Multi Object tool
        if (index != -1)
        {
            if (DDM.AllPanels[index].ObjectReplacement == PanelSettings.ObjectReplace.MultiObjectMode)
            {
                if (!SwitchStatus)
                    DDM.AllPanels[index].RemoveMultiObject(Id);
                SetPrevPanelId(index);
            }
        }

        if (DDM.AllPanels[i].ObjectReplacement == PanelSettings.ObjectReplace.MultiObjectMode)
        {
            SetPrevPanelId(index);
            DDM.AllPanels[i].SetMultiObject(Id);
        }

        // Setup ScaleOnDropped tool
        ScaleOnDroppedTool();

        if ((!SwitchObjects && !DefaultPanel) || DDM.AllPanels[i].ObjectId == "")
        {
            SetPrevPanelId(index);
        }

        DDM.SetPanelObject(i, Id);

        // Setup customization tools of the panel
        if (DDM.AllPanels[i].ObjectPosition == PanelSettings.ObjectPosStates.PanelPosition)
        {
            thisRT.position = new Vector3(DDM.AllPanels[i].GetComponent<RectTransform>().position.x, DDM.AllPanels[i].GetComponent<RectTransform>().position.y, thisRT.position.z);
        }

        // Panel Events Management
        if (DDM.AllPanels[i].OnObjectDropped != null)
            DDM.AllPanels[i].OnObjectDropped.Invoke();

        // Implements OnObjectExit event
        if (index != -1)
        {
            if (DDM.AllPanels[index].OnObjectExit != null)
                DDM.AllPanels[index].OnObjectExit.Invoke();
        }

        // Events Management
  

        if (OnDroppedSuccessfully != null)
        {
            try
            {
                OnDroppedSuccessfully.Invoke();
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error invoking OnDroppedSuccessfully: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("OnDroppedSuccessfully is NULL");
        }

    }

    void ObjectsReplacement(int i)
    {
        for (int j = 0; j < DDM.AllObjects.Count; j++)
        {
            if (DDM.AllPanels[i].ObjectId == DDM.AllObjects[j].Id && DDM.AllObjects[j].Dropped)
            {
                DDM.AllObjects[j].Dropped = false;
                RectTransform rt = DDM.AllObjects[j].GetComponent<RectTransform>();

                // Reset scale to the stored original scale.
                rt.localScale = DDM.AllObjects[j].FirstScale;

                if (!ReplaceSmoothly)
                {
                    // Use anchoredPosition to let the layout group handle positioning.
                    rt.anchoredPosition = DDM.AllObjects[j].FirstAnchoredPosition;
                    if (DDM.SaveStates)
                    {
                        PlayerPrefs.SetFloat(DDM.AllObjects[j].Id + "X", DDM.AllObjects[j].FirstPos.x);
                        PlayerPrefs.SetFloat(DDM.AllObjects[j].Id + "Y", DDM.AllObjects[j].FirstPos.y);
                    }
                }
                else
                {
                    DDM.SmoothMoveStarter("User", rt, DDM.AllObjects[j].FirstPos, ReplacementSpeed);
                }

                // Re-parent the replaced object back to its original parent (scroll view content)
                rt.SetParent(DDM.AllObjects[j].OriginalParent, false);
                rt.SetSiblingIndex(DDM.AllObjects[j].OriginalSiblingIndex);
                // Force the layout group to rebuild so that the object is positioned correctly.
           //     LayoutRebuilder.ForceRebuildLayoutImmediate(DDM.AllObjects[j].OriginalParent.GetComponent<RectTransform>());
                LayoutRebuilder.ForceRebuildLayoutImmediate(OriginalParent.GetComponent<RectTransform>());

                Debug.LogError("OnReplaced1");
                DDM.CallEvent(DDM.AllObjects[j].OnReplaced);
                rt.localScale = DDM.AllObjects[j].FirstScale;
            }
        }
    }


    bool ObjectsSwitching(int i)
    {
        int index = DDM.IndexOfObjectId(Id);

        if (index != -1)
        {
            for (int j = 0; j < DDM.AllObjects.Count; j++)
            {
                if (DDM.AllPanels[i].ObjectId == DDM.AllObjects[j].Id)
                {
                    if (DDM.AllPanels[index].ObjectReplacement == PanelSettings.ObjectReplace.MultiObjectMode)
                    {
                        // Setup Multi Object tool
                        DDM.AllPanels[index].RemoveMultiObject(Id);
                        DDM.AllPanels[index].SetMultiObject(DDM.AllObjects[j].Id);
                    }

                    DDM.SetPanelObject(index, DDM.AllObjects[j].Id);

                    DDM.AllObjects[j].GetComponent<RectTransform>().SetAsLastSibling();

                    if (!MoveSmoothly)
                    {
                        DDM.AllObjects[j].GetComponent<RectTransform>().position = new Vector3(CurrentPos.x, CurrentPos.y, DDM.AllObjects[j].GetComponent<RectTransform>().position.z);

                        // Save last position of second object
                        if (DDM.SaveStates)
                        {
                            PlayerPrefs.SetFloat(DDM.AllObjects[j].Id + "X", CurrentPos.x);
                            PlayerPrefs.SetFloat(DDM.AllObjects[j].Id + "Y", CurrentPos.y);
                        }
                    }
                    else
                    {
                        DDM.SmoothMoveStarter("User", DDM.AllObjects[j].GetComponent<RectTransform>(), CurrentPos, MovementSpeed);
                    }
                    Debug.LogError("OnReplaced2");
                    DDM.CallEvent(DDM.AllObjects[j].OnReplaced);
                }
            }
            // Implements OnObjectExit event
            DDM.CallEvent(DDM.AllPanels[index].OnObjectExit);

            return true;
        }
        else
        {
            return false;
        }
    }

    void SetPrevPanelId(int index)
    {
        if (index != -1)
        {
            if (DDM.AllPanels[index].PanelIdManager.Count == 0)
            {
                DDM.SetPanelObject(index, "");
            }
            else
            {
                DDM.SetPanelObject(index, DDM.AllPanels[index].PanelIdManager[DDM.AllPanels[index].PanelIdManager.Count - 1]);
            }
        }
    }

    void ReturnObjectTool()
    {
        if (!ReturnObject)
        {
            Dropped = true;
        }
        else
        {
            thisRT.position = CurrentPos;
        }
    }

    void ScaleOnDroppedTool()
    {
        if (Dropped && ScaleOnDropped)
        {
            thisRT.localScale = DropScale;
        }
    }
 

}