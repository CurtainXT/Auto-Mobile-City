using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    public GameObject UnitToSpawn;
    public float SpawnTimeInterval = 5f;

    float spawnTime;
    
    // Start is called before the first frame update
    void Start()
    {
        spawnTime = SpawnTimeInterval;
    }

    // Update is called once per frame
    void Update()
    {
        spawnTime -= Time.deltaTime;
        //Demo
        if (spawnTime <= 0 && UnitToSpawn != null)
        {
            spawnTime = SpawnTimeInterval;
            GameObject Unit_ref = GameObject.Instantiate(UnitToSpawn);
            Unit_ref.transform.position = this.transform.position;
            Unit_ref.transform.rotation = this.transform.rotation;
        }
    }
}
