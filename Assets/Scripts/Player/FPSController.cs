using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class FPSController : MonoBehaviour
{
    public CharacterController controller;
    [SerializeField] UIManager uiManager;
    public float jumpSpeed = 4f;
    public float speed = 12f;
    public float sprintSpeed = 20f;
    public float normSpeed = 12f;
    public float gravity = -9.81f;
    public float jumpForce = 5;

    public bool lockCursor;
   // public Vector2 pitchMinMax = new Vector2(-40, 85);

    Camera cam;
    public float yaw;
    public float pitch;
    float smoothYaw;
    float smoothPitch;
    float verticalVelocity;
    Vector3 velocity;

    bool jumping;
    float lastGroundedTime;
    public float maxStamina = 100f;
    public float lastRegen;
    public float staminaRegenSpeed = 5;
    public static float stamina;
    public bool isSprinting = false;
    public bool isJumping = false;

    public Transform groundCheck;
    public float groundDistance = 0.1f;
    public LayerMask groundMask;
    private Coroutine regen;

    bool isGrounded;
    private void Awake()
    {
        uiManager = FindObjectOfType<UIManager>();
    }
    void Start()
    {
        cam = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
        jumping = false;

        controller = GetComponent<CharacterController>();

        yaw = transform.eulerAngles.y;
        pitch = cam.transform.localEulerAngles.x;
        smoothYaw = yaw;
        smoothPitch = pitch;
        stamina = maxStamina;
    }

    void Update()
    {
       
        //movement
        Debug.Log("Stamina is: " + stamina);

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        //sprint
        if (Input.GetKey(KeyCode.LeftShift) && stamina > 0 && isGrounded)
        {
            speed = sprintSpeed;
            stamina -= 20 * Time.deltaTime;
            uiManager.UpdateStaminaSlider();
            Debug.Log("Stamina draining");
            isSprinting = true;
            Debug.Log("left shift down " + isSprinting);
            StopCoroutine(RegenStamina());

        }
        if (Input.GetKeyUp(KeyCode.LeftShift) && isGrounded)
        {
            speed = 12f;
            isSprinting = false;
            Debug.Log("left shift up " + isSprinting);
        }
        Vector3 move = transform.right * x + transform.forward * z;
        //Jump

        var flags = controller.Move(move * speed * Time.deltaTime);

        if (flags == CollisionFlags.Below)
        {
            jumping = false;
            lastGroundedTime = Time.time;
            Debug.Log("last ground time: " + lastGroundedTime);
            verticalVelocity = 0;
            Debug.Log("collision flag below");
            speed = normSpeed;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("jUMP PRESSED");
            float timeSinceLastTouchedGround = Time.time - lastGroundedTime;
            if (isGrounded || (!jumping && timeSinceLastTouchedGround < 0.15f))
            {
                Debug.Log("Jumping");
                jumping = true;
                velocity.y = jumpForce;
                speed = jumpSpeed;
                stamina -= 30;
                StopCoroutine(RegenStamina());
            }
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        if (isGrounded && !isSprinting && stamina < 100f)
        {
            StartCoroutine(RegenStamina());
            Debug.Log("Coroutine is running");

            if (regen != null)
            {
                StopCoroutine(RegenStamina());
                Debug.Log("coroutine has stopped");
            }

            regen = StartCoroutine(RegenStamina());
        }

    }

    private IEnumerator RegenStamina()
    {
        yield return new WaitForSeconds(2);

        if (stamina < maxStamina) ;
        {
            stamina += maxStamina / 1000;
            if (stamina > 100)
            {
                stamina = 100;
            }
            uiManager.UpdateStaminaSlider();
            yield return new WaitForEndOfFrame();
        }
        regen = null;
    }
    
}
