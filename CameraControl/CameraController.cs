using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    public bool surface_look;
    private float move_speed = 0.2f;
    private float move_time = 5;
    private Vector3 zoom_towards = new Vector3(0,1,-1);
    private Vector3 new_postion;
    private Vector3 new_zoom;
    private float zoom_speed = 0.3f;
    private float zoom_time = 5;
    private Transform[] children;
    void Start()
    {
        children = GetComponentsInChildren<Transform>();
        new_postion = this.transform.position;
        new_zoom = children[1].localPosition;
        surface_look = true;
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }
    void HandleInput()
    {
        if(surface_look)
        {
            

            if(Input.GetKey(KeyCode.E))
            {
                if((new_zoom.y + zoom_towards.y) > -5)
                {
                    new_zoom -= zoom_towards * zoom_speed;
                }
                    
            }
            if(Input.GetKey(KeyCode.Q))
            {
                if((new_zoom.y + zoom_towards.y) < 80)
                {
                    new_zoom += zoom_towards * zoom_speed;
                }
                    
            }

            if(Input.GetKey(KeyCode.W))
            {
                new_postion += this.transform.forward * (new_zoom.y + 10) / 25 * move_speed;
            }
            if(Input.GetKey(KeyCode.S))
            {
                new_postion -= this.transform.forward * (new_zoom.y + 10) / 25 * move_speed;
            }
            if(Input.GetKey(KeyCode.A))
            {
                new_postion -= this.transform.right * (new_zoom.y + 10) / 25 * move_speed;
            }
            if(Input.GetKey(KeyCode.D))
            {
                new_postion += this.transform.right * (new_zoom.y + 10) / 25 * move_speed;
            }

            // Debug.Log(children[1].localPosition);
            children[1].localPosition = Vector3.Lerp(children[1].localPosition, new_zoom, Time.deltaTime * zoom_time);
            this.transform.position = Vector3.Lerp(this.transform.position, new_postion, Time.deltaTime * move_time);
        }
        else
        {

        }
        
    }
}
