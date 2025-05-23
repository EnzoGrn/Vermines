using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

namespace Vermines.Menu.Tools {

    public class LoadingManager : MonoBehaviour {

        private List<ILoadingSteps> _LoadingSteps = new();

        private float _TotalProgress = 0;

        public Action OnLoadingDone;

        public void AddStep(ILoadingSteps step)
        {
            if (step == null)
                return;
            _LoadingSteps.Add(step);
        }

        public void StartLoading()
        {
            _TotalProgress = 0;

            StartCoroutine(ExecuteSteps());
        }

        public void ClearSteps()
        {
            _LoadingSteps.Clear();
        }

        private IEnumerator ExecuteSteps()
        {
            foreach (ILoadingSteps step in _LoadingSteps) {
                yield return StartCoroutine(step.Execute());

                _TotalProgress += 1f / _LoadingSteps.Count;
            }

            OnLoadingDone?.Invoke();
        }
    }
}
