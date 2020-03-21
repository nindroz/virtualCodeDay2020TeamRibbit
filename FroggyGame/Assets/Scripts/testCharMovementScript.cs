﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class testCharMovementScript : MonoBehaviour
{
    //Note - this movement script is mass independant
    //Singleton 
    static testCharMovementScript charMoveScript;
    private void Awake()
    {
        charMoveScript = this;
    }

    //Components
    private Rigidbody2D charRb;
    private BoxCollider2D charCollider;
    //input
    private float xInput;
    //Horizontal Movement vars
    public float moveVelocity = 20;
    private float moveForceGround = 100f;
    private float moveForceAir = 70f;
    private bool horizontalMovementActive = true;

    //Jumping vars
    public float jumpChargeTime = 2f;
    public float jumpMaxVel;
    public float jumpMinVel;
    private float jumpTimer = 0f;
    private float jumpHoldIgnoreTime = 0.2f;
    private float jumpIgnoreGroundedTime = 0.1f;
    private float jumpIgnoreGrounedTimer = 0f;

    public Image jumpBar;

    //character states
    private bool isGrounded = false;
    void Start()
    {
        //Gets components
        charRb = gameObject.GetComponent<Rigidbody2D>();
        charCollider = gameObject.GetComponent<BoxCollider2D>();
        
        charRb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
    //input
    void Update()
    {
        jumpIgnoreGrounedTimer -= Time.deltaTime;
        xInput = Input.GetAxisRaw("Horizontal");
        //Charging jump
        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            jumpTimer += Time.deltaTime;
            if (jumpTimer > jumpHoldIgnoreTime)
            {
                //Updates jump bar
                jumpBar.transform.localScale = new Vector3(Mathf.Min((jumpTimer-jumpHoldIgnoreTime) / jumpChargeTime, 1), 1, 0);
                //No horizontal movement
                xInput = 0;
                horizontalMovementActive = false;
            }
        }
        //Release
        if (Input.GetKeyUp(KeyCode.Space))
        {
            //Hold jump
            if (isGrounded)
            {
                if (jumpTimer > jumpHoldIgnoreTime)
                {
                    //Gets magnitude of jump and direction of jump
                    float jumpRatio = Mathf.Min((jumpTimer - jumpHoldIgnoreTime) / jumpChargeTime, 1);
                    float jumpMagnitude = Mathf.Lerp(jumpMinVel, jumpMaxVel, jumpRatio);
                    Vector2 direction = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                    float angle = Mathf.Atan2(direction.y, direction.x);
                    charRb.velocity = new Vector2(jumpMagnitude * Mathf.Cos(angle),jumpMagnitude* Mathf.Sin(angle));
                }
                //Normal jump
                else
                {
                    charRb.velocity = new Vector2(charRb.velocity.x, jumpMinVel);
                }
                jumpIgnoreGrounedTimer = jumpIgnoreGroundedTime;
            }            
            //Resets jumpbar and horizontalMovementActive
            jumpTimer = 0;
            jumpBar.transform.localScale = new Vector3(0, 1, 0);
            horizontalMovementActive = true;
        }
    }

    private void FixedUpdate()
    {
        isGrounded = CheckForGrounded();
        //Prevents being checked as grounded immediately after jumping
        if (jumpIgnoreGrounedTimer > 0)
            isGrounded = false;
        //Velocity slowdown when on ground
        if(isGrounded && (xInput == 0 || !horizontalMovementActive))
        {
            charRb.velocity = new Vector2(charRb.velocity.x / 1.45f, charRb.velocity.y);
        }
        //Applies horizontal movement
        if(horizontalMovementActive)
        {
            if(isGrounded)
            {
                charRb.AddForce(Vector2.right * xInput * moveForceGround * charRb.mass);
                //Clamps to maxspeed
                charRb.velocity = new Vector2(Mathf.Clamp(charRb.velocity.x, -moveVelocity, moveVelocity), charRb.velocity.y);
            }
            else
            {
                //Clamps air speed
                if(charRb.velocity.x * xInput < moveVelocity)
                    charRb.AddForce(Vector2.right * xInput * moveForceAir * charRb.mass);
            }
        }
    }

    //Grounded check
    public LayerMask groundedCheckLayerMask;
    private bool CheckForGrounded()
    {
        Vector2 size = new Vector2(charCollider.size.x -0.1f, charCollider.size.y - 0.2f);
        RaycastHit2D groundRaycast = Physics2D.BoxCast(charCollider.bounds.center, size, 0, Vector2.down, 0.2f,groundedCheckLayerMask);
        if(groundRaycast.collider != null)
        {
            return true;
        }
        return false;
    }
}