using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace InsaneSystems.RTSStarterKit.UI
{
	public class Selection : MonoBehaviour
	{
		[SerializeField] GameObject selfObject;
		[SerializeField] RectTransform selectionPanel;

		Vector2 startPoint;
		Vector2 midPoint;

		bool isSelectionStarted;

		void Start()
		{
			Hide();

			Controls.Selection.selectionStarted += StartSelectionAction;
			Controls.Selection.selectionEnded += EndSelectionAction;
		}

		void Update()
		{
			if (isSelectionStarted)
				SelectionWorkAction();
		}

		void StartSelectionAction()
		{
			startPoint = Input.mousePosition;
			Show();

			isSelectionStarted = true;
		}

		void SelectionWorkAction()
		{
			midPoint = Vector2.Lerp(startPoint, Input.mousePosition, 0.5f);

			selectionPanel.transform.position = midPoint;
			selectionPanel.sizeDelta = new Vector2(Mathf.Abs(Input.mousePosition.x - startPoint.x), Mathf.Abs(Input.mousePosition.y - startPoint.y));
		}

		void EndSelectionAction()
		{
			selectionPanel.transform.position = Input.mousePosition;
			selectionPanel.sizeDelta = Vector2.zero;

			Hide();
			isSelectionStarted = false;
		}

		void Show()
		{
			selfObject.SetActive(true);
			isSelectionStarted = true;
		}

		void Hide()
		{
			selfObject.SetActive(false);
			isSelectionStarted = false;
		}

		void OnDestroy()
		{
			Controls.Selection.selectionStarted -= StartSelectionAction;
			Controls.Selection.selectionEnded -= EndSelectionAction;
		}
	}
}