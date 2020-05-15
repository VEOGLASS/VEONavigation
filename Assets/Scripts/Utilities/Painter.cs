using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XploriaAR
{
    /// <summary>
    /// UI rendering methods, used mostly for code-based animations and rect calculations.
    /// </summary>
    public static class Painter
    {
        #region Const fields

        public const float portraitRot = 0;
        public const float portraitUpsideRot = 180;
        public const float landscapeRightRot = 90;
        public const float landscapeLeftRot = -90;

        public const float canvasScale = 1.6875f;

        #endregion

        #region Animations

        /// <summary>
        /// Fades <see cref="CanvasGroup"/> to chosen alpha value. Used for code-based UI animations.
        /// </summary>
        /// <returns></returns>
        public static IEnumerator FadeTo(CanvasGroup group, float alpha, float time = 1f)
        {
            time = Mathf.Max(0, time);

            var startAlpha = group.alpha;
            var elapsed = 0.0f;

            while (elapsed < time)
            {
                group.alpha = Mathf.Lerp(startAlpha, alpha, elapsed / time);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            group.alpha = alpha;
        }

        /// <summary>
        /// Fades <see cref="CanvasGroup"/> to chosen alpha value. Used for code-based UI animations.
        /// </summary>
        /// <returns></returns>
        public static IEnumerator DisableByFade(CanvasGroup group, float time = 1f)
        {
            time = Mathf.Max(0, time);

            var startAlpha = group.alpha;
            var elapsed = 0.0f;

            while (elapsed < time)
            {
                group.alpha = Mathf.Lerp(startAlpha, 0, elapsed / time);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            group.alpha = 0;
            group.gameObject.SetActive(false);
        }

        /// <summary>
        /// Fades <see cref="Image"/> to chosen alpha value. Used for code-based UI animations.
        /// </summary>
        /// <returns></returns>
        public static IEnumerator FadeTo(Image image, float alpha, float time = 1f)
        {
            time = Mathf.Max(0, time);

            var color = image.color;
            var startAlpha = color.a;
            var elapsed = 0.0f;

            while (elapsed < time)
            {
                color.a = Mathf.Lerp(startAlpha, alpha, elapsed / time);
                image.color = color;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            color.a = alpha;
            image.color = color;
        }

        /// <summary>
        /// Moves <see cref="Rect"/> from point to point in time. Used for code-based UI animations.
        /// </summary>
        /// <returns></returns>
        public static IEnumerator MoveRectTo(RectTransform rect, Vector2 newPos, float duration = 0.12f)
        {
            yield return MoveRectTo(rect, rect.anchoredPosition, newPos, duration);
        }

        public static IEnumerator MoveRectTo(RectTransform rect, Vector2 fromPosition, Vector2 toPosition, float duration = 0.12f)
        {
            var elapsed = 0.0f;

            while (elapsed < duration)
            {
                rect.anchoredPosition = Vector2.Lerp(fromPosition, toPosition, elapsed / duration);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            rect.anchoredPosition = toPosition;
        }

        public static IEnumerator RotRectTo(RectTransform rect, float rotZ, float duration = 0.12f)
        {
            var elapsed = 0.0f;
            var x = rect.rotation.eulerAngles.x;
            var y = rect.rotation.eulerAngles.y;

            var startRot = rect.rotation;
            var newRot = Quaternion.Euler(x, y, rotZ);

            while (elapsed < duration)
            {
                rect.rotation = Quaternion.Lerp(startRot, newRot, elapsed / duration);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            rect.rotation = newRot;
        }

        public static IEnumerator FitRectToSize(RectTransform rect, Vector2 size, float duration = 0.2f)
        {
            var elapsed = 0.0f;
            var start = rect.sizeDelta;

            while (elapsed < duration)
            {
                rect.sizeDelta = Vector2.Lerp(start, size, elapsed / duration);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            rect.sizeDelta = size;
        }

        public static IEnumerator FitRectToScreen(RectTransform rect, bool horizontal, float duration = 0.2f)
        {
            var screenOffset = 10;
            var elapsed = 0.0f;
            var startSize = rect.sizeDelta;
            //TODO:
            //RectTransformUtility.
            var newSize = horizontal 
                ? new Vector2(ScreenCenter.y + screenOffset, ScreenCenter.x + screenOffset) / canvasScale
                : new Vector2(ScreenCenter.x + screenOffset, ScreenCenter.y + screenOffset) / canvasScale;

            while (elapsed < duration)
            {
                rect.sizeDelta = Vector2.Lerp(startSize, newSize, elapsed / duration);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            rect.sizeDelta = newSize;
        }

        #endregion

        /// <summary>
        /// Return anchored position of screen center fir this <see cref="RectTransform"/>.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Vector2 GetScreenCenterInRect(RectTransform rect)
        {
            var centerPosition = Vector2.zero;
            var basicPosition = rect.anchoredPosition;
            var basicParent = rect.parent;

            while (rect.parent.parent)
            {
                rect.SetParent(rect.parent.parent);
            }

            rect.anchoredPosition = centerPosition;
            rect.SetParent(basicParent);
            centerPosition = rect.anchoredPosition;
            rect.anchoredPosition = basicPosition;

            return centerPosition;
        }

        [System.Obsolete]
        public static Vector2 GetScreenCenterInRect(RectTransform rect, Camera camera)
        {
            Vector2 center = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, ScreenCenter, camera, out center);
            return rect.anchoredPosition + center;
        }

        public static bool ClosePoints(Vector2 a, Vector2 b, float dist)
        {
            return Mathf.Sqrt((b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y)) < dist;
        }

        public static Rect RectTransformToScreenSpace(RectTransform transform)
        {
            Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
            return new Rect((Vector2)transform.position - (size * 0.5f), size);
        }

        #region Properties

        public static Vector2 ScreenCenter
        {
            get { return new Vector2(Screen.width / 2, Screen.height / 2); }
        }

        #endregion
    }
}