using UnityEngine;

namespace InfiniteAdventure
{
    /// <summary>Lets systems find the player without a direct scene reference.</summary>
    [CreateAssetMenu(menuName = "Game/Transform Anchor")]
    public class TransformAnchorSO : ScriptableObject
    {
        public Transform Value { get; private set; }
        public void Set(Transform t) => Value = t;
        public void Clear(Transform t) { if (Value == t) Value = null; }
        void OnEnable() => Value = null;
    }
}
