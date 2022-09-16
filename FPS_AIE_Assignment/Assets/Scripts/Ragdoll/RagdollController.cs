using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RagdollController : MonoBehaviour
{
    private Animator animator;
    private List<Rigidbody> rigidbodies = new List<Rigidbody>();

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        foreach (Rigidbody rb in transform.root.GetComponentsInChildren<Rigidbody>())
        {
            rigidbodies.Add(rb);
            rb.isKinematic = true;
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
        }
    }

    public Animator Anim { get { return animator; } }

    public List<Rigidbody> RigidBodies { get { return rigidbodies; } }
}
