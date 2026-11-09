using System;
using UnityEngine;

namespace InfiniteAdventure
{
    /// <summary>Typed ScriptableObject event channel. Publisher and subscriber share only the asset.</summary>
    public abstract class EventChannelSO<T> : ScriptableObject
    {
        public event Action<T> Raised;
        public void Raise(T value) => Raised?.Invoke(value);
    }
}
