using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace _Scripts.UI
{
    public sealed class PauseSettingsTransition : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private Image _frame;
        [SerializeField] private Image _header;
        [SerializeField, Min(0.1f), Tooltip("Total transition time in seconds, in either direction.")]
        private float _settingsMorphDuration = 0.75f;
        [SerializeField] private Timing _settingsMorphTiming = new();

        [System.Serializable]
        internal sealed class Timing
        {
            [Header("Frame and Header")]
            [Range(0f, 1f), Tooltip("When the frame starts resizing, as a fraction of the total transition.")]
            public float FrameStart = 0.08f / 0.75f;
            [Range(0f, 1f), Tooltip("How long the frame resizes, as a fraction of the total transition.")]
            public float FrameDuration = 0.44f / 0.75f;
            public Ease FrameEase = Ease.InOutSine;

            [Header("Titles")]
            [Range(0f, 1f), Tooltip("When the outgoing title starts erasing, as a fraction of the total transition.")]
            public float TitleEraseStart;
            [Range(0f, 1f), Tooltip("How long the outgoing title takes to erase, as a fraction of the total transition.")]
            public float TitleEraseDuration = 0.16f / 0.75f;
            [Range(0f, 1f), Tooltip("When the incoming title starts appearing, as a fraction of the total transition.")]
            public float TitleRevealStart = 0.36f / 0.75f;
            [Range(0f, 1f), Tooltip("How long the incoming title takes to appear, as a fraction of the total transition.")]
            public float TitleRevealDuration = 0.39f / 0.75f;

            [Header("Content Fade Out")]
            [Range(0f, 1f), Tooltip("When the outgoing content starts fading, as a fraction of the total transition.")]
            public float ContentFadeOutStart;
            [Range(0f, 1f), Tooltip("How long the outgoing content fades, as a fraction of the total transition.")]
            public float ContentFadeOutDuration = 0.16f / 0.75f;
            public Ease ContentFadeOutEase = Ease.InQuad;

            [Header("Content Reveal")]
            [Range(0f, 1f), Tooltip("When the incoming panel starts its usual staggered reveal, as a fraction of the total transition.")]
            public float ContentRevealStart = 0.55f;
        }

        internal readonly struct Panel
        {
            internal readonly Image Frame;
            internal readonly Image Header;
            internal readonly TextRevealView Title;
            internal readonly CanvasGroup Group;

            internal Panel(Image frame, Image header, TextRevealView title, CanvasGroup group)
            {
                Frame = frame;
                Header = header;
                Title = title;
                Group = group;
            }
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        internal async UniTask PlayAsync(bool opening, SlidingPanelView slide, TextRevealView title,
            SettingsMenuView settings, Button settingsButton, System.Action onReveal, CancellationToken token)
        {
            if (opening)
            {
                slide.StopSlide();
                settings.PrepareFromPause();
            }
            else
                slide.ShowInstant();

            var group = slide.Group;
            group.interactable = false;
            group.blocksRaycasts = false;
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);

            var pause = new Panel(_frame, _header, title, group);
            var settingsPanel = settings.MorphPanel;
            try
            {
                await AnimateAsync(opening ? pause : settingsPanel, opening ? settingsPanel : pause,
                    (RectTransform)transform.parent, transform.GetSiblingIndex(), _settingsMorphDuration,
                    _settingsMorphTiming, RevealIncoming, token);
            }
            finally
            {
                if (opening)
                    slide.HideInstant();
                else
                    settings.FinishClosingFromPause();
            }

            void RevealIncoming()
            {
                onReveal();
                if (opening)
                {
                    settings.RevealFromPause();
                    return;
                }

                group.interactable = true;
                group.blocksRaycasts = true;
                if (EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(settingsButton.gameObject);
            }
        }

        private static async UniTask AnimateAsync(Panel from, Panel to, RectTransform space, int siblingIndex,
            float duration, Timing timing, System.Action revealIncoming, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Canvas.ForceUpdateCanvases();

            var frame = CreateImage(from.Frame, space, "Pause settings transition");
            frame.anchorMin = frame.anchorMax = frame.pivot = new Vector2(0.5f, 0.5f);
            frame.SetSiblingIndex(siblingIndex);
            var header = CreateImage(from.Header, frame, "Header");
            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = Vector2.one;
            header.pivot = new Vector2(0.5f, 1f);
            header.anchoredPosition = Vector2.zero;

            var fromTitle = (RectTransform)from.Title.transform;
            var toTitle = (RectTransform)to.Title.transform;
            var fromTitleOrder = fromTitle.GetSiblingIndex();
            var toTitleOrder = toTitle.GetSiblingIndex();
            var fromTitlePosition = fromTitle.anchoredPosition;
            var toTitlePosition = toTitle.anchoredPosition;
            var incomingRect = to.Frame.rectTransform;
            var clip = incomingRect.gameObject.AddComponent<RectMask2D>();
            var contentRevealed = false;

            void UpdateFrame(float progress)
            {
                // Both windows stay anchored to the canvas, including during a display mode change.
                var start = GetRectIn(space, from.Frame.rectTransform);
                var end = GetRectIn(space, to.Frame.rectTransform);
                frame.localPosition = Vector2.LerpUnclamped(start.center, end.center, progress);
                frame.sizeDelta = Vector2.LerpUnclamped(start.size, end.size, progress);
                var startHeight = GetRectIn(space, from.Header.rectTransform).height;
                var endHeight = GetRectIn(space, to.Header.rectTransform).height;
                header.sizeDelta = new Vector2(0f, Mathf.LerpUnclamped(startHeight, endHeight, progress));
                var visible = GetRectIn(incomingRect, frame);
                var bounds = incomingRect.rect;
                clip.padding = new Vector4(Mathf.Max(0f, visible.xMin - bounds.xMin),
                    Mathf.Max(0f, visible.yMin - bounds.yMin), Mathf.Max(0f, bounds.xMax - visible.xMax),
                    Mathf.Max(0f, bounds.yMax - visible.yMax));
            }

            try
            {
                UpdateFrame(0f);
                MoveTitle(fromTitle, header, fromTitlePosition);
                MoveTitle(toTitle, header, toTitlePosition);
                from.Frame.enabled = from.Header.enabled = false;
                to.Frame.enabled = to.Header.enabled = false;
                to.Group.alpha = 0f;

                var frameEnd = Mathf.Min(1f, timing.FrameStart + timing.FrameDuration);
                var geometry = DOVirtual.Float(0f, 1f, duration, time =>
                {
                    var progress = time >= frameEnd ? 1f : Mathf.InverseLerp(timing.FrameStart, frameEnd, time);
                    UpdateFrame(DOVirtual.EasedValue(0f, 1f, progress, timing.FrameEase));
                }).SetEase(Ease.Linear);
                // Keep each phase within the total duration when a delay and its length overlap the end.
                float PhaseDuration(float start, float length) => Mathf.Min(length, 1f - start) * duration;
                var sequence = DOTween.Sequence().SetUpdate(true)
                    .Insert(0f, geometry)
                    .Insert(timing.TitleEraseStart * duration, from.Title.Hide(
                        PhaseDuration(timing.TitleEraseStart, timing.TitleEraseDuration)))
                    .Insert(timing.TitleRevealStart * duration, to.Title.Show(
                        PhaseDuration(timing.TitleRevealStart, timing.TitleRevealDuration)))
                    .Insert(timing.ContentFadeOutStart * duration, from.Group.DOFade(0f,
                        PhaseDuration(timing.ContentFadeOutStart, timing.ContentFadeOutDuration))
                        .SetEase(timing.ContentFadeOutEase))
                    .InsertCallback(timing.ContentRevealStart * duration, () =>
                    {
                        contentRevealed = true;
                        revealIncoming();
                        to.Group.alpha = 1f;
                    });

                await sequence.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, token);
            }
            finally
            {
                RestorePanel(from, fromTitleOrder, fromTitlePosition, 0f);
                RestorePanel(to, toTitleOrder, toTitlePosition, contentRevealed ? 1f : 0f);
                if (clip)
                {
                    clip.enabled = false;
                    Object.Destroy(clip);
                }
                if (frame)
                {
                    frame.gameObject.SetActive(false);
                    Object.Destroy(frame.gameObject);
                }
            }
        }

        private static RectTransform CreateImage(Image source, RectTransform parent, string name)
        {
            var visual = new GameObject(name, typeof(RectTransform), typeof(Image));
            visual.layer = parent.gameObject.layer;
            var image = visual.GetComponent<Image>();
            image.rectTransform.SetParent(parent, false);
            image.sprite = source.sprite;
            image.material = source.material;
            image.color = source.color;
            image.type = source.type;
            image.pixelsPerUnitMultiplier = source.pixelsPerUnitMultiplier;
            image.raycastTarget = false;
            return image.rectTransform;
        }

        private static Rect GetRectIn(RectTransform space, RectTransform rect)
        {
            var min = (Vector2)space.InverseTransformPoint(rect.TransformPoint(rect.rect.min));
            var max = (Vector2)space.InverseTransformPoint(rect.TransformPoint(rect.rect.max));
            return new Rect(min, max - min);
        }

        private static void MoveTitle(RectTransform title, RectTransform parent, Vector2 position)
        {
            title.SetParent(parent, false);
            title.anchoredPosition = position;
        }

        private static void RestorePanel(Panel panel, int titleOrder, Vector2 titlePosition, float alpha)
        {
            if (panel.Title && panel.Header)
            {
                var title = (RectTransform)panel.Title.transform;
                MoveTitle(title, panel.Header.rectTransform, titlePosition);
                title.SetSiblingIndex(titleOrder);
                panel.Title.Restore();
            }

            if (panel.Group) panel.Group.alpha = alpha;
            if (panel.Frame) panel.Frame.enabled = true;
            if (panel.Header) panel.Header.enabled = true;
        }
    }
}
