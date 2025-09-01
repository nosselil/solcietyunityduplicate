using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class SavableEnvironment : AbstractSavable, IEquatable<SavableEnvironment>
    {
        public override bool Equals(object obj)
        {
            return Equals(obj as SavableEnvironment);
        }

        public bool Equals(SavableEnvironment other)
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
