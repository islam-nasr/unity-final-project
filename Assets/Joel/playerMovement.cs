﻿using System.Collections;

using System.Collections.Generic;

using UnityEngine;



public class playerMovement : MonoBehaviour

{
    float jumpVal = 0;
    bool falling = true;

    bool walkingSoundBool = false;

    bool runningSoundBool = false;



    bool foundPrevious = false;



    //public GameObject player;

    //public AudioSource audioSource;

    public AudioSource walking;

    public AudioSource running;
    public AudioSource alert;

    public playerHealth healthComponent;


    public CharacterController controller;

    public float speed = 2f;



    public Animator animator;



    public Transform groundCheck;

    public float groundDistance = 0.1f;

    public LayerMask groundMask;

    bool isGrounded;



    // Start is called before the first frame update

    void Start()

    {
   

        speed = 2f;

        walkingSoundBool = false;
        healthComponent = gameObject.GetComponent<playerHealth>();

    }



    // Update is called once per frame

    void Update()

    {



        if (!animator.GetCurrentAnimatorStateInfo(3).IsName("dying") && !healthComponent.isPinned())

        {

            float x = Input.GetAxis("Horizontal");

            float z = Input.GetAxis("Vertical");



            Vector3 move = transform.right * x + transform.forward * z;

            controller.Move(move * speed * Time.deltaTime);



            //vertical

            if (z == 1)

            {

                animator.SetBool("forward", true);

            }

            if (z == 0)

            {

                animator.SetBool("forward", false);

                animator.SetBool("backward", false);

            }

            if (z == -1)

            {

                animator.SetBool("backward", true);

            }



            //horizontal

            if (x == 1)

            {



                animator.SetBool("right", true);

            }

            if (x == 0)

            {

                animator.SetBool("right", false);

                animator.SetBool("left", false);

            }

            if (x == -1)

            {

                animator.SetBool("left", true);

            }



            //run

            if (Input.GetKeyDown(KeyCode.LeftShift))

            {

                animator.SetBool("run", true);

                speed = 2f * speed;
                if ((Input.GetAxis("Vertical") != 0) || (Input.GetAxis("Horizontal") != 0))
                {
                    walkingSoundPlay(2);
                }

            }

            if (Input.GetKeyUp(KeyCode.LeftShift))

            {

                animator.SetBool("run", false);

                speed = speed / 2f;

                walkingSoundStop(2);

            }





            //jump

            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            //Debug.Log("grounded: " + isGrounded);


            if (!isGrounded && falling)

            {

                Vector3 moveH = new Vector3(0, -2f, 0);

                controller.Move(moveH * Time.deltaTime);
                 
            }
            if (jumpVal > 0 && falling)
                jumpVal += -2*Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Space))

            {
                
                if(jumpVal <= 0)
                {
                    falling = false;
                    animator.SetBool("jump", true);
                }
            }
           // Debug.Log(jumpVal);
           // if (Input.GetKey(KeyCode.Space))
            //{
                if (!falling)
                {

                    if (jumpVal < 0.5f)
                    {
                        Vector3 moveH = new Vector3(0, 2f, 0);
                        jumpVal+=moveH.y*Time.deltaTime;
                        controller.Move(moveH * Time.deltaTime);
                    }

                    if (animator.GetCurrentAnimatorStateInfo(2).IsName("jump"))

                    {
                        animator.SetBool("jump", false);
                    }
                }
            //}
            //Debug.Log("Falling: " + falling);   
            if (jumpVal >= 0.5f)

            {
                falling = true;
                //jumpVal = 0;
                animator.SetBool("jump", false);


            }

        }



        //Dead
        /*
                if (Input.GetKeyDown(KeyCode.L))

                {

                    animator.SetBool("dead", true);



                }*/



        //reload

        if (Input.GetKeyDown(KeyCode.R))

        {

            animator.SetBool("reload", true);



        }

        if (Input.GetKeyUp(KeyCode.R))

        {

            animator.SetBool("reload", false);

        }



        //fire

        if (Input.GetKeyDown(KeyCode.Mouse0))

        {

            animator.SetBool("fire", true);

            /*    audioSource.clip = shootingClip;

                audioSource.loop = true;

                audioSource.Play();

            */
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))

        {

            animator.SetBool("fire", false);

            /*            audioSource.Stop();

                        audioSource.clip = null;

                        audioSource.loop = false;*/

        }



        //toss

        if (Input.GetKeyDown(KeyCode.Mouse1))

        {

            animator.SetBool("toss", true);

        }

        if (Input.GetKeyUp(KeyCode.Mouse1))

        {

            animator.SetBool("toss", false);

        }

        infectedAheadCheck();

        checkWalking();

    }



    void infectedAheadCheck()

    {

        GameObject[] normals = GameObject.FindGameObjectsWithTag("Normal");

        GameObject[] hunters = GameObject.FindGameObjectsWithTag("Hunter");

        GameObject[] chargers = GameObject.FindGameObjectsWithTag("Charger");

        GameObject[] spitters = GameObject.FindGameObjectsWithTag("Spitter");

        GameObject[] tanks = GameObject.FindGameObjectsWithTag("Tank");





        bool found = false;



        if (infectedAheadCheckHelper(normals))

        {

            found = true;

        }

        if (infectedAheadCheckHelper(hunters))

        {

            found = true;

        }

        if (infectedAheadCheckHelper(chargers))

        {

            found = true;

        }

        if (infectedAheadCheckHelper(spitters))

        {

            found = true;

        }

        if (infectedAheadCheckHelper(tanks))

        {

            found = true;

        }



        //play alert here

        if (found != foundPrevious)

        {
            alert.enabled = true;
            alert.PlayOneShot(alert.clip);

/*            AudioClip infectedAlert = GameObject.Find("infectedAlert").GetComponent<AudioSource>().clip;

            this.audioSource.PlayOneShot(infectedAlert);*/

           // Debug.Log(" INFECTRED ALERTTTT");

        }

        foundPrevious = found;

    }



    bool infectedAheadCheckHelper(GameObject[] infected)

    {

        foreach (GameObject inf in infected)

        {

            float dist = Vector3.Distance(this.transform.position, inf.transform.position);

            //distance to check

            if (dist < 10f)

            {

                return true;


            }

        }

        return false;

    }





    void checkWalking()

    {
        if ((Input.GetAxis("Vertical") == 0) && (Input.GetAxis("Horizontal") == 0))
        {
            walkingSoundStop(2);
            walkingSoundStop(1);
        }
        else
        {
            walkingSoundPlay(1);
        }
    }



    void walkingSoundPlay(int i)

    {

        if (i == 1)

        {
            if (!runningSoundBool)
            {
                if (!walkingSoundBool)

                {
                    
                    //AudioClip walkingClip = GameObject.Find("walkingSound").GetComponent<AudioSource>().clip;

                 //   Debug.Log("walkingsound");
                    //this.GetComponent<AudioSource>().clip = walkingClip;
                    // this.GetComponent<AudioSource>().loop = true;
                    walking.enabled = true;
                    walking.loop = true;
                    //this.GetComponent<AudioSource>().Play();
                    walking.Play();
                    walkingSoundBool = true;
                }
            }
        }

        if (i == 2)
        {
            if (!runningSoundBool)
            {
                //AudioClip runningClip = GameObject.Find("runningSound").GetComponent<AudioSource>().clip;
                walkingSoundBool = false;
                running.enabled = true;
               // Debug.Log("runningsound");
                /*                this.GetComponent<AudioSource>().clip = runningClip;
                                this.GetComponent<AudioSource>().loop = true;
                                this.GetComponent<AudioSource>().Play();*/
                running.enabled = true;
                running.loop = true;
                running.Play();
                runningSoundBool = true;
            }
        }
    }



    void walkingSoundStop(int i)

    {

        if (i == 1)

        {

            if (walkingSoundBool)

            {

                walking.enabled = false;

               // Debug.Log("stopping walking sound");
                walking.loop = false;
                walking.Stop();
/*                this.GetComponent<AudioSource>().clip = null;

                this.GetComponent<AudioSource>().loop = false;*/

                walkingSoundBool = false;

            }

        }

        if (i == 2)

        {

            if (runningSoundBool)

            {
                running.enabled = false;
               // Debug.Log("stopping running sound");

                /*                this.GetComponent<AudioSource>().clip = null;

                                this.GetComponent<AudioSource>().loop = false;*/
                running.loop = false;
                running.Stop();
                runningSoundBool = false;

                checkWalking();

                walkingSoundPlay(1);

            }

        }
    }
}