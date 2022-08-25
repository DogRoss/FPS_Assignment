using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * inherits movement for movement variables
 * handles player interactions like weapons and worldspace interactions
 */

public class PlayerController : Movement
{
    public Transform gunObj;
    public Vector3 gunOffset;

    //camera rays
    Ray ray;
    RaycastHit hitData;

    public override void Start()
    {
        base.Start();
    }
    public override void Update()
    {
        base.Update();
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        gunObj.transform.position = cam.transform.position + gunOffset;

        ray.origin = cam.transform.position; ray.direction = cam.transform.forward;
        Physics.Raycast(ray, out hitData);
    }
}
