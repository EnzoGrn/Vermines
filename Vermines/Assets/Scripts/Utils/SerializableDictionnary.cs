using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using UnityEngine;

public class SerializableDictionary {}

[Serializable]
public class SerializableDictionary<TKey, TValue> : SerializableDictionary, IDictionary<TKey, TValue>, ISerializationCallbackReceiver {

    [SerializeField]
    private List<SerializableKeyValuePair> List = new();

    [Serializable]
    public struct SerializableKeyValuePair {

        public TKey Key;

        public TValue Value;

        public SerializableKeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public void SetValue(TValue value)
        {
            Value = value;
        }
    }

    private Lazy<Dictionary<TKey, uint>> _KeyPositions;

    private Dictionary<TKey, uint> KeyPositions => _KeyPositions.Value;

    public SerializableDictionary()
    {
        _KeyPositions = new Lazy<Dictionary<TKey, uint>>(MakeKeyPositions);
    }

    public SerializableDictionary(IDictionary<TKey, TValue> dictionary)
    {
        _KeyPositions = new Lazy<Dictionary<TKey, uint>>(MakeKeyPositions);

        if (dictionary == null)
            throw new ArgumentException("The passed dictionary is null.");
        foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            Add(pair.Key, pair.Value);
    }

    private Dictionary<TKey, uint> MakeKeyPositions()
    {
        int numEntries = List.Count;

        Dictionary<TKey, uint> result = new Dictionary<TKey, uint>(numEntries);

        for (int i = 0; i < numEntries; ++i)
            result[List[i].Key] = (uint)i;
        return result;
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {
        // After deserialization, the key positions might be changed
        _KeyPositions = new Lazy<Dictionary<TKey, uint>>(MakeKeyPositions);
    }

    #region IDictionary

    public TValue this[TKey key]
    {
        get => List[(int)KeyPositions[key]].Value;
        set
        {
            if (KeyPositions.TryGetValue(key, out uint index))
                List[(int)index].SetValue(value);
            else {
                KeyPositions[key] = (uint)List.Count;

                List.Add(new SerializableKeyValuePair(key, value));
            }
        }
    }

    public ICollection<TKey> Keys => List.Select(tuple => tuple.Key).ToArray();

    public ICollection<TValue> Values => List.Select(tuple => tuple.Value).ToArray();

    public void Add(TKey key, TValue value)
    {
        if (KeyPositions.ContainsKey(key))
            throw new ArgumentException("An element with the same key already exists in the dictionary.");
        else {
            KeyPositions[key] = (uint)List.Count;

            List.Add(new SerializableKeyValuePair(key, value));
        }
    }

    public bool ContainsKey(TKey key) => KeyPositions.ContainsKey(key);

    public bool Remove(TKey key)
    {
        if (KeyPositions.TryGetValue(key, out uint index)) {
            Dictionary<TKey, uint> kp = KeyPositions;

            kp.Remove(key);
            List.RemoveAt((int)index);

            int numEntries = List.Count;

            for (uint i = index; i < numEntries; i++)
                kp[List[(int)i].Key] = i;
            return true;
        }
        return false;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (KeyPositions.TryGetValue(key, out uint index)) {
            value = List[(int)index].Value;

            return true;
        }
        value = default;

        return false;
    }

    #endregion

    #region ICollection

    public int Count => List.Count;

    public bool IsReadOnly => false;

    public void Add(KeyValuePair<TKey, TValue> kvp) => Add(kvp.Key, kvp.Value);

    public void Clear()
    {
        List.Clear();
        KeyPositions.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> kvp) => KeyPositions.ContainsKey(kvp.Key);

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        int numKeys = List.Count;

        if (array.Length - arrayIndex < numKeys)
            throw new ArgumentException("arrayIndex");
        for (int i = 0; i < numKeys; ++i, ++arrayIndex) {
            SerializableKeyValuePair entry = List[i];

            array[arrayIndex] = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
        }
    }

    public bool Remove(KeyValuePair<TKey, TValue> kvp) => Remove(kvp.Key);

    #endregion

    #region IEnumerable

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return List.Select(ToKeyValuePair).GetEnumerator();

        static KeyValuePair<TKey, TValue> ToKeyValuePair(SerializableKeyValuePair skvp)
        {
            return new KeyValuePair<TKey, TValue>(skvp.Key, skvp.Value);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}
