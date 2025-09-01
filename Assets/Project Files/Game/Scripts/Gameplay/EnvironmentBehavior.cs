using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class EnvironmentBehavior : MonoBehaviour, ILevelInitable, IClearable
    {
        public void Init(AbstractLevelData data)
        {
            EnvironmentLevelData environemntData = (EnvironmentLevelData)data;

            if (environemntData == null)
            {
                Debug.LogError("You are trying to init EnvironmentBehavior with the wrong data!");

                return;
            }

            transform.position = data.Position;
        }

        public void Clear()
        {
            if(this != null)
            {
                if (gameObject != null)
                    gameObject.SetActive(false);
            }
        }
    }
}
