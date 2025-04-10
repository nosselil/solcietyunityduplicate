// Copyright (C) 2015-2021 gamevanilla - All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement.
// A Copy of the Asset Store EULA is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

namespace UltimateClean
{
    /// <summary>
    /// This class is responsible for creating and opening a popup of the
    /// given prefab and adding it to the UI canvas of the current scene.
    /// </summary>
    public class PopupOpener : MonoBehaviour
    {
        public GameObject popupPrefab;

        protected Canvas m_canvas;
        protected GameObject m_popup;
        public GameObject BG;
        protected void Start()
        {
            GameObject GUICanvas = GameObject.Find("GUI");
            /*if (GUICanvas != null)
                m_canvas = GUICanvas.GetComponent<Canvas>();*/
        }

        public virtual void OpenPopupWAGER()
        {
            BG.SetActive(true);
            // m_popup = Instantiate(popupPrefab, m_canvas.transform, false);
            popupPrefab.SetActive(true);
            popupPrefab.transform.localScale = Vector3.zero;
        //    popupPrefab.GetComponent<Popup>().Open();
        }


        public virtual void OpenPopup()
        {
            m_popup = Instantiate(popupPrefab, m_canvas.transform, false);
            m_popup.SetActive(true);
            m_popup.transform.localScale = Vector3.zero;
            m_popup.GetComponent<Popup>().Open();
        }
    }
}
