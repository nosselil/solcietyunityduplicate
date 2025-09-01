using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class LevelNumberSave : ISaveObject
    {
        public int levelNumber;
        public int LevelNumber => levelNumber;

        public void IncrementLevelNumber()
        {
            levelNumber++;
        }

        public void SetLevelNumber(int levelNumber)
        {
            this.levelNumber = levelNumber;
        }

        public void Flush()
        {
            
        }
    }
}
