using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ”ниверсальный пул объектов дл€ оптимизации (не создаЄм новые объекты, а переиспользуем).
/// </summary>
public class ObjectPool<T> : MonoBehaviour where T : Component
{
    private readonly T _prefab;
    private readonly Queue<T> _objects = new Queue<T>();
    private readonly Transform _parent;

    public ObjectPool(T prefab, int initialSize, Transform parent = null)
    {
        _prefab = prefab;
        _parent = parent;

        for (int i = 0; i < initialSize; i++)
        {
            var obj = Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            _objects.Enqueue(obj);
        }
    }

    public T Get()
    {
        if (_objects.Count == 0)
        {
            var obj = Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            _objects.Enqueue(obj);
        }

        T instance = _objects.Dequeue();
        instance.gameObject.SetActive(true);
        return instance;
    }

    public void ReturnToPool(T obj)
    {
        obj.gameObject.SetActive(false);
        _objects.Enqueue(obj);
    }
}