using System;
using System.Collections.Generic;
using System.Threading;
using _Scripts.Configs;
using _Scripts.Dices;
using _Scripts.Utility;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using VContainer;

namespace _Scripts.Managers
{
    public class ReturnSequenceController : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Timings")]
        [SerializeField] private float _delayPerDice = 0.045f;
        [SerializeField] private float _returnTime = 0.32f;

        [Header("Animation")]
        [SerializeField] private float _hopHeight = 0.45f;
        [SerializeField] private float _hopScaleMult = 1.15f;
        [SerializeField] private float _hopDurationRatio = 0.3f;
        [SerializeField] private Vector3 _flyRotation = new(180f, 360f, 90f);
        [SerializeField] private Ease _hopEase = Ease.OutQuad;
        [SerializeField] private Ease _flyMoveEase = Ease.InBack;
        [SerializeField] private Ease _flyScaleEase = Ease.InQuad;

        [Header("SFX")]
        [SerializeField] private string _returnSfx = "DiceReturn";
        [SerializeField] private float _pitchStep = 0.06f;

        private CancellationTokenSource _cts;

        #region DI
        private ISfxPlayer _sfxPlayer;
        private GameStateManager _gameStateManager;
        private GameSpeedConfig _speedConfig;
        private Vector3 _returnTarget;

        [Inject]
        public void Construct(
            ISfxPlayer sfxPlayer,
            GameStateManager gameStateManager,
            GameSpeedConfig speedConfig,
            DiceSpawnPoint spawnPoint)
        {
            _sfxPlayer = sfxPlayer;
            _gameStateManager = gameStateManager;
            _speedConfig = speedConfig;
            _returnTarget = spawnPoint.transform.position;

            _gameStateManager.StateChanged += OnStateChanged;
        }
        #endregion
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void OnDestroy()
        {
            _gameStateManager.StateChanged -= OnStateChanged;
            CancelSequence();
        }

        private void OnStateChanged(GameState state)
        {
            if (state != GameState.Game)
                CancelSequence();
        }

        private void CancelSequence()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        public async UniTask ReturnAllDiceAsync(IReadOnlyList<Dice> dices)
        {
            CancelSequence();
            _cts = new();
            var token = _cts.Token;

            var tasks = new List<UniTask>(dices.Count);

            try
            {
                for (int i = 0; i < dices.Count; i++)
                {
                    tasks.Add(ReturnDiceAsync(dices[i], i, token));

                    if (i < dices.Count - 1)
                        await _speedConfig.DelayAsync(_delayPerDice, token);
                }

                await UniTask.WhenAll(tasks);
            }
            catch (OperationCanceledException) { }
        }

        public async UniTask ReturnDiceAsync(Dice dice, int stepIndex = 0, CancellationToken token = default)
        {
            var duration = _speedConfig.ScaleTime(_returnTime);
            PlayReturnSfx(dice.transform.position, stepIndex);

            var diceTransform = dice.transform;
            diceTransform.DOKill();
            dice.View.ResetVisual();

            var initialScale = dice.View.InitialScale;
            var hopPos = diceTransform.position + Vector3.up * _hopHeight;
            var hopDuration = duration * _hopDurationRatio;
            var flyDuration = duration * (1f - _hopDurationRatio);

            var seq = DOTween.Sequence().SetLink(dice.gameObject);

            seq.Append(diceTransform.DOMove(hopPos, hopDuration).SetEase(_hopEase));
            seq.Join(diceTransform.DOScale(initialScale * _hopScaleMult, hopDuration).SetEase(_hopEase));

            seq.Append(diceTransform.DOMove(_returnTarget, flyDuration).SetEase(_flyMoveEase));
            seq.Join(diceTransform.DOScale(Vector3.zero, flyDuration).SetEase(_flyScaleEase));
            seq.Join(diceTransform.DORotate(_flyRotation, flyDuration, RotateMode.LocalAxisAdd));

            await seq.ToUniTask(cancellationToken: token);
        }

        private void PlayReturnSfx(Vector3 position, int step)
        {
            var pitchMult = 1f + (_pitchStep * step);
            _sfxPlayer.Play(_returnSfx, position, pitchMult);
        }
    }
}