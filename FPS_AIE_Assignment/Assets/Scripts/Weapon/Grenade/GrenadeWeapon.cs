using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeWeapon : MonoBehaviour
{
    private Ray ray;
    private RaycastHit hit;
    public List<Transform> damageableObjs = new List<Transform>();

    public float explosionTime = 5f;
    public float explosionDamage = 10f;



    private void OnTriggerEnter(Collider other)
    {
        print("bruh");
        if (other.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            damageableObjs.Add(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        print("bruh");
        if (other.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            if(damageableObjs.Contains(other.transform))
                damageableObjs.Remove(other.transform);
        }
    }

    [ContextMenu("StartGrenadeTimer")]
    public void StartGrenadeTimer()
    {
        StartCoroutine(GrenadeTimer());
    }

    public void CheckObjectWithinGrenade(Transform damageableObject)
    {
        //damage event happens here
        ray.origin = transform.position;
        ray.direction = (damageableObject.position - ray.origin).normalized;
        if(Physics.Raycast(ray, out hit))
        {
            if(hit.transform == damageableObject && damageableObject.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                damageable?.TakeDamage(explosionDamage);
            }
        }
    }

    public IEnumerator GrenadeTimer()
    {
        float currentTime = 0f;
        while (currentTime < explosionTime)
        {
            currentTime += Time.deltaTime;
            yield return null;
        }

        foreach(Transform obj in damageableObjs)
        {
            CheckObjectWithinGrenade(obj);
        }

        DestroyImmediate(gameObject);
        yield return null;
    }
}
