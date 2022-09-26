using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple grenade weapon that works based off raycasting all IDamageables in a certain distance.
/// </summary>
public class GrenadeWeapon : MonoBehaviour
{
    private Ray ray;
    private RaycastHit hit;
    public List<Transform> damageableObjs = new List<Transform>();

    public float explosionTime = 5f;
    public float explosionDamage = 100f;
    public float explosionForce = 10f;



    private void OnTriggerEnter(Collider other)
    {
        print("bruh");
        if (other.TryGetComponent<IDamageable>(out IDamageable damageable) || other.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            damageableObjs.Add(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        print("bruh");
        if (other.TryGetComponent<IDamageable>(out IDamageable damageable) || other.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            if(damageableObjs.Contains(other.transform))
                damageableObjs.Remove(other.transform);
        }
    }

    /// <summary>
    /// Checks given object to see if it is within sightline of grenade blast.
    /// </summary>
    /// <param name="damageableObject"></param>
    public void CheckObjectWithinGrenade(Transform damageableObject)
    {
        if (damageableObject == null)
            return;

        //damage event happens here
        ray.origin = transform.position;
        ray.direction = (damageableObject.position - ray.origin).normalized;
        if(Physics.Raycast(ray, out hit))
        {
            if(hit.transform == damageableObject)
            {
                if(hit.transform.TryGetComponent<Rigidbody>(out Rigidbody rb))
                    rb.AddForce(ray.direction * explosionForce, ForceMode.Impulse);

                if(damageableObject.TryGetComponent<IDamageable>(out IDamageable damageable))
                    damageable?.TakeDamage(explosionDamage);
            }
        }
    }
    /// <summary>
    /// Coroutine that handles the time before grenade explosion and the given interaction.
    /// </summary>
    /// <returns></returns>
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
