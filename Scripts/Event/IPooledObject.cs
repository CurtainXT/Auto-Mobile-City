using UnityEngine;

namespace ATMC
{
    public interface IPooledObject
    {
        // ����ض���ʹ������������г�ʼ�� ������Start
        void OnObjectSpawn();

        // ������رռ���
        void DeActive();
    }
}