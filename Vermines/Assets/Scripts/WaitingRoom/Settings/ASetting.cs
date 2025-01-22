
using System;
using UnityEngine;

    [System.Serializable]
    public abstract class ASetting
    {
        [System.Serializable]
        public enum SettingType
        {
            Bool,
            Int,
        }

        [SerializeField, HideInInspector] public SettingType Type;

        [SerializeField, HideInInspector] public string Category;
        [SerializeField, HideInInspector] public string Name;

        [SerializeField, HideInInspector] public string ErrorMessage;

        //[SerializeField] public object Value;

        public virtual void SetUpErrorMessage() {}
        public abstract void RestrictionCheck<T>(T Value);

}
