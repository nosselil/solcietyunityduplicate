using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class CameraBlendSettings
    {
        [SerializeField] CameraType firstCameraType;
        public CameraType FirstCameraType => firstCameraType;

        [SerializeField] CameraType secondCameraType;
        public CameraType SecondCameraType => secondCameraType;

        [Space]
        [SerializeField] CameraBlendData blendData;
        public CameraBlendData BlendData => blendData;
    }
}