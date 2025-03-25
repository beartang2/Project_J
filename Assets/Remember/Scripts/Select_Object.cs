using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Select_Object : MonoBehaviour
{
    RaycastHit lr_hit;
    private GameObject hit_obj;
    private LineRenderer laser;

    private void Start()
    {
        laser = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        //Get_Pinch_Action();
    }

    /*
    public void Get_Pinch_Action()
    {
        if(OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
        {
            Controller();
        }
        else if(OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
        {
            if (hit_obj != null)
            {
                if(hit_obj.name.Contains("Button"))
                {
                   Rem_System_Manager.is_Start = true;
                }
                else if (hit_obj.GetComponent<Panel_Transform>())
                {
                    hit_obj.GetComponent<Panel_Transform>().Select_Pick_Function();
                }
                else
                {
                    hit_obj = null;
                }
            }
        }
        GetComponent<LineRenderer>().enabled = OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger);
    }

    private void Controller()
    {
        laser.SetPosition(0, transform.position);

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out lr_hit, 30))
        {
            hit_obj = lr_hit.transform.gameObject;
            laser.SetPosition(1, lr_hit.point);
        }
        else
        {
            hit_obj = null;
            laser.SetPosition(1, transform.TransformDirection(Vector3.forward) * 30);
        }
    }
    */
}

