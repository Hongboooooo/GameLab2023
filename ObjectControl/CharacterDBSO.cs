using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CharacterDBSO : ScriptableObject
{
    public List<CharacterDataBase> CDB;
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
public class CharacterDataBase
{
    [field: SerializeField]
    public string Name {get; private set;}
    [field: SerializeField]
    public int ID {get; private set;}
    [field: SerializeField]
    public GameObject Prefab {get; private set;}
}