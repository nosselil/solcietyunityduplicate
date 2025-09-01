using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class BonusStageSave : ISaveObject
    {
        [SerializeField] int bonusStageId;
        [SerializeField] float bonusStageProgress;

        public int BonusStageId { get => bonusStageId; set => bonusStageId = value; }
        public float BonusStageProgress { get => bonusStageProgress; set => bonusStageProgress = value; }   

        public void Flush()
        {

        }
    }
}
