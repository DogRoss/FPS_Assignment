using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField]Transform aPosition;
    [SerializeField]Transform bPosition;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float waitTime = 2f;

    public Vector3 velocity = Vector3.zero;
    public Vector3 lastPos = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.useGravity = false;

        StartCoroutine(MovePlatform(true));
    }
    private void Update()
    {
        velocity = (transform.position - lastPos) / Time.deltaTime;
        lastPos = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        print("enter: " + other.transform.name);
        if(other.transform.TryGetComponent<Movement>(out Movement move))
        {
            other.transform.parent = transform;
            other.transform.up = transform.up;
            other.transform.lossyScale.Set(1,1,1);
            other.GetComponent<Movement>()?.AddForce(-velocity);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        print("exit: " + other.transform.name);
        if(other.transform.parent == transform)
        {
            other.transform.parent = null;
            other.transform.up = Vector3.up;
            other.transform.localScale = Vector3.one;
            other.GetComponent<Movement>()?.AddForce(velocity);
        }
    }

    private IEnumerator MovePlatform(bool aPos)
    {
        Transform tPosition = aPos ? aPosition : bPosition;
        Vector3 direction = (tPosition.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, tPosition.position);
        while (distance > 0.1f)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
            transform.Rotate(transform.forward, 30 * Time.deltaTime);
            distance = Vector3.Distance(transform.position, tPosition.position);
            direction = (tPosition.position - transform.position).normalized;
            yield return null;
        }
        transform.position = tPosition.position;

        yield return new WaitForSeconds(waitTime);
        //choose next move
        if (aPos)
            StartCoroutine(MovePlatform(false));
        else
            StartCoroutine(MovePlatform(true));
        yield return null;
    }
}
