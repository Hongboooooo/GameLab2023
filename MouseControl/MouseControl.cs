using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Diagnostics;

public class MouseControl : MonoBehaviour
{
    // Start is called before the first frame update
    private bool mouse_is_down = false;
    public LineRenderer line;
    private Vector3 box_start;
    private Vector3 box_end;
    private Vector3 box_left;
    private Vector3 box_right;
    private float camera_forward = 5;
    private RaycastHit MouseHit;
    private RaycastHit StandHit;
    private RaycastHit IntermHit;
    private int character_walk_lay_mask; 
    private int mouse_selection_lay_mask;  
    private int right_mouse_selection_lay_mask;
    public List<CharacterStatus> character_list = new List<CharacterStatus>();
    private BuildingStatus bss;
    MeshCollider selection_box = new MeshCollider();
    Vector3[] selection_box_corners = new Vector3[5];
    Vector3 character_destination;
    String interm_destination = "";
    Vector3 interm_position = new Vector3(0,0,0);
    MeshFilter mf;
    MeshRenderer mr;
    public Dictionary<string, Vector3> portal_coordinates = new Dictionary<string, Vector3>();

    void Start()
    {
        line = this.GetComponent<LineRenderer>();
        line.loop = true;
        // line.startColor = Color.green;
        line.startWidth = 0.02f;
        line.endWidth = 0.02f;
        
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();

        portal_coordinates.Add("TopLayerActiveArea", GameObject.Find("TopLayerActiveArea").transform.position);
        portal_coordinates.Add("MiddleLayerActiveArea", GameObject.Find("MiddleLayerActiveArea").transform.position);
        portal_coordinates.Add("GroundLayerActiveArea", GameObject.Find("GroundLayerActiveArea").transform.position);
        
    }

    // Update is called once per frame
    void Update()
    {
        character_walk_lay_mask = 1<<LayerMask.NameToLayer("TopLayer") | 1<<LayerMask.NameToLayer("MiddleLayer") | 1<<LayerMask.NameToLayer("GroundLayer") | 1<<LayerMask.NameToLayer("Elevator");
        mouse_selection_lay_mask = 1<<LayerMask.NameToLayer("TopLayer") | 1<<LayerMask.NameToLayer("MiddleLayer") | 1<<LayerMask.NameToLayer("GroundLayer") | 1<<LayerMask.NameToLayer("Elevator") | 1<<LayerMask.NameToLayer("Character");
        right_mouse_selection_lay_mask = 1<<LayerMask.NameToLayer("TopLayer") | 1<<LayerMask.NameToLayer("MiddleLayer") | 1<<LayerMask.NameToLayer("GroundLayer") | 1<<LayerMask.NameToLayer("Elevator") | 1<<LayerMask.NameToLayer("Building");
        if(Input.GetMouseButtonDown(0))
        {
            box_start = Input.mousePosition;
            mouse_is_down = true;
            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out MouseHit, 10000, mouse_selection_lay_mask))
            {
                selection_box_corners[0] = MouseHit.point;
            }
            else
            {
                selection_box_corners[0] = Camera.main.transform.position + 200 * Camera.main.ScreenPointToRay(Input.mousePosition).direction;
            }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            mouse_is_down = false;
            line.positionCount = 0;

            for(int i = 0; i < character_list.Count; i++)
            {
                if(character_list[i] != null)
                    character_list[i].IsSelected(false);
            }
            character_list.Clear();
            if(bss != null)
            {
                bss.IsSelected(false);
                bss = null;
            }
            

            int cast_count = 0;

            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out MouseHit, 10000, mouse_selection_lay_mask))
            {
                selection_box_corners[3] = MouseHit.point;
                cast_count++;
            }
            else
            {
                selection_box_corners[3] = Camera.main.transform.position + 100 * Camera.main.ScreenPointToRay(Input.mousePosition).direction;
            }

            if(Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(box_start.x, Input.mousePosition.y,0)), out MouseHit, 10000, mouse_selection_lay_mask))
            {
                selection_box_corners[1] = MouseHit.point;
                cast_count++;
            }
            else
            {
                selection_box_corners[1] = Camera.main.transform.position + 100 * Camera.main.ScreenPointToRay(new Vector3(box_start.x, Input.mousePosition.y,0)).direction;
            }

            if(Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, box_start.y,0)), out MouseHit, 10000, mouse_selection_lay_mask))
            {
                selection_box_corners[2] = MouseHit.point;
                cast_count++;
            }
            else
            {
                selection_box_corners[2] = Camera.main.transform.position + 100 * Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, box_start.y,0)).direction;
            }
            // if(cast_count == 3)
            {
                if(box_start.x != box_end.x && box_start.y != box_end.y)
                {
                    selection_box_corners[4] = Camera.main.transform.position;

                    int[] triangles = {0,1,3,0,3,2,0,4,1,1,4,3,2,3,4,0,2,4};
                    Mesh selection_mesh = new Mesh();
                    selection_mesh.Clear();
                    selection_mesh.vertices = selection_box_corners;
                    selection_mesh.triangles = triangles;
                    mf.mesh = selection_mesh; //Draw Collision Mesh

                    selection_box = gameObject.AddComponent<MeshCollider>();
                    selection_box.sharedMesh = selection_mesh;
                    selection_box.convex = true;
                    selection_box.isTrigger = true;

                    Destroy(selection_box, 0.02f);
                }
                else
                {
                    if(Physics.Raycast(Camera.main.ScreenPointToRay(box_start), out MouseHit, 10000, mouse_selection_lay_mask))
                    {
                        CharacterStatus gs = MouseHit.transform.gameObject.GetComponent<CharacterStatus>();
                        if(gs != null)
                        {
                            gs.IsSelected(true);
                            character_list.Add(gs);
                        }
                    }
                }
            }
        }        

        //selection box debug
        Debug.DrawRay(Camera.main.ScreenPointToRay(Input.mousePosition).origin, Camera.main.ScreenPointToRay(Input.mousePosition).direction, Color.green);
        Debug.DrawRay(Camera.main.transform.position, selection_box_corners[0] - Camera.main.transform.position, Color.green);
        Debug.DrawRay(Camera.main.transform.position, selection_box_corners[1] - Camera.main.transform.position, Color.green);
        Debug.DrawRay(Camera.main.transform.position, selection_box_corners[2] - Camera.main.transform.position, Color.green);
        Debug.DrawRay(Camera.main.transform.position, selection_box_corners[3] - Camera.main.transform.position, Color.green);

        //draw selection box
        if(mouse_is_down)
        {
            box_end = Input.mousePosition;

            //draw selection rect by lines
            box_start.z = camera_forward;
            box_end.z = camera_forward;
            box_left = new Vector3(box_end.x, box_start.y, camera_forward);
            box_right = new Vector3(box_start.x, box_end.y, camera_forward);

            line.positionCount = 4;

            line.SetPosition(0, Camera.main.ScreenToWorldPoint(box_start));
            line.SetPosition(1, Camera.main.ScreenToWorldPoint(box_left));
            line.SetPosition(2, Camera.main.ScreenToWorldPoint(box_end));
            line.SetPosition(3, Camera.main.ScreenToWorldPoint(box_right));

        }

        RightClickAction();
        
    }

    private void OnTriggerStay(Collider other)
    {
        // Debug.Log("Have");
        CharacterStatus gs = other.GetComponent<CharacterStatus>();
        if(gs != null)
        {
            gs.IsSelected(true);
            character_list.Add(gs);
        }
    }

    private void RightClickAction()
    {
        if(Input.GetMouseButtonDown(1))
        {
            Debug.Log("right clicked!");
            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out MouseHit, 10000, right_mouse_selection_lay_mask)) // mouse hit position
            {
                if(character_list.Count >= 1) // deploy people
                {
                    if(MouseHit.transform.gameObject.layer == LayerMask.NameToLayer("Building"))
                    {
                        Debug.Log("Found Building!");
                        for(int idx=0; idx<character_list.Count; idx++) // iterate every selected unit and deploy them
                        {
                            if(character_list[idx] != null)
                            {
                                character_list[idx].end_vacation();
                                building_deploy(character_list[idx], MouseHit.transform.gameObject);

                                BuildingStatus dbs = MouseHit.transform.gameObject.GetComponent<BuildingStatus>();
                                if(dbs.Name == "mensa" || dbs.Name == "MedBay" || dbs.Name == "bed" || dbs.Name == "Yoga" || dbs.Name == "Bar")
                                {
                                    character_list[idx].vacation_from_here(dbs.ID);
                                }
                            }
                                
                        }
                    }
                    else
                    {
                         for(int idx=0; idx<character_list.Count; idx++) // iterate every selected unit and deploy them
                        {
                            if(character_list[idx] != null)
                            {
                                character_list[idx].end_vacation();
                                location_deploy(character_list[idx], MouseHit.point, MouseHit.transform.gameObject.name);
                            }
                        }
                    }
                }
                else //select building
                {
                    if(MouseHit.transform.gameObject.layer == LayerMask.NameToLayer("Building"))
                    {
                        Debug.Log("Found Building!");
                        if(bss != null)
                        {
                            bss.IsSelected(false);
                            bss = null;
                        }

                        bss = MouseHit.transform.gameObject.GetComponent<BuildingStatus>();
                        bss.IsSelected(true);
                        // Debug.Log(MouseHit.transform.gameObject.GetComponent<BuildingStatus>().Name);
                    }
                }
                
                
            }
        }
        
        // for(int idx = 0; idx < character_list.Count; idx++) // check every selecter character's status
        // {
        //     if(character_list[idx] != null)
        //         Debug.DrawRay(character_list[idx].transform.position, character_list[idx].current_destination_position - character_list[idx].transform.position, Color.red);
        // }
        
        // Debug.DrawRay(Camera.main.transform.position, character_destination - Camera.main.transform.position, Color.blue); // orient destination
    }

    public void building_deploy(CharacterStatus character, GameObject building_for_deployment)
    {
        Vector3 input_destination = new Vector3(0,0,0);
        string input_destination_name = "";
        if(building_for_deployment != null)
        {
            // Debug.Log(building_for_deployment.name);
            if(Physics.Raycast(building_for_deployment.transform.position + new Vector3(0,1,0),  new Vector3(0,-1,0), out StandHit, 10, character_walk_lay_mask))
            {
                float x_swift = (float)(building_for_deployment.GetComponent<BuildingStatus>().Size.x) / 2;
                float y_swift = (float)(building_for_deployment.GetComponent<BuildingStatus>().Size.y) / 2;
                input_destination = StandHit.point + new Vector3(x_swift, 0, y_swift);
                input_destination_name = StandHit.transform.gameObject.name;
            }
            else
            {
                Debug.LogError("Building stands on empty");
                return;
            }

            //deploy this character
            character.final_destination = null;
            if(Physics.Raycast(character.StandPosition()+ new Vector3(0,1,0), new Vector3(0,-1,0), out StandHit, 10, character_walk_lay_mask))
            {
                // move at the same layer
                if(StandHit.transform.gameObject.name == input_destination_name || StandHit.transform.gameObject.name == input_destination_name+"ActiveArea")
                {
                    character_destination = input_destination;
                }
                // move between different layers
                else
                {
                    character_destination = take_elevator(character, input_destination_name, input_destination);
                }

                character.UpdateDeployment(building_for_deployment);
                character.Move(character_destination);

            }
        }

    
            
    }

    private void location_deploy(CharacterStatus character, Vector3 input_destination, string input_destination_name)
    {
        // Debug.Log(character_list.Count);
        character.final_destination = null;

        if(Physics.Raycast(character.StandPosition()+ new Vector3(0,1,0), new Vector3(0,-1,0), out StandHit, 10, character_walk_lay_mask))
        {
            // move at the same layer
            if(StandHit.transform.gameObject.name == input_destination_name || StandHit.transform.gameObject.name == input_destination_name+"ActiveArea")
            {
                character_destination = input_destination;
            }
            // move between different layers
            else
            {
                character_destination = take_elevator(character, input_destination_name, input_destination);
            }

            character.UpdateDeployment(null);
            character.Move(character_destination);

        }

    }

    private Vector3 take_elevator(CharacterStatus character, string idn, Vector3 idp)
    {
        Vector3 outcome;

        character.final_destination = idn;
        character.final_destination_position = idp;
                        
        if(StandHit.transform.gameObject.name == "GroundLayer")
        {
            interm_position = portal_coordinates["GroundLayerActiveArea"];
        }
        if(StandHit.transform.gameObject.name == "MiddleLayer")
        {
            interm_position = portal_coordinates["MiddleLayerActiveArea"];
        }
        if(StandHit.transform.gameObject.name == "TopLayer")
        {
            interm_position = portal_coordinates["TopLayerActiveArea"];
        }

        // Debug.Log(interm_destination);
                        
        outcome = interm_position;

        return outcome;
    }
}
