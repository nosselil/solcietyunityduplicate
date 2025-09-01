using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Movement Mode Database", menuName = "Data/Movement/Database")]
    public class MovementModeDatabase : ScriptableObject
    {
        [SerializeField] List<MovementModeData> movementModes;

        public int ModesCount => movementModes.Count;

        public MovementModeData GetModeData(int index)
        {
            return movementModes[index];
        }

        public MovementModeData GetModeData(MovementModeType modeType)
        {
            for(int i = 0; i < movementModes.Count; i++)
            {
                MovementModeData modeData = movementModes[i];
                if(modeData.MovementModeTyple == modeType) return modeData;
            }

            return null;
        }
    }
}