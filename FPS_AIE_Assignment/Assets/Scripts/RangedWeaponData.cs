using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "WeaponData/Create WeaponData")]
public class RangedWeaponData : ScriptableObject
{
    [Header("Gun Data")]
    [Tooltip("Rounds Per Minute")]
    public float rateOfFire = 300f;
    public float clipSize = 1f;
    public bool canAuto = true;

    [Header("Bullet Data")]
    public float damagePerBullet = 50f;
    [Tooltip("UPS = Units Per Second: how many in game units the bullet will travel per second")]
    public float bulletTravelUPS = 200f; //set bulletSpeed will also be first inital raycast to point before drop is applied
    public float bulletsPerShot = 1f; //how many bullets are shot when the Shoot() function is used

    [Header("Gun Handling")]
    public Vector3 hipPositionalOffset = Vector3.zero;
    public Vector3 aimPositionalOffset = Vector3.zero;
    public float turnForce = 10f;

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
