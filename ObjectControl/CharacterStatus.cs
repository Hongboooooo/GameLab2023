using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class CharacterStatus : MonoBehaviour
{
    // Start is called before the first frame update
    public Animator animator;
    public NavMeshAgent agent;
    private GameObject selected_effect;
    public Transform stand_point;
    private RaycastHit StandHit;
    private int character_walk_lay_mask;
    public string final_destination;
    public Vector3 final_destination_position;
    public Vector3 current_destination_position;
    public GameObject task_building;
    public GameObject former_task_building;
    public int is_on_position;
    public int former_position;
    public bool ready;
    private ObjectsDataBaseSO database;
    public string current_pose;
    private float timer;
    private float live_timer;
    private float die_timer;
    private bool no_death;
    public float health_current;
    public float health_max;
    public float focus_current;
    public float focus_max;
    public float hunger_current;
    public float hunger_max;
    private HealthBarControl hbc;
    [SerializeField] private GameObject hb;
    private Voltage vlt;
    private bool character_is_selected;
    private MouseControl mc;
    private ProductionLibrary PL;
    private CitizenCommander CC;
    public bool is_on_vacation;
    private float idle_timer = 0;
    private float vacation_timer = 0;
    public int next_rest_deploy = 0;
    public int rest_destination_id = -1;
    private int former_rest_destination_id = -1;
    public bool vacation_destination_decided;
    public GUIStyle cgs;
    private float r, g, b;
    // private CitizenCommander CC;
    public int ID;
    private List<bool> vacation_prior = new List<bool>();
    private List<bool> vacation_satis = new List<bool>();
    private List<int> vacation_proj = new List<int>();
    int offset = 30;
    public bool new_born = false;
    public bool is_eating = false;
    

    void Start()
    {
        animator = this.GetComponentInChildren<Animator>();
        agent = this.GetComponent<NavMeshAgent>();
        selected_effect = this.transform.Find("FootEffect").gameObject;
        hb = this.transform.Find("HealthBar").gameObject;
        hbc = hb.GetComponent<HealthBarControl>();

        end_vacation();

        stand_point = this.GetComponent<Transform>();
        character_walk_lay_mask = 1<<LayerMask.NameToLayer("TopLayer") | 1<<LayerMask.NameToLayer("MiddleLayer") | 1<<LayerMask.NameToLayer("GroundLayer") | 1<<LayerMask.NameToLayer("Elevator");

        vlt = GameObject.Find("Voltage").GetComponent<Voltage>();
        mc = GameObject.Find("SelectBox").GetComponent<MouseControl>();
        PL = GameObject.Find("InterfaceProductionLibrary").GetComponent<ProductionLibrary>();
        CC = GameObject.Find("CitizenCommander").GetComponent<CitizenCommander>();

        ID = -1;
        if(ID < 0)
        {
            CC.CitizenAnmeldung(this.GetComponent<CharacterStatus>());
        }

        cgs.fontSize = 15;
        r = 1; 
        g = 100f / 255f;
        b = 0;
        cgs.normal.textColor = new Color(r, g, b, 1);

        IsSelected(false);
        character_is_selected = false;
        task_building = null;
        former_task_building = null;
        is_on_position = -1;
        former_position = -1;
        ready = false;
        current_pose = null;
        no_death = true;
        
        health_max = 100;
        health_current = 100;
        focus_max = 100;
        focus_current = 100;
        hunger_max = 100;
        hunger_current = 100;

        vacation_proj.Add(0);
        vacation_proj.Add(6);
        vacation_proj.Add(1);
        vacation_proj.Add(9);
        vacation_proj.Add(3);

        vacation_prior.Add(false);
        vacation_prior.Add(false);
        vacation_prior.Add(false);
        vacation_prior.Add(false);
        vacation_prior.Add(false);

        // vacation_satis.Add(hunger_current >= hunger_max);
        // vacation_satis.Add(health_current >= health_max);
        // vacation_satis.Add(focus_current >= focus_max);
        // vacation_satis.Add(health_max >= 200);
        // vacation_satis.Add(focus_current >= 200);
    }
    // Update is called once per frame
    void Update()
    {   
        // if(new_born)
        // {
        //     // Debug.Log(this.transform.position);
        //     Debug.Log(task_building.GetComponent<BuildingStatus>().Name);
        // }
        
        if(health_current > 0)
        {
            live_timer += Time.deltaTime;
            if(live_timer >= 1)
            {
                if(!is_eating)
                {
                    HungerCurrentDec(PL.feel_hungry_speed);
                    if(hunger_current <= 0)
                    {
                        HealthUBDec(PL.health_bar_shrink_speed_when_hungry);
                        FocusUBDec(PL.focus_bar_shrink_speed_when_hungry);
                    }
                }
                
                // Debug.Log("is on vacation? " + is_on_vacation);
                
                live_timer = 0;
            }

            // Movement
            animator.SetBool("IsWalk", agent.velocity.magnitude >= 0.01f);
            if(animator.GetBool("IsWalk"))
            {
                if(!animator.GetBool("IsFar"))
                {
                    if((current_destination_position - stand_point.position).magnitude > 5)
                    {
                        animator.SetBool("IsFar", true);
                        agent.speed = 20;
                    }
                }
                else
                {
                    if((current_destination_position - stand_point.position).magnitude <= 5)
                    {
                        animator.SetBool("IsFar", false);
                        agent.speed = 3;
                    }
                }
                
            }            

            if(agent.velocity.magnitude <= 0.001f && task_building == null)
            {
                idle_timer += Time.deltaTime;
                if(idle_timer >= 3)
                {
                    start_vacation();

                    idle_timer = 0;
                }
            }
            else
            {
                idle_timer = 0;
            }

            if(is_on_vacation)
            {
                // vacation_timer += Time.deltaTime;
                // if(vacation_timer >= 3)
                // {
                //     start_vacation();

                //     vacation_timer = 0;
                // }
                have_a_rest();
            }

            // Occupy Seats
            if(task_building != null && is_on_position < 0)
            {
                if(!OccupiedSeat())
                {
                    // is_on_vacation = true;
                    start_vacation();
                }
                

            }
            
            // Start Using Seats
            if(agent.velocity.magnitude <= 0.001f && task_building != null && is_on_position >= 0 && !ready)
            {
                // Debug.Log("Start Using Seats");
                agent.enabled = false;
                animator.SetBool("IsFar", false);
                animator.SetBool(task_building.GetComponent<BuildingStatus>().seat_pose[is_on_position], true);
                current_pose = task_building.GetComponent<BuildingStatus>().seat_pose[is_on_position];
                stand_point.position = task_building.GetComponent<BuildingStatus>().seat_positions[is_on_position] + task_building.transform.position;
                stand_point.forward = task_building.GetComponent<BuildingStatus>().seat_orientations[is_on_position];
                ready = true;
    
            }


            
            // Effects From Buildings
            
            if(ready)
            {
                if(task_building != null)
                {
                    

                    if(task_building.GetComponent<BuildingStatus>().Name == "BioEG")
                    {
                        timer += Time.deltaTime;
                        if(timer >= 1)
                        {
                            HealthCurrentDec(PL.health_consume_speed_when_bioeg);
                            timer = 0;
                        }
                    }

                    if(task_building.GetComponent<BuildingStatus>().Name == "bed")
                    {
                        timer += Time.deltaTime;
                        if(timer >= 1)
                        {
                            FocueCurrentInc(PL.focus_recover_speed_when_bed);
                            timer = 0;
                        }
                    }

                    if(task_building.GetComponent<BuildingStatus>().Name == "Yoga")
                    {
                        if(hunger_current > 0)
                        {
                            timer += Time.deltaTime;
                            if(timer >= 1)
                            {
                                HealthUBInc(PL.health_bar_expand_speed_when_yoga);
                                timer = 0;
                            }
                        }
                        
                    }
                }
                else
                {
                    // Debug.Log("Lost Building!");
                    UpdateDeployment(null);
                    Move(stand_point.position);
                }
            }
            
        }
        else
        {
            if(no_death)
            {
                UpdateDeployment(null);
                Move(stand_point.position);
                animator.SetBool("IsDead", true);
                CC.CitizenABA(ID);
                no_death = false;
            }
            
            die_timer += Time.deltaTime;
            if(die_timer >= 10)
            {
                Destroy(this.gameObject);
                die_timer = 0;
            }

        }
    }

    public void Move(Vector3 destination)
    {
        if(no_death)
        {
            ready = false;
            agent.enabled = true;
            if(current_pose != null)
                animator.SetBool(current_pose, false);
            agent.SetDestination(destination);
            current_destination_position = destination;
        }

    }
    public void FlashMove(Vector3 destination)
    {
        agent.enabled = false;
        stand_point.position = destination;
        agent.enabled = true;
    }
    public Vector3 StandPosition()
    {
        return stand_point.position;
    }
    public string StandLayerName()
    {
        if(Physics.Raycast(stand_point.position, new Vector3(0,-1,0), out StandHit, 10, character_walk_lay_mask))
        {
            return StandHit.transform.gameObject.name;
        }
        else
        {
            return "empty";
        }
    }
    public void IsSelected(bool is_selected)
    {
        character_is_selected = is_selected;
        selected_effect.SetActive(is_selected);
        // hb.SetActive(is_selected);
    }

    public bool OccupiedSeat()
    {
        bool outcome = true;

        BuildingStatus bs = task_building.GetComponent<BuildingStatus>();
        if((stand_point.position - task_building.transform.position).magnitude <= bs.Size.x)
        {
            bool find_seat = false;
            if(bs.Name == "ParFac" || bs.Name == "AssFac" || bs.Name == "GunFac")
            {
                find_seat = TryFindEmptySeat(bs);
                if(!find_seat)
                {
                    find_seat = ReplaceTiredSeat(bs);
                }
                if(!find_seat)
                {
                    task_building = FindEmptyBuildings(bs.ID);
                    if(task_building == null)
                    {
                        task_building = FindTiredPeople(bs.ID);
                    }
                    if(task_building != null)
                    {
                        mc.building_deploy(this.GetComponent<CharacterStatus>(), task_building);
                    }
                    else
                    {
                        outcome = false;
                    }
                    is_on_position = -1;
                }
            }
            // else if(bs.Name == "BioEG")
            // {
            //     find_seat = TryFindEmptySeat(bs);
            //     if(!find_seat)
            //     {
            //         task_building = FindEmptyBuildings(bs.ID);
            //     }
            //     if(task_building == null)
            //     {
            //         find_seat = ReplaceInjuredSeat(bs);
            //         if(!find_seat)
            //         {
            //             task_building = FindInjuredPeople(bs.ID);
            //             is_on_position = -1;   
            //         }
            //         if(task_building != null)
            //         {
            //             mc.building_deploy(this.GetComponent<CharacterStatus>(), task_building);
            //         }
            //         else
            //         {
            //             outcome = false;
            //         }
            //     }
            //     else
            //     {
            //         mc.building_deploy(this.GetComponent<CharacterStatus>(), task_building);
            //         is_on_position = -1;
            //     }
            // }
            else
            {
                find_seat = TryFindEmptySeat(bs);
                if(!find_seat)
                {
                    task_building = FindEmptyBuildings(bs.ID);
                    if(task_building != null)
                    {
                        // Debug.Log("Found Alternative: " + task_building);
                        mc.building_deploy(this.GetComponent<CharacterStatus>(), task_building);
                    }
                    else
                    {
                        // start_vacation();
                        outcome = false;
                    }
                    is_on_position = -1;
                }
            }
            
        }

        return outcome;
    }

    public void have_a_rest()
    {
        vacation_prior.Clear();
        vacation_prior.Add(hunger_current < hunger_max * 0.3f);
        vacation_prior.Add(health_current < health_max);
        vacation_prior.Add(focus_current < focus_max);
        vacation_prior.Add(health_max < 200);
        vacation_prior.Add(focus_max < 200);

        if(!vacation_destination_decided)
        {
            // for(int idx = next_rest_deploy; idx < vacation_prior.Count; idx++)
            // {
            //     // int check_idx = (idx + next_rest_deploy) % vacation_prior.Count;
            //     if(vacation_prior[idx])
            //     {
            //         next_rest_deploy = (idx + 1) % vacation_prior.Count;
            //         vacation_destination_decided = true;
            //         rest_destination_id = vacation_proj[idx];

            //         break;
            //     }
            // }
            for(int idx = 0; idx < vacation_prior.Count; idx++)
            {
                int check_idx = (idx + next_rest_deploy) % vacation_prior.Count;
                if(vacation_prior[check_idx])
                {
                    next_rest_deploy = (check_idx + 1) % vacation_prior.Count;
                    vacation_destination_decided = true;
                    rest_destination_id = vacation_proj[check_idx];

                    break;
                }
            }

        }
        else if(vacation_destination_decided)
        {
            if(ready)
            {
                if(hunger_current <= 0)
                {
                    vacation_destination_decided = false;
                    next_rest_deploy = 0;
                }
                if(next_rest_deploy == 1)
                {
                    if(hunger_current >= hunger_max)
                    {
                        vacation_destination_decided = false;
                        next_rest_deploy = 0;
                    }
                }
                if(next_rest_deploy == 2)
                {
                    if(health_current >= health_max)
                    {
                        vacation_destination_decided = false;
                        next_rest_deploy = 0;
                    }
                }
                if(next_rest_deploy == 3)
                {
                    if(focus_current >= focus_max)
                    {
                        vacation_destination_decided = false;
                        next_rest_deploy = 0;
                    }
                }
                if(next_rest_deploy == 4)
                {
                    if(health_max >= 200)
                    {
                        vacation_destination_decided = false;
                        next_rest_deploy = 0;
                    }
                }
                if(next_rest_deploy == 0)
                {
                    if(focus_max >= 200)
                    {
                        vacation_destination_decided = false;
                        next_rest_deploy = 0;
                    }
                }
            }
            if(rest_destination_id != former_rest_destination_id)
            {
                if(FindEmptyBuildings(rest_destination_id) != null)
                {
                    mc.building_deploy(this, FindEmptyBuildings(rest_destination_id));
                    former_rest_destination_id = rest_destination_id;
                }
                else
                {
                    former_rest_destination_id = -1;
                    vacation_destination_decided = false;
                }
                
            }
        }
        
    }

    public void start_vacation()
    {
        is_on_vacation = true;
        former_rest_destination_id = -1;
        next_rest_deploy = 0;
        vacation_destination_decided = false;

        // vacation_prior.Clear();
        // vacation_prior.Add(hunger_current < hunger_max * 0.3f);
        // vacation_prior.Add(health_current < health_max);
        // vacation_prior.Add(focus_current < focus_max);
        // vacation_prior.Add(health_max < 200);
        // vacation_prior.Add(focus_current < 200);
    }

    public void end_vacation()
    {
        is_on_vacation = false;
        former_rest_destination_id = -1;
        next_rest_deploy = 0;
        vacation_destination_decided = false;
    }

    public void vacation_from_here(int vaca_id)
    {
        is_on_vacation = true;
        next_rest_deploy = (vacation_proj.FindIndex(x => x.Equals(vaca_id)) + 1) % vacation_prior.Count;
        former_rest_destination_id = vaca_id;
        rest_destination_id = former_rest_destination_id;
        vacation_destination_decided = true;
        // Debug.Log(former_rest_destination_id);

        // Debug.Log(vaca_id);

        vacation_prior.Clear();
        vacation_prior.Add(hunger_current < hunger_max * 0.3f);
        vacation_prior.Add(health_current < health_max);
        vacation_prior.Add(focus_current < focus_max);
        vacation_prior.Add(health_max < 200);
        vacation_prior.Add(focus_max < 200);
    }

    public void UpdateDeployment(GameObject new_deployment)
    {
        is_eating = false;
        if(task_building != null)
        {
            former_task_building = task_building;
            former_position = is_on_position;
        }
        task_building = new_deployment;
        is_on_position = -1;
        ReleaseSeat();
    }

    public void ReleaseSeat()
    {
        if(former_task_building != null && former_position >=0)
        {
            // Debug.Log("Release Seats");
            former_task_building.GetComponent<BuildingStatus>().seat_occupations[former_position] = false;
            former_task_building.GetComponent<BuildingStatus>().seat_members[former_position] = null;
            former_task_building = null;
            former_position = -1;
        }
    }

    private int FindBuildingData(GameObject search_name)
    {
        for(int idx = 0; idx < database.building_data.Count; idx++)
        {
            if(database.building_data[idx].Prefab == search_name)
            {
                return database.building_data[idx].ID;
            }
        }
        return -1;
    }


    private bool TryFindEmptySeat(BuildingStatus target_building)
    {
        bool find_seat = false;
        for(int idx = 0; idx < target_building.seat_occupations.Count; idx++)
        {
            if(!target_building.seat_occupations[idx])
            {
                Move(target_building.seat_positions[idx] + task_building.transform.position);
                target_building.seat_occupations[idx] = true;
                target_building.seat_members[idx] = this.GetComponent<CharacterStatus>();
                is_on_position = idx;
                find_seat = true;
                // Debug.Log("Work Building: " + task_building.name);
                break;
            }
        }
        return find_seat;
    }

    private bool ReplaceTiredSeat(BuildingStatus target_building)
    {
        bool find_seat = false;
        float tiredest = 200;
        int tiredest_id = 0;
        for(int idx = 0; idx < target_building.seat_occupations.Count; idx++)
        {
            if(target_building.seat_members[idx].focus_current < tiredest)
            {
                tiredest = target_building.seat_members[idx].focus_current;
                tiredest_id = idx;
            }
            
        }
        if(tiredest < this.focus_current)
        {
            // target_building.seat_members[idx].Move(target_building.seat_members[idx].StandPosition()+new Vector3(-1,0,-1));
            target_building.seat_members[tiredest_id].start_vacation();
            target_building.seat_members[tiredest_id].UpdateDeployment(null);
            
            Move(target_building.seat_positions[tiredest_id] + task_building.transform.position);
            target_building.seat_occupations[tiredest_id] = true;
            target_building.seat_members[tiredest_id] = this.GetComponent<CharacterStatus>();
            is_on_position = tiredest_id;
            find_seat = true;
            // Debug.Log("Work Building: " + task_building.name);
        }
        return find_seat;
    }

    private bool ReplaceInjuredSeat(BuildingStatus target_building)
    {
        bool find_seat = false;
        float strongest = 200;
        int strongest_id = 0;
        for(int idx = 0; idx < target_building.seat_occupations.Count; idx++)
        {
            if(target_building.seat_members[idx].health_current < strongest)
            {
                strongest = target_building.seat_members[idx].health_current;
                strongest_id = idx;
            }
            
        }
        if(strongest < this.health_current)
        {
            // target_building.seat_members[idx].Move(target_building.seat_members[idx].StandPosition()+new Vector3(-1,0,-1));
            target_building.seat_members[strongest_id].start_vacation();
            target_building.seat_members[strongest_id].UpdateDeployment(null);
            
            Move(target_building.seat_positions[strongest_id] + task_building.transform.position);
            target_building.seat_occupations[strongest_id] = true;
            target_building.seat_members[strongest_id] = this.GetComponent<CharacterStatus>();
            is_on_position = strongest_id;
            find_seat = true;
            // Debug.Log("Work Building: " + task_building.name);
        }
        return find_seat;
    }

    private GameObject FindEmptyBuildings(int building_id)
    {
        GameObject outcome = null;
        for(int idx = 0; idx < vlt.electricity_connected_buildings.Count; idx++)
        {
            if(vlt.electricity_connected_buildings[idx].ID == building_id)
            {
                if(!vlt.electricity_connected_buildings[idx].fully_occupied())
                {
                    // Debug.Log("need id: " + building_id + "found id: " + vlt.electricity_connected_buildings[idx].ID + "found building: " + vlt.electricity_connected_buildings[idx].ID);
                    if(outcome == null)
                    {
                        outcome = vlt.electricity_connected_buildings[idx].gameObject;
                    }
                    else
                    {
                        if((outcome.transform.position - this.transform.position).magnitude > (vlt.electricity_connected_buildings[idx].gameObject.transform.position - this.transform.position).magnitude)
                        {
                            outcome = vlt.electricity_connected_buildings[idx].gameObject;
                        }
                    }
                }
            }
        }
        return outcome;
    }

    private GameObject FindTiredPeople(int building_id)
    {
        GameObject outcome = null;
        for(int idx = 0; idx < vlt.electricity_connected_buildings.Count; idx++)
        {
            if(vlt.electricity_connected_buildings[idx].ID == building_id)
            {
                if(vlt.electricity_connected_buildings[idx].has_tired_people(this.focus_current))
                {
                    // Debug.Log("need id: " + building_id + "found id: " + vlt.electricity_connected_buildings[idx].ID + "found building: " + vlt.electricity_connected_buildings[idx].ID);
                    if(outcome == null)
                    {
                        outcome = vlt.electricity_connected_buildings[idx].gameObject;
                    }
                    else
                    {
                        if((outcome.transform.position - this.transform.position).magnitude > (vlt.electricity_connected_buildings[idx].gameObject.transform.position - this.transform.position).magnitude)
                        {
                            outcome = vlt.electricity_connected_buildings[idx].gameObject;
                        }
                    }
                }
            }
        }
        return outcome;
    }

    private GameObject FindInjuredPeople(int building_id)
    {
        GameObject outcome = null;
        for(int idx = 0; idx < vlt.electricity_connected_buildings.Count; idx++)
        {
            if(vlt.electricity_connected_buildings[idx].ID == building_id)
            {
                if(vlt.electricity_connected_buildings[idx].has_injured_people(this.health_current))
                {
                    // Debug.Log("need id: " + building_id + "found id: " + vlt.electricity_connected_buildings[idx].ID + "found building: " + vlt.electricity_connected_buildings[idx].ID);
                    if(outcome == null)
                    {
                        outcome = vlt.electricity_connected_buildings[idx].gameObject;
                    }
                    else
                    {
                        if((outcome.transform.position - this.transform.position).magnitude > (vlt.electricity_connected_buildings[idx].gameObject.transform.position - this.transform.position).magnitude)
                        {
                            outcome = vlt.electricity_connected_buildings[idx].gameObject;
                        }
                    }
                }
            }
        }
        return outcome;
    }
    private string pose_table(int pose_num)
    {
        if(pose_num == 1)
        {
            return "IsSit";
        }
        else if(pose_num == 2)
        {
            return "IsLay";
        }

        return "";
    }

    public void HungerCurrentInc(float iv)
    {
        hunger_current += iv;
        if(hunger_current > hunger_max)
        {
            hunger_current = hunger_max;
        }
        hbc.UpdateHungerBar(hunger_current, hunger_max);
    }

    public void HungerCurrentDec(float dv)
    {
        hunger_current -= dv;
        if(hunger_current < 0)
        {
            hunger_current = 0;
        }
        hbc.UpdateHungerBar(hunger_current, hunger_max);
    }

    private void FocueCurrentInc(float iv)
    {
        focus_current += iv;
        if(focus_current > focus_max)
            focus_current = focus_max;
        hbc.UpdateFocusBar(focus_current, focus_max);
    }

    public void FocueCurrentDec(float dv)
    {
        focus_current -= dv;
        if(focus_current < 0)
            focus_current = 0;
        hbc.UpdateFocusBar(focus_current, focus_max);
    }

    public void HealthCurrentInc(float iv)
    {
        health_current += iv;
        if(health_current > health_max)
            health_current = health_max;
        hbc.UpdateHealthBar(health_current, health_max);
    }

    public void HealthCurrentDec(float dv)
    {
        health_current -= dv;
        if(health_current < 0)
            health_current = 0;
        hbc.UpdateHealthBar(health_current, health_max);
    }

    private void HealthUBInc(float iv)
    {
        float original_health_max = health_max;
        health_max += iv;
        if(health_max > 200)
            health_max = 200;
        health_current = health_current * health_max / original_health_max;
        
        hbc.UpdateHealthBar(health_current, health_max);
    }

    public void FocusUBInc(float iv)
    {
        float original_focus_max = focus_max;
        focus_max += iv;
        if(focus_max > 200)
            focus_max = 200;
        focus_current = focus_current * focus_max / original_focus_max;
        
        hbc.UpdateFocusBar(focus_current, focus_max);
    }

    private void HealthUBDec(float dv)
    {
        float original_health_max = health_max;
        health_max -= dv;
        if(health_max <= 5)
            health_max = 5;
        health_current = health_current * health_max / original_health_max;
        
        hbc.UpdateHealthBar(health_current, health_max);
    }

    private void FocusUBDec(float dv)
    {
        float original_focus_max = focus_max;
        focus_max -= dv;
        if(focus_max <= 5)
            focus_max = 5;
        focus_current = focus_current * focus_max / original_focus_max;
        
        hbc.UpdateFocusBar(focus_current, focus_max);
    }
    
    private string BuildingIDtoName(int ID)
    {
        string outcome = "No";
        if(ID == 0)
        {
            outcome = "mensa";
        }
        if(ID == 6)
        {
            outcome = "medbay";
        }
        if(ID == 1)
        {
            outcome = "bed";
        }
        if(ID == 9)
        {
            outcome = "yoga";
        }
        if(ID == 3)
        {
            outcome = "bar";
        }
        return outcome;

    }

    // private void OnGUI()
    // {
    //     if(character_is_selected)
    //     {
    //         GUI.Label(new Rect(10,270+offset,100,20), "Is resting?: " + is_on_vacation, cgs);
    //         GUI.Label(new Rect(10,285+offset,100,20), "Rest Nr: " + BuildingIDtoName(rest_destination_id), cgs);
    //         GUI.Label(new Rect(10,300+offset,100,20), "Previous Rest Nr: " + BuildingIDtoName(former_rest_destination_id), cgs);
    //         GUI.Label(new Rect(10,315+offset,100,20), "Rest at mensa(0)?: " + vacation_prior[0], cgs);
    //         GUI.Label(new Rect(10,330+offset,100,20), "Rest at medbay(6)?: " + vacation_prior[1], cgs);
    //         GUI.Label(new Rect(10,345+offset,100,20), "Rest at bed(1)?: " + vacation_prior[2], cgs);
    //         GUI.Label(new Rect(10,360+offset,100,20), "Rest at yoga(9)?: " + vacation_prior[3], cgs);
    //         GUI.Label(new Rect(10,375+offset,100,20), "Rest at bar(3)?: " + vacation_prior[4], cgs);
    //         GUI.Label(new Rect(10,400+offset,100,20), "next_rest_deploy: " + next_rest_deploy, cgs);
    //         GUI.Label(new Rect(10,415+offset,100,20), "vacation_destination_decided: " + vacation_destination_decided, cgs);
    //     }
        
    // }
}

public class VacaD
{
    public bool deter;
    public int proj_id;
}
