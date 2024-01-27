using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

using static Custom_Utility;

public class Player : MonoBehaviour
{
    public float _moveSpeed = 5f, airMoveRatio = 0.5f, jumpEnterRatio = 0.8f, _jumpVelocity = 3f, _dashSpeed = 20f, dash_duration = 0.5f, dash_non_interruption = 0.5f, slash_speed = 15f, slash_duration = 0.3f, slash_drag = 0.5f;
    public Camera Main_Camera;
    public GameObject Crosshair;
    //public Camera_Controller camera_controller;
    private Animator animator;
    private bool onGround, isWalking, isDashing, canDash, to_idle, isFacingRight, isAttacking;
    private int floor_counter;
    private Rigidbody2D Main_Rigidbody;
    private Collider2D Main_Collider;
    private SpriteRenderer Sprite_Renderer;
    private TrailRenderer Trail_Renderer;
    private PlayerInput playerInput;
    private PlayerInputActions playerInputActions;
    private Vector2 DirectionalInputVector, DashRuntimeVector, JumpEnterVelocity, AirBaseVelocity, mouse_input_vector;
    private Vector3 target;
    private float current_dash_time, DirectionalInputAxis, normalGravityScale, current_slash_time;
    private string current_control_scheme;

    private void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("to_idle", true);
        to_idle = true;
        onGround = false;
        isWalking = false;
        isFacingRight = true;
        isAttacking = false;
        floor_counter = 0;
        Main_Rigidbody = GetComponent<Rigidbody2D>();
        Main_Collider = GetComponent<Collider2D>();
        Sprite_Renderer = GetComponent<SpriteRenderer>();
        Trail_Renderer = GetComponent<TrailRenderer>();

        playerInput = GetComponent<PlayerInput>();
        playerInputActions = new PlayerInputActions();

        current_control_scheme = "Keyboard";
        playerInputActions.Default.Enable();

        playerInputActions.Default.Jump.performed += Jump;
        playerInputActions.Default.Dash.performed += startDash => Dash(DirectionalInputVector);
        playerInputActions.Default.Control_Scheme.performed += Switch_Control_Scheme;
        playerInputActions.Default.Mouse_Input.performed += set => mouse_input_vector = set.ReadValue<Vector2>();
        playerInputActions.Default.Attack.performed += Slash;

        var bindingGroup = playerInputActions.KeyboardScheme.bindingGroup;
        playerInputActions.bindingMask = InputBinding.MaskByGroup(bindingGroup);
  
        isDashing = false;
        canDash = true;

        DirectionalInputAxis = 0f;
        DirectionalInputVector = Vector2.zero;
        JumpEnterVelocity = Vector2.zero;
        AirBaseVelocity = Vector2.zero;
        DashRuntimeVector = Vector2.zero;
        mouse_input_vector = Vector2.zero;
        
        normalGravityScale = Main_Rigidbody.gravityScale;
        
        current_dash_time = 0f;

        //Cursor.visible = false;
    }
    private void Slash(InputAction.CallbackContext context)
    {
        if(isAttacking | !onGround){return;}

        isAttacking = true;
        to_idle = false;

        Vector3 slash_direction = Crosshair.transform.position - transform.position;
        slash_direction.z = 0f;
        slash_direction = Vector3.Normalize(slash_direction);
        
        Main_Rigidbody.velocity = new Vector2(slash_direction.x, slash_direction.y) * slash_speed;
        
        current_slash_time = 0f;

        if(isFacingRight && slash_direction.x < 0){Flip();}
        if(!isFacingRight && slash_direction.x > 0){Flip();}
        
        animator.Play("Attack_1");

        AirBaseVelocity = new Vector2(slash_direction.x, 0) * slash_speed * slash_drag;
    }

    private void Slash_Runtime()
    {
        if(isAttacking){
            current_slash_time += Time.deltaTime;
            to_idle = false;
            if(current_slash_time >= slash_duration)
            {
                isAttacking = false;
                to_idle = true;
                return;
            }
        }
    }
    private void Mouse_Input()
    {
        if(current_control_scheme != "Keyboard"){return;}
        target = Main_Camera.ScreenToWorldPoint(new Vector3(mouse_input_vector.x, mouse_input_vector.y, 0));
        Crosshair.transform.position = target + new Vector3(0 ,0 ,1);
    }
    
    private void Dash(Vector2 inputVector)
    {
        if(canDash)
        {
            current_dash_time = 0f;
            if(inputVector.Equals(Vector2.zero))
            {
                DashRuntimeVector = new Vector2(1, 0); //нет направления -> дэш вперед
                if(!isFacingRight){Flip();}
            }
            else
            {
                DashRuntimeVector = inputVector.normalized;
            }
            isDashing = true;
            canDash = false;
            AirBaseVelocity = AirBaseVelocity.magnitude * DashRuntimeVector;
            Main_Rigidbody.gravityScale = 0f;
            //добавить ограничение/запись вертикальнорй скорости (прыжка)
        } 
    }

    private void Dash_Runtime()
    {
        if(isDashing)
        {
            Trail_Renderer.emitting = true;

            Main_Rigidbody.velocity = DashRuntimeVector * _dashSpeed;//+ Main_Rigidbody.velocity.y * new Vector2(0, 1);
            current_dash_time += Time.deltaTime;
            
            if(current_dash_time >= dash_non_interruption * dash_duration)
            {
                canDash = true;
            }
            if(current_dash_time >= dash_duration) //резкая остановка, возможно потом добавлю замедление в конце
            {
                Main_Rigidbody.gravityScale = normalGravityScale;
                Trail_Renderer.emitting = false;
                isDashing = false;
            }
        }
    }
    private void Air_Inertia()
    {
        if(!onGround)
        {
            Main_Rigidbody.velocity += AirBaseVelocity;
        }
    }
    private void Walk(Vector2 inputVector)
    {
        inputVector.Normalize();
        if (inputVector.x !=  0)
        {
            to_idle = false;
            isWalking = true;
            if(onGround){animator.Play("Run");}
            //Debug.Log(inputVector);
            Main_Rigidbody.velocity = new Vector2(iff<float>(onGround, 1f, airMoveRatio) * _moveSpeed * inputVector.x/*transform.TransformVector(inputVector)*/, Main_Rigidbody.velocity.y); // возможен глайд в воздухе
        }
        else
        {
            Main_Rigidbody.velocity = Vector2.zero + Main_Rigidbody.velocity.y * new Vector2(0 ,1);//transform.up; // ради краткости можно velocity включить в строчку сверху и вынести из if 
            isWalking = false;
        }  
    }
    private void Movement_Input()
    {
        isWalking = false;

        DirectionalInputAxis = playerInputActions.Default.Movement.ReadValue<float>(); // возвращает нормализованный вектор 
        DirectionalInputVector = DirectionInputToVector(DirectionalInputAxis);

        if(isFacingRight && DirectionalInputVector.x < 0){Flip();}
        if(!isFacingRight && DirectionalInputVector.x > 0){Flip();}

        Walk(DirectionalInputVector); // потом придется переделать если добавлю бег
        Air_Inertia();
        Dash_Runtime();

        if(!onGround && Main_Rigidbody.velocity.y > 0.01f)
        {
            to_idle = false;
            animator.Play("Jump");
        }

        if(!onGround && Main_Rigidbody.velocity.y < -0.01f)
        {
            to_idle = false;
            animator.Play("Fall");
        }

    }

    private void Jump(InputAction.CallbackContext context)
    {
        //Debug.Log(onGround);
        if (onGround)
        {   
            animator.Play("Jump");
            //Debug.Log("Jump"+context.phase);
            JumpEnterVelocity = Main_Rigidbody.velocity;
            AirBaseVelocity =  JumpEnterVelocity * jumpEnterRatio;
            Main_Rigidbody.velocity = new Vector2(AirBaseVelocity.x, _jumpVelocity);
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    private void Switch_Control_Scheme(InputAction.CallbackContext context)
    {
        current_control_scheme = iff<string>(current_control_scheme == "Keyboard", "Gamepad", "Keyboard");
        if(current_control_scheme == "Keyboard")
        {
            var bindingGroup = playerInputActions.KeyboardScheme.bindingGroup;
            playerInputActions.bindingMask = InputBinding.MaskByGroup(bindingGroup);
            Crosshair.SetActive(true);                                                  // Переместить в контроллер
        }
        if(current_control_scheme == "Gamepad")
        {
            var bindingGroup = playerInputActions.GamepadScheme.bindingGroup;
            playerInputActions.bindingMask = InputBinding.MaskByGroup(bindingGroup);
            Crosshair.SetActive(false);
        }
    }

    private void Update()
    {
        to_idle = true;
        if(!isAttacking){Movement_Input();}
        Mouse_Input();
        Slash_Runtime();
        Debug.Log(isAttacking);
        animator.SetBool("to_idle", to_idle);
    }    

    private void OnCollisionEnter2D(Collision2D collision) // дискретно работает, может багануть
    {
        if (collision.gameObject.tag == "Floor")
        {
            floor_counter++;
        }
        //Debug.Log(collision.gameObject.name);
        if (floor_counter == 0){
            onGround = false;
        }
        else{
            onGround = true;
        }
    }
 
    private void OnCollisionExit2D (Collision2D collision)
     {
        if (collision.gameObject.tag == "Floor")
        {
            floor_counter--;
        }
        //Debug.Log("Collision Exit");
        if (floor_counter == 0)
        {
            onGround = false;
        }
        else
        {
            onGround = true;
        }
    }

}