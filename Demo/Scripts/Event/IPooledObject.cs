using UnityEngine;

public interface IPooledObject
{
    // ����ض���ʹ������������г�ʼ�� ������Start
    void OnObjectSpawn();

    // ������رռ���
    void DeActive();
}
