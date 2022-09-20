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
    bool posA;

    [HideInInspector]
    public float finalMoveSpeed = 0f;

    public Vector3 velocity = Vector3.zero;
    public Vector3 lastPos = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.useGravity = false;

        finalMoveSpeed = moveSpeed * Time.fixedDeltaTime;

        StartCoroutine(MovePlatform(true));
    }
    private void Update()
    {
        //Transform tPosition = posA ? aPosition : bPosition;
        //transform.LookAt(tPosition);
        //Vector3 direction = (tPosition.position - transform.position).normalized;
        //float distance = Vector3.Distance(transform.position, tPosition.position);
        //if(distance < 0.1f)
        //{
        //    print("flip");
        //    posA = !posA;
        //}

        //transform.position += (direction * moveSpeed) * Time.deltaTime;

        //velocity = (transform.position - lastPos) * Time.deltaTime;
        velocity = (transform.position - lastPos);
        lastPos = transform.position;
    }

    private IEnumerator MovePlatform(bool aPos)
    {
        posA = aPos;
        Transform tPosition = aPos ? aPosition : bPosition;
        Vector3 direction = (tPosition.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, tPosition.position);
        while (distance > 0.1f)
        {
            transform.position += direction * finalMoveSpeed;
            distance = Vector3.Distance(transform.position, tPosition.position);
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
