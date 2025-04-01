using System.Collections;
using UnityEngine;

namespace SoulGames.EasyGridBuilderPro.Examples
{
    public class ExampleSetGridModeMoveAtStart : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(LateStart());
        }

        private IEnumerator LateStart()
        {
            yield return new WaitForSeconds(0.1f);
            GridManager.Instance.SetActiveGridModeInAllGrids(GridMode.MoveMode);
        }
    }
}