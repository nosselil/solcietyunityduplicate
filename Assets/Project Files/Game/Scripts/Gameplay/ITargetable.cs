using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public interface ITargetable
    {
        void GetHit(float damage, Vector3 hitPoint, float gunModifier = 1);
    }
}