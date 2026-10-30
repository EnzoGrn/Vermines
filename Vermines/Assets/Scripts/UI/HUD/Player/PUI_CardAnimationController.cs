using UnityEngine;
using DG.Tweening;
using System;

namespace Vermines.UI {

    /// <summary>
    /// Why a card left the player's cards (hand + deck + discard), used to pick the exit animation.
    /// </summary>
    public enum PUI_CardRemoveReason {
        Recycled   = 0, // Left the player's cards for anything else (recycled, given back to a shop...).
        ToTable    = 1, // Played on the table or equipped.
        Sacrificed = 2  // Sent to the graveyard.
    }

    /// <summary>
    /// Animations of the <see cref="PUI_CardView"/> in the mini deck drawer.
    /// </summary>
    public static class PUI_CardAnimationController {

        #region Attributes

        /// <summary>
        /// Duration of the flip used to swap the sprite of a card.
        /// </summary>
        private const float _FlipDuration = 0.09f;

        #endregion

        #region Add

        /// <summary>
        /// New card: a dotted card appears and shakes, then flips to its real type with a small pop.
        /// </summary>
        public static void PlayAdd(PUI_CardView view, float revealDelay)
        {
            RectTransform rt = view.Rect;
            CanvasGroup   cg = view.Group;

            DOTween.Kill(view);

            view.SetGhost(true);

            rt.localScale    = Vector3.one * 0.6f;
            rt.localRotation = Quaternion.identity;
            cg.alpha         = 0f;

            Sequence seq = DOTween.Sequence().SetTarget(view).SetLink(view.gameObject);

            seq.Append(cg.DOFade(1f, 0.15f));
            seq.Join(rt.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
            seq.Append(rt.DOShakeRotation(0.4f, new Vector3(0f, 0f, 12f), 12, 90f, true));
            seq.AppendInterval(Mathf.Max(0f, revealDelay - 0.55f));
            seq.Append(rt.DOScaleX(0f, _FlipDuration).SetEase(Ease.InQuad));
            seq.AppendCallback(() => view.SetGhost(false));
            seq.Append(rt.DOScaleX(1f, _FlipDuration).SetEase(Ease.OutQuad));
            seq.Append(rt.DOPunchScale(Vector3.one * 0.25f, 0.25f, 6, 0.6f));
            seq.OnKill(() => {
                if (rt == null)
                    return;
                rt.localScale    = Vector3.one;
                rt.localRotation = Quaternion.identity;
                cg.alpha         = 1f;

                view.SetGhost(false);
            });
        }

        #endregion

        #region Remove

        /// <summary>
        /// Card leaving the player's cards, the animation depends on where it went.
        /// </summary>
        public static void PlayRemove(PUI_CardView view, PUI_CardRemoveReason reason, Action onComplete)
        {
            RectTransform rt = view.Rect;
            CanvasGroup   cg = view.Group;

            view.StopMove();

            DOTween.Kill(view);

            rt.localScale    = Vector3.one;
            rt.localRotation = Quaternion.identity;
            cg.alpha         = 1f;

            Sequence seq = DOTween.Sequence().SetTarget(view).SetLink(view.gameObject);

            switch (reason) {
                case PUI_CardRemoveReason.ToTable:
                    // Lifted out of the drawer, as if the player took it to put it on the table.
                    seq.Append(rt.DOAnchorPosY(rt.anchoredPosition.y + 28f, 0.35f).SetEase(Ease.OutCubic));
                    seq.Join(rt.DOScale(1.15f, 0.35f));
                    seq.Insert(0.15f, cg.DOFade(0f, 0.2f));
                    break;
                case PUI_CardRemoveReason.Sacrificed:
                    // Flips to the broken card, then falls.
                    AppendFlip(seq, rt, view.SetSacrificed);
                    seq.Append(rt.DOPunchScale(Vector3.one * 0.2f, 0.2f, 6, 0.5f));
                    seq.AppendInterval(0.2f);
                    seq.Append(rt.DOAnchorPosY(rt.anchoredPosition.y - 30f, 0.35f).SetEase(Ease.InCubic));
                    seq.Join(rt.DORotate(new Vector3(0f, 0f, -25f), 0.35f));
                    seq.Join(cg.DOFade(0f, 0.35f).SetEase(Ease.InQuad));
                    break;
                default:
                    // Flips to the recycle card, then spins away.
                    AppendFlip(seq, rt, view.SetRecycled);
                    seq.Append(rt.DOPunchScale(Vector3.one * 0.2f, 0.2f, 6, 0.5f));
                    seq.AppendInterval(0.2f);
                    seq.Append(rt.DOScale(0f, 0.3f).SetEase(Ease.InBack));
                    seq.Join(rt.DORotate(new Vector3(0f, 0f, 180f), 0.3f, RotateMode.FastBeyond360));
                    seq.Join(cg.DOFade(0f, 0.3f));
                    break;
            }
            seq.OnKill(() => onComplete?.Invoke());
        }

        #endregion

        #region Helpers

        private static void AppendFlip(Sequence seq, RectTransform rt, TweenCallback swap)
        {
            seq.Append(rt.DOScaleX(0f, _FlipDuration).SetEase(Ease.InQuad));
            seq.AppendCallback(swap);
            seq.Append(rt.DOScaleX(1f, _FlipDuration).SetEase(Ease.OutQuad));
        }

        #endregion
    }
}
