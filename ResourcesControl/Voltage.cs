using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voltage : MonoBehaviour
{
    // Start is called before the first frame update
    private float current_voltage_load;
    private float current_voltage;
    public float current_voltage_sat;
    public float electricity_efficiency;
    private float timer;
    private float defaut_focus_max = 100;
    public GUIStyle gs;
    public List<BuildingStatus> electricity_connected_buildings;
    void Start()
    {
        current_voltage_load = 0;
        current_voltage = 0;
        electricity_efficiency = 0;
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        electricity_efficiency = EfficiencyCalculation();
        current_voltage_sat = VPCalculation();
        // timer += Time.deltaTime;
        // if(timer >= 2)
        // {
        //     Debug.Log(EfficiencyCalculation());
        //     timer = 0;
        // }

        float cl = 0;
        float cv = 0;
        for(int idx = 0; idx < electricity_connected_buildings.Count; idx++)
        {
            if(electricity_connected_buildings[idx].Name == "BioEG")
            {
                // cv = cv + electricity_connected_buildings[idx].occupation_count();
                for(int m_idx = 0; m_idx < electricity_connected_buildings[idx].seat_occupations.Count; m_idx++)
                {
                    if(electricity_connected_buildings[idx].seat_occupations[m_idx])
                    {
                        if(electricity_connected_buildings[idx].seat_members[m_idx].health_current > 0)
                        cv += 1;
                    }
                    
                }
            }
            else if(electricity_connected_buildings[idx].Name == "bed")
            {

            }
            else if(electricity_connected_buildings[idx].Name == "Yoga")
            {

            }
            else if(electricity_connected_buildings[idx].Name == "AssFac")
            {
                if(electricity_connected_buildings[idx].production_name != "Stop")
                {
                    cl = cl + 1;
                }
            }
            else if(electricity_connected_buildings[idx].Name == "GunFac")
            {
                if(electricity_connected_buildings[idx].production_name != "Stop")
                {
                    cl = cl + 1;
                }
            }
            else if(electricity_connected_buildings[idx].Name == "ParFac")
            {
                if(electricity_connected_buildings[idx].production_name != "Stop")
                {
                    cl = cl + 1;
                }
            }
            else
            {
                cl = cl + 1;
            }
        }

        current_voltage_load = cl;
        current_voltage = cv;
    }

    // private void OnGUI()
    // {
    //     GUI.Label(new Rect(0,0,100,100), "Voltage: " + ((int)(current_voltage_sat * 100)).ToString() + "%", gs);
    // }

    private float EfficiencyCalculation()
    {
        float outcome = 1;
        if(current_voltage_load > 0)
        {
            outcome = current_voltage / current_voltage_load;
            if(outcome > 1)
            {
                outcome = 1;
            }
        }
        return outcome;
    }
    private float VPCalculation()
    {
        float outcome = 1;
        if(current_voltage_load > 0)
        {
            outcome = current_voltage / current_voltage_load;
            if(outcome > 1)
            {
                outcome = 1;
            }
        }
        return outcome;
    }
}
