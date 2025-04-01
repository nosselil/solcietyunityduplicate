using System.Collections.Generic;
using UnityEngine;

namespace SoulGames.EasyGridBuilderPro.Examples
{
    public class ExampleInventoryCellUnlocker : MonoBehaviour
    {
        [SerializeField] private List<GameObject> unlockObjectList;
        
        public void UnlockObject()
        {
            if (unlockObjectList.Count <= 0) return;
            unlockObjectList[0].transform.position = Vector3.zero;
            unlockObjectList.RemoveAt(0);
        }
    }
}