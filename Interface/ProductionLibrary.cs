using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductionLibrary : MonoBehaviour
{
    public Dictionary<string, int> gun_library = new Dictionary<string, int>();
    public Dictionary<string, int> bot_library = new Dictionary<string, int>();
    public Dictionary<string, int> mat_library = new Dictionary<string, int>();
    public Dictionary<string, int> part_requirement_library = new Dictionary<string, int>();
    public Dictionary<string, int> scrap_requirement_library = new Dictionary<string, int>();
    public Dictionary<string, int> food_requirement_library = new Dictionary<string, int>();
    public Dictionary<string, float> bot_product_speed_map = new Dictionary<string, float>();
    public Dictionary<string, float> gun_product_speed_map = new Dictionary<string, float>();
    public float feel_hungry_speed;
    public float feed_speed;
    public float health_bar_shrink_speed_when_hungry;
    public float health_bar_expand_speed_when_yoga;
    public float health_recover_speed_when_medbay;
    public float health_consume_speed_when_bioeg;
    public float focus_bar_shrink_speed_when_hungry;
    public float focus_bar_expand_speed_when_bar;
    public float focus_recover_speed_when_bed;
    public float focus_consume_speed_when_work;
    public float part_produce_speed;
    public float child_birth_speed;
    public float parents_consume_speed;
    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        gun_library.Add("Pistol", 0);
        gun_library.Add("Assault Rifle", 0);
        gun_library.Add("Shotgun", 0);
        gun_library.Add("Long Rifle", 0);

        bot_library.Add("Harvester", 0);
        bot_library.Add("Light Combat Robot", 0);
        bot_library.Add("Middle Combat Robot", 0);
        bot_library.Add("Heavy Combat Robot", 0);

        mat_library.Add("Food", 1000);
        mat_library.Add("Scrap", 1000);
        mat_library.Add("Part", 100);

        part_requirement_library.Add("Harvester", 2);
        part_requirement_library.Add("Light Combat Robot", 2);
        part_requirement_library.Add("Middle Combat Robot", 6);
        part_requirement_library.Add("Heavy Combat Robot", 10);
        part_requirement_library.Add("Pistol", 1);
        part_requirement_library.Add("Assault Rifle", 2);
        part_requirement_library.Add("Shotgun", 2);
        part_requirement_library.Add("Long Rifle", 3);
        part_requirement_library.Add("AssFac", 20);
        part_requirement_library.Add("GunFac", 15);
        part_requirement_library.Add("Bar", 5);
        part_requirement_library.Add("MedBay", 10);

        scrap_requirement_library.Add("mensa", 8);
        scrap_requirement_library.Add("bed", 2);
        scrap_requirement_library.Add("AssFac", 40);
        scrap_requirement_library.Add("Bar", 20);
        scrap_requirement_library.Add("BioEG", 12);
        scrap_requirement_library.Add("GunFac", 35);
        scrap_requirement_library.Add("MedBay", 10);
        scrap_requirement_library.Add("ParFac", 20);
        scrap_requirement_library.Add("PopG", 30);
        scrap_requirement_library.Add("Yoga", 10);

        scrap_requirement_library.Add("Part", 1);

        food_requirement_library.Add("mensa", 1);
        food_requirement_library.Add("Bar", 1);

        bot_product_speed_map.Add("Harvester", 30f);
        bot_product_speed_map.Add("Light Combat Robot", 30f);
        bot_product_speed_map.Add("Middle Combat Robot", 20f);
        bot_product_speed_map.Add("Heavy Combat Robot", 10f);

        gun_product_speed_map.Add("Pistol", 60f);
        gun_product_speed_map.Add("Assault Rifle", 40f);
        gun_product_speed_map.Add("Shotgun", 40f);
        gun_product_speed_map.Add("Long Rifle", 20f);

        part_produce_speed = 30f;

        feel_hungry_speed = 0.2f;
        feed_speed = 10f;
        health_bar_shrink_speed_when_hungry = 1.5f;
        health_bar_expand_speed_when_yoga = 1;
        health_consume_speed_when_bioeg = 0.2f;
        health_recover_speed_when_medbay = 5f;
        focus_bar_shrink_speed_when_hungry = 1.5f;
        focus_bar_expand_speed_when_bar = 1;
        focus_consume_speed_when_work = 1f;
        focus_recover_speed_when_bed = 20f;

        child_birth_speed = 10;
        parents_consume_speed = 2.5f;

        part_requirement_library.Add("mensa", 0); // don't change
        part_requirement_library.Add("bed", 0); // don't change
        part_requirement_library.Add("BioEG", 0); // don't change
        part_requirement_library.Add("ParFac", 0); // don't change
        part_requirement_library.Add("PopG", 0); // don't change
        part_requirement_library.Add("Yoga", 0); // don't change

        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(timer >= 10)
        {
            if(bot_library["Harvester"] > 0)
            {
                mat_library["Food"] += 1*bot_library["Harvester"];
                mat_library["Scrap"] += 1*bot_library["Harvester"];
            }
            
            timer = 0;
        }
    }
}
