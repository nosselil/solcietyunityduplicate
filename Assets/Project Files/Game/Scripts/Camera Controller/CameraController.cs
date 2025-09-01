using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [DefaultExecutionOrder(100)]
    public sealed class CameraController : MonoBehaviour, ISceneSavingCallback
    {
        private static CameraController cameraController;

        [SerializeField] CameraType firstCamera;

        [Space]
        [ReadOnly]
        [SerializeField] VirtualCamera[] virtualCameras;

        [Header("Blends")]
        [SerializeField] CameraBlendSettings[] blendSettings;

        [UnpackNested]
        [SerializeField] CameraBlendData defaultBlendData;

        private static Transform cameraTransform;

        private static Dictionary<CameraType, int> virtualCamerasLink;

        private static Camera mainCamera;
        public static Camera MainCamera => mainCamera;

        private static VirtualCamera activeVirtualCamera;
        public static VirtualCamera ActiveVirtualCamera => activeVirtualCamera;

        private static bool isBlending;
        public static bool IsBlending => isBlending;

        private static CameraBlendCase currentBlendCase;

        public void Initialise()
        {
            cameraController = this;

            // Get camera component
            mainCamera = GetComponent<Camera>();
            cameraTransform = transform;

            // Initialise cameras link
            virtualCamerasLink = new Dictionary<CameraType, int>();
            for (int i = 0; i < virtualCameras.Length; i++)
            {
                virtualCameras[i].Init();

                virtualCamerasLink.Add(virtualCameras[i].CameraType, i);
            }

            VirtualCamera firstVirtualCamera = GetCamera(firstCamera);
            firstVirtualCamera.Activate();

            activeVirtualCamera = firstVirtualCamera;

            UpdateCamera();
        }

        private static void UpdateCamera()
        {
            UpdateCamera(activeVirtualCamera.CameraData);
        }

        private static void UpdateCamera(CameraLocalData cameraData)
        {
            if (activeVirtualCamera.Target == null) return;

            mainCamera.fieldOfView = cameraData.FieldOfView;
            mainCamera.nearClipPlane = cameraData.NearClipPlane;
            mainCamera.farClipPlane = cameraData.FarClipPlane;

            cameraTransform.SetPositionAndRotation(cameraData.Position, cameraData.Rotation);
        }

        private void LateUpdate()
        {
            if (activeVirtualCamera == null) return;

            // Update camera position
            if(isBlending)
            {
                UpdateCamera(currentBlendCase.CameraData);

                return;
            }

            UpdateCamera();
        }

        public static VirtualCamera GetCamera(CameraType cameraType)
        {
            return cameraController.virtualCameras[virtualCamerasLink[cameraType]];
        }

        private static CameraBlendData GetBlendData(CameraType firstCameraType, CameraType secondCameraType)
        {
            for (int i = 0; i < cameraController.blendSettings.Length; i++)
            {
                if (cameraController.blendSettings[i].FirstCameraType == firstCameraType && cameraController.blendSettings[i].SecondCameraType == secondCameraType)
                {
                    return cameraController.blendSettings[i].BlendData;
                }
            }

            return cameraController.defaultBlendData;
        }

        public static void EnableCamera(CameraType cameraType)
        {
            if (activeVirtualCamera != null && activeVirtualCamera.CameraType == cameraType)
                return;

            VirtualCamera virtualCamera = GetCamera(cameraType);
            if (virtualCamera == null)
            {
                Debug.LogError($"Camera of type {cameraType} not found.");

                return;
            }

            if (activeVirtualCamera == null)
            {
                activeVirtualCamera = virtualCamera;
                activeVirtualCamera.Activate();

                UpdateCamera();

                return;
            }

            CameraType currentCameraType = activeVirtualCamera.CameraType;

            // Get blend data
            CameraBlendData blendData = GetBlendData(currentCameraType, cameraType);

            if (blendData.BlendTime <= 0)
            {
                activeVirtualCamera.Disable();

                activeVirtualCamera = virtualCamera;
                activeVirtualCamera.Activate();

                UpdateCamera();

                return;
            }

            isBlending = true;

            currentBlendCase = new CameraBlendCase(activeVirtualCamera, virtualCamera, blendData, () =>
            {
                activeVirtualCamera.Disable();
                activeVirtualCamera = virtualCamera;
                activeVirtualCamera.Activate();

                isBlending = false;
            });
        }

        public void OnSceneSaving()
        {
            VirtualCamera[] cachedVirtualCameras = FindObjectsByType<VirtualCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (!cachedVirtualCameras.SafeSequenceEqual(virtualCameras))
            {
                virtualCameras = cachedVirtualCameras;

                RuntimeEditorUtils.SetDirty(this);
            }
        }
    }

    public class CameraBlendCase
    {
        public readonly VirtualCamera FirstCamera;
        public readonly VirtualCamera SecondCamera;

        private CameraBlendData cameraBlendData;

        private Ease.IEasingFunction easingFunction;

        private TweenCase tweenCase;

        private CameraLocalData cameraData;
        public CameraLocalData CameraData => cameraData;

        public CameraBlendCase(VirtualCamera firstCamera, VirtualCamera secondCamera, CameraBlendData cameraBlendData, SimpleCallback completeCallback)
        {
            this.cameraBlendData = cameraBlendData;

            FirstCamera = firstCamera;
            SecondCamera = secondCamera;

            firstCamera.StartTransition();
            secondCamera.StartTransition();

            cameraData = new CameraLocalData(firstCamera.CameraData);

            easingFunction = Ease.GetFunction(cameraBlendData.BlendEaseType);

            tweenCase = Tween.DoFloat(0, 1.0f, cameraBlendData.BlendTime, (value) =>
            {
                cameraData.Lerp(secondCamera.CameraData, value);
            }).SetCustomEasing(easingFunction).OnComplete(() =>
            {
                FirstCamera.StopTransition();
                SecondCamera.StopTransition();

                completeCallback?.Invoke();
            });
        }

        public void Clear()
        {
            tweenCase.KillActive();
        }
    }

    public class CameraLocalData
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public float FieldOfView;
        public float NearClipPlane;
        public float FarClipPlane;

        public CameraLocalData() { }

        public CameraLocalData(CameraLocalData cameraData)
        {
            Position = cameraData.Position;
            Rotation = cameraData.Rotation;

            FieldOfView = cameraData.FieldOfView;
            NearClipPlane = cameraData.NearClipPlane;
            FarClipPlane = cameraData.FarClipPlane;
        }

        public void Lerp(CameraLocalData target, float t)
        {
            Position = Vector3.Lerp(Position, target.Position, t);
            Rotation = Quaternion.Slerp(Rotation, target.Rotation, t);
            FieldOfView = Mathf.Lerp(FieldOfView, target.FieldOfView, t);
            NearClipPlane = Mathf.Lerp(NearClipPlane, target.NearClipPlane, t);
            FarClipPlane = Mathf.Lerp(FarClipPlane, target.FarClipPlane, t);
        }
    }
}