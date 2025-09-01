using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [StaticUnload]
    public class MovementManager : MonoBehaviour
    {
        [SerializeField] MovementModeDatabase database;

        private InputHandler inputHandler;

        private static MovementMode currentMovementMode;
        private static PlayerBehavior player;

        private static List<OppositeMover> oppositeMovers = new List<OppositeMover>();

        private bool IsInitialised { get; set; }

        private void Awake()
        {
            if (!IsInitialised) Initialise();
        }

        private void Initialise()
        {
            IsInitialised = true;

            currentMovementMode = null;
            player = null;
            oppositeMovers = new List<OppositeMover>();
        }

        public void Init(MovementModeType movementModeType, RoadData roadData)
        {
            if (!IsInitialised) Initialise();

            UIGame gameUI = UIController.GetPage<UIGame>();
            inputHandler = gameUI.InputHandler;

            inputHandler.OnPointerDragged += OnPointerDragged;
            currentMovementMode = CreateMovementMode(movementModeType, roadData);
        }

        private void OnPointerDragged(Vector2 delta)
        {
            currentMovementMode.ProcessPointerInput(delta);
        }

        public void SetPlayer(PlayerBehavior player)
        {
            if (!IsInitialised) Initialise();

            MovementManager.player = player;
            if(currentMovementMode != null) player.SetIsMovingForward(currentMovementMode.ModeData.ForwardMovementEnabled);
        }

        public static void RegisterOppositeMover(OppositeMover oppositeMover)
        {
            if (!oppositeMovers.Contains(oppositeMover))
            {
                oppositeMovers.Add(oppositeMover);

                currentMovementMode.UpdateOppositeMover(oppositeMover);
            }
        }

        public static void RemoveOppositeMover(OppositeMover oppositeMover)
        {
            oppositeMovers.Remove(oppositeMover);
        }

        public void SetInitialPosition(float z)
        {
            currentMovementMode.SetPosition(Vector3.forward * z);
        }

        public void ResetMovement()
        {
            currentMovementMode?.Reset();
        }

        public static void StopMoving()
        {
            for(int i = 0; i < oppositeMovers.Count; i++)
            {
                OppositeMover oppositeMover = oppositeMovers[i];

                oppositeMover.SetMoveSpeed(false, 0);

                player.SetIsMovingForward(false);
            }
        }

        private void Update()
        {
            if (!GameController.IsGameplayActive) return;
            currentMovementMode.Update();

            player.UpdatePosition(currentMovementMode.Position);
        }

        private void OnDestroy()
        {
            if (inputHandler != null)
            {
                inputHandler.OnPointerDragged -= OnPointerDragged;
            }

            currentMovementMode = null;

            oppositeMovers.Clear();
        }

        private MovementMode CreateMovementMode(MovementModeType modeType, RoadData roadData)
        {
            MovementModeData data = database.GetModeData(modeType);
            
            if (player != null) player.SetIsMovingForward(data.ForwardMovementEnabled);

            switch (modeType)
            {
                case MovementModeType.Classic:

                    ClassicMovementMode classicMode = new ClassicMovementMode(data);
                    classicMode.SetRoadWidth(roadData.RoadWidth);

                    return classicMode;

                case MovementModeType.Sideways:

                    SidewaysMovementMode sidewaysMode = new SidewaysMovementMode(data);
                    sidewaysMode.SetRoadWidth(roadData.RoadWidth);

                    return sidewaysMode;
            }

            return null;
        }

        private static void UnloadStatic()
        {
            oppositeMovers = new List<OppositeMover>();

            player = null;
            currentMovementMode = null;
        }
    }
}