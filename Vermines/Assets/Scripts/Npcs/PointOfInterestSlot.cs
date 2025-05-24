using UnityEngine;

[System.Serializable]
public class PointOfInterestSlot: MonoBehaviour
{
    #region Public Properties
    public int Id;
    public Transform SlotTransform;

    [HideInInspector] public bool IsAvailable = true;
    #endregion

    public void Reserve() => IsAvailable = false;
    public void Free() => IsAvailable = true;
}