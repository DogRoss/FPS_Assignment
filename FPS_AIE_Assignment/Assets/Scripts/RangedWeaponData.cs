using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "WeaponData/Create WeaponData")]
public class RangedWeaponData : ScriptableObject
{
    [Header("Gun Info")]
    [Tooltip("Rounds Per Minute")]
    public float rateOfFire = 5;
    public bool canAuto;

    [Header("Bullet Info")]
    public float damagePerBullet = 50f;
    [Tooltip("UPS = Units Per Second: how many in game units the bullet will travel per second")]
    public float bulletTravelUPS = 200f; //set bulletSpeed will also be first inital raycast to point before drop is applied
    public float bulletsPerShot = 1f; //how many bullets are shot when the Shoot() function is used

    public float RoundsPerSecond
    {
        get
        {
            return rateOfFire / 60;
        }
    }
    public float TimeBetweenBullets
    {
        get
        {
            return 60 / rateOfFire;
        }
    }

    [Header("Fx")]
    public LineRenderer trail;
    public float fadeTime = .1f;

}
