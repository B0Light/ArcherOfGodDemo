using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Variable<T> 
{
    [SerializeField] private T value; 
    
    public event Action<T> OnValueChanged;

    public Variable(T initialValue)
    {
        value = initialValue;
    }

    public T Value
    {
        get => value;
        set
        {
            if (!EqualityComparer<T>.Default.Equals(this.value, value))
            {
                this.value = value;
                OnValueChanged?.Invoke(this.value);
            }
        }
    }
    
    public void ClearAllSubscribers()
    {
        OnValueChanged = null;
    }
}
