using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Portal : MonoBehaviour
{
    // MeshCollider portal_box = new MeshCollider();

    // Start is called before the first frame update
    private Vector3 ground_layer_portal_coordinate;
    private Vector3 middle_layer_portal_coordinate;
    private Vector3 top_layer_portal_coordinate;
    
    void Start()
    {
        ground_layer_portal_coordinate = GameObject.Find("GroundLayerActiveArea").transform.position;
        middle_layer_portal_coordinate = GameObject.Find("MiddleLayerActiveArea").transform.position;
        top_layer_portal_coordinate = GameObject.Find("TopLayerActiveArea").transform.position;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerStay(Collider other)
    {
        CharacterStatus gs = other.GetComponent<CharacterStatus>();
        if(gs != null)
        {
            if(gs.final_destination != null)
            {
                if(gs.final_destination == "GroundLayer")
                {
                    gs.FlashMove(ground_layer_portal_coordinate);
                }
                else if(gs.final_destination == "MiddleLayer")
                {
                    gs.FlashMove(middle_layer_portal_coordinate);
                }
                else if(gs.final_destination == "TopLayer")
                {
                    gs.FlashMove(top_layer_portal_coordinate);
                }
                
                gs.Move(gs.final_destination_position);
                gs.final_destination = null;
            }
        }
    }
}
