using System.Threading;
using _Scripts.Utility.Extensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
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
        [SerializeField] private Ease _panelSlideEase = Ease.InOutSine;
        [SerializeField] private Ease _titleRollEase = Ease.InOutSine;
        [SerializeField] private Ease _tokenFlightEase = Ease.InOutSine;
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
            var rewardsTitle = _rewardsHeader.GetComponentInChildren<TMP_Text>().rectTransform;
            var workshopTitle = _workshopTitle.rectTransform;
            var workshopHeader = (RectTransform)workshopTitle.parent;
            var workshopHeaderOrder = workshopHeader.GetSiblingIndex();
            var workshopBackground = _workshopFrame.GetComponent<Image>();
            var workshopBackgroundWasEnabled = workshopBackground.enabled;
            var rewardsTitleStart = rewardsTitle.anchoredPosition;
            var workshopTitleStart = workshopTitle.anchoredPosition;
            var rewardsTitleOrder = rewardsTitle.GetSiblingIndex();
            var balanceAlpha = _workshopBalance.alpha;

            var headerHeight = rewardsHeader.rect.height;
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
            var titleClip = GetClip(workshopHeader);
            var bodyClipWasEnabled = bodyClip.enabled;
            var titleClipWasEnabled = titleClip.enabled;
            var bodyPadding = bodyClip.padding;
            var canvas = (RectTransform)_rewardsFrame.canvas.rootCanvas.transform;
            var headerClipHeight = SizeIn(canvas, rewardsHeader).y;

            // Keep both panels aligned with the upper edge as their frames expand.
            PinToTop(_rewardsDetails);
            PinToTop(conveyorHolder);

            // Keep the shared header above both sets of panels during the handoff.
            workshopHeader.SetAsLastSibling();
            rewardsTitle.SetParent(workshopHeader, false);
            workshopTitle.anchoredPosition = workshopTitleStart - Vector2.up * headerHeight;
            bodyClip.padding = new Vector4(bodyPadding.x, bodyPadding.y, bodyPadding.z, headerClipHeight);
            bodyClip.enabled = true;
            titleClip.enabled = true;
            tokenRect.SetAsLastSibling();
            workshopBackground.enabled = false;
            _rewardsHeader.enabled = false;
            _balanceToken.enabled = false;
            _workshopBalance.alpha = 0f;
            workshopStart.Apply(_workshopFrame);

            var balanceRect = _workshopBalance.transform;
            var balanceScale = balanceRect.localScale;
            var panelSlideStart = _duration * 0.08f;
            var panelSlideDuration = _duration * 0.62f;
            var titleRollStart = _duration * 0.14f;
            var titleRollDuration = _duration * 0.48f;
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
                .Insert(titleRollStart, rewardsTitle.DOAnchorPosY(rewardsTitleStart.y + headerHeight, titleRollDuration).SetEase(_titleRollEase))
                .Insert(titleRollStart, workshopTitle.DOAnchorPosY(workshopTitleStart.y, titleRollDuration).SetEase(_titleRollEase))
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
                    rewardsTitle.SetParent(rewardsHeader, false);
                    rewardsTitle.anchoredPosition = rewardsTitleStart;
                    rewardsTitle.SetSiblingIndex(rewardsTitleOrder);
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
                    workshopTitle.anchoredPosition = workshopTitleStart;
                    workshopHeader.SetSiblingIndex(workshopHeaderOrder);
                    titleClip.enabled = titleClipWasEnabled;
                    _workshopBalance.alpha = balanceAlpha;
                    balanceRect.localScale = balanceScale;
                    _balanceToken.enabled = true;
                }
            }
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
