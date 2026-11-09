using System;
using UnityEngine;

namespace InfiniteAdventure
{
    /// <summary>Shared runtime float (e.g. difficulty multiplier). Resets to Initial when the asset loads.</summary>
    [CreateAssetMenu(menuName = "Game/Float Variable")]
    public class FloatVariableSO : ScriptableObject
    {
        [SerializeField] float initial = 1f;
        public float Value { get; private set; }
        public event Action<float> Changed;

        void OnEnable() => Value = initial;

        public void Set(float v)
        {
            if (Mathf.Approximately(v, Value)) return;
            Value = v;
            Changed?.Invoke(v);
        }
    }
}
