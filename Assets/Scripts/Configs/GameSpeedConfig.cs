using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Scripts.Configs
{
    [CreateAssetMenu(fileName = "GameSpeedConfig", menuName = "Configs/Game Speed")]
    public class GameSpeedConfig : ScriptableObject
    {
        [field: SerializeField, Range(0.5f, 8f)] public float SpeedMultiplier { get; set; } = 1f;

        public float Multiplier => Mathf.Max(0.1f, SpeedMultiplier);

        public float ScaleDuration(float baseDuration) => baseDuration / Multiplier;

        public UniTask DelayAsync(float baseSeconds, CancellationToken token = default) => 
            UniTask.Delay(TimeSpan.FromSeconds(ScaleDuration(baseSeconds)), cancellationToken: token);
    }
}