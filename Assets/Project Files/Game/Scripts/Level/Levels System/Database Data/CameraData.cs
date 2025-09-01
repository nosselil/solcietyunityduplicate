using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class CameraData
    {
        [SerializeField] string id;
        public string Id => id;

        [SerializeField] Vector3 followOffset = new Vector3(0, 20, -20);
        public Vector3 FollowOffset => followOffset;

        [SerializeField] Vector3 rotation;
        public Vector3 Rotation => rotation;

        [SerializeField] float fov = 40;
        public float FOV => fov;

        [SerializeField] bool isMovingSideways = false;
        public bool IsMovingSideways => isMovingSideways;
    }
}
