using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    private CharacterController characterController;
    private PlayerInput playerInput;
    private Animator animator;

    private Camera followCam;

    public float speed = 6f;            //움직임 속도
    public float jumpVelocity = 20f;    //점프시 속도
    [Range(0.01f, 1f)] public float airControlPercent;

    public float speedSmoothTime = 0.1f;
    public float turnSmoothTime = 0.1f;

    private float speedSmoothVelocity;
    private float turnSmoothVelocity;

    private float currentVelocityY;

    public float currentSpeed =>
        new Vector2(characterController.velocity.x, characterController.velocity.z).magnitude;

    private void Start() {
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        followCam = Camera.main;
    }

    private void FixedUpdate() {
        if (currentSpeed > 0.2f || playerInput.fire) Rotate();

        Move(playerInput.moveInput);

        if (playerInput.jump) Jump();
    }

    private void Update() {
        UpdateAnimation(playerInput.moveInput);
    }

    public void Move(Vector2 moveInput) {
        var targetSpeed = speed + moveInput.magnitude;
        var moveDirection = Vector3.Normalize(transform.forward * moveInput.y + transform.right * moveInput.x);

        var smoothTime = characterController.isGrounded ? speedSmoothTime : speedSmoothTime / airControlPercent;


        currentVelocityY += Time.deltaTime * Physics.gravity.y;

        var velocity = moveDirection * targetSpeed + Vector3.up * currentVelocityY;

        characterController.Move(velocity * Time.deltaTime);

        if (characterController.isGrounded) currentVelocityY = 0f;      //초기화 안하면 무한정 커진다

    }

    public void Rotate() {

    }

    public void Jump() {

    }

    private void UpdateAnimation(Vector2 moveInput) {

    }
}