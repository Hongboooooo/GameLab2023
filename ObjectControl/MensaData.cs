using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingData : MonoBehaviour
{
    // Start is called before the first frame update

    public string Name;
    public int ID;
    public Vector2Int Size = Vector2Int.one;
    public List<Vector3> seat_positions;
    public List<Vector3> seat_orientations;
    public List<bool> seat_occupations;
    void Start()
    {
        this.Size = new Vector2Int(3,3);
        this.seat_positions = new List<Vector3>
        {
            new Vector3(1,0,0.75f),
            new Vector3(2,0,0.75f),
            new Vector3(1,0,2.25f),
            new Vector3(2,0,2.25f)
        };
        this.seat_orientations = new List<Vector3>
        {
            new Vector3(0,0,1),
            new Vector3(0,0,1),
            new Vector3(0,0,-1),
            new Vector3(0,0,-1)
        };
        this.seat_occupations = new List<bool>
        {
            false,
            false,
            false,
            false
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
