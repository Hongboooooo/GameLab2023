using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField]
    private GameObject mouse_indicator, cell_indicator, cell_indicator_active;
    [SerializeField]
    private ConstructInputManager cim;
    [SerializeField]
    private Grid grids;
    [SerializeField]
    private ObjectsDataBaseSO database;
    private int selected_object_index = -1;
    public int build_start = -1;
    private int place_building_number = 0;
    private bool building_selected = false;
    private int placement_lay_mask;
    private RaycastHit MouseHit;
    private LineRenderer indicator_line;
    public GUIStyle cgsg;
    public GUIStyle cgso;
    public GUIStyle cgsb;
    public GUIStyle cgsr;
    public GUIStyle cgsgs;
    public GUIStyle cgsos;
    public GUIStyle cgsbs;
    public GUIStyle cgsrs;
    // public GUIStyle gs;
    private float r, g, b;
    public List<BuildingDataBaseSupply> building_data_supply = new List<BuildingDataBaseSupply>();
    private bool occupied;
    private int[,] ground_occupation = new int[58,57];
    private int[,] middle_occupation = new int[47,63];
    private int[,] top_occupation = new int[58,57];
    private int x_occupation_iter = 0;
    private int y_occupation_iter = 0;
    private Voltage voltage_system;
    private float live_timer;
    private int layer_idx = -1;
    private ProductionLibrary PL;
    private MouseControl mc;
    private int interval_x = 110;
    private int interval_y = 55;
    private int offset_x = 10;
    private int offset_y = 125;
    private int main_line_interval = 24;
    private float long_press_timer = 0;
    public Rect DisplayPos;
    public Texture DisplayBack;


    // Start is called before the first frame update
    void Start()
    {
        placement_lay_mask = 1<<LayerMask.NameToLayer("TopBuildPlane") | 1<<LayerMask.NameToLayer("MiddleBuildPlane") | 1<<LayerMask.NameToLayer("GroundBuildPlane");
        cell_indicator.SetActive(false);
        cell_indicator_active.SetActive(false);

        indicator_line = this.GetComponent<LineRenderer>();
        indicator_line.loop = true;
        indicator_line.startWidth = 0.08f;
        indicator_line.endWidth = 0.08f;

        DisplayPos.width = 450;
        // DisplayPos.height = 180;


        mc = GameObject.Find("SelectBox").GetComponent<MouseControl>();
        voltage_system = GameObject.Find("Voltage").GetComponent<Voltage>();
        PL = GameObject.Find("InterfaceProductionLibrary").GetComponent<ProductionLibrary>();

        cgsg.fontSize = 20;
        r = 10f / 255f; 
        g = 255f / 255f;
        b = 0;
        cgsg.normal.textColor = new Color(r, g, b, 1);

        cgso.fontSize = 20;
        r = 1; 
        g = 100f / 255f;
        b = 0;
        cgso.normal.textColor = new Color(r, g, b, 1);

        cgsb.fontSize = 20;
        r = 0; 
        g = 250f / 255f;
        b = 1;
        cgsb.normal.textColor = new Color(r, g, b, 1);

        cgsr.fontSize = 20;
        r = 255f / 255f; 
        g = 0;
        b = 0;
        cgsr.normal.textColor = new Color(r, g, b, 1);

        cgsgs.fontSize = 15;
        r = 10f / 255f; 
        g = 255f / 255f;
        b = 0;
        cgsgs.normal.textColor = new Color(r, g, b, 1);

        cgsos.fontSize = 15;
        r = 1; 
        g = 100f / 255f;
        b = 0;
        cgsos.normal.textColor = new Color(r, g, b, 1);

        cgsbs.fontSize = 15;
        r = 0; 
        g = 250f / 255f;
        b = 1;
        cgsbs.normal.textColor = new Color(r, g, b, 1);

        cgsrs.fontSize = 15;
        r = 255f / 255f; 
        g = 0;
        b = 0;
        cgsrs.normal.textColor = new Color(r, g, b, 1);
        
        // ground occupation init
        for(int idxx = 0; idxx < 58; idxx++)
        {
            for(int idxy = 0; idxy < 57; idxy++)
            {
                ground_occupation[idxx, idxy] = 0;
            }
        }
        for(int idxx = 0; idxx < 12; idxx++)
        {
            for(int idxy = 52; idxy < 57; idxy++)
            {
                ground_occupation[idxx, idxy] = 1;
            }
        }
        for(int idxx = 25; idxx < 58; idxx++)
        {
            for(int idxy = 17; idxy < 57; idxy++)
            {
                ground_occupation[idxx, idxy] = 1;
            }
        }

        // middle occupation init
        for(int idxx = 0; idxx < 47; idxx++)
        {
            for(int idxy = 0; idxy < 63; idxy++)
            {
                middle_occupation[idxx, idxy] = 0;
            }
        }
        for(int idxx = 0; idxx < 34; idxx++)
        {
            for(int idxy = 0; idxy < 17; idxy++)
            {
                middle_occupation[idxx, idxy] = 1;
            }
        }
        for(int idxx = 30; idxx < 47; idxx++)
        {
            for(int idxy = 27; idxy < 63; idxy++)
            {
                middle_occupation[idxx, idxy] = 1;
            }
        }
        for(int idxx = 16; idxx < 47; idxx++)
        {
            for(int idxy = 43; idxy < 63; idxy++)
            {
                middle_occupation[idxx, idxy] = 1;
            }
        }
        for(int idxx = 0; idxx < 6; idxx++)
        {
            for(int idxy = 57; idxy < 63; idxy++)
            {
                middle_occupation[idxx, idxy] = 1;
            }
        }
        
        // top occupation init
        for(int idxx = 0; idxx < 31; idxx++)
        {
            for(int idxy = 0; idxy < 36; idxy++)
            {
                top_occupation[idxx, idxy] = 0;
            }
        }
        for(int idxx = 0; idxx < 13; idxx++)
        {
            for(int idxy = 0; idxy < 16; idxy++)
            {
                top_occupation[idxx, idxy] = 1;
            }
        }

        BuildingDataBaseSupply mensa_transmit = new BuildingDataBaseSupply
        {
            seat_positions = new List<Vector3>
            {
                new Vector3(1,0,0.75f),
                new Vector3(2,0,0.75f),
                new Vector3(1,0,2.25f),
                new Vector3(2,0,2.25f)
            },
            seat_orientations = new List<Vector3>
            {
                new Vector3(0,0,1),
                new Vector3(0,0,1),
                new Vector3(0,0,-1),
                new Vector3(0,0,-1)
            },
            seat_pose = new List<string>
            {
                "IsSit",
                "IsSit",
                "IsSit",
                "IsSit"
            },
            seat_num = 4
        };

        BuildingDataBaseSupply bed_transmit = new BuildingDataBaseSupply
        {
            seat_positions = new List<Vector3>
            {
                new Vector3(1,0.5f,0.5f)
            },
            seat_orientations = new List<Vector3>
            {
                new Vector3(-1,0,0)
            },
            seat_pose = new List<string>
            {
                "IsLay"
            },
            seat_num = 1
        };

        BuildingDataBaseSupply assfac_transmit = new BuildingDataBaseSupply
        {
            seat_positions = new List<Vector3>
            {
                new Vector3(1.5f,0,1.5f),
                new Vector3(4,0,1),
            },
            seat_orientations = new List<Vector3>
            {
                new Vector3(1,0,1),
                new Vector3(0,0,1)
            },
            seat_pose = new List<string>
            {
                "StandWork",
                "SitWork",
            },
            seat_num = 2
        };

        BuildingDataBaseSupply bar_transmit = new BuildingDataBaseSupply
        {
            seat_positions = new List<Vector3>
            {
                new Vector3(2,0,1),
                new Vector3(3,0,1),
                new Vector3(4,0,1),
                new Vector3(1,0,2),
                new Vector3(1,0,3),
                new Vector3(1,0,4),
            },
            seat_orientations = new List<Vector3>
            {
                new Vector3(0,0,1),
                new Vector3(0,0,1),
                new Vector3(0,0,1),
                new Vector3(1,0,0),
                new Vector3(1,0,0),
                new Vector3(1,0,0),
            },
            seat_pose = new List<string>
            {
                "IsDrink",
                "IsDrink",
                "IsDrink",
                "IsDrink",
                "IsDrink",
                "IsDrink",
            },
            seat_num = 6
        };

        BuildingDataBaseSupply bioeg_transmit = new BuildingDataBaseSupply
        {
            seat_positions = new List<Vector3>
            {
                new Vector3(1.5f,0.2f,1.5f),

            },
            seat_orientations = new List<Vector3>
            {
                new Vector3(-1,0,0),
            },
            seat_pose = new List<string>
            {
                "IsSuffer",
            },
            seat_num = 1
        };
        
        BuildingDataBaseSupply gunfac_transmit = new BuildingDataBaseSupply
        {
            seat_positions = new List<Vector3>
            {
                new Vector3(1.5f,0,1.5f),
                new Vector3(4,0,1),
            },
            seat_orientations = new List<Vector3>
            {
                new Vector3(1,0,1),
                new Vector3(0,0,1)
            },
            seat_pose = new List<string>
            {
                "StandWork",
                "SitWork",
            },
            seat_num = 2
        };

        BuildingDataBaseSupply medbay_transmit = new BuildingDataBaseSupply
        {
            seat_positions = new List<Vector3>
            {
                new Vector3(2,0.5f,2.5f)
            },
            seat_orientations = new List<Vector3>
            {
                new Vector3(-1,0,0)
            },
            seat_pose = new List<string>
            {
                "IsLay"
            },
            seat_num = 1
        };

        BuildingDataBaseSupply parfac_transmit = new BuildingDataBaseSupply
        {
            seat_positions = new List<Vector3>
            {
                new Vector3(1.5f,0,1.5f),
                new Vector3(4,0,1),
            },
            seat_orientations = new List<Vector3>
            {
                new Vector3(1,0,1),
                new Vector3(0,0,1)
            },
            seat_pose = new List<string>
            {
                "StandWork",
                "SitWork",
            },
            seat_num = 2
        };

        BuildingDataBaseSupply popg_transmit = new BuildingDataBaseSupply
        {
            seat_positions = new List<Vector3>
            {
                new Vector3(2,0.2f,1),
                new Vector3(1,0.2f,2),
            },
            seat_orientations = new List<Vector3>
            {
                new Vector3(-1,0,0),
                new Vector3(0,0,-1)
            },
            seat_pose = new List<string>
            {
                null,
                null,
            },
            seat_num = 2
        };

        BuildingDataBaseSupply yoga_transmit = new BuildingDataBaseSupply
        {
            seat_positions = new List<Vector3>
            {
                new Vector3(3.5f,0,3.5f),
                new Vector3(1.5f,0,5.5f),
                new Vector3(1.5f,0,3.5f),
                new Vector3(1.5f,0,1.5f),
            },
            seat_orientations = new List<Vector3>
            {
                new Vector3(-1,0,0),
                new Vector3(1,0,0),
                new Vector3(1,0,0),
                new Vector3(1,0,0),
            },
            seat_pose = new List<string>
            {
                "IsExe",
                "IsExe",
                "IsExe",
                "IsExe",
            },
            seat_num = 4
        };

        // Debug.Log(mensa_transmit.seat_positions);
        building_data_supply.Add(mensa_transmit);
        building_data_supply.Add(bed_transmit);
        building_data_supply.Add(assfac_transmit);
        building_data_supply.Add(bar_transmit);
        building_data_supply.Add(bioeg_transmit);
        building_data_supply.Add(gunfac_transmit);
        building_data_supply.Add(medbay_transmit);
        building_data_supply.Add(parfac_transmit);
        building_data_supply.Add(popg_transmit);
        building_data_supply.Add(yoga_transmit);
    }

    // Update is called once per frame
    void Update()
    {
        // live_timer += Time.deltaTime;
        // if(live_timer >= 2)
        // {
        //     if(voltage_system.electricity_connected_buildings.Count > 0)
        //     {
        //         Destroy(voltage_system.electricity_connected_buildings[0].gameObject);
        //         voltage_system.electricity_connected_buildings.RemoveAt(0);
        //     }
        //     Debug.Log(voltage_system.electricity_connected_buildings.Count);
        //     live_timer = 0;
        // }

        CastData cd = CastCast();
        mouse_indicator.transform.position = cd.position;
        Vector3Int grid_coord = grids.WorldToCell(cd.position);
        string layer_name = cd.layer_name;
        if(build_start > 0)
        {
            if(building_selected)
            {
                
                occupied = true;

                if(layer_name == "GroundBuildPlane")
                {
                    x_occupation_iter = grid_coord.x + database.building_data[place_building_number].Size.x - 1;
                    y_occupation_iter = grid_coord.z + database.building_data[place_building_number].Size.y - 1;
                    if(0 < x_occupation_iter && x_occupation_iter < 58 && 0 < y_occupation_iter && y_occupation_iter < 57)
                    {
                        occupied = false;
                        for(int idx = 0; idx < database.building_data[place_building_number].Size.x; idx++)
                        {
                            for(int idy = 0; idy < database.building_data[place_building_number].Size.y; idy++)
                            {
                                if(ground_occupation[grid_coord.x + idx, grid_coord.z + idy] > 0)
                                {
                                    occupied = true;
                                }   
                            }
                        }
                    }

                }
                else if(layer_name == "MiddleBuildPlane")
                {
                    x_occupation_iter = grid_coord.x + database.building_data[place_building_number].Size.x - 1;
                    y_occupation_iter = grid_coord.z + database.building_data[place_building_number].Size.y - 1;
                    if(0 < x_occupation_iter && x_occupation_iter < 47 && 0 < y_occupation_iter + 17 && y_occupation_iter + 17 < 63)
                    {
                        occupied = false;
                        for(int idx = 0; idx < database.building_data[place_building_number].Size.x; idx++)
                        {
                            for(int idy = 0; idy < database.building_data[place_building_number].Size.y; idy++)
                            {
                                if(middle_occupation[grid_coord.x + idx, grid_coord.z + idy + 17] > 0)
                                {
                                    occupied = true;
                                }   
                            }
                        }
                    }
                }

                else if(layer_name == "TopBuildPlane")
                {
                    x_occupation_iter = grid_coord.x + database.building_data[place_building_number].Size.x - 1;
                    y_occupation_iter = grid_coord.z + database.building_data[place_building_number].Size.y - 1;
                    if(0 < x_occupation_iter + 13 && x_occupation_iter + 13 < 31 && 0 < y_occupation_iter + 16 && y_occupation_iter + 16 < 36)
                    {
                        occupied = false;
                        for(int idx = 0; idx < database.building_data[place_building_number].Size.x; idx++)
                        {
                            for(int idy = 0; idy < database.building_data[place_building_number].Size.y; idy++)
                            {
                                if(top_occupation[grid_coord.x + idx + 13, grid_coord.z + idy + 16] > 0)
                                {
                                    occupied = true;
                                }   
                            }
                        }
                    }
                }
                
                

                Vector3 line_start = grids.CellToWorld(grids.WorldToCell(cd.position));
                indicator_line.positionCount = 4;
                indicator_line.startColor = Color.blue;
                indicator_line.endColor = Color.blue;
                indicator_line.SetPosition(0, line_start + new Vector3(0, 0.1f, 0));
                indicator_line.SetPosition(1, line_start + new Vector3(database.building_data[place_building_number].Size.x, 0.1f, 0));
                indicator_line.SetPosition(2, line_start + new Vector3(database.building_data[place_building_number].Size.x, 0.1f, database.building_data[place_building_number].Size.y));
                indicator_line.SetPosition(3, line_start + new Vector3(0, 0.1f, database.building_data[place_building_number].Size.y));
                
                
            }
            else
            {
                cell_indicator.transform.position = grids.CellToWorld(grids.WorldToCell(cd.position));
            }   
            
        }

        if(Input.GetKeyUp(KeyCode.R))
        {
            build_start = -1 * build_start;
            if(build_start > 0)
            {
                cell_indicator.SetActive(true);
                cell_indicator_active.SetActive(false);
                mc.line.positionCount = 0;

                for(int i = 0; i < mc.character_list.Count; i++)
                {
                    if(mc.character_list[i] != null)
                        mc.character_list[i].IsSelected(false);
                }
                mc.character_list.Clear();
            }
            else
            {
                StopPlacement();
            }
        }

        if(build_start > 0)
        {
            if(Input.GetKeyUp(KeyCode.Alpha1))
            {
                // if(PL.mat_library["Scrap"] >= PL.scrap_requirement_library[database.building_data[0].Name] && PL.mat_library["Part"] >= PL.part_requirement_library[database.building_data[0].Name])
                // {

                // }
                place_building_number = 4;
                building_selected = true;
                cell_indicator.SetActive(false);
                // cell_indicator_active.SetActive(true);
            }
            if(Input.GetKeyUp(KeyCode.Alpha2))
            {
                place_building_number = 0;
                building_selected = true;
                cell_indicator.SetActive(false);
                // cell_indicator_active.SetActive(true);
            }
            if(Input.GetKeyUp(KeyCode.Alpha3))
            {
                place_building_number = 1;
                building_selected = true;
                cell_indicator.SetActive(false);
                // cell_indicator_active.SetActive(true);
            }
            if(Input.GetKeyUp(KeyCode.Alpha4))
            {
                place_building_number = 6;
                building_selected = true;
                cell_indicator.SetActive(false);
                // cell_indicator_active.SetActive(true);
            }
            if(Input.GetKeyUp(KeyCode.Alpha5))
            {
                place_building_number = 7;
                building_selected = true;
                cell_indicator.SetActive(false);
                // cell_indicator_active.SetActive(true);
            }
            if(Input.GetKeyUp(KeyCode.Alpha6))
            {
                place_building_number = 2;
                building_selected = true;
                cell_indicator.SetActive(false);
                // cell_indicator_active.SetActive(true);
            }
            if(Input.GetKeyUp(KeyCode.Alpha7))
            {
                place_building_number = 5;
                building_selected = true;
                cell_indicator.SetActive(false);
                // cell_indicator_active.SetActive(true);
            }
            if(Input.GetKeyUp(KeyCode.Alpha8))
            {
                place_building_number = 3;
                building_selected = true;
                cell_indicator.SetActive(false);
                // cell_indicator_active.SetActive(true);
            }
            if(Input.GetKeyUp(KeyCode.Alpha9))
            {
                place_building_number = 9;
                building_selected = true;
                cell_indicator.SetActive(false);
                // cell_indicator_active.SetActive(true);
            }
            if(Input.GetKeyUp(KeyCode.Alpha0))
            {
                place_building_number = 8;
                building_selected = true;
                cell_indicator.SetActive(false);
                // cell_indicator_active.SetActive(true);
            }

            if(place_building_number >= database.building_data.Count)
            {
                building_selected = false;
            }

            if(building_selected)
            {
                if(Input.GetMouseButtonUp(0))
                {
                    if(!occupied)
                    {
                        if(PL.mat_library["Scrap"] >= PL.scrap_requirement_library[database.building_data[place_building_number].Name] && PL.mat_library["Part"] >= PL.part_requirement_library[database.building_data[place_building_number].Name])
                        {
                            for(int idx = 0; idx < database.building_data[place_building_number].Size.x; idx++)
                            {
                                for(int idy = 0; idy < database.building_data[place_building_number].Size.y; idy++)
                                {
                                    if(layer_name == "GroundBuildPlane")
                                    {
                                        ground_occupation[grid_coord.x + idx, grid_coord.z + idy] = 1;
                                    }
                                    else if(layer_name == "MiddleBuildPlane")
                                    {
                                        middle_occupation[grid_coord.x + idx, grid_coord.z + idy + 17] = 1;
                                    }
                                    else if(layer_name == "TopBuildPlane")
                                    {
                                        
                                        top_occupation[grid_coord.x + idx + 13, grid_coord.z + idy + 16] = 1;
                                    }

                                }
                            }
                            PL.mat_library["Scrap"] -= PL.scrap_requirement_library[database.building_data[place_building_number].Name];
                            PL.mat_library["Part"] -= PL.part_requirement_library[database.building_data[place_building_number].Name];
                            PlaceStructure(place_building_number, cd);

                        }
                        
                    }
                    
                }
            }

            if(Input.GetMouseButton(1))
            {
                long_press_timer += Time.deltaTime;
                if(long_press_timer >= 1)
                {
                    if(layer_name == "GroundBuildPlane")
                    {
                        layer_idx = 0;
                    }
                    else if(layer_name == "MiddleBuildPlane")
                    {
                        layer_idx = 1;
                    }
                    else if(layer_name == "TopBuildPlane")
                    {
                        layer_idx = 2;
                    }
                    
                    RemoveBuilding(cd);
                    long_press_timer = 0;
                }
            }
            if(Input.GetMouseButtonUp(1))
            {
                long_press_timer = 0;
            }
            
        }

    }


    private void OnGUI()
    {
        GUI.DrawTexture(DisplayPos, DisplayBack);

        GUI.Label(new Rect(offset_x,offset_y - 125,100,100), "Voltage: " + ((int)(voltage_system.current_voltage_sat * 100)).ToString() + "%", voltage_system.gs);

        GUI.Label(new Rect(offset_x,offset_y - 75,100,20), "Food:\t" + PL.mat_library["Food"].ToString(), cgso);
        GUI.Label(new Rect(offset_x+120,offset_y - 75,100,20), "Scrap:" + PL.mat_library["Scrap"].ToString(), cgso);
        GUI.Label(new Rect(offset_x+250,offset_y - 75,100,20), "Part:\t" + PL.mat_library["Part"].ToString(), cgso);

        GUI.Label(new Rect(offset_x,offset_y - 50,100,20), "PST:\t" + PL.gun_library["Pistol"].ToString(), cgso);
        GUI.Label(new Rect(offset_x+100,offset_y - 50,100,20), "ARF:\t" + PL.gun_library["Assault Rifle"].ToString(), cgso);
        GUI.Label(new Rect(offset_x+200,offset_y - 50,100,20), "STG:\t" + PL.gun_library["Shotgun"].ToString(), cgso);
        GUI.Label(new Rect(offset_x+300,offset_y - 50,100,20), "LRF:\t" + PL.gun_library["Long Rifle"].ToString(), cgso);
        GUI.Label(new Rect(offset_x,offset_y - 25,100,20), "HVT:\t" + PL.bot_library["Harvester"].ToString(), cgso);
        GUI.Label(new Rect(offset_x+100,offset_y - 25,100,20), "LCR:\t" + PL.bot_library["Light Combat Robot"].ToString(), cgso);
        GUI.Label(new Rect(offset_x+200,offset_y - 25,100,20), "MCR:\t" + PL.bot_library["Middle Combat Robot"].ToString(), cgso);
        GUI.Label(new Rect(offset_x+300,offset_y - 25,100,20), "HCR:\t" + PL.bot_library["Heavy Combat Robot"].ToString(), cgso);
        

        if(build_start < 0)
        {
            DisplayPos.height = 160;
            GUI.Label(new Rect(offset_x,offset_y,100,100), "Press R to enter construction mode", cgsg);
        }
        else
        {
            DisplayPos.height = 375;
            GUI.Label(new Rect(offset_x,offset_y,100,20), "Long Right Click to destory buildings", cgsg);
            GUI.Label(new Rect(offset_x,offset_y+main_line_interval,100,20), "Press NumKey to select building", cgsg);
            GUI.Label(new Rect(offset_x,offset_y+main_line_interval*2,100,20), "Selected Building:", cgsg);
            if(building_selected)
            {
                GUI.Label(new Rect(offset_x + 180,offset_y+main_line_interval*2,100,20), database.building_data[place_building_number].Name, cgsb);
            }

            DrawBuildingMenue(0,0,"1: BioEG","BioEG");
            DrawBuildingMenue(1,0,"2: Mensa","mensa");
            DrawBuildingMenue(2,0,"3: Bed","bed");
            DrawBuildingMenue(3,0,"4: MedBay","MedBay");
            DrawBuildingMenue(0,1,"5: Parfac","ParFac");
            DrawBuildingMenue(1,1,"6: AssFac","AssFac");
            DrawBuildingMenue(2,1,"7: GunFac","GunFac");
            DrawBuildingMenue(3,1,"8: Bar","Bar");
            DrawBuildingMenue(0,2,"9: Yoga","Yoga");
            DrawBuildingMenue(1,2,"0: PopG","PopG");


        }
        
    }

    private void DrawBuildingMenue(int x, int y, string title, string name)
    {
        GUI.Label(new Rect(offset_x + interval_x*x,offset_y+interval_y*y+main_line_interval*3,100,20), title, cgsb);
        GUI.Label(new Rect(offset_x + interval_x*x,offset_y+interval_y*y+main_line_interval*3+20,100,20), "SCP:", cgsgs);
        if(PL.mat_library["Scrap"] >= PL.scrap_requirement_library[name])
        {
            GUI.Label(new Rect(offset_x + interval_x*x+40,offset_y+interval_y*y+main_line_interval*3+20,100,20), PL.mat_library["Scrap"] + "/" +PL.scrap_requirement_library[name], cgsgs);
        }
        else
        {
            GUI.Label(new Rect(offset_x + interval_x*x+40,offset_y+interval_y*y+main_line_interval*3+20,100,20), PL.mat_library["Scrap"] + "/" +PL.scrap_requirement_library[name], cgsrs);
        }
        GUI.Label(new Rect(offset_x + interval_x*x,offset_y+interval_y*y+main_line_interval*3+35,100,20), "PRT:", cgsgs);
        if(PL.mat_library["Part"] >= PL.part_requirement_library[name])
        {
            GUI.Label(new Rect(offset_x + interval_x*x+40,offset_y+interval_y*y+main_line_interval*3+35,100,20), PL.mat_library["Part"] + "/" +PL.part_requirement_library[name], cgsgs);
        }
        else
        {
            GUI.Label(new Rect(offset_x + interval_x*x+40,offset_y+interval_y*y+main_line_interval*3+35,100,20), PL.mat_library["Part"] + "/" +PL.part_requirement_library[name], cgsrs);
        }
    }

    public void StartPlacement(int ID)
    {
        selected_object_index = 0; //database.objects_data.FindIndex(data => data.ID == ID)
        if(selected_object_index < 0)
        {
            Debug.LogError($"No ID found {ID}");
            return;
        }
        building_selected = false;

    }
    public void StopPlacement()
    {
        selected_object_index = -1;
        building_selected = false;
        cell_indicator.SetActive(false);
        cell_indicator_active.SetActive(false);
        indicator_line.positionCount = 0;
        // cim.BuildEnter -= PlaceStructure;
        // cim.BuildExit -= StopPlacement;
    }
    private void PlaceStructure(int building_id, CastData cd)
    {
        Vector3Int coord = grids.WorldToCell(cd.position);
        
        if(cd.layer_name == "GroundBuildPlane")
        {
            layer_idx = 0;
        }
        if(cd.layer_name == "MiddleBuildPlane")
        {
            layer_idx = 1;
        }
        if(cd.layer_name == "TopBuildPlane")
        {
            layer_idx = 2;
        }
        

        // Debug.Log("PlaceStructure");
        if(building_id >= database.building_data.Count)
        {
            Debug.LogError($"Building List Over Bound!");
            return;
        }

        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out MouseHit, 10000, placement_lay_mask))
        {
            GameObject new_object = Instantiate(database.building_data[building_id].Prefab);
            new_object.transform.position = grids.CellToWorld(grids.WorldToCell(MouseHit.point));

            new_object.GetComponent<BuildingStatus>().Name = database.building_data[building_id].Name;
            new_object.GetComponent<BuildingStatus>().Size = database.building_data[building_id].Size;
            new_object.GetComponent<BuildingStatus>().ID = database.building_data[building_id].ID;

            new_object.GetComponent<BuildingStatus>().seat_positions = building_data_supply[building_id].seat_positions;
            new_object.GetComponent<BuildingStatus>().seat_orientations = building_data_supply[building_id].seat_orientations;
            new_object.GetComponent<BuildingStatus>().seat_pose = building_data_supply[building_id].seat_pose;
            new_object.GetComponent<BuildingStatus>().seat_num = building_data_supply[building_id].seat_num;

            new_object.GetComponent<BuildingStatus>().coordinate = new Vector3Int(layer_idx, coord.x, coord.z);
        }
        

    }
    private void RemoveBuilding(CastData cd)
    {
        Vector3Int grid_coord_remove = grids.WorldToCell(cd.position);
        string layer_name_remove = cd.layer_name;
        for(int idx = 0; idx < voltage_system.electricity_connected_buildings.Count; idx++)
        {
            Vector3Int bcc = voltage_system.electricity_connected_buildings[idx].coordinate;
            if(bcc.x == layer_idx)
            {
                for(int ix = 0; ix < voltage_system.electricity_connected_buildings[idx].Size.x; ix++)
                {
                    for(int iy = 0; iy < voltage_system.electricity_connected_buildings[idx].Size.y; iy++)
                    {
                        if(grid_coord_remove.x == bcc.y + ix && grid_coord_remove.z == bcc.z + iy)
                        {
                            //remove buildings
                            for(int ixx = 0; ixx < voltage_system.electricity_connected_buildings[idx].Size.x; ixx++)
                            {
                                for(int iyy = 0; iyy < voltage_system.electricity_connected_buildings[idx].Size.y; iyy++)
                                {
                                    if(layer_name_remove == "GroundBuildPlane")
                                    {
                                        ground_occupation[voltage_system.electricity_connected_buildings[idx].coordinate.y + ixx, voltage_system.electricity_connected_buildings[idx].coordinate.z + iyy] = 0;
                                    }
                                    else if(layer_name_remove == "MiddleBuildPlane")
                                    {
                                        middle_occupation[voltage_system.electricity_connected_buildings[idx].coordinate.y + ixx, voltage_system.electricity_connected_buildings[idx].coordinate.z + iyy + 17] = 0;
                                    }
                                    else if(layer_name_remove == "TopBuildPlane")
                                    {
                                        top_occupation[voltage_system.electricity_connected_buildings[idx].coordinate.y + ixx + 13, voltage_system.electricity_connected_buildings[idx].coordinate.z + iyy + 16] = 0;
                                    }
                                }
                            }
                            PL.mat_library["Scrap"] += PL.scrap_requirement_library[voltage_system.electricity_connected_buildings[idx].Name] / 2;
                            PL.mat_library["Part"] += PL.part_requirement_library[voltage_system.electricity_connected_buildings[idx].Name] / 2; 
                            Destroy(voltage_system.electricity_connected_buildings[idx].gameObject);
                            voltage_system.electricity_connected_buildings.RemoveAt(idx);
                            return;
                        }
                    }
                }
            }
        }
        return;
    }
    private Vector3 GetSelectedMapPosition() //define a new structure
    {
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out MouseHit, 10000, placement_lay_mask))
        {
            grids.transform.position = MouseHit.transform.gameObject.transform.position;
            return MouseHit.point;
        }
        else
        {
            return new Vector3(0,0,0);
        }
    }
    private CastData CastCast() //define a new structure
    {
        CastData outcome = new CastData();
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out MouseHit, 10000, placement_lay_mask))
        {
            grids.transform.position = MouseHit.transform.gameObject.transform.position;
            outcome.position = MouseHit.point;
            outcome.casted = true;
            outcome.layer_name = MouseHit.transform.gameObject.name;
        }
        else
        {
            outcome.position = new Vector3(0,0,0);
            outcome.casted = false;
            outcome.layer_name = null;
        }
        return outcome;
    }

}

public class BuildingDataBaseSupply
{
    public List<Vector3> seat_positions;
    public List<Vector3> seat_orientations;
    public List<string> seat_pose;
    public int seat_num;
}

public class CastData
{
    public Vector3 position;
    public bool casted;
    public string layer_name;
}   