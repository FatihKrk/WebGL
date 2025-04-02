using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    MoveButtons moveButtons;
    [SerializeField] Animator animator;
    [SerializeField] Transform cameraParent;
    Camera playerCamera;
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpPower = 7f;
    public float gravity = 10f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;
    public float climbSpeed = 3f;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;

    private bool canMove = true;
    private bool isClimbing = false;

    private EventSystem eventSystem;

    void Awake()
    {
        playerCamera = Camera.main;

        GameObject obj = GameObject.Find("MovementsPanel");
        moveButtons = obj.GetComponent<MoveButtons>();

        characterController = GetComponent<CharacterController>();
        eventSystem = EventSystem.current; // EventSystem referansını al
    }

    void Update()
    {
        if (moveButtons.avatar && Cursor.lockState == CursorLockMode.Locked)
        {
            OnApplicationFocus(false);
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);

            // UI etkileşimini engelle
            if (eventSystem.IsPointerOverGameObject())
            {
                // Eğer UI öğesinin üzerine gelindiyse, hareket etmeyi engelle
                return;
            }

            playerCamera.transform.position = cameraParent.position;
            playerCamera.transform.SetParent(cameraParent);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                OnApplicationFocus(true);
                playerCamera.transform.SetParent(null);
                moveButtons.Avatar();
                Destroy(gameObject);
            }

            if (isClimbing)
            {
                animator.SetBool("Walk", false);
                Climb();
            }
            else
            {
                animator.SetBool("Climb", false);
                animator.SetBool("ClimbDown", false);
                Move();
            }

            if (!isClimbing && Input.GetButton("Vertical") || Input.GetButton("Horizontal"))
            {
                animator.SetBool("Walk", true);
            }
            else
            {
                animator.SetBool("Walk", false);
            }
        }
        else
        {
            OnApplicationFocus(true);
            playerCamera.transform.SetParent(null);
            
        }
    }

    private void Move()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.R) && canMove)
        {
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;
            runSpeed = crouchSpeed;
        }
        else
        {
            characterController.height = defaultHeight;
            walkSpeed = 3f;
            runSpeed = 6f;
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void Climb()
    {
        float verticalInput = Input.GetAxis("Vertical");

        if (verticalInput > 0) // İleriye basıldığında
        {
            animator.SetBool("Climb", true);
            animator.SetBool("ClimbDown", false);
        }
        else if (verticalInput < 0) // Geriye basıldığında
        {
            animator.SetBool("ClimbDown", true);
            animator.SetBool("Climb", false);
        }
        else // Hiçbir tuşa basılmadığında
        {
            animator.SetBool("Climb", false);
            animator.SetBool("ClimbDown", false);
        }

        moveDirection = new Vector3(0, verticalInput * climbSpeed, 0);
        characterController.Move(moveDirection * Time.deltaTime);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Climbable"))
        {
            isClimbing = true;
            gravity = 0; // Tırmanma sırasında yerçekimi etkisini kaldır
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Climbable"))
        {
            isClimbing = false;
            gravity = 10; // Tırmanma bittiğinde yerçekimi tekrar devreye girsin
        }
    }

    // UI etkileşim kontrolü
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            // Eğer oyun penceresi yeniden odaklanmışsa, EventSystem'i etkinleştir
            eventSystem.enabled = true;
        }
        else
        {
            // Eğer oyun penceresi odak dışıysa, UI ile etkileşimi devre dışı bırak
            eventSystem.enabled = false;
        }
    }
}
