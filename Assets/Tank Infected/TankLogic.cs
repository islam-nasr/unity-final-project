﻿using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class TankLogic : MonoBehaviour
{
    public GameObject bile;

    NavMeshAgent agent;
    Transform player;
    Animator animator;
    Laser laser;
    AudioSource runClip;
    AudioSource dieClip;
    playerHealth healthComponent;
    WeaponSwitching weaponHolder;
    //Patroling
    Vector3 walkPoint;
    bool walkPointSet;
    float walkPointTranslation = 7f;
    int randomDirection;

    //Attacking
    bool alreadyAttacked = false;
    float timeBetweenAttacks = 1.967f;

    //States
    float sightRange = 10f;
    float attackRange;
    float firingRange = 20f;
    bool playerInSightRange, playerInAttackRange, playerIsFiring;
    bool isDead = false;
    bool isHit = false;
    bool isDistracted = false;
    bool isStunned = false;
    

    //Health 
    int health = 1000;

    //Joel 
    GameObject playerScript;
        
    private void Start()
    {
        randomDirection = Random.Range(0, 2);
        attackRange = randomDirection == 0 ? 1.5f : 1.7f;
        agent = GetComponent<NavMeshAgent>();
        GameObject joel = GameObject.FindGameObjectWithTag("Joel");
        player = joel.transform;
        healthComponent = joel.GetComponent<playerHealth>();
        weaponHolder = player.GetComponentInChildren<WeaponSwitching>();
        animator = GetComponent<Animator>();
        laser = GetComponent<Laser>();
        runClip = transform.GetChild(3).GetComponent<AudioSource>();
        dieClip = transform.GetChild(4).GetComponent<AudioSource>();
        // playerScript = GameObject.FindGameObjectWithTag("Joel").GetComponent<playerHealth>();
        SearchWalkPoint();
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        GameObject joel = GameObject.FindGameObjectWithTag("Joel");
        player = joel.transform;
        healthComponent = joel.GetComponent<playerHealth>();
        weaponHolder = player.GetComponentInChildren<WeaponSwitching>();
    }

    private void Update()
    {
        if (!isDead && !healthComponent.isDead())
        {

            if (!playerInSightRange && !playerInAttackRange && !isDistracted && !isHit)
            {
                if (walkPointSet) Patroling();
                else SearchWalkPoint();

            }

            //Check for sight, attack, and firing ranges

            if (isInLineOfSight() && isInFront() && !isDistracted)
            {
               
                    Vector3 distVector = player.position - transform.position;
                    distVector.y = 0;
                    Quaternion angle = Quaternion.LookRotation(distVector);
                    transform.rotation = Quaternion.Slerp(transform.rotation, angle, Time.deltaTime * agent.speed);

                    playerInSightRange = true;


                    if (Vector3.Distance(player.position, transform.position) <= attackRange)
                    {
                    agent.SetDestination(transform.position);
                        playerInAttackRange = true;
                    }
                    else
                        playerInAttackRange = false;
                
            }
            else
                playerInAttackRange = false;



            if (Vector3.Distance(player.position, transform.position) <= firingRange) // + CHECK IF JOEL IS CURRENTLY FIRING !!!
            {
                Gun currWeapon = weaponHolder.getCurrentGun();
                if (currWeapon != null && currWeapon.isShooting()) // + CHECK IF JOEL IS CURRENTLY FIRING !!!
                    playerIsFiring = true;
            }
            else
                playerIsFiring = false;


            if (((playerInSightRange && !playerInAttackRange) || playerIsFiring) && !isDistracted && !isHit) ChasePlayer();
            if (playerInSightRange && playerInAttackRange && !isDistracted && !isHit) AttackPlayer();

        }


    }

    private bool isInFront()
    {
        Vector3 playerDirection = transform.position - player.position;
        float angle = Vector3.Angle(transform.forward, playerDirection);
        if (Mathf.Abs(angle) > 90 && Mathf.Abs(angle) < 270) return true;
        return false;
    }

    private bool isInLineOfSight()
    {
        RaycastHit hit;
        Vector3 direction = player.position - transform.position;
        if (Physics.Raycast(transform.position, direction, out hit, sightRange))
        {
            if (hit.transform == player)
                return true;
        }
        return false;

    }

    private void Patroling()
    {
        animator.speed = 1f;
        animator.SetBool("walking", true);
        laser.laserHit = null;
        agent.speed = 1f;
        if (Vector3.Distance(transform.position, walkPoint) < 1f)
            walkPointSet = false;

        else
            agent.SetDestination(walkPoint);



    }

    private void SearchWalkPoint()
    {

        Vector3 randomDirection = Random.insideUnitSphere * walkPointTranslation;
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, walkPointTranslation, 1);
        walkPoint = hit.position;
        walkPointSet = true;

        /* //Calculate random point in range
         // float randomZ = Random.Range(-walkPointRange, walkPointRange);
         walkPointTranslation = walkPointTranslation == 5 ? -5 : 5;

         if (randomDirection == 0)
             walkPoint = new Vector3(transform.position.x + walkPointTranslation, transform.position.y, transform.position.z);
         else
             walkPoint = new Vector3(transform.position.x, transform.position.y, transform.position.z + walkPointTranslation);
         walkPointSet = true;*/

    }

    private void ChasePlayer()
    {
        if (!runClip.isPlaying)
            runClip.PlayOneShot(runClip.clip);

        animator.SetBool("walking", false);
        animator.SetBool("chasing", true);
        if (isStunned)
        {
            agent.speed = 1f;
            animator.speed = 0.5f;
        }

        else
        {
            agent.speed = 3f;
            animator.speed = 1.5f;
        }
        agent.SetDestination(player.position);
        laser.laserHit = player;


    }

    private void AttackPlayer()
    {
        //Make sure enemy doesn't move
        agent.SetDestination(transform.position);
        animator.SetBool("chasing", false);
        animator.SetBool("walking", false);

        if (isStunned)
        {
            animator.speed = 0.5f;
        }
        else animator.speed = 1f;

        if (!alreadyAttacked)
        {
            if (randomDirection == 0)
                animator.SetTrigger("attacking 1");
            else
                animator.SetTrigger("attacking 2");

            alreadyAttacked = true;
            // CALL A METHOD TO APPLY DAMAGE TO JOEL !!!
            healthComponent.applyDamage(30);
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }


    }

    private void ResetAttack()
    {
        alreadyAttacked = false;

    }

    public void TakeDamage(int damage)
    {
        animator.speed = 1f;
        animator.SetBool("walking", false);
        animator.SetBool("chasing", false);
        health -= damage;
        if (health <= 0)
        {
            Die();
            isDead = true;

        }
        else
        {
            isHit = true;
            agent.SetDestination(transform.position);
            animator.SetTrigger("damage");
            Invoke(nameof(DamageEnded), 1f);

        }

    }

    private void DamageEnded()
    {
        isHit = false;
        playerInSightRange = true;
    }

    public void Distract(Transform pipe)
    {
        animator.speed = 1.5f;
        agent.speed = 3f;
        isDistracted = true;
        animator.SetBool("walking", false);
        animator.SetBool("chasing", true);
        agent.SetDestination(pipe.position);
        laser.laserHit = pipe;
        Invoke(nameof(DistractionEnded), 4f);
    }

    private void DistractionEnded()
    {
        animator.speed = 1f;
        animator.SetBool("chasing", false);
        isDistracted = false;
        laser.laserHit = null;
    }

    public void Stun()
    {
        isStunned = true;
        Invoke(nameof(StunEnded), 3f);
    }

    private void StunEnded()
    {
        isStunned = false;

    }

    private void Die()
    {
        if (!isDead)
        {
            dieClip.PlayOneShot(dieClip.clip);
            isDead = true;
            animator.speed = 1f;
            agent.SetDestination(transform.position);
            animator.SetTrigger("dying");
            //CALL A METHOD TO INSTANTIATE BILE !!!
            Destroy(gameObject, 2);
            GameObject instan = Instantiate(bile, transform);
            instan.transform.SetParent(null);
            healthComponent.rageMeterAdd(50);
            healthComponent.infectedIsKilled();

        }
    }

}


