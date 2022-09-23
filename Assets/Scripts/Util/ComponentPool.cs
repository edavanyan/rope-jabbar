using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ComponentPool<T> : Pool<T> where T : MonoBehaviour, IPoolable
{
    public ComponentPool(T prototype) : base(prototype)
    {
    }
    
    protected override T CreateItem(T prototype)
    {
        return GameObject.Instantiate(prototype);
    }

}
