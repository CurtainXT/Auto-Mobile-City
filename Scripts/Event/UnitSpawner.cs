using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ATMC
{
    public class UnitSpawner : MonoBehaviour
    {
        public bool isRandomSpawner = false;
        public string[] RandomPoolTagsPool;

        public string poolTag;
        public float spawnTimeDelay;

        private float spawnTimer;
        private ObjectPool pool;

        // Start is called before the first frame update
        void Start()
        {
            pool = ObjectPool.Instance;
        }

        // Update is called once per frame
        void Update()
        {
            // Demo
            if (spawnTimer <= 0 && pool != null)
            {
                if(isRandomSpawner)
                {
                    poolTag = RandomPoolTagsPool[Random.Range(0, RandomPoolTagsPool.Length - 1)];
                }
                ObjectPool.Instance.SpawnFromPool(poolTag, this.transform.position, this.transform.rotation);
                spawnTimer = spawnTimeDelay;
            }
            spawnTimer -= Time.deltaTime;
            //
        }
    }
}