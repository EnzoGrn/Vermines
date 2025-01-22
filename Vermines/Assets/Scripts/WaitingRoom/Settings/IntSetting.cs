using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;

[System.Serializable]
public class IntSetting : ASetting
{
    [SerializeField] public int Value;
    [SerializeField] public int MinValue;
    [SerializeField] public int MaxValue;

    public IntSetting(string name, int value, int minValue, int maxValue, string category)
    {
        //name = nameuh;
        Name = name;
        Value = value;
        MinValue = minValue;
        MaxValue = maxValue;
        Category = category;

        Type = SettingType.Int;

        ErrorMessage = $"Value of {Name} must be between {MinValue} and {MaxValue}";
    }

    public void RestrictionCheck(int value)
    {
        Debug.Log("Restriction check for int setting Min : " + MinValue + ", Value : " + value + ", Max Value : " + MaxValue);
        if (value < MinValue || value > MaxValue)
        {
            throw new System.Exception(ErrorMessage);
        }
        else if (MinValue > MaxValue)
        {
            throw new System.Exception("Min value cannot be greater than max value");
        }
    }

    public override void RestrictionCheck<T>(T value)
    {
        // Check if T is an int
        if (value is not int)
        {
            throw new System.Exception("Value may be wrong.");
        }

        int intValue = (int)(object)value;

        Debug.Log("Restriction check for int setting Min : " + MinValue + ", Value : " + intValue + ", Max Value : " + MaxValue);
        if (intValue < MinValue || intValue > MaxValue)
        {
            throw new System.Exception(ErrorMessage);
        }
        else if (MinValue > MaxValue)
        {
            throw new System.Exception("Min value cannot be greater than max value");
        }
    }

    public override void SetUpErrorMessage()
    {
        ErrorMessage = $"Value of {Name} must be between {MinValue} and {MaxValue}";
    }
}

