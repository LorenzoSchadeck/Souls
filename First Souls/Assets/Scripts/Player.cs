using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player References")]
    [Space(5)]
    [SerializeField] Transform cam;
    
    [Header("Animation")]
    [Space(5)]
    [SerializeField] Animator playerAnimator;

    [Header("Controller")]
    [Space(5)]
    [SerializeField] float runSpeed;
    [SerializeField] float walkSpeed;
    [SerializeField] float gravity;
    [SerializeField] float turnSmoothTime = 0.1f;
    float turnSmoothSpeed;
    CharacterController controller;
    public static bool canMove = true;

    [Header("Roll")]
    [Space(5)]
    [SerializeField] float rollSpeed;
    [SerializeField] float rollDuration;
    private bool isRolling = false;
    private Vector3 rollDirection;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void FixedUpdate()
    {
        if (canMove)
        {
            if (isRolling)
            {
                RollPlayer();
            }
            else
            {
                MovePlayer();
                StartRoll();           
            }
        }
    }

    private void MovePlayer()
    {
        float moveH = Input.GetAxisRaw("Horizontal");
        float moveV = Input.GetAxisRaw("Vertical");

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float speed = isRunning ? runSpeed : walkSpeed;
        bool isMoving = moveH != 0 || moveV != 0;

        Vector3 direction = new Vector3(moveH, 0f, moveV).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothSpeed, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir * speed * Time.deltaTime);
        }

        Vector3 gravityVector = new Vector3(0, gravity, 0);
        controller.Move(gravityVector * Time.deltaTime);

        if (isMoving)
        {
            if (isRunning)
            {
                playerAnimator.SetBool("isRunning", true);
                playerAnimator.SetBool("isWalking", false);
                playerAnimator.SetBool("isIdle", false);
            }
            else
            {
                playerAnimator.SetBool("isRunning", false);
                playerAnimator.SetBool("isWalking", true);
                playerAnimator.SetBool("isIdle", false);
            }
        }
        else
        {
            playerAnimator.SetBool("isRunning", false);
            playerAnimator.SetBool("isWalking", false);
            playerAnimator.SetBool("isIdle", true);
        } 
    }

    private void StartRoll()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isRolling = true;
            rollDirection = transform.forward;
            playerAnimator.SetTrigger("isRolling");
            StartCoroutine(RollCoroutine());     
        }  
    }

    private void RollPlayer()
    {
        controller.Move(rollSpeed * Time.deltaTime * rollDirection);
    }

    private IEnumerator RollCoroutine()
    {
        yield return new WaitForSeconds(rollDuration);
        isRolling = false;
    }
}
