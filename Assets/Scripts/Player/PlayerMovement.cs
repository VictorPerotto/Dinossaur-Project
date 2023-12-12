using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float groundDrag;
    [SerializeField] private Transform orientation;
    private float moveSpeed;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    private float playerHeight;
    public bool onGround;

    [Header("Jump")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;
    private bool canJump;

    [Header("Crouching")]
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float crouchYScale;
    private float startYScale;


    [Header("Keybinds")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;

    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private Rigidbody rb;

    public MovementState state;

    // Start is called before the first frame update
    private  void Start()
    {
        canJump = true;

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        playerHeight = GetComponentInChildren<CapsuleCollider>().height;

        startYScale = transform.localScale.y;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void Update()
    {
        Inputs();
        GroundCheck();
        SpeedControl();
        StateHandler();
    }

    private void Inputs()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKey(jumpKey) && canJump && onGround)
        {
            canJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if(Input.GetKeyDown(crouchKey)){
            Crouch();
        }

        if(Input.GetKeyUp(crouchKey)){
            StopCrouch();
        }
    }

    private void StateHandler()
    {
        if(Input.GetKey(crouchKey)){
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }

        else if(onGround && Input.GetKey(sprintKey)){
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }

        else if(onGround){
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }

        else{
            state = MovementState.air;
        }
    }

    private void GroundCheck()
    {
        onGround = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);

        if(onGround){
            rb.drag = groundDrag;
        }

        else{
            rb.drag = 0f;
        }
        
    }

    private void MovePlayer()
    {
        //orientation.foward pega o eixo da seta azul da unity (ou seja pra frente)
        //orientation.right pega o eixo da seta vermelha da unity (ou seja para os lados)
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if(onGround){
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }

        else if(!onGround){
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3 (rb.velocity.x, 0f, rb.velocity.z);

        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3 (limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector3 (rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        canJump = true;
    }

    private void Crouch()
    {
        //para mudar a escala dele e "empurrar" ele para nao ficar caindo devagar
        transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        
    }

    private void StopCrouch()
    {
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);     
    }
}

public enum MovementState
{
    walking,
    sprinting,
    crouching,
    air
}