using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public class FogOfWarModule : Module
	{
		const float updateRate = 0.25f;

		[Tooltip("List of all unit Renderer components which should be hidden when enemy unit enters FOW and shown when exits FOW.")]
		[SerializeField] Renderer[] renderersToHide = new Renderer[0];
		[Tooltip("Can be empty. List of all unit child game objects which should be hidden when enemy unit enters FOW and shown when exits FOW.")]
		[SerializeField] GameObject[] gameObjectsToDisable = new GameObject[0];

		float updateVisibilityTimer;
		
		public bool isVisibleInFOW { get; protected set; }
		
		void Start()
		{
			if (!GameController.instance.MainStorage.isFogOfWarOn)
			{
				enabled = false;
				return;
			}
			
			for (var i = 0; i < gameObjectsToDisable.Length; i++)
				if (gameObjectsToDisable[i] == gameObject)
				{
					Debug.LogWarning("Fog of war module of unit " + name + " setted up incorrectly. You shouldn't add self unit object to the Game Objects To Disable field! Add models here.");
					enabled = false;
					return;
				}
			
			
			CheckVisibleState();
			
			if (IsPlayerTeamUnit())
				isVisibleInFOW = true;
		}
		
		void Update()
		{
			if (updateVisibilityTimer >= 0)
			{
				updateVisibilityTimer -= Time.deltaTime;
				return;
			}

			CheckVisibleState();

			updateVisibilityTimer = updateRate;
		}

		void CheckVisibleState()
		{
			if (IsPlayerTeamUnit())
				return;

			var allLocalPlayerTeamUnits = GetAllLocalPlayerTeamUnits();

			bool isVisible = false;
			var selfPosition = transform.position;
			
			for (int i = 0; i < allLocalPlayerTeamUnits.Count; i++)
			{
				if (!allLocalPlayerTeamUnits[i])
					continue;

				var otherUnitSqrVisibility = Mathf.Pow(allLocalPlayerTeamUnits[i].data.visionDistance, 2f);
				var otherPosition = allLocalPlayerTeamUnits[i].transform.position;
				if ((selfPosition - otherPosition).sqrMagnitude <= otherUnitSqrVisibility)
				{
					isVisible = true;
					break;
				}
			}

			SetShownState(isVisible);
		}

		List<Unit> GetAllLocalPlayerTeamUnits()
		{
			var allUnits = Unit.allUnits;
			var allUnitsCount = allUnits.Count;

			var resultUnits = new List<Unit>();
			
			for (int i = 0; i < allUnitsCount; i++)
				if (allUnits[i].IsOwnedByPlayer(Player.localPlayerId) || allUnits[i].IsInMyTeam(Player.localPlayerId))
					resultUnits.Add(allUnits[i]);

			return resultUnits;
		}

		bool IsPlayerTeamUnit()
		{
			return selfUnit.IsOwnedByPlayer(Player.localPlayerId) || selfUnit.IsInMyTeam(Player.localPlayerId);
		}

		public void OnShownFromFOW()
		{
			SetShownState(true);
		}

		public void OnHideInFOW()
		{
			SetShownState(false);
		}

		void SetShownState(bool visibility)
		{
			for (int i = 0; i < renderersToHide.Length; i++)
				renderersToHide[i].enabled = visibility;

			for (int i = 0; i < gameObjectsToDisable.Length; i++)
			{
				if (gameObjectsToDisable[i] == selfUnit.gameObject)
				{
					Debug.LogWarning("In FOW unit module of unit " + name +
					                 " in hideable game object set its self object. It is wrong. Ignoring.");
					continue;
				}

				gameObjectsToDisable[i].SetActive(visibility);
			}

			isVisibleInFOW = visibility;
		}
	}
}