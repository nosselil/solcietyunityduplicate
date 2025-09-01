using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public abstract class AbstractSavable : MonoBehaviour, IEquatable<AbstractSavable>
    {
        [SerializeField, HideInInspector] protected string id;
        public string Id { get => id; set => id = value; }
        public Vector3 Position => transform.position;

        public override bool Equals(object obj)
        {
            return Equals(obj as AbstractSavable);
        }

        public bool Equals(AbstractSavable other)
        {
            return other is not null &&
                   base.Equals(other) &&
                   Id == other.Id &&
                   Position.Equals(other.Position);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Id, Position);
        }
    }
}
