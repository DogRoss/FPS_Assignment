using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls whether or not ragdoll is active or not using a RigController and Animator.
/// </summary>
[RequireComponent(typeof(Animator))]
public class RagdollController : MonoBehaviour
{
    private Animator animator;
    private RigController rigController;
    private List<Rigidbody> rigidbodies = new List<Rigidbody>();
    private List<Collider> colliders = new List<Collider>();



    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rigController = GetComponentInChildren<RigController>();

        foreach (Rigidbody rb in transform.GetComponentsInChildren<Rigidbody>())
        {
            rigidbodies.Add(rb);
            rb.isKinematic = true;
        }

        foreach(Collider collider in transform.GetComponentsInChildren<Collider>())
        {
            colliders.Add(collider);
            collider.enabled = false;
        }
    }

    [ContextMenu("Collapse Character")]
    public void Context()
    {
        RagdollEnabled = true;
    }

    public bool RagdollEnabled
    {
        get { return !animator.enabled; }
        set
        {
            animator.enabled = !value;
            foreach (Rigidbody rb in rigidbodies)
                rb.isKinematic = !value;
            foreach (Collider collider in colliders)
                collider.enabled = value;
        }
    }

    public RigController RigController { get { return rigController; } }

    public Animator Anim { get { return animator; } }

    public List<Rigidbody> RigidBodies { get { return rigidbodies; } }
}
