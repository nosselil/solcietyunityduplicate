using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public sealed class VirtualCamera : MonoBehaviour
    {
        [SerializeField] CameraType cameraType;
        public CameraType CameraType => cameraType;

        [Header("Lens")]
        [SerializeField] float fov = 60.0f;
        [SerializeField] float nearClipPlane = 0.1f;
        [SerializeField] float farClipPlane = 1000.0f;

        [Header("Position")]
        [SerializeField] Vector3 followOffset;
        [SerializeField] Vector3 followRotation;

        private bool isShaking;
        private float shakeGain = 0.0f;
        private TweenCase shakeTweenCase;

        private bool isActive;
        public bool IsActive => isActive;

        private bool isTransitioning;
        public bool IsTransitioning => isTransitioning;

        private CameraLocalData cameraData;
        public CameraLocalData CameraData => cameraData;

        private Transform target;
        public Transform Target => target;

        public void Init()
        {
            isActive = false;
            isTransitioning = false;

            cameraData = new CameraLocalData()
            {
                Position = followOffset,
                Rotation = Quaternion.Euler(followRotation),

                FieldOfView = fov,
                NearClipPlane = nearClipPlane,
                FarClipPlane = farClipPlane
            };

            enabled = false;
        }

        public void SetTarget(Transform target)
        {
            this.target = target;

            enabled = true;
        }

        public void SetFollowOffset(Vector3 followOffset)
        {
            this.followOffset = followOffset;

            cameraData.Position = followOffset;
        }

        public void SetRotation(Vector3 rotation)
        {
            this.followRotation = rotation;

            cameraData.Rotation = Quaternion.Euler(followRotation);
        }

        public void SetFov(float fOV)
        {
            this.fov = fOV;

            cameraData.FieldOfView = fOV;
        }

        private void LateUpdate()
        {
            if (!isActive && !isTransitioning)
                return;

            if(isShaking)
            {
                // Recalculate camera position
                cameraData.Position = target.position + followOffset + (Random.onUnitSphere * shakeGain * Time.deltaTime);
            }
            else
            {
                // Recalculate camera position
                cameraData.Position = target.position + followOffset;
            }
        }

        public void StartTransition()
        {
            isTransitioning = true;
        }

        public void StopTransition()
        {
            isTransitioning = false;
        }

        public void Activate()
        {
            isActive = true;
        }

        public void Disable()
        {
            isActive = false;
        }

        public void Shake(float fadeInTime, float fadeOutTime, float duration, float gain)
        {
            if (isShaking) return;

            isShaking = true;

            if (shakeTweenCase != null && !shakeTweenCase.IsCompleted)
                shakeTweenCase.Kill();

            shakeGain = 0;

            shakeTweenCase = Tween.DoFloat(0.0f, gain, fadeInTime, (float fadeInValue) =>
            {
                shakeGain = fadeInValue;
            }).OnComplete(delegate
            {
                shakeTweenCase = Tween.DelayedCall(duration, delegate
                {
                    shakeTweenCase = Tween.DoFloat(gain, 0.0f, fadeOutTime, (float fadeOutValue) =>
                    {
                        shakeGain = fadeOutValue;

                        isShaking = false;
                    });
                });
            });
        }
    }
}