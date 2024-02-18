using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BuildingStatus : MonoBehaviour
{

    public string Name;
    public int ID;
    public Vector2Int Size = Vector2Int.one;
    public List<Vector3> seat_positions;
    public List<Vector3> seat_orientations;
    public List<string> seat_pose;
    public List<bool> seat_occupations;
    public List<CharacterStatus> seat_members;
    public int seat_num;
    public Vector3Int coordinate;
    private bool initial_finish;
    private Voltage voltage_system;
    private ProductionLibrary PL;
    private bool building_is_selected;
    private GameObject progress_bar = null;
    public GUIStyle cgs;
    public GUIStyle cgss;
    public string production_name;
    private string former_production_name;
    private float r, g, b;
    private float current_progress;
    private float max_progress;
    private float live_timer;
    private ProgressBarControl pbc;
    private bool ready_to_produce;
    [SerializeField]
    private CharacterDBSO CDBSO;
    private PlacementSystem PS;
    private int offset_x;
    private int offset_y;
    private CitizenCommander CC;
    private MouseControl MC;
    private Animator building_animator = null;

    public bool sent_call_message = false;
    private float recuit_interval = 1f;

    // Start is called before the first frame update
    void Start()
    {
        

        initial_finish = false;
        building_is_selected = false;
        ready_to_produce = false;
        voltage_system = GameObject.Find("Voltage").GetComponent<Voltage>();
        voltage_system.electricity_connected_buildings.Add(this.GetComponent<BuildingStatus>());

        PL = GameObject.Find("InterfaceProductionLibrary").GetComponent<ProductionLibrary>();
        PS = GameObject.Find("PlacementSystem").GetComponent<PlacementSystem>();
        CC = GameObject.Find("CitizenCommander").GetComponent<CitizenCommander>();
        MC = GameObject.Find("SelectBox").GetComponent<MouseControl>();

        cgs.fontSize = 20;
        r = 1; 
        g = 100f / 255f;
        b = 0;
        cgs.normal.textColor = new Color(r, g, b, 1);
        cgss.fontSize = 20;
        r = 0; 
        g = 250f / 255f;
        b = 1;
        cgss.normal.textColor = new Color(r, g, b, 1);

        offset_x = 0;
        offset_y = 160;

        if(this.transform.Find("ProgressBar") != null)
        {
            progress_bar = this.transform.Find("ProgressBar").gameObject;
            progress_bar.GetComponent<Canvas>().enabled = false;
            pbc = progress_bar.GetComponent<ProgressBarControl>();
        }
        if(Name == "AssFac")
        {
            production_name = "Stop";
            former_production_name = "Stop";

            building_animator = this.GetComponent<Animator>();
            assfac_animation_switch(production_name);
            building_animator.speed = 0;
        }
        if(Name == "GunFac")
        {
            production_name = "Stop";
            former_production_name = "Stop";

            building_animator = this.GetComponent<Animator>();
            assfac_animation_switch(production_name);
            building_animator.speed = 0;
        }
        if(Name == "ParFac")
        {
            production_name = "Stop";
            former_production_name = "Stop";

            building_animator = this.GetComponent<Animator>();
            assfac_animation_switch(production_name);
            building_animator.speed = 0;
        }
        current_progress = 0;
        max_progress = 100;
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if(PS.build_start > 0)
        {
            // offset_y = 360;
            IsSelected(false);
        }
        else
        {
            // offset_y = 160;
        }

        if(seat_num > 0 && !initial_finish)
        {
            occupation_expand(seat_num);
            // Debug.Log(seat_num);
            initial_finish = true;
        }

        if(Name == "BioEG")
        {
            if(building_is_selected)
            {
                if(occupation_count() > 0)
                {
                    seat_members[0].GetComponent<CharacterStatus>().start_vacation();
                    seat_members[0].GetComponent<CharacterStatus>().UpdateDeployment(null);
                }
                // Debug.Log("don't die!");
                building_is_selected = false;
            }
        }

        if(Name == "mensa")
        {
            live_timer += Time.deltaTime;
            if(live_timer >= 1f)
            {
                for(int idx = 0; idx < seat_num; idx++)
                {
                    if(seat_members[idx] != null)
                    {
                        if(PL.mat_library["Food"] >= PL.food_requirement_library["mensa"] && voltage_system.electricity_efficiency > 0)
                        {
                            seat_members[idx].GetComponent<CharacterStatus>().is_eating = true;
                            if(seat_members[idx].GetComponent<CharacterStatus>().hunger_current < seat_members[idx].GetComponent<CharacterStatus>().hunger_max)
                            {
                                PL.mat_library["Food"] -= PL.food_requirement_library["mensa"];
                                seat_members[idx].GetComponent<CharacterStatus>().HungerCurrentInc(PL.feed_speed);
                            }
                            
                        }
                        else
                        {
                            seat_members[idx].GetComponent<CharacterStatus>().is_eating = false;
                        }
                    }
                    
                }

                live_timer = 0;
            }
            
        }

        if(Name == "Bar")
        {
            live_timer += Time.deltaTime;
            if(live_timer >= 1f)
            {
                for(int idx = 0; idx < seat_num; idx++)
                {
                    if(seat_members[idx] != null)
                    {
                        if(PL.mat_library["Food"] >= PL.food_requirement_library["Bar"] && voltage_system.electricity_efficiency > 0)
                        {
                            PL.mat_library["Food"] -= PL.food_requirement_library["Bar"];
                            seat_members[idx].GetComponent<CharacterStatus>().FocusUBInc(PL.focus_bar_expand_speed_when_bar);
                        }
                    }
                    
                }

                live_timer = 0;
            }
            
        }

        if(Name == "MedBay")
        {
            live_timer += Time.deltaTime;
            if(live_timer >= 1f)
            {
                for(int idx = 0; idx < seat_num; idx++)
                {
                    if(seat_members[idx] != null)
                    {
                        if(voltage_system.electricity_efficiency > 0)
                        {
                            seat_members[idx].GetComponent<CharacterStatus>().HealthCurrentInc(PL.health_recover_speed_when_medbay);
                        }
                        
                    }
                    
                }

                live_timer = 0;
            }
            
        }

        if(Name == "AssFac")
        {
            if(building_is_selected)
            {
                if(Input.GetKeyUp(KeyCode.Alpha1))
                {
                    production_name = "Harvester";
                    if(former_production_name != production_name)
                    {
                        ProgressCurrentClean();
                        if(former_production_name != null && ready_to_produce == true)
                        {
                            PL.mat_library["Part"] += PL.part_requirement_library[former_production_name];
                        }
                        ready_to_produce = false;
                        building_animator.speed = 0;
                        assfac_animation_switch(production_name);
                        
                    }
                    former_production_name = production_name;

                }
                if(Input.GetKeyUp(KeyCode.Alpha2))
                {
                    production_name = "Light Combat Robot";
                    if(former_production_name != production_name)
                    {
                        ProgressCurrentClean();
                        if(former_production_name != null && ready_to_produce == true)
                        {
                            PL.mat_library["Part"] += PL.part_requirement_library[former_production_name];
                        }
                        ready_to_produce = false;
                        building_animator.speed = 0;
                        assfac_animation_switch(production_name);
                        
                    }
                    former_production_name = production_name;

                }
                if(Input.GetKeyUp(KeyCode.Alpha3))
                {
                    production_name = "Middle Combat Robot";
                    if(former_production_name != production_name)
                    {
                        ProgressCurrentClean();
                        if(former_production_name != null && ready_to_produce == true)
                        {
                            PL.mat_library["Part"] += PL.part_requirement_library[former_production_name];
                        }
                        ready_to_produce = false;
                        building_animator.speed = 0;
                        assfac_animation_switch(production_name);
                        
                    }
                    former_production_name = production_name;

                }
                if(Input.GetKeyUp(KeyCode.Alpha4))
                {
                    production_name = "Heavy Combat Robot";
                    if(former_production_name != production_name)
                    {
                        ProgressCurrentClean();
                        if(former_production_name != null && ready_to_produce == true)
                        {
                            PL.mat_library["Part"] += PL.part_requirement_library[former_production_name];
                        }
                        ready_to_produce = false;
                        building_animator.speed = 0;
                        assfac_animation_switch(production_name);
                        
                    }
                    former_production_name = production_name;

                }
                if(Input.GetKeyUp(KeyCode.Alpha5))
                {
                    production_name = "Stop";
                    if(former_production_name != production_name)
                    {
                        ProgressCurrentClean();
                        if(former_production_name != null && ready_to_produce == true)
                        {
                            PL.mat_library["Part"] += PL.part_requirement_library[former_production_name];
                        }
                        ready_to_produce = false;
                        building_animator.speed = 0;
                        assfac_animation_switch(production_name);
                        
                    }
                    former_production_name = production_name;

                }
            }


            if(workers_efficiency() * voltage_system.electricity_efficiency > 0)// have labor and elec
            {
                
                assfac_animation_switch(production_name);
                if(current_progress == 0)
                {
                    if(!ready_to_produce) // didn't start new production
                    {
                        if(PL.bot_library.ContainsKey(production_name))
                        {
                            if(PL.mat_library["Part"] >= PL.part_requirement_library[production_name]) // have enough resource
                            {
                                PL.mat_library["Part"] -= PL.part_requirement_library[production_name];
                                ready_to_produce = true; // start to produce new production
                                if(building_animator.GetCurrentAnimatorClipInfo(0).Length > 0)
                                {
                                    building_animator.Play(building_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name,0,0f);
                                }
                                // Debug.Log("reset!");
                                building_animator.speed = 0;

                            }
                        }
                        
                    }
                    
                }


                live_timer += Time.deltaTime;
                if(live_timer >= 1f)
                {
                    if(current_progress == max_progress) // finish production
                    {
                        // Debug.Log(building_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name);
                        ProgressCurrentClean();
                        if(PL.bot_library.ContainsKey(production_name))
                            PL.bot_library[production_name] += 1;
                        ready_to_produce = false;
                        
                    }

                    if(ready_to_produce) // start to produce new production
                    {   
                        building_animator.speed = 0.6f * workers_efficiency() * voltage_system.electricity_efficiency;
                        // ProgressCurrentInc(workers_efficiency() * voltage_system.electricity_efficiency * PL.bot_product_speed_map[production_name]); // progress bar increase
                        if(building_animator.GetCurrentAnimatorClipInfo(0).Length > 0)
                        {   
                            ProgressCurrentInc((99f / building_animator.GetCurrentAnimatorClipInfo(0)[0].clip.length) * building_animator.speed);
                        }
                        
                        for(int idx = 0; idx < seat_num; idx++)
                        {
                            if(seat_members[idx] != null)
                            {
                                seat_members[idx].GetComponent<CharacterStatus>().FocueCurrentDec(PL.focus_consume_speed_when_work); // people consume energy
                                
                            }
                            
                        }
                    }
                        
                    live_timer = 0;
                }
            }
            else
            {
                // assfac_animation_switch("");
                building_animator.speed = 0;
            }

            if(production_name != "Stop")
            {
                if(fully_occupied() && tired_people_count(0f) == 0)
                {
                    sent_call_message = false;
                }
                if(!fully_occupied())
                {
                    if(!sent_call_message)
                    {
                        Debug.Log("Should Recuit More");
                        for(int idx = 0; idx < (seat_num - occupation_count()); idx++)
                        {
                            CharacterStatus free_pr = CC.FindEnergeticPeople();
                            if(free_pr != null)
                            {
                                free_pr.is_on_vacation = false;
                                MC.building_deploy(free_pr, this.gameObject);
                            }

                        }
                        sent_call_message = true;
                    }
                }
                if(tired_people_count(0f) > 0)
                {
                    
                    if(!sent_call_message)
                    {
                        Debug.Log("Should Replace Tired");
                        for(int idx = 0; idx < tired_people_count(0f); idx++)
                        {
                            CharacterStatus free_pr = CC.FindEnergeticPeople();
                            if(free_pr != null)
                            {
                                free_pr.is_on_vacation = false;
                                MC.building_deploy(free_pr, this.gameObject);
                            }
                            
                        }
                        sent_call_message = true;
                    }
                }
                if(!fully_occupied() || tired_people_count(0f) > 0)
                {
                    recuit_interval += Time.deltaTime;
                    if(recuit_interval >= 5f)
                    {
                        recuit_interval = 0;
                        sent_call_message = false;
                    }
                }
                
            }
            else if(production_name == "Stop")
            {
                for(int idx = 0; idx < seat_members.Count; idx++)
                {
                    if(seat_members[idx] != null)
                    {
                        seat_members[idx].GetComponent<CharacterStatus>().start_vacation();
                        seat_members[idx].GetComponent<CharacterStatus>().UpdateDeployment(null);
                    }
                        
                }
            }
        }

        if(Name == "GunFac")
        {
            if(building_is_selected)
            {
                if(Input.GetKeyUp(KeyCode.Alpha1))
                {
                    production_name = "Pistol";
                    if(former_production_name != production_name)
                    {
                        ProgressCurrentClean();
                        if(former_production_name != null && ready_to_produce == true)
                        {
                            PL.mat_library["Part"] += PL.part_requirement_library[former_production_name];
                        }
                        ready_to_produce = false;
                        building_animator.speed = 0;
                        gunfac_animation_switch(production_name);
                    }
                    former_production_name = production_name;
                }
                if(Input.GetKeyUp(KeyCode.Alpha2))
                {
                    production_name = "Assault Rifle";
                    if(former_production_name != production_name)
                    {
                        ProgressCurrentClean();
                        if(former_production_name != null && ready_to_produce == true)
                        {
                            PL.mat_library["Part"] += PL.part_requirement_library[former_production_name];
                        }
                        ready_to_produce = false;
                        building_animator.speed = 0;
                        gunfac_animation_switch(production_name);
                    }
                    former_production_name = production_name;
                }
                if(Input.GetKeyUp(KeyCode.Alpha3))
                {
                    production_name = "Shotgun";
                    if(former_production_name != production_name)
                    {
                        ProgressCurrentClean();
                        if(former_production_name != null && ready_to_produce == true)
                        {
                            PL.mat_library["Part"] += PL.part_requirement_library[former_production_name];
                        }
                        ready_to_produce = false;
                        building_animator.speed = 0;
                        gunfac_animation_switch(production_name);
                    }
                    former_production_name = production_name;
                }
                if(Input.GetKeyUp(KeyCode.Alpha4))
                {
                    production_name = "Long Rifle";
                    if(former_production_name != production_name)
                    {
                        ProgressCurrentClean();
                        if(former_production_name != null && ready_to_produce == true)
                        {
                            PL.mat_library["Part"] += PL.part_requirement_library[former_production_name];
                        }
                        ready_to_produce = false;
                        building_animator.speed = 0;
                        gunfac_animation_switch(production_name);
                    }
                    former_production_name = production_name;
                }
                if(Input.GetKeyUp(KeyCode.Alpha5))
                {
                    production_name = "Stop";
                    if(former_production_name != production_name)
                    {
                        ProgressCurrentClean();
                        if(former_production_name != null && ready_to_produce == true)
                        {
                            PL.mat_library["Part"] += PL.part_requirement_library[former_production_name];
                        }
                        ready_to_produce = false;
                        building_animator.speed = 0;
                        gunfac_animation_switch(production_name);
                    }
                    former_production_name = production_name;
                }
            }

            if(workers_efficiency() * voltage_system.electricity_efficiency > 0)
            {
                gunfac_animation_switch(production_name);
                if(current_progress == 0)
                {
                    if(!ready_to_produce)
                    {
                        if(PL.gun_library.ContainsKey(production_name))
                        {
                            if(PL.mat_library["Part"] >= PL.part_requirement_library[production_name])
                            {
                                PL.mat_library["Part"] -= PL.part_requirement_library[production_name];
                                ready_to_produce = true;

                                if(building_animator.GetCurrentAnimatorClipInfo(0).Length > 0)
                                {
                                    building_animator.Play(building_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name,0,0f);
                                }
                                // Debug.Log("reset!");
                                building_animator.speed = 0;
                            }
                        }
                        
                    }

                }
                live_timer += Time.deltaTime;
                if(live_timer >= 1f)
                {
                    if(current_progress == max_progress)
                    {
                        ProgressCurrentClean();
                        if(PL.gun_library.ContainsKey(production_name))
                            PL.gun_library[production_name] += 1;
                        ready_to_produce = false;
                    }
                    if(ready_to_produce)
                    {
                        building_animator.speed = 1.5f * workers_efficiency() * voltage_system.electricity_efficiency;
                        // ProgressCurrentInc(workers_efficiency() * voltage_system.electricity_efficiency * PL.bot_product_speed_map[production_name]); // progress bar increase
                        if(building_animator.GetCurrentAnimatorClipInfo(0).Length > 0)
                        {   
                            ProgressCurrentInc((99f / building_animator.GetCurrentAnimatorClipInfo(0)[0].clip.length) * building_animator.speed);
                        }

                        for(int idx = 0; idx < seat_num; idx++)
                        {
                            if(seat_members[idx] != null)
                            {
                                seat_members[idx].GetComponent<CharacterStatus>().FocueCurrentDec(PL.focus_consume_speed_when_work);
                                
                            }
                            
                        }
                        
                        
                    }
                        

                    live_timer = 0;
                }
            }
            else
            {
                building_animator.speed = 0;
            }
        
            if(production_name != "Stop")
            {
                if(fully_occupied() && tired_people_count(0f) == 0)
                {
                    sent_call_message = false;
                }
                if(!fully_occupied())
                {
                    if(!sent_call_message)
                    {
                        Debug.Log("Should Recuit More");
                        for(int idx = 0; idx < (seat_num - occupation_count()); idx++)
                        {
                            CharacterStatus free_pr = CC.FindEnergeticPeople();
                            if(free_pr != null)
                            {
                                free_pr.is_on_vacation = false;
                                MC.building_deploy(free_pr, this.gameObject);
                            }

                        }
                        sent_call_message = true;
                    }
                }
                if(tired_people_count(0f) > 0)
                {
                    
                    if(!sent_call_message)
                    {
                        Debug.Log("Should Replace Tired");
                        for(int idx = 0; idx < tired_people_count(0f); idx++)
                        {
                            CharacterStatus free_pr = CC.FindEnergeticPeople();
                            if(free_pr != null)
                            {
                                free_pr.is_on_vacation = false;
                                MC.building_deploy(free_pr, this.gameObject);
                            }
                            
                        }
                        sent_call_message = true;
                    }
                }
                if(!fully_occupied() || tired_people_count(0f) > 0)
                {
                    recuit_interval += Time.deltaTime;
                    if(recuit_interval >= 5f)
                    {
                        recuit_interval = 0;
                        sent_call_message = false;
                    }
                }
                
            }
            else if(production_name == "Stop")
            {
                for(int idx = 0; idx < seat_members.Count; idx++)
                {
                    if(seat_members[idx] != null)
                    {
                        seat_members[idx].GetComponent<CharacterStatus>().start_vacation();
                        seat_members[idx].GetComponent<CharacterStatus>().UpdateDeployment(null);
                    }
                        
                }
            }
        }

        if(Name == "ParFac")
        {
            if(building_is_selected)
            {
                if(Input.GetKeyUp(KeyCode.Alpha1))
                {
                    production_name = "Part";
                    if(former_production_name != production_name)
                    {
                        ProgressCurrentClean();
                        // if(former_production_name = "Stop")
                        // {
                        //     if(PL.scrap_requirement_library.ContainsKey(former_production_name))
                        //         PL.mat_library["Scrap"] += PL.scrap_requirement_library[former_production_name];
                        // }
                        ready_to_produce = false;
                        building_animator.speed = 0;
                        parfac_animation_switch(production_name);
                    }
                    former_production_name = production_name;
                }
                if(Input.GetKeyUp(KeyCode.Alpha2))
                {
                    production_name = "Stop";
                    if(former_production_name != production_name)
                    {
                        ProgressCurrentClean();
                        if(former_production_name == "Part"&& ready_to_produce == true)
                        {
                            // if(PL.scrap_requirement_library.ContainsKey(former_production_name))
                            PL.mat_library["Scrap"] += PL.scrap_requirement_library[former_production_name];
                        }
                        ready_to_produce = false;
                        building_animator.speed = 0;
                        parfac_animation_switch(production_name);
                    }
                    former_production_name = production_name;
                    sent_call_message = false;
                }
            }
            if(workers_efficiency() * voltage_system.electricity_efficiency > 0)
            {
                parfac_animation_switch(production_name);
                if(!ready_to_produce)
                {
                    if(PL.scrap_requirement_library.ContainsKey(production_name))
                    {
                        if(PL.mat_library["Scrap"] >= PL.scrap_requirement_library[production_name])
                        {
                            PL.mat_library["Scrap"] -= PL.scrap_requirement_library[production_name];
                            ready_to_produce = true;
                            if(building_animator.GetCurrentAnimatorClipInfo(0).Length > 0)
                            {
                                building_animator.Play(building_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name,0,0f);
                            }
                            Debug.Log("reset!");
                            building_animator.speed = 0;
                        }
                    }

                }


                live_timer += Time.deltaTime;
                if(live_timer >= 1f)
                {
                    if(current_progress == max_progress)
                    {
                        ProgressCurrentClean();
                        PL.mat_library["Part"] += 1;
                        ready_to_produce = false;
                    }
                    if(ready_to_produce)
                    {
                        building_animator.speed = 1.2f * workers_efficiency() * voltage_system.electricity_efficiency;
                        if(building_animator.GetCurrentAnimatorClipInfo(0).Length > 0)
                        {   
                            ProgressCurrentInc((99f / building_animator.GetCurrentAnimatorClipInfo(0)[0].clip.length) * building_animator.speed);
                        }
                        // ProgressCurrentInc(workers_efficiency() * voltage_system.electricity_efficiency * PL.part_produce_speed);
                        for(int idx = 0; idx < seat_num; idx++)
                        {
                            if(seat_members[idx] != null)
                            {
                                seat_members[idx].GetComponent<CharacterStatus>().FocueCurrentDec(PL.focus_consume_speed_when_work);
                                
                            }
                            
                        }
                        
                    }
                        

                    live_timer = 0;
                }
            }
            else
            {
                building_animator.speed = 0;
            }
        
            if(production_name != "Stop")
            {
                if(fully_occupied() && tired_people_count(0f) == 0)
                {
                    sent_call_message = false;
                }
                if(!fully_occupied())
                {
                    if(!sent_call_message)
                    {
                        Debug.Log("Should Recuit More");
                        for(int idx = 0; idx < (seat_num - occupation_count()); idx++)
                        {
                            CharacterStatus free_pr = CC.FindEnergeticPeople();
                            if(free_pr != null)
                            {
                                free_pr.is_on_vacation = false;
                                MC.building_deploy(free_pr, this.gameObject);
                            }

                        }
                        sent_call_message = true;
                    }
                }
                if(tired_people_count(0f) > 0)
                {
                    
                    if(!sent_call_message)
                    {
                        Debug.Log("Should Replace Tired");
                        for(int idx = 0; idx < tired_people_count(0f); idx++)
                        {
                            CharacterStatus free_pr = CC.FindEnergeticPeople();
                            if(free_pr != null)
                            {
                                free_pr.is_on_vacation = false;
                                MC.building_deploy(free_pr, this.gameObject);
                            }
                            
                        }
                        sent_call_message = true;
                    }
                }
                if(!fully_occupied() || tired_people_count(0f) > 0)
                {
                    recuit_interval += Time.deltaTime;
                    if(recuit_interval >= 5f)
                    {
                        recuit_interval = 0;
                        sent_call_message = false;
                    }
                }
                
            }
            else if(production_name == "Stop")
            {
                for(int idx = 0; idx < seat_members.Count; idx++)
                {
                    if(seat_members[idx] != null)
                    {
                        seat_members[idx].GetComponent<CharacterStatus>().start_vacation();
                        seat_members[idx].GetComponent<CharacterStatus>().UpdateDeployment(null);
                    }
                        
                }
            }
        }

        if(Name == "PopG")
        {
            if(occupation_count() == seat_num && voltage_system.electricity_efficiency > 0)
            {
                if(!ready_to_produce)
                {
                    ready_to_produce = true;
                    
                }

                live_timer += Time.deltaTime;
                if(live_timer >= 1f)
                {
                    if(current_progress == max_progress) //birth success
                    {
                        GameObject new_object = Instantiate(CDBSO.CDB[0].Prefab);
                        new_object.transform.position = this.transform.position + new Vector3(1,0.1f,1);
                        // Debug.Log(this.transform.position + new Vector3(1,0.1f,1));
                        new_object.transform.forward =  new Vector3(-1,0,-1);
                        // Debug.Log(new_object.transform.position);
                        new_object.GetComponent<CharacterStatus>().new_born = true;
                        new_object.GetComponent<NavMeshAgent>().enabled = false;
                        new_object.GetComponent<NavMeshAgent>().enabled = true;

                        ProgressCurrentClean();
                        ready_to_produce = false;
                    }
                    if(ready_to_produce) //birth in progress
                    {
                        ProgressCurrentInc(PL.child_birth_speed);
                        for(int idx = 0; idx < seat_num; idx++)
                        {
                            if(seat_members[idx] != null)
                            {
                                seat_members[idx].GetComponent<CharacterStatus>().HungerCurrentDec(PL.parents_consume_speed);
                                if(seat_members[idx].hunger_current <= 0)
                                {
                                    seat_members[idx].HealthCurrentDec(PL.parents_consume_speed);
                                }
                                seat_members[idx].GetComponent<CharacterStatus>().animator.SetBool("IsSuffer", true);
                                seat_members[idx].GetComponent<CharacterStatus>().current_pose = "IsSuffer";
                            }
                            
                        }
                    }
                    else
                    {
                        for(int idx = 0; idx < seat_num; idx++)
                        {
                            if(seat_members[idx] != null)
                            {
                                seat_members[idx].GetComponent<CharacterStatus>().animator.SetBool("IsSuffer", false);
                            }
                            
                        } 
                    }
                    
                    live_timer = 0;
                }
                        
            }
            else
            {
                for(int idx = 0; idx < seat_num; idx++)
                {
                    if(seat_members[idx] != null)
                    {
                        seat_members[idx].GetComponent<CharacterStatus>().animator.SetBool("IsSuffer", false);
                    }
                    
                } 
                ProgressCurrentClean(); //birth fall
            }
        }
    }

    private void assfac_animation_switch(string production_name)
    {
        if(production_name == "Heavy Combat Robot")
        {
            building_animator.SetBool("IsProducingHVT", false);
            building_animator.SetBool("IsProducingMCR", false);
            building_animator.SetBool("IsProducingHCR", true);
        }
        else if(production_name == "Middle Combat Robot")
        {
            building_animator.SetBool("IsProducingHVT", false);
            building_animator.SetBool("IsProducingMCR", true);
            building_animator.SetBool("IsProducingHCR", false);
        }
        else if(production_name == "Light Combat Robot")
        {
            building_animator.SetBool("IsProducingHVT", true);
            building_animator.SetBool("IsProducingMCR", false);
            building_animator.SetBool("IsProducingHCR", false);
        }
        else if(production_name == "Harvester")
        {
            building_animator.SetBool("IsProducingHVT", true);
            building_animator.SetBool("IsProducingMCR", false);
            building_animator.SetBool("IsProducingHCR", false);
        }
        else
        {
            building_animator.SetBool("IsProducingHVT", false);
            building_animator.SetBool("IsProducingMCR", false);
            building_animator.SetBool("IsProducingHCR", false);
        }
        
    }

    private void parfac_animation_switch(string production_name)
    {
        if(production_name == "Part")
        {
            building_animator.SetBool("IsProducingPart", true);
        }
        else
        {
            building_animator.SetBool("IsProducingPart", false);
        }
    }

    private void gunfac_animation_switch(string production_name)
    {
        if(production_name != "Stop")
        {
            building_animator.SetBool("IsProducingAR", true);
        }
        else
        {
            building_animator.SetBool("IsProducingAR", false);
        }
    }

    private void occupation_expand(int num)
    {
        for(int idx = 0; idx < num; idx++)
        {
            seat_occupations.Add(false);
            seat_members.Add(null);
        }
    }

    public int occupation_count()
    {
        int outcome = 0;

        for(int idx = 0; idx < seat_occupations.Count; idx++)
        {
            if(seat_occupations[idx])
            {
                outcome++;
            }
        }

        return outcome;
    }

    public float workers_efficiency()
    {
        float outcome = 0;

        for(int idx = 0; idx < seat_occupations.Count; idx++)
        {
            if(seat_occupations[idx])
            {
                if(seat_members[idx].focus_current > 0)
                    outcome += seat_members[idx].focus_max;
            }
        }
        
        return outcome/100;
    }

    public bool fully_occupied()
    {
        int occupied_num = 0;

        for(int idx = 0; idx < seat_occupations.Count; idx++)
        {
            if(seat_occupations[idx])
            {
                occupied_num++;
            }
        }

        if(occupied_num < seat_occupations.Count)
            return false;
        else
            return true;
    }

    public bool has_tired_people(float relative_tired)
    {
        int tired_num = 0;

        for(int idx = 0; idx < seat_occupations.Count; idx++)
        {
            if(seat_occupations[idx])
            {
                if(seat_members[idx].focus_current < relative_tired)
                {
                    tired_num += 1;
                }
            }
        }

        if(tired_num > 0)
            return true;
        else
            return false;
    }

    public int tired_people_count(float relative_tired)
    {
        int tired_num = 0;

        for(int idx = 0; idx < seat_occupations.Count; idx++)
        {
            if(seat_occupations[idx])
            {
                if(seat_members[idx].focus_current <= relative_tired)
                {
                    tired_num += 1;
                }
            }
        }

        return tired_num;
    }

    public bool has_injured_people(float relative_injured)
    {
        int injured_num = 0;

        for(int idx = 0; idx < seat_occupations.Count; idx++)
        {
            if(seat_occupations[idx])
            {
                if(seat_members[idx].health_current < relative_injured)
                {
                    injured_num += 1;
                }
            }
        }

        if(injured_num > 0)
            return true;
        else
            return false;
    }

    public void IsSelected(bool is_selected)
    {
        building_is_selected = is_selected;
        if(progress_bar != null)
        {
            progress_bar.GetComponent<Canvas>().enabled = is_selected;
        }
        
    }

    private void ProgressCurrentInc(float iv)
    {
        current_progress += iv;
        if(current_progress > max_progress)
            current_progress = max_progress;
        pbc.UpdateProgressBar(current_progress, max_progress);
    }
    private void ProgressCurrentClean()
    {
        current_progress = 0;
        pbc.UpdateProgressBar(current_progress, max_progress);
    }

    private void DrawResource(int x, int y, int resource_type, string unit_name)
    {
        if(resource_type == 0)
        {
            GUI.Label(new Rect(x, y + 3,100,20), "SCP:", PS.cgsgs);
            if(PL.mat_library["Scrap"] >= PL.scrap_requirement_library[unit_name])
            {
                GUI.Label(new Rect(x + 40, y + 3,100,20), PL.mat_library["Scrap"] + "/" +PL.scrap_requirement_library[unit_name], PS.cgsgs);
            }
            else
            {
                GUI.Label(new Rect(x + 40, y + 3,100,20), PL.mat_library["Scrap"] + "/" +PL.scrap_requirement_library[unit_name], PS.cgsrs);
            }
        }
        else if(resource_type == 1)
        {
            GUI.Label(new Rect(x, y + 3,100,20), "PRT:", PS.cgsgs);
            if(PL.mat_library["Part"] >= PL.part_requirement_library[unit_name])
            {
                GUI.Label(new Rect(x + 40, y + 3,100,20), PL.mat_library["Part"] + "/" +PL.part_requirement_library[unit_name], PS.cgsgs);
            }
            else
            {
                GUI.Label(new Rect(x + 40, y + 3,100,20), PL.mat_library["Part"] + "/" +PL.part_requirement_library[unit_name], PS.cgsrs);
            }
        }

    }

    private void OnGUI()
    {
        if(building_is_selected)
        {
            if(Name == "AssFac")
            {
                // PS.StopPlacement();
                GUI.DrawTexture(new Rect(offset_x,offset_y,450,130),PS.DisplayBack);
                GUI.Label(new Rect(offset_x,offset_y,100,20), "Current Production: ", cgs);
                GUI.Label(new Rect(offset_x+190,offset_y,100,20), production_name, cgss);
                GUI.Label(new Rect(offset_x,offset_y + 20,100,20), "Press 1 to produce HVT", cgs);
                DrawResource(offset_x + 250, offset_y + 20, 1, "Harvester");
                GUI.Label(new Rect(offset_x,offset_y + 40,100,20), "Press 2 to produce LCR", cgs);
                DrawResource(offset_x + 250, offset_y + 40, 1, "Light Combat Robot");
                GUI.Label(new Rect(offset_x,offset_y + 60,100,20), "Press 3 to produce MCR", cgs);
                DrawResource(offset_x + 250, offset_y + 60, 1, "Middle Combat Robot");
                GUI.Label(new Rect(offset_x,offset_y + 80,100,20), "Press 4 to produce HCR", cgs);
                DrawResource(offset_x + 250, offset_y + 80, 1, "Heavy Combat Robot");
                GUI.Label(new Rect(offset_x,offset_y + 100,100,20), "Press 5 to stop production", cgs);
            }

            if(Name == "GunFac")
            {
                // PS.StopPlacement();
                GUI.DrawTexture(new Rect(offset_x,offset_y,450,130),PS.DisplayBack);
                GUI.Label(new Rect(offset_x,offset_y,100,20), "Current Production: ", cgs);
                GUI.Label(new Rect(offset_x + 190,offset_y,100,20), production_name, cgss);
                GUI.Label(new Rect(offset_x,offset_y + 20,100,20), "Press 1 to produce PST", cgs);
                DrawResource(offset_x + 250, offset_y + 20, 1, "Pistol");
                GUI.Label(new Rect(offset_x,offset_y + 40,100,20), "Press 2 to produce ARF", cgs);
                DrawResource(offset_x + 250, offset_y + 40, 1, "Assault Rifle");
                GUI.Label(new Rect(offset_x,offset_y + 60,100,20), "Press 3 to produce STG", cgs);
                DrawResource(offset_x + 250, offset_y + 60, 1, "Shotgun");
                GUI.Label(new Rect(offset_x,offset_y + 80,100,20), "Press 4 to produce LRF", cgs);
                DrawResource(offset_x + 250, offset_y + 80, 1, "Long Rifle");
                GUI.Label(new Rect(offset_x,offset_y + 100,100,20), "Press 5 to stop production", cgs);
            }
            if(Name == "ParFac")
            {
                // PS.StopPlacement();
                GUI.DrawTexture(new Rect(offset_x,offset_y,450,120),PS.DisplayBack);
                GUI.Label(new Rect(offset_x,offset_y,100,20), "Current Production: ", cgs);
                GUI.Label(new Rect(offset_x + 190,offset_y,100,20), production_name, cgss);
                GUI.Label(new Rect(offset_x,offset_y + 20,100,20), "Press 1 to produce PRT", cgs);
                DrawResource(offset_x + 240, offset_y + 20, 0, "Part");
                GUI.Label(new Rect(offset_x,offset_y + 40,100,20), "Press 2 to stop production", cgs);
                
            }
            
        }
        
    }
}
