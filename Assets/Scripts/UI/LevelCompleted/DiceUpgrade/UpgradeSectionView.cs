using System.Collections.Generic;
using System.Linq;
using _Scripts.Configs;
using _Scripts.Dices;
using _Scripts.Managers;
using _Scripts.Utility.Extensions;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
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
        [SerializeField] private Ease _rotateEase = Ease.OutQuart;

        private readonly LocalizedString _diceTitle = new("String Table", "upgrade_die_name");

        private DiceData _data;
        private Tween _rotateTween;
        private Tween _scaleTween;

        internal StaggeredReveal FaceReveal { get; private set; }

        #region DI
        private DiceUpgradeService _upgradeService;
        private RepairTokensManager _tokensManager;
        private DiceAtlasConfig _atlasConfig;
        private ISfxPlayer _sfxPlayer;

        [Inject]
        public void Construct(
            DiceUpgradeService upgradeService,
            RepairTokensManager tokensManager,
            DiceAtlasConfig atlasConfig,
            ISfxPlayer sfxPlayer)
        {
            _upgradeService = upgradeService;
            _tokensManager = tokensManager;
            _atlasConfig = atlasConfig;
            _sfxPlayer = sfxPlayer;
        }
        #endregion
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private void Awake()
        {
            var facesInLayoutOrder = _faceButtons.OrderBy(face => face.transform.GetSiblingIndex()).ToArray();
            FaceReveal = new StaggeredReveal(facesInLayoutOrder, blockInteraction: false);

            for (var i = 0; i < _faceButtons.Count; i++)
            {
                _faceButtons[i].Init(i);
                _faceButtons[i].OnFaceHovered += HandleFaceHovered;
                _faceButtons[i].OnFaceUnhovered += HandleFaceUnhovered;
                _faceButtons[i].OnFaceClicked += HandleFaceClicked;
            }
        }

        private void OnEnable() => LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;

        private void OnDisable() => LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;

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

            UpdateDiceTitle();
            _costText.text = cost.ToString();

            for (var i = 0; i < _faceButtons.Count; i++)
            {
                var faceValue = _data.GetFaceValue(i);
                var sprite = _atlasConfig.GetSprite(faceValue);
                _faceButtons[i].Refresh(sprite, _data.GetFaceBonus(i), canAfford);
            }
        }

        private void UpdateDiceTitle() => _diceTitleText.text = _diceTitle.GetLocalizedString(_data.Id + 1);

        private void HandleLocaleChanged(Locale _)
        {
            if (_data != null)
                UpdateDiceTitle();
        }

        private void HandleTokensChanged(int _) => Refresh();

        private void HandleFaceHovered(int faceIndex)
        {
            _previewSlot.Rotator.ResetVelocity();
            _rotateTween?.Kill();

            var rotatorTransform = _previewSlot.Rotator.transform;
            var camTransform = _previewSlot.Camera.transform;

            var localFaceDir = _data.GetFaceDirection(faceIndex).normalized;
            var localUp = Mathf.Abs(Vector3.Dot(localFaceDir, Vector3.up)) > 0.99f
                ? Vector3.forward
                : Vector3.up;
            var localFaceUp = Vector3.ProjectOnPlane(localUp, localFaceDir).normalized;

            var toCameraDir = (camTransform.position - rotatorTransform.position).normalized;
            var screenUp = Vector3.ProjectOnPlane(camTransform.up, toCameraDir).normalized;

            // Целевая ориентация куба в мире: нормаль грани -> к камере, её верх -> вверх экрана
            var desiredWorldRot = Quaternion.LookRotation(toCameraDir, screenUp);
            var localBasisRot = Quaternion.LookRotation(localFaceDir, localFaceUp);
            var targetRot = desiredWorldRot * Quaternion.Inverse(localBasisRot);

            _rotateTween = rotatorTransform
                .DORotateQuaternion(targetRot, _rotateDuration)
                .SetEase(Ease.OutCubic);
        }

        private void HandleFaceUnhovered(int _) { }

        private void HandleFaceClicked(int faceIndex)
        {
            if (_upgradeService.TryUpgradeFace(_data, faceIndex, out _))
            {
                _sfxPlayer.PlayUI("DiceFaceUpgrade", (RectTransform)_faceButtons[faceIndex].transform);
                _scaleTween?.Kill();
                _scaleTween = _previewSlot.DisplayView.transform.DoScalePop(killExistingTweens: false, scaleMult: 1.5f);
                Refresh();
            }
        }

        private void Unbind()
        {
            _rotateTween?.Kill();
            _scaleTween?.Kill(true);

            if (_data != null)
                _data.OnDataChanged -= Refresh;

            if (_tokensManager != null)
                _tokensManager.OnTokensCountChange -= HandleTokensChanged;
        }
    }
}
