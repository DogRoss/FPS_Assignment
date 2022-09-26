using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// Handles Ragdoll rig, used mainly for IK application.
/// </summary>
public class RigController : MonoBehaviour
{
    //General Rig
    [Header("Gemeral")]
    private Rig rig;
    private float targetWeight = 1f;
    //Aim Rig
    [Header("Head")]
    public Transform head;
    public Transform headLookTarget;
    public MultiAimConstraint headLook;
    private float targetAimWeight = 1f;

    //Arm IK
    [Header("Arms")]
    public TwoBoneIKConstraint lftArm;
    public Transform lftArmTarget;
    public TwoBoneIKConstraint rhtArm;
    public Transform rhtArmTarget;

    public float rigSmoothing = 10f;

    // Start is called before the first frame update
    void Start()
    {
        rig = GetComponent<Rig>();
        headLook = GetComponentInChildren<MultiAimConstraint>();
    }

    // Update is called once per frame
    void Update()
    {
        if (rig.enabled)
        {
            rig.weight = Mathf.Lerp(rig.weight, targetWeight, Time.deltaTime * rigSmoothing);
            headLook.weight = Mathf.Lerp(headLook.weight, targetAimWeight, Time.deltaTime * rigSmoothing);

            //Arm Ik
        }
    }

    public bool RigEnabled
    {
        get { return rig.enabled; }
        set
        {
            rig.enabled = value;
            if(value == false)
            {
                rig.weight = 0;
            }
        }
    }

    //Getters
    //Setters
    public void SetAimDirection(Vector3 dir)
    {
        headLookTarget.position = head.position + dir;
    }
    public void SetAimTargetPos(Vector3 pos)
    {
        headLookTarget.position = pos;
    }
    public void SetAimTargetPos(float x, float y, float z)
    {
        headLookTarget.position = new Vector3(x, y, z);
    }
    public void SetAimWeight(float weight)
    {
        targetAimWeight = weight;
    }
    public void SetAimWeightInstant(float weight)
    {
        targetAimWeight = weight;
        headLook.weight = weight;
    }
}
