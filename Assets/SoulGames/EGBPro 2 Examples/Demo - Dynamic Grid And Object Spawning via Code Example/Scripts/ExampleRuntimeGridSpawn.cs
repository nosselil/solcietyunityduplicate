using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SoulGames.EasyGridBuilderPro.Examples
{
    public class ExampleRuntimeGridSpawn : MonoBehaviour
    {
        [SerializeField] private EasyGridBuilderPro gridTemplate;
        [SerializeField] private Vector3 spawnPosition;
        [SerializeField] private List<BuildableObjectSO> buildableObjectSOList;

        [Space]
        [SerializeField] private BuildableObjectSO firstBuildableObjectSO;
        [SerializeField] private Vector3 firstObjectWorldPosition;

        [Space]
        [SerializeField] private BuildableObjectSO secondBuildableObjectSO;
        [SerializeField] private Vector2Int secondObjectGridCellPosition;

        private EasyGridBuilderPro easyGridBuilderPro;

        private void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame) SpawnGridSystem();
            if (Keyboard.current.jKey.wasPressedThisFrame) AddBuildableObjectSOToTheList();
            if (Keyboard.current.kKey.wasPressedThisFrame) RemoveAllBuildableObjectSOFromTheList();

            if (Keyboard.current.digit1Key.wasPressedThisFrame) SpawnFirstBuildableObjectUsingWorldPosition();
            if (Keyboard.current.digit2Key.wasPressedThisFrame) SpawnSecondBuildableObjectUsingCellPosition();
        }

        private void SpawnGridSystem()
        {
            if (easyGridBuilderPro) return;
            GridManager.Instance.InstantiateGridSystem(gridTemplate, spawnPosition, out easyGridBuilderPro);
        }

        private void AddBuildableObjectSOToTheList()
        {
            if (!easyGridBuilderPro) return;
            foreach (BuildableObjectSO buildableObjectSO in buildableObjectSOList)
            {
                easyGridBuilderPro.AddBuildableObjectSOToTheList(buildableObjectSO);
            }
        }

        private void RemoveBuildableObjectSOFromTheList()
        {
            if (!easyGridBuilderPro) return;
            foreach (BuildableObjectSO buildableObjectSO in buildableObjectSOList)
            {
                easyGridBuilderPro.RemoveBuildableObjectSOFromTheList(buildableObjectSO);
            }
        }

        private void RemoveAllBuildableObjectSOFromTheList()
        {
            if (!easyGridBuilderPro) return;
            easyGridBuilderPro.ClearBuildableObjectSOLists();
        }

        private void SpawnFirstBuildableObjectUsingWorldPosition()
        {
            if (!easyGridBuilderPro) return;
            
            easyGridBuilderPro.SetActiveGridMode(GridMode.BuildMode);
            easyGridBuilderPro.SetInputBuildableObjectPlacement(firstBuildableObjectSO, firstObjectWorldPosition);
            easyGridBuilderPro.SetActiveGridMode(GridMode.None);
        }

        private void SpawnSecondBuildableObjectUsingCellPosition()
        {
            if (!easyGridBuilderPro) return;

            Vector3 worldPosition = easyGridBuilderPro.GetActiveGridCellWorldPosition(secondObjectGridCellPosition);
            easyGridBuilderPro.SetActiveGridMode(GridMode.BuildMode);
            easyGridBuilderPro.SetInputBuildableObjectPlacement(secondBuildableObjectSO, worldPosition);
            easyGridBuilderPro.SetActiveGridMode(GridMode.None);
        }
    }
}