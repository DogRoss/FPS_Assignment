using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePool : MonoBehaviour
{
    public ParticleSystem particlePrefab;
    public int poolSize;
    public List<ParticleSystem> effects;

    // Start is called before the first frame update
    void Start()
    {

        for(int i = 0; i < poolSize; i++)
        {
            effects.Add(Instantiate(particlePrefab, Vector3.zero, Quaternion.identity));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetParticle(Vector3 position, Quaternion rotation)
    {
        for(int i = 0; i < poolSize; i++)
        {
            if (!effects[i].isPlaying)
            {
                effects[i].transform.position = position;
                effects[i].transform.rotation = rotation;
                effects[i].Play();
                return;
            }
        }

        print("create new spot");
        poolSize++;
        ParticleSystem particle = Instantiate(particlePrefab, position, rotation);
        effects.Add(particle);
        particle.Play();
    }

    public void GetParticle(Vector3 position, Vector3 direction)
    {
        for (int i = 0; i < poolSize; i++)
        {
            if (!effects[i].isPlaying)
            {
                effects[i].transform.position = position;
                effects[i].transform.forward = direction;
                effects[i].Play();
                return;
            }
        }

        print("create new spot");
        poolSize++;
        ParticleSystem particle = Instantiate(particlePrefab, position, Quaternion.identity);
        effects.Add(particle);
        particle.transform.forward = direction;
        particle.Play();

    }
}
