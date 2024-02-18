using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ObjectsDataBaseSO : ScriptableObject
{
    public List<BuildingDataBase> building_data;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[Serializable]
public class BuildingDataBase
{
    [field: SerializeField]
    public string Name {get; private set;}
    [field: SerializeField]
    public int ID {get; private set;}
    [field: SerializeField]
    public Vector2Int Size {get; private set;} = Vector2Int.one;
    [field: SerializeField]
    public GameObject Prefab {get; private set;}
}