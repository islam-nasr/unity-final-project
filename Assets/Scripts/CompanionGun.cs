﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class CompanionGun : MonoBehaviour
{
    public float fireRate = 1f;
    public int clipCapacity = 15;
    private int currentClip;
    public Camera camera;
    public ThirdPersonCharacter gunHolder;
    public List<GameObject> currHitObjects = new List<GameObject>();
    public int damage = 36;
    public float sphereRadius;
    public float maxDistance;
    public LayerMask layerMask;
    public ParticleSystem muzzleFlash;
    public AudioSource shootingSound;
    public playerHealth healthComponent;
    private int increasedClips=0;
    private Vector3 origin;
    private Vector3 direction;
    private float currentHitDistance;
    private bool shooting = false;
    private float lastPlayed_shoot = 0f;
    public int maxClips = 3;
    private int clips = 0;
    //private int gunAmmo = 0;
    private bool companionEllie = false;
    float timer;
    private void Start()
    {
        currentClip = clipCapacity;
        companionEllie = PlayerPrefs.GetString("Companion").Equals("Ellie");
        if (companionEllie)
            healthComponent.setRageMultiplier(2);
        else
            timer = 1.0f;

    }
    void Update()
    {
        if (healthComponent.isDead())
            return;
        if (!companionEllie)
            if (timer - Time.deltaTime <= 0)
            {
                timer = 1.0f;
                healthComponent.increaseHealth(1);
            }
            else
            {
                timer -= Time.deltaTime;
            }
        //healthComponent.setRageMultiplier(int multiplier)
        //healthComponent.increaseHealth(int add)
        clipsCheck();
        if (Input.GetKeyDown(KeyCode.Q))
        {
            shooting = !shooting;
            gunHolder.GetComponent<AICharacterControl>().SetTarget(camera.transform);

        }

        if (shooting)
        {
            origin = transform.position;
            direction = transform.forward;
            currentHitDistance = maxDistance;
            currHitObjects.Clear();
            RaycastHit[] hits = Physics.SphereCastAll(origin, sphereRadius, direction, maxDistance, layerMask, QueryTriggerInteraction.UseGlobal);
            if(hits.Length==0)
            {
                shooting = !shooting;
                gunHolder.GetComponent<AICharacterControl>().SetTarget(camera.transform);
            }
            foreach (RaycastHit hit in hits)
            {
                if (hit.transform.gameObject.GetComponent<NormalLogic>() != null ||
                   hit.transform.gameObject.GetComponent<HunterLogic>() != null ||
                   hit.transform.gameObject.GetComponent<ChargerLogic>() != null ||
                   hit.transform.gameObject.GetComponent<TankLogic>() != null 
                   || hit.transform.gameObject.name == "Cube"
                   )
                {
                    currHitObjects.Add(hit.transform.gameObject);
                }

                foreach (GameObject infected in currHitObjects)
                {
                    if (infected.GetComponent<HunterLogic>() != null)
                    {
                        
                        gunHolder.GetComponent<AICharacterControl>().SetTarget(infected.transform);
                        Shoot(infected,"Hunter");
                        break;
                    }
                    else if (infected.GetComponent<ChargerLogic>() != null)
                    {

                        gunHolder.GetComponent<AICharacterControl>().SetTarget(infected.transform);
                        Shoot(infected,"Charger");
                        break;
                    }
                    else if (infected.GetComponent<TankLogic>() != null)
                    {

                        gunHolder.GetComponent<AICharacterControl>().SetTarget(infected.transform);
                        Shoot(infected,"Tank");
                        break;
                    }
                    else if (infected.GetComponent<SpitterLogic>() != null)
                    {

                        gunHolder.GetComponent<AICharacterControl>().SetTarget(infected.transform);
                        Shoot(infected, "Spitter");
                        break;
                    }
                    else if (infected.GetComponent<NormalLogic>() != null)
                    {

                        gunHolder.GetComponent<AICharacterControl>().SetTarget(infected.transform);
                        Shoot(infected,"Normal");
                        break;
                    }
/*                    else if (infected.name == "Cube")
                    {

                        gunHolder.GetComponent<AICharacterControl>().SetTarget(infected.transform);
                        //gunHolder.GetComponent<Animation>().Play("Shooting");
                        Shoot(); 
                        //Debug.Log(count);
                        break;
                    }*/

                }

                currentHitDistance = hit.distance;
            }
        }

 

        //cheat
        if (Input.GetKey(KeyCode.LeftAlt)&& Input.GetKeyDown(KeyCode.Comma))
        {
            clips += 1;
        }
        
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Debug.DrawLine(origin, origin + direction * currentHitDistance);
        Gizmos.DrawWireSphere(origin + direction * currentHitDistance, sphereRadius);
    }

    void Shoot(GameObject inf,string type)
    {
        bool isRaging = healthComponent.isRaging();
        if (currentClip==0 && clips > 0 && !isRaging)
        {
            currentClip = clipCapacity;
            clips -= 1;
        }
        if(!isRaging && currentClip==0)
        {
                shooting = !shooting;
                gunHolder.GetComponent<AICharacterControl>().SetTarget(camera.transform);
        }
        if (Time.time - lastPlayed_shoot >= 1/fireRate && (currentClip>0 || isRaging))
        {
            if(!isRaging)
                currentClip -= 1;
            
            shootingSound.Play();
            muzzleFlash.Play();
            if(type.Equals("Hunter"))
                inf.GetComponent<HunterLogic>().TakeDamage(damage);
            else if(type.Equals("Tank"))
                inf.GetComponent<TankLogic>().TakeDamage(damage);
            else if (type.Equals("Charger"))
                inf.GetComponent<ChargerLogic>().TakeDamage(damage);
            else if (type.Equals("Spitter"))
                inf.GetComponent<SpitterLogic>().TakeDamage(damage);
            else if (type.Equals("Normal"))
                inf.GetComponent<NormalLogic>().TakeDamage(damage);
            lastPlayed_shoot = Time.time;
        }

    }
    void clipsCheck()
    {
        if (healthComponent.getTotalKilled() - (increasedClips * 10) >= 10 && clips<maxClips)
        {
            increasedClips += 1;
            clips += 1;
        }

    }
/*    void Shoot()
    {

        if (Time.time - lastPlayed_shoot >= .1f && gunAmmo < clipCapacity * clips)
        {
            //gunAmmo += 1;
            shootingSound.Play();
            muzzleFlash.Play();
            
            lastPlayed_shoot = Time.time;
        }

    }*/
    public int getAmmoCount()
    {
        if (!healthComponent.isRaging())
            return currentClip;
        else
            return 1000;
    }
    public int getMaxAmmo()
    {
        return (clips * clipCapacity);
    }
}
