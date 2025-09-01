using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class TargetDetector : MonoBehaviour
    {
        private List<ITargetable> detectedTargets = new List<ITargetable>();

        public bool HasDetectedTargets => detectedTargets.Count > 0;

        public event SimpleCallback OnFirstTargetDetected;
        public event SimpleCallback OnNoMoreTargetsDetected;

        private BoxCollider boxCollider;

        public void Init()
        {
            boxCollider = GetComponent<BoxCollider>();
            boxCollider.enabled = true;

            ObstacleBehavior.OnObstacleDestroyed += RemoveTarget;
        }

        private void RemoveTarget(ITargetable target)
        {
            if (target != null && detectedTargets.Contains(target))
            {
                detectedTargets.Remove(target);

                if (detectedTargets.Count == 0)
                {
                    OnNoMoreTargetsDetected?.Invoke();
                }
            }
        }

        public void Clear()
        {
            detectedTargets.Clear();

            OnFirstTargetDetected = null;
            OnNoMoreTargetsDetected = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            ITargetable target = other.gameObject.GetComponent<ITargetable>();

            if(target != null && !detectedTargets.Contains(target))
            {
                detectedTargets.Add(target);

                if(detectedTargets.Count == 1)
                {
                    OnFirstTargetDetected?.Invoke();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            ITargetable target = other.gameObject.GetComponent<ITargetable>();

            RemoveTarget(target);
        }
    }
}