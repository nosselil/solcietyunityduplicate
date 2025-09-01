using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class ObstacleIniter : MonoBehaviour
    {
        [SerializeField] int health;
        [SerializeField] float boosterHeight;
        [SerializeField] DropableItemType dropType;
        [SerializeField] float dropAmount;

        private void Start()
        {
            ObstacleBehavior obstacle = GetComponent<ObstacleBehavior>();

            if(obstacle != null)
            {
                ObstacleLevelData obstacleData = new ObstacleLevelData();

                obstacleData.SetHealth(health);
                obstacleData.SetBoosterHeight(boosterHeight);
                obstacleData.SetPosition(transform.position);
                obstacleData.SetDropType(dropType);
                obstacleData.SetDropItemValue(dropAmount);

                obstacle.Init(obstacleData);
            }
        }
    }
}
