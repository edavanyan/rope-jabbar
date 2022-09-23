using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Pool<T> where T : IPoolable
{   
    private readonly List<T> _freeItemList = new List<T>();
    private readonly T _prototype;

    public Pool(T prototype)
    {
        _prototype = prototype;
    }
    
    public T NewItem() {
        if (_freeItemList.Count > 0) {
            var item = _freeItemList[0];
            _freeItemList.RemoveAt(0);

            item.New();
            return item;
        }

        return CreateItem(_prototype);
    }

    protected abstract T CreateItem(T prototype);

    public void DestoryItem(T item) {
        _freeItemList.Add(item);
        item.Free();
    }
}
