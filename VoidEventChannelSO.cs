using System;
using UnityEngine;

namespace InfiniteAdventure
{
    [CreateAssetMenu(menuName = "Game/Events/Void")]
    public class VoidEventChannelSO : ScriptableObject
    {
        public event Action Raised;
        public void Raise() => Raised?.Invoke();
    }
}
