#pragma warning disable CS0414

using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using System;
using System.Runtime.CompilerServices;

namespace Watermelon
{
    [ExecuteInEditMode]
    public class WeaponPlacementHelper : MonoBehaviour
    {

#if UNITY_EDITOR
        [SerializeField, ReadOnly] GameObject receiverObject;
        private IWeaponPlacementReceiver receiver;

        [SerializeField, ReadOnly] LevelsDatabase database;

        [SerializeField, HideInInspector] bool isInited = false;
        public bool IsInited => isInited && receiver != null;

        [ShowIf("HasAnimation")]
        [SerializeField, Range(0, 1)] float animationTime;

        private List<GunData> Guns { get; set; }

        private GunBehavior Gun { get; set; }
        private int GunIndex { get; set; }
        private AnimationClip AnimationClip { get; set; }

        [SerializeField, HideInInspector] bool isClonerActive = false;

        [Order(101)]
        [SerializeField, ShowIf("isClonerActive")] GameObject cloneDataFrom;

        [Order(100)]
        [Button("Activate Cloner", "isClonerActive", ButtonVisibility.HideIf)]
        private void ActivateCloner()
        {
            if (!IsInited) Init();
            isClonerActive = true;
        }

        [Order(100)]
        [Button("Disable Cloner", "isClonerActive", ButtonVisibility.ShowIf)]
        private void DisableCloner()
        {
            isClonerActive = false;

            RuntimeEditorUtils.SetDirty(transform);
        }

        [Order(102)]
        [Button("Clone", "isClonerActive", ButtonVisibility.ShowIf)]
        private void Clone()
        {
            if (!IsInited) Init();
            if (!IsInited) return;

            if (cloneDataFrom != null)
            {
                IWeaponPlacementReceiver clonerReceiver = cloneDataFrom.GetComponent<IWeaponPlacementReceiver>();

                if(clonerReceiver != null)
                {
                    receiver.CloneWeaponPlacementData(clonerReceiver);

                    RuntimeEditorUtils.SetDirty(transform);

                    Debug.Log("The Tool Data has been cloned successfully");

                    isClonerActive = false;
                }
            }
        }

        public void Init()
        {
            receiver = GetComponent<IWeaponPlacementReceiver>();
            receiverObject = receiver.GameObject;

            string[] databaseGuids = AssetDatabase.FindAssets("t:"+typeof(LevelsDatabase).Name); //FindAssets uses tags check documentation for more info

            if(databaseGuids.Length != 0)
            {
                string databasePath = AssetDatabase.GUIDToAssetPath(databaseGuids[0]);
                database = AssetDatabase.LoadAssetAtPath<LevelsDatabase>(databasePath);
            } else
            {
                database = null;
            }

            if (receiver == null || database == null)
            {
                isInited = false;
                return;
            }

            isInited = true;
        }

        private void Awake()
        {
            if (Application.isPlaying)
            {
                Destroy(this);
            }
        }

        void Update()
        {
            if (IsInited && receiverObject != null && Gun != null)
            {
                if (receiver.HasWeaponDataChanged(Guns[GunIndex].Id, Gun.transform))
                {
                    receiver.SetWeaponData(Guns[GunIndex].Id, Gun.transform);

                    RuntimeEditorUtils.SetDirty(transform);
                }
            }
        }

        private void OnValidate()
        {
            if (!IsInited || receiverObject != null) Init();

            if (HasAnimation())
            {
                SampleAnimation();
            }
        }

        [Order(0)]
        [Button("Start Calibration", "IsReadyToCalibrate", ButtonVisibility.ShowIf)]
        private void StartCalibration()
        {
            if (!IsInited) Init();
            if (!IsInited) return;

            Guns = new List<GunData>();

            for (int i = 0; i < database.GunsData.Count; i++)
            {
                GunData data = database.GunsData[i];
                if (data.Prefab == null) continue;

                Guns.Add(data);
            }

            if (Guns.Count > 0)
            {
                UnityEngine.Object gunObject = PrefabUtility.InstantiatePrefab(Guns[0].Prefab);
                Gun = (gunObject as GameObject).GetComponent<GunBehavior>();
                Gun.transform.SetParent(receiver.GunHolder);
                GunIndex = 0;
                Gun.gameObject.hideFlags = HideFlags.DontSave;

                receiver.TransferWeaponData(Guns[GunIndex].Id, Gun.transform);

                AnimationClip = Guns[GunIndex].PoseAnimation;

                AnimationMode.StartAnimationMode();
                PrefabStage.prefabStageClosing += OnPrefabClosing;

                animationTime = 0f;
                SampleAnimation();
            }
        }

        private void OnPrefabClosing(PrefabStage obj)
        {
            StopCalibration();
        }


        [Button("Stop Calibration", "CanStopCalibration", ButtonVisibility.ShowIf)]
        private void StopCalibration()
        {
            GunBehavior gun = Gun;

            DestroyImmediate(gun.gameObject);

            Gun = null;
            Guns = null;

            AnimationMode.StopAnimationMode();
            PrefabStage.prefabStageClosing -= OnPrefabClosing;

            RuntimeEditorUtils.SetDirty(transform);
        }

        [HorizontalGroup("Weapon",110)]
        [Button("Prev Weapon", "PrevWeaponAvailable", ButtonVisibility.ShowIf)]
        private void PrevWeapon()
        {
            GunBehavior gun = Gun;

            UnityEditor.EditorApplication.delayCall += () =>
            {
                DestroyImmediate(gun.gameObject);
            };

            GunIndex--;

            GunData gunData = Guns[GunIndex];

            Gun = Instantiate(gunData.Prefab).GetComponent<GunBehavior>();
            Gun.transform.SetParent(receiver.GunHolder);
            receiver.TransferWeaponData(gunData.Id, Gun.transform);
            Gun.gameObject.hideFlags = HideFlags.DontSave;

            AnimationClip = gunData.PoseAnimation;

            animationTime = 0f;
            SampleAnimation();
        }

        [HorizontalGroup("Weapon")]
        [Button("Next Weapon", "NextWeaponAvailable", ButtonVisibility.ShowIf)]
        private void NextWeapon()
        {
            GunBehavior gun = Gun;

            UnityEditor.EditorApplication.delayCall += () =>
            {
                DestroyImmediate(gun.gameObject);
            };

            GunIndex++;

            GunData gunData = Guns[GunIndex];

            Gun = Instantiate(gunData.Prefab).GetComponent<GunBehavior>();
            Gun.transform.SetParent(receiver.GunHolder);
            receiver.TransferWeaponData(gunData.Id, Gun.transform);
            Gun.gameObject.hideFlags = HideFlags.DontSave;

            AnimationClip = gunData.PoseAnimation;

            animationTime = 0f;
            SampleAnimation();
        }

        [Order(103)]
        [Button("Clear Weapons", "ShoudClearWeapons")]
        private void ClearWeapons()
        {
            if (receiverObject != null && receiver.GunHolder != null && receiver.GunHolder.childCount != 0)
            {
                for (int i = receiver.GunHolder.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(receiver.GunHolder.GetChild(i).gameObject);
                }
            }

            RuntimeEditorUtils.SetDirty(transform);
        }

        private bool ShoudClearWeapons()
        {
            return IsReadyToCalibrate() && receiverObject != null && receiver.GunHolder != null && receiver.GunHolder.childCount != 0;
        }

        private void SampleAnimation()
        {
            if (AnimationClip != null)
            {
                GameObject animatorObject = gameObject;

                if (gameObject.GetComponent<Animator>() == null)
                {
                    Animator animator = gameObject.GetComponentInChildren<Animator>();
                    if (animator != null)
                    {
                        animatorObject = animator.gameObject;
                    }
                }

                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(animatorObject, AnimationClip, Mathf.Lerp(0, AnimationClip.length, animationTime));
                AnimationMode.EndSampling();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsReadyToCalibrate() => IsInited && Gun == null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CanStopCalibration() => Gun != null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasAnimation() => IsInited && Gun != null && AnimationClip != null && AnimationMode.InAnimationMode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool NextWeaponAvailable() => Gun != null && GunIndex < Guns.Count - 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool PrevWeaponAvailable() => Gun != null && GunIndex > 0;

#endif
    }
}
