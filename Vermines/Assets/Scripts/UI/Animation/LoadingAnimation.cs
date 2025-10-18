using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace Vermines.Animations {

    public class LoadingAnimationPlugin : MonoBehaviour {

        #region Attributes

        public List<RectTransform> _Dots;

        [SerializeField]
        private float _JumpHeight = 20f;

        [SerializeField]
        private float _JumpDuration = 0.3f;

        [SerializeField]
        private float _DelayBetweenJumps = 0.2f;

        private Vector2[] _OriginalPositions;

        #endregion

        #region MonoBehaviour Methods

        protected void OnEnable()
        {
            StartCoroutine(AnimateDots());
        }

        protected void OnDisable()
        {
            StopAllCoroutines();

            for (int i = 0; i < _Dots.Count; i++)
                _Dots[i].anchoredPosition = _OriginalPositions[i];
        }

        protected void Awake()
        {
            _OriginalPositions = new Vector2[_Dots.Count];

            for (int i = 0; i < _Dots.Count; i++)
                _OriginalPositions[i] = _Dots[i].anchoredPosition;
        }

        #endregion

        #region Coroutine

        IEnumerator AnimateDots()
        {
            while (true) {
                for (int i = 0; i < _Dots.Count; i++) {
                    StartCoroutine(Jump(_Dots[i], _OriginalPositions[i]));

                    yield return new WaitForSeconds(_DelayBetweenJumps);
                }
            }
        }

        IEnumerator Jump(RectTransform dot, Vector2 originalPos)
        {
            float timer = 0.0f;

            while (timer < _JumpDuration / 2f) {
                float t       = timer / (_JumpDuration / 2f);
                float yOffset = Mathf.Lerp(0f, _JumpHeight, t);

                dot.anchoredPosition = originalPos + new Vector2(0f, yOffset);

                timer += Time.deltaTime;

                yield return null;
            }

            timer = 0.0f;

            while (timer < _JumpDuration / 2f) {
                float t       = timer / (_JumpDuration / 2f);
                float yOffset = Mathf.Lerp(_JumpHeight, 0f, t);

                dot.anchoredPosition = originalPos + new Vector2(0f, yOffset);

                timer += Time.deltaTime;

                yield return null;
            }

            dot.anchoredPosition = originalPos;
        }

        #endregion
    }
}
