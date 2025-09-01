using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class PlayerBehavior : MonoBehaviour
    {
        private static PlayerBehavior instance;
        public static PlayerBehavior Instance => instance;

        [UnpackNested]
        [SerializeField] PlayerStats playerStats;
        public PlayerStats PlayerStats => playerStats;

        [Space]
        [SerializeField] TargetDetector targetDetector;
        [SerializeField] List<Transform> characterPositions;

        public float CurrentSideShift { get; private set; }
        public float AimingPositionOffsetX { get; private set; }

        private List<CharacterBehavior> characters = new List<CharacterBehavior>();

        private List<StatModifier> damageModifiers = new List<StatModifier>();
        private List<StatModifier> rangeModifiers = new List<StatModifier>();
        private List<StatModifier> fireRateModifiers = new List<StatModifier>();

        public CharacterType CharacterType { get; private set; }
        public string GunId { get; private set; }

        private bool IsShooting { get; set; }
        private bool IsMovingForward { get; set; }
        public bool ShouldMoveSideways { get; private set; }

        public event SimpleCallback Cleared;

        private void Awake()
        {
            targetDetector.gameObject.SetActive(false);
        }

        public void Init()
        {
            instance = this;

            PlayerStats.Init();

            GunId = LevelController.SkinsManager.DefaultGunId;

            for (int i = 0; i < characters.Count; i++)
            {
                characters[i].Init(this, characterPositions[i], PlayerStats.Health);
            }
        }

        private void OnDestroy()
        {
            characters.Clear();
            IsShooting = false;

            targetDetector.Clear();
            targetDetector.gameObject.SetActive(false);

            damageModifiers.Clear();
            fireRateModifiers.Clear();
            rangeModifiers.Clear();
        }

        public void StartLevel(CharacterType characterType)
        {
            CharacterType = characterType;

            targetDetector.OnFirstTargetDetected += StartShooting;
            targetDetector.OnNoMoreTargetsDetected += StopShooting;
            targetDetector.gameObject.SetActive(true);
            targetDetector.Init();

            damageModifiers.Add(StatModifier.Plus(PlayerStats.Damage));
            fireRateModifiers.Add(StatModifier.Plus(PlayerStats.FireRate));
            rangeModifiers.Add(StatModifier.Plus(PlayerStats.BulletRange));

            AddCharacter(LevelController.SkinsManager.GetDefaultCharacter(characterType));
        }

        public void SetInitialPosition(float z)
        {
            Vector3 position = transform.position;
            position.z = z;
            transform.position = position;
        }

        public CharacterBehavior GetFirstCharacter()
        {
            if (characters.Count == 0)
                return null;

            return characters[0];
        }

        public void AddCharacter(CharacterBehavior character)
        {
            if (characterPositions.Count <= characters.Count)
            {
                Transform newPosition = new GameObject().transform;

                newPosition.SetParent(characterPositions[^1].parent);
                newPosition.localPosition = characterPositions[^1].localPosition + Vector3.back * 0.5f;

                characterPositions.Add(newPosition);
            }
            character.Init(this, characterPositions[characters.Count], PlayerStats.Health);

            if (IsMovingForward)
            {
                character.EnableMovingAnimation();
            }
            else
            {
                character.DisableMovingAnimation();
            }

            if (!character.Data.IsGunLocked)
            {
                character.ReinitGun();
            }

            character.RecalculateBulletRange(rangeModifiers, PlayerStats.MinRange);
            character.RecalculateDamage(damageModifiers, PlayerStats.MinDamage);
            character.RecalculateFireRate(fireRateModifiers, PlayerStats.MinFireRate);

            if (IsShooting)
                character.StartShooting();

            characters.Add(character);

            Tween.DelayedCall(0.1f, () =>
            {
                RecalculateAimingPosition();
            });
        }

        private void RecalculateAimingPosition()
        {
            float mostLeftCharacterPosX = float.MaxValue;
            float mostRightCharacterPosX = -float.MaxValue;

            for (int i = 0; i < characters.Count; i++)
            {
                if (characters[i].transform.position.x < mostLeftCharacterPosX)
                    mostLeftCharacterPosX = characters[i].transform.position.x;

                if (characters[i].transform.position.x > mostRightCharacterPosX)
                    mostRightCharacterPosX = characters[i].transform.position.x;
            }

            AimingPositionOffsetX = (mostLeftCharacterPosX + (mostRightCharacterPosX - mostLeftCharacterPosX) * 0.5f) - CurrentSideShift;
        }

        public void ChangeGun(string gunId)
        {
            GunId = gunId;

            for (int i = 0; i < characters.Count; i++)
            {
                CharacterBehavior character = characters[i];
                CharacterData characterData = character.Data;

                if (!characterData.IsGunLocked)
                {
                    character.ReinitGun();

                    character.RecalculateBulletRange(rangeModifiers, PlayerStats.MinRange);
                    character.RecalculateDamage(damageModifiers, PlayerStats.MinDamage);
                    character.RecalculateFireRate(fireRateModifiers, PlayerStats.MinFireRate);
                }
            }
        }

        private void StartShooting()
        {
            IsShooting = true;
            for (int i = 0; i < characters.Count; i++)
            {
                characters[i].StartShooting();
            }
        }

        private void StopShooting()
        {
            IsShooting = false;
            for (int i = 0; i < characters.Count; i++)
            {
                characters[i].StopShooting();
            }
        }

        public void SetIsMovingForward(bool isMovingForward)
        {
            IsMovingForward = isMovingForward;
            for (int i = 0; i < characters.Count; i++)
            {
                if (isMovingForward)
                {
                    characters[i].EnableMovingAnimation();
                }
                else
                {
                    characters[i].DisableMovingAnimation();
                }

            }
        }

        public void SetShouldMoveSideways(bool shouldMoveSideways)
        {
            ShouldMoveSideways = shouldMoveSideways;
        }

        public void UpdatePosition(Vector3 position)
        {
            CurrentSideShift = position.x;

            if (ShouldMoveSideways)
            {
                transform.position = position;

                for (int i = 0; i < characters.Count; i++)
                {
                    characters[i].UpdatePosition(position.SetZ(0).SetX(0));
                }
            }
            else
            {
                transform.position = position.SetX(0);

                for (int i = 0; i < characters.Count; i++)
                {
                    characters[i].UpdatePosition(position.SetZ(0));
                }
            }
        }

        public void OnCharacterDied(CharacterBehavior character)
        {
            if (characters.Contains(character))
            {
                int characterIndex = characters.IndexOf(character);

                characters.RemoveAt(characterIndex);

                if (characters.Count == 0)
                {
                    if (LevelController.StageId == 1)
                    {
                        GameController.OnGameFail();
                    }
                    else
                    {
                        GameController.OnLevelComplete();
                    }

                    return;
                }

                if (characterIndex == characters.Count)
                    return;

                CharacterBehavior lastCharacter = characters[^1];

                characters.RemoveAt(characters.Count - 1);
                characters.Insert(characterIndex, lastCharacter);

                lastCharacter.ChangeFormationPosition(characterPositions[characterIndex]);

                Tween.DelayedCall(0.55f, () =>
                {
                    RecalculateAimingPosition();
                });
            }

        }

        public void Clear()
        {
            if(this != null)
            {
                if(transform != null)
                    transform.position = Vector3.zero;

                if(targetDetector != null)
                {
                    targetDetector.Clear();
                    targetDetector.gameObject.SetActive(false);
                }
            }

            if(!characters.IsNullOrEmpty())
            {
                for (int i = 0; i < characters.Count; i++)
                {
                    CharacterBehavior character = characters[i];
                    if(character != null)
                        character.Clear();
                }

                characters.Clear();
            }

            IsShooting = false;

            damageModifiers.Clear();
            fireRateModifiers.Clear();
            rangeModifiers.Clear();

            GunId = LevelController.SkinsManager.DefaultGunId;

            Cleared?.Invoke();
        }

        public void Revive(CharacterType characterType)
        {
            AddCharacter(LevelController.SkinsManager.GetDefaultCharacter(characterType));

            if (GameController.Data.IsInvulnerableAfterRevive)
            {
                for (int i = 0; i < characters.Count; i++)
                {
                    CharacterBehavior character = characters[i];

                    character.MakeImmortal();
                }

                Tween.DelayedCall(GameController.Data.InvulnerabilityAfterReviveDuration, () =>
                {
                    for (int i = 0; i < characters.Count; i++)
                    {
                        CharacterBehavior character = characters[i];

                        character.RemoveImmortality();
                    }
                });
            }
        }

        #region Boosters

        public void RegisterFireRateModifier(StatModifier modifier)
        {
            fireRateModifiers.Add(modifier);

            for (int i = 0; i < characters.Count; i++)
            {
                CharacterBehavior character = characters[i];

                character.RecalculateFireRate(fireRateModifiers, PlayerStats.MinFireRate);
            }
        }

        public void RegisterDamageModifier(StatModifier modifier)
        {
            damageModifiers.Add(modifier);

            for (int i = 0; i < characters.Count; i++)
            {
                CharacterBehavior character = characters[i];

                character.RecalculateDamage(damageModifiers, PlayerStats.MinDamage);
            }
        }

        public void RegisterRangeModifier(StatModifier modifier)
        {
            rangeModifiers.Add(modifier);

            for (int i = 0; i < characters.Count; i++)
            {
                CharacterBehavior character = characters[i];

                character.RecalculateBulletRange(rangeModifiers, PlayerStats.MinRange);
            }
        }

        public void AddHealth(StatModifier modifier)
        {
            for (int i = 0; i < characters.Count; i++)
            {
                CharacterBehavior character = characters[i];

                character.AddHealth(modifier);
            }
        }

        public void AddHealth(float value)
        {
            for (int i = 0; i < characters.Count; i++)
            {
                CharacterBehavior character = characters[i];

                character.AddHealth(value);
            }
        }

        public void AddCharactersBooster(NumericalBoosterBehavior booster)
        {
            int newCount = Mathf.RoundToInt(booster.ApplyBoosterToValue(characters.Count));

            if (newCount < 1)
                newCount = 1;
            if (newCount > characterPositions.Count)
                newCount = characterPositions.Count;

            if (newCount == characters.Count)
                return;

            if (newCount > characters.Count)
            {
                for (int i = characters.Count; i < newCount; i++)
                {
                    CharacterBehavior character = LevelController.SkinsManager.GetDefaultCharacter(CharacterType.Humanoid);
                    AddCharacter(character);
                }
            }
            else
            {
                int count = characters.Count - newCount;

                for (int i = 0; i < count; i++)
                {
                    CharacterBehavior character = characters[^1];
                    character.Die();

                    characters.RemoveAt(characters.Count - 1);
                }
            }
        }

        public void RecalculateStatsForCharacter(CharacterBehavior character)
        {
            character.RecalculateBulletRange(rangeModifiers, PlayerStats.MinRange);
            character.RecalculateDamage(damageModifiers, PlayerStats.MinDamage);
            character.RecalculateFireRate(fireRateModifiers, PlayerStats.MinFireRate);
        }

        #endregion

        public static void SubscribeToOnCleared(SimpleCallback callback)
        {
            instance.Cleared += callback;
        }

        public static void UnsubscribeFromOnCleared(SimpleCallback callback)
        {
            instance.Cleared -= callback;
        }
    }
}