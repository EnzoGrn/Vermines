
using UnityEngine;

[System.Serializable]
public class BoolSetting : ASetting
{
    [SerializeField] public bool Value;

    public BoolSetting(string name, bool value, string category)
    {
        Name = name;
        Value = value;
        Category = category;

        Type = SettingType.Bool;
    }

    public override void RestrictionCheck<T>(T Value)
    {
        if (Value is not bool)
        {
            throw new System.Exception("Value may be wrong.");
        }
    }

    public override void SetUpErrorMessage()
    {
        ErrorMessage = $"Value of {Name} must be a boolean";
    }
}

