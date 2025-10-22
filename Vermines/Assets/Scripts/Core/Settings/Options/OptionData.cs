using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Vermines.Core.Settings {

    [Serializable]
    public sealed class OptionsData {

        #region Attributes

        [SerializeField]
        private List<OptionsValue> _Values;

        [NonSerialized]
        private List<OptionsValue> _AllValues = new(64);

        private bool _IsInitialized;

        #endregion

        #region Getters & Setters

        public List<OptionsValue> Values => _AllValues;

        public bool TryGet(string key, out OptionsValue value)
        {
            Assert.IsTrue(_IsInitialized);

            value = default;

            foreach (OptionsValue v in _AllValues) {
                if (v.Key == key) {
                    value = v;

                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            if (_IsInitialized)
                return;
            _AllValues.AddRange(_Values);
            _IsInitialized = true;
        }

        public void AddRuntimeValue(OptionsValue value)
        {
            Assert.IsTrue(_IsInitialized);

            if (Contains(value.Key))
                return;
            _AllValues.Add(value);
        }

        public bool Contains(string key)
        {
            Assert.IsTrue(_IsInitialized);

            return TryGet(key, out OptionsValue value);
        }

        #endregion
    }
}
