using System.Threading;
using _Scripts.Utility.Extensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class RewardsWorkshopTransition : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Rewards")]
        [SerializeField] private Image _rewardsFrame;
        [SerializeField] private Image _rewardsHeader;
        [SerializeField] private CanvasGroup _rewardsContent;
        [SerializeField] private RectTransform _rewardsDetails;
        [SerializeField] private Image _rewardToken;

        [Header("Workshop")]
        [SerializeField] private RectTransform _workshopFrame;
        [SerializeField] private TMP_Text _workshopTitle;
        [SerializeField] private CanvasGroup _workshopBalance;
        [SerializeField] private Image _balanceToken;

        [Header("Transition")]
        [SerializeField, Min(0.1f)] private float _duration = 0.95f;
        [SerializeField] private Ease _frameEase = Ease.InOutSine;
        [SerializeField, Min(0.01f), Tooltip("Seconds for the rewards panel to slide out, limited by the overall transition duration.")]
        private float _rewardsSlideDuration = 0.59f;
        [SerializeField] private Ease _panelSlideEase = Ease.InOutSine;
        [SerializeField] private Ease _tokenFlightEase = Ease.InOutSine;

        [Header("Title Animation")]
        [SerializeField, Min(0.01f)] private float _titleEraseDuration = 0.19f;
        [SerializeField] private Ease _titleEraseEase = Ease.Linear;
        [SerializeField, Min(0.01f)] private float _titleTypeDuration = 0.33f;
        [SerializeField, FormerlySerializedAs("_titleRollEase")] private Ease _titleTypeEase = Ease.Linear;

        internal float Duration => _duration;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        internal async UniTask PlayAsync(RectTransform rewardsButton, SlidingPanelView firstWorkshopSlide, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            Canvas.ForceUpdateCanvases();

            var space = (RectTransform)transform;
            var rewards = _rewardsFrame.rectTransform;
            var rewardsPose = new FramePose(rewards.sizeDelta, rewards.anchoredPosition);
            var workshopPose = new FramePose(_workshopFrame.sizeDelta, _workshopFrame.anchoredPosition);
            var rewardsEnd = FramePose.Match(rewards, _workshopFrame);
            var workshopStart = FramePose.Match(_workshopFrame, rewards);
            var detailsStart = _rewardsDetails.anchoredPosition;
            var detailsAnchorMin = _rewardsDetails.anchorMin;
            var detailsAnchorMax = _rewardsDetails.anchorMax;
            var conveyorHolder = (RectTransform)firstWorkshopSlide.transform;
            var conveyorStart = conveyorHolder.anchoredPosition;
            var conveyorAnchorMin = conveyorHolder.anchorMin;
            var conveyorAnchorMax = conveyorHolder.anchorMax;
            var buttonStart = rewardsButton.anchoredPosition;
            var rewardsHeader = _rewardsHeader.rectTransform;
            var rewardsTitle = _rewardsHeader.GetComponentInChildren<TMP_Text>();
            var rewardsTitleRect = rewardsTitle.rectTransform;
            var workshopHeader = (RectTransform)_workshopTitle.transform.parent;
            var workshopHeaderOrder = workshopHeader.GetSiblingIndex();
            var workshopBackground = _workshopFrame.GetComponent<Image>();
            var workshopBackgroundWasEnabled = workshopBackground.enabled;
            var rewardsTitleOrder = rewardsTitleRect.GetSiblingIndex();
            var rewardsTitlePosition = rewardsTitleRect.anchoredPosition;
            var rewardsTitleVisibility = rewardsTitle.maxVisibleCharacters;
            var workshopTitleVisibility = _workshopTitle.maxVisibleCharacters;
            rewardsTitle.ForceMeshUpdate();
            _workshopTitle.ForceMeshUpdate();
            var rewardsTitleLength = Mathf.Min(rewardsTitle.textInfo.characterCount, rewardsTitleVisibility);
            var workshopTitleLength = Mathf.Min(_workshopTitle.textInfo.characterCount, workshopTitleVisibility);
            var balanceAlpha = _workshopBalance.alpha;

            var detailsExitX = _rewardsDetails.GetHiddenPosition(SlideDirection.Left, detailsStart, rewards).x;
            var buttonExitY = -rewardsButton.rect.yMax - 1f;

            var token = Instantiate(_rewardToken, space);
            token.name = "Flying reward token";
            token.raycastTarget = false;
            token.enabled = false;
            var tokenRect = token.rectTransform;
            tokenRect.pivot = new Vector2(0.5f, 0.5f);
            tokenRect.localScale = Vector3.one;
            var tokenStart = CenterIn(space, _rewardToken.rectTransform);
            var tokenStartSize = SizeIn(space, _rewardToken.rectTransform);
            var tokenEndSize = SizeIn(space, _balanceToken.rectTransform);
            tokenRect.localPosition = tokenStart;
            tokenRect.sizeDelta = tokenStartSize;

            var bodyClip = GetClip(rewards);
            var bodyClipWasEnabled = bodyClip.enabled;
            var bodyPadding = bodyClip.padding;
            var canvas = (RectTransform)_rewardsFrame.canvas.rootCanvas.transform;
            var headerClipHeight = SizeIn(canvas, rewardsHeader).y;

            // Keep both panels aligned with the upper edge as their frames expand.
            PinToTop(_rewardsDetails);
            PinToTop(conveyorHolder);

            // Keep the shared header above both sets of panels during the handoff.
            workshopHeader.SetAsLastSibling();
            rewardsTitleRect.SetParent(workshopHeader, false);
            rewardsTitle.maxVisibleCharacters = rewardsTitleLength;
            _workshopTitle.maxVisibleCharacters = 0;
            bodyClip.padding = new Vector4(bodyPadding.x, bodyPadding.y, bodyPadding.z, headerClipHeight);
            bodyClip.enabled = true;
            tokenRect.SetAsLastSibling();
            workshopBackground.enabled = false;
            _rewardsHeader.enabled = false;
            _balanceToken.enabled = false;
            _workshopBalance.alpha = 0f;
            workshopStart.Apply(_workshopFrame);

            var balanceRect = _workshopBalance.transform;
            var balanceScale = balanceRect.localScale;
            var panelSlideStart = _duration * 0.08f;
            var panelSlideDuration = Mathf.Min(_rewardsSlideDuration, _duration - panelSlideStart);
            var titleEraseStart = _duration * 0.13f;
            var titleTimeScale = Mathf.Min(1f, (_duration - titleEraseStart) / (_titleEraseDuration + _titleTypeDuration));
            var titleEraseDuration = _titleEraseDuration * titleTimeScale;
            var titleTypeDuration = _titleTypeDuration * titleTimeScale;
            var tokenLiftTime = _duration * 0.12f;
            var tokenLandTime = _duration * 0.84f;
            var balanceRevealTime = _duration * 0.62f;
            var sequence = DOTween.Sequence().SetUpdate(true)
                .Insert(0f, DOVirtual.Float(0f, 1f, _duration, progress =>
                {
                    FramePose.Lerp(workshopStart, workshopPose, progress).Apply(_workshopFrame);
                    FramePose.Lerp(rewardsPose, rewardsEnd, progress).Apply(rewards);
                }).SetEase(_frameEase))
                .Insert(panelSlideStart, _rewardsDetails.DOAnchorPosX(detailsExitX, panelSlideDuration).SetEase(_panelSlideEase))
                .Insert(0f, rewardsButton.DOAnchorPosY(buttonExitY, _duration * 0.44f).SetEase(_panelSlideEase))
                .Insert(titleEraseStart, TweenTitle(rewardsTitle, 0, rewardsTitleLength, titleEraseDuration, _titleEraseEase))
                .Insert(titleEraseStart + titleEraseDuration, TweenTitle(_workshopTitle, workshopTitleLength, workshopTitleLength,
                    titleTypeDuration, _titleTypeEase))
                .Insert(balanceRevealTime, _workshopBalance.DOFade(balanceAlpha, tokenLandTime - balanceRevealTime).SetEase(Ease.OutCubic))
                .InsertCallback(tokenLiftTime, () =>
                {
                    tokenStart = CenterIn(space, _rewardToken.rectTransform);
                    tokenRect.localPosition = tokenStart;
                    _rewardToken.enabled = false;
                    token.enabled = true;
                })
                .Insert(tokenLiftTime, DOVirtual.Float(0f, 1f, tokenLandTime - tokenLiftTime, progress =>
                {
                    var arc = Mathf.Sin(progress * Mathf.PI);
                    var destination = CenterIn(space, _balanceToken.rectTransform);
                    tokenRect.localPosition = Vector2.LerpUnclamped(tokenStart, destination, progress) + Vector2.up * (arc * 16f);
                    tokenRect.sizeDelta = Vector2.LerpUnclamped(tokenStartSize, tokenEndSize, progress);
                }).SetEase(_tokenFlightEase))
                .InsertCallback(tokenLandTime, () =>
                {
                    token.enabled = false;
                    _balanceToken.enabled = true;
                })
                .Insert(tokenLandTime, balanceRect.DOPunchScale(balanceScale * 0.045f, _duration - tokenLandTime, 2, 0.5f));

            try
            {
                await sequence.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, ct);
            }
            finally
            {
                if (token)
                {
                    token.enabled = false;
                    Destroy(token.gameObject);
                }
                if (rewards)
                {
                    _rewardsContent.alpha = 0f;
                    rewardsPose.Apply(rewards);
                    _rewardsDetails.anchorMin = detailsAnchorMin;
                    _rewardsDetails.anchorMax = detailsAnchorMax;
                    _rewardsDetails.anchoredPosition = detailsStart;
                    rewardsButton.anchoredPosition = buttonStart;
                    rewardsTitleRect.SetParent(rewardsHeader, false);
                    rewardsTitleRect.anchoredPosition = rewardsTitlePosition;
                    rewardsTitleRect.SetSiblingIndex(rewardsTitleOrder);
                    rewardsTitle.maxVisibleCharacters = rewardsTitleVisibility;
                    bodyClip.padding = bodyPadding;
                    bodyClip.enabled = bodyClipWasEnabled;
                    _rewardsHeader.enabled = true;
                    _rewardToken.enabled = true;
                }
                if (_workshopFrame)
                {
                    workshopPose.Apply(_workshopFrame);
                    conveyorHolder.anchorMin = conveyorAnchorMin;
                    conveyorHolder.anchorMax = conveyorAnchorMax;
                    conveyorHolder.anchoredPosition = conveyorStart;
                    workshopBackground.enabled = workshopBackgroundWasEnabled;
                    _workshopTitle.maxVisibleCharacters = workshopTitleVisibility;
                    workshopHeader.SetSiblingIndex(workshopHeaderOrder);
                    _workshopBalance.alpha = balanceAlpha;
                    balanceRect.localScale = balanceScale;
                    _balanceToken.enabled = true;
                }
            }
        }

        private static Tween TweenTitle(TMP_Text title, int visibleCount, int characterCount, float duration, Ease ease)
        {
            return DOTween.To(() => title.maxVisibleCharacters,
                    value => title.maxVisibleCharacters = Mathf.Clamp(value, 0, characterCount), visibleCount, duration)
                .SetEase(ease);
        }

        private static void PinToTop(RectTransform panel)
        {
            var position = panel.localPosition;
            panel.anchorMin = new Vector2(panel.anchorMin.x, 1f);
            panel.anchorMax = new Vector2(panel.anchorMax.x, 1f);
            panel.localPosition = position;
        }

        private static RectMask2D GetClip(RectTransform rect)
        {
            if (rect.TryGetComponent<RectMask2D>(out var clip))
                return clip;

            clip = rect.gameObject.AddComponent<RectMask2D>();
            clip.enabled = false;
            return clip;
        }

        private static Vector2 CenterIn(RectTransform space, RectTransform rect)
        {
            return space.InverseTransformPoint(rect.TransformPoint(rect.rect.center));
        }

        private static Vector2 SizeIn(RectTransform space, RectTransform rect)
        {
            var min = space.InverseTransformPoint(rect.TransformPoint(rect.rect.min));
            var max = space.InverseTransformPoint(rect.TransformPoint(rect.rect.max));
            return max - min;
        }

        private readonly struct FramePose
        {
            private readonly Vector2 _sizeDelta;
            private readonly Vector2 _anchoredPosition;

            public FramePose(Vector2 sizeDelta, Vector2 anchoredPosition)
            {
                _sizeDelta = sizeDelta;
                _anchoredPosition = anchoredPosition;
            }

            public static FramePose Match(RectTransform frame, RectTransform other)
            {
                var parent = (RectTransform)frame.parent;
                var size = SizeIn(parent, other);
                var pivotOffset = Vector2.Scale(size, frame.pivot - new Vector2(0.5f, 0.5f));
                var anchor = (Vector2)frame.localPosition - frame.anchoredPosition;
                return new FramePose(frame.sizeDelta + size - frame.rect.size, CenterIn(parent, other) + pivotOffset - anchor);
            }

            public static FramePose Lerp(FramePose from, FramePose to, float progress)
            {
                return new FramePose(Vector2.LerpUnclamped(from._sizeDelta, to._sizeDelta, progress),
                    Vector2.LerpUnclamped(from._anchoredPosition, to._anchoredPosition, progress));
            }

            public void Apply(RectTransform frame)
            {
                frame.sizeDelta = _sizeDelta;
                frame.anchoredPosition = _anchoredPosition;
            }
        }
    }
}
