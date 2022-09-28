using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// has list of required objects to be disabled to be able to interaction with the door connected to joint
/// </summary>
public class LockedDoor : MonoBehaviour
{
    public Rigidbody doorRB;
    public HingeJoint hJoint;
    public List<GameObject> requirements = new List<GameObject> ();

    // Start is called before the first frame update
    void Start()
    {
        hJoint = GetComponent<HingeJoint> ();
        doorRB = hJoint.connectedBody;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (CheckRequirements())
        {
            doorRB.isKinematic = false;
        }
    }

    bool CheckRequirements()
    {
        bool pass = true;

        foreach(GameObject requirement in requirements)
        {
            if (requirement.activeSelf)
            {
                pass = false;
                break;
            }
        }

        return pass;
    }
}
