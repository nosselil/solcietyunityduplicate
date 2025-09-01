using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class BonusStageBehavior : MonoBehaviour
    {
        [SerializeField] SecondStageFinishBehavior finish;
        [SerializeField] ChestBehavior chest;
        [SerializeField] Transform stripesParent;
        [SerializeField] List<Color> stripesColors;

        [Space]
        [SerializeField] List<SpriteRenderer> stripes;

        public BonusStageData Data { get; private set; }

        private List<ObstacleBehavior> obstacles = new List<ObstacleBehavior>();

        private FirstStageFinishBehavior firstStageFinish;

        private TweenCase delayTweenCase;

        private void Awake()
        {
            transform.GetComponentsInChildren(obstacles);

            for(int i = 0; i < stripes.Count; i++)
            {
                Color color = stripesColors[i % stripesColors.Count];

                stripes[i].color = color;
            }
        }

        public void Init(BonusStageData bonusStageData, FirstStageFinishBehavior firstStageFinish)
        {
            Data = bonusStageData;

            this.firstStageFinish = firstStageFinish;
            firstStageFinish.onFinishReached += OnStartedBonusStage;

            transform.position = firstStageFinish.transform.position;

            finish.onFinishReached += OnFinishReached;

            finish.Init();
            chest.Init();
        }

        private void OnStartedBonusStage()
        {
            firstStageFinish.onFinishReached -= OnStartedBonusStage;
            chest.ShakeChest();
        }

        private void OnFinishReached()
        {
            finish.onFinishReached -= OnFinishReached;

            MovementManager.StopMoving();

            GameController.OnChestReached();

            chest.OpenChest();

            delayTweenCase = Tween.DelayedCall(3f, GameController.OnLevelComplete);
        }

        private void OnDestroy()
        {
            delayTweenCase.KillActive();
        }

        public void Clear()
        {
            delayTweenCase.KillActive();

            if (finish != null)
                finish.Clear();

            if(!obstacles.IsNullOrEmpty())
            {
                for (int i = 0; i < obstacles.Count; i++)
                {
                    if (obstacles[i] != null)
                        obstacles[i].Clear();
                }
            }
        }
    }
}
