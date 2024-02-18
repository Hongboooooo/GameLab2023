using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CitizenCommander : MonoBehaviour
{
    public List<CharacterStatus> citizen_list;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for(int idx = 0; idx < citizen_list.Count; idx++)
        {

        }
    }

    public CharacterStatus FindEnergeticPeople()
    {
        float energy_max = 1;
        int outcome = -1;
        for(int i=0;i<citizen_list.Count;i++)
        {
            if(citizen_list[i].is_on_vacation)
            {
                if(citizen_list[i].focus_current > energy_max)
                {
                    energy_max = citizen_list[i].focus_current;
                    outcome = i;
                }
            }
            
        }
        if(outcome >= 0)
            return citizen_list[outcome];
        else
            return null;
    }

    public void RecuitEnergeticPeople()
    {

    }

    public bool CitizenRegistered()
    {
        return false;
    }

    public void CitizenAnmeldung(CharacterStatus new_citizen)
    {
        if(new_citizen.ID >= 0)
        {
            Debug.LogError("People Already Registered!");
        }
        else
        {
            new_citizen.ID = citizen_list.Count;
            citizen_list.Add(new_citizen);
            // Debug.Log(citizen_list.Count);
        }
    }
    public void CitizenABA(int citizen_id)
    {
        for(int i=0;i<citizen_list.Count;i++)
        {
            if(citizen_list[i].ID == citizen_id)
            {
                citizen_list.RemoveAt(i);
                return;
            }
        }
    }
}
