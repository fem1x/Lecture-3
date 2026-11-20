using System.Collections.Generic;
using _Scripts.Configs;
using _Scripts.Dices;
using _Scripts.Managers;
using DG.Tweening;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Scripts.UI.Workshop
{
    public class UpgradeSectionView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private TMP_Text _diceTitleText;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private DicePreviewView _previewSlot;
        [SerializeField] private List<FaceUpgradeButtonView> _faceButtons = new();
        [SerializeField] private float _rotateDuration = 0.25f;

        private DiceData _data;
        private Tween _rotateTween;

        #region DI
        private DiceUpgradeService _upgradeService;
        private RepairTokensManager _tokensManager;
        private DiceAtlasConfig _atlasConfig;

        [Inject]
        public void Construct(
            DiceUpgradeService upgradeService,
            RepairTokensManager tokensManager,
            DiceAtlasConfig atlasConfig)
        {
            _upgradeService = upgradeService;
            _tokensManager = tokensManager;
            _atlasConfig = atlasConfig;
        }
        #endregion
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private void Awake()
        {
            for (var i = 0; i < _faceButtons.Count; i++)
            {
                _faceButtons[i].Init(i);
                _faceButtons[i].OnFaceHovered += HandleFaceHovered;
                _faceButtons[i].OnFaceUnhovered += HandleFaceUnhovered;
                _faceButtons[i].OnFaceClicked += HandleFaceClicked;
            }
        }

        private void OnDestroy()
        {
            Unbind();

            for (var i = 0; i < _faceButtons.Count; i++)
            {
                _faceButtons[i].OnFaceHovered -= HandleFaceHovered;
                _faceButtons[i].OnFaceUnhovered -= HandleFaceUnhovered;
                _faceButtons[i].OnFaceClicked -= HandleFaceClicked;
            }
        }

        public void Bind(DiceData data)
        {
            Unbind();

            _data = data;
            _previewSlot.Bind(_data);

            _data.OnDataChanged += Refresh;
            _tokensManager.OnTokensCountChange += HandleTokensChanged;

            Refresh();
        }

        public void Refresh()
        {
            var cost = _upgradeService.GetUpgradeCost(_data);
            var canAfford = _tokensManager.CurrentTokens >= cost;

            _diceTitleText.text = $"Кубик #{_data.Id + 1}";
            _costText.text = cost.ToString();

            for (var i = 0; i < _faceButtons.Count; i++)
            {
                var faceValue = _data.GetFaceValue(i);
                var sprite = _atlasConfig.GetSprite(faceValue);
                _faceButtons[i].Refresh(sprite, _data.GetFaceBonus(i), canAfford);
            }
        }

        private void HandleTokensChanged(int _) => Refresh();

        private void HandleFaceHovered(int faceIndex)
        {
            _previewSlot.Rotator.ResetVelocity();
            _rotateTween?.Kill();

            var rotatorTransform = _previewSlot.Rotator.transform;
            var localFaceDir = _data.GetFaceDirection(faceIndex).normalized;
            var worldFaceDir = rotatorTransform.TransformDirection(localFaceDir);
            var toCameraDir = (_previewSlot.Camera.transform.position - rotatorTransform.position).normalized;

            var deltaRot = Quaternion.FromToRotation(worldFaceDir, toCameraDir);
            var targetRot = deltaRot * rotatorTransform.rotation;

            _rotateTween = rotatorTransform
                .DORotateQuaternion(targetRot, _rotateDuration)
                .SetEase(Ease.OutCubic);
        }

        private void HandleFaceUnhovered(int _) { }

        private void HandleFaceClicked(int faceIndex)
        {
            _rotateTween?.Kill();

            if (_upgradeService.TryUpgradeFace(_data, faceIndex, out _))
                Refresh();
        }

        private void Unbind()
        {
            _rotateTween?.Kill();

            if (_data != null)
                _data.OnDataChanged -= Refresh;

            if (_tokensManager != null)
                _tokensManager.OnTokensCountChange -= HandleTokensChanged;
        }
    }
}