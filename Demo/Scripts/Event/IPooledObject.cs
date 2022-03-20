using UnityEngine;

public interface IPooledObject
{
    // 对象池对象使用这个函数进行初始化 而不是Start
    void OnObjectSpawn();

    // 将对象关闭激活
    void DeActive();
}
