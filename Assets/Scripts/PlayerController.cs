using UnityEngine;

public class PlayerController : MonoBehaviour {
  // Object components
  Rigidbody2D rb;
  BoxCollider2D bodycollider;

  // Other variables
  float velocityXSmoothing;
  float accelerationTimeInAir = 0.2f;
  float accelerationTimeGrounded = 0.1f;

  // Player inputs
  public float horizontal;
  public bool crouchHeld;
  public bool crouchPressed;
  public bool jumpPressed;
  public bool jumpHeld;
  public bool jumpRelease;

  // Player movement properties
  public float speed = 8f;
  public float speedCrouching = 3f;
  public int direction = 1;
  // Jump properties
  float jumpTimeCounter;
  public float jumpTime = 0.35f;
  public float timeToJumpApex = 0.4f;
  public float jumpHeight = 4;
  float jumpGravity;
  float jumpForce;
  // Sliding properties
  public Vector2 wallClimbing;
  public Vector2 wallJumpOff;
  public Vector2 wallLeap;
  public float wallSlideSpeedMax = 1.2f;
  public float slidingJumpTime = 0.5f;
  float wallDirecX;
  float slidingJumpTimeCounter;


  // Player states
  public bool isJumping;
  public bool isGrounded;
  public bool isCrouching;
  public bool nextToWall;
  public bool isSliding;
  
  // Ground check variables
  public Transform groundCheck;
  public float groundCheckRadius;

  // Head Blocked check variables
  public Transform headCheck;
  public float headCheckRadius;

  // Wall Check check variables
  public Transform wallCheck;
  public float wallCheckRadius;

  // Collider values
  Vector2 standingColliderSize;
  Vector2 standingColliderOffset;
  Vector2 crouchingColliderSize;
  Vector2 crouchingColliderOffset;

  public LayerMask groundLayer;   // Ground layer variable

  void Start()
  {
    rb = GetComponent<Rigidbody2D>();
    bodycollider = GetComponent<BoxCollider2D>();


    standingColliderSize = bodycollider.size;
    standingColliderOffset = bodycollider.offset;

    crouchingColliderSize = new Vector2(standingColliderSize.x, standingColliderSize.y * 0.5f);
    crouchingColliderOffset = new Vector2(standingColliderOffset.x, -0.33f);

    jumpGravity = -(2 * jumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		jumpForce = Mathf.Abs(jumpGravity) * timeToJumpApex;
  }

  void Update()
  {
    CheckInputs();
  }

  void FixedUpdate()
  {
    GroundMovements();
    SlidingDetector();
    JumpController();
  }

  void CheckInputs()
  {
    horizontal = Input.GetAxisRaw("Horizontal1");
    crouchPressed = Input.GetButtonDown("Crouch1");
    crouchHeld = Input.GetButton("Crouch1");
    jumpPressed = Input.GetButtonDown("Jump1");
    jumpHeld = Input.GetButton("Jump1");
    jumpRelease = Input.GetButtonUp("Jump1");
  }

  void GroundMovements()
  {
    GroundCheck();
    CrouchCheck();
    WallCheck();

    float targetVelocityX;
    if (isCrouching && isGrounded)
      targetVelocityX = horizontal * speedCrouching;
    else
      targetVelocityX = horizontal * speed;

    rb.velocity = new Vector2(
      Mathf.SmoothDamp(rb.velocity.x, targetVelocityX, ref velocityXSmoothing, (isGrounded) ? accelerationTimeGrounded : accelerationTimeInAir)
      , rb.velocity.y);

    if ((horizontal < 0 && direction > 0) || (horizontal > 0 && direction < 0))
      Flip();
  }

  void JumpController()
  {
    if (jumpPressed && !isJumping)
    {
      if (isSliding)
      {
        if(horizontal == 0)
          rb.velocity = new Vector2(wallJumpOff.x * -wallDirecX, wallJumpOff.y);
        else if(horizontal == wallDirecX)
          rb.velocity = new Vector2(wallClimbing.x * -wallDirecX, wallClimbing.y);
        else
          rb.velocity = new Vector2(wallLeap.x * -wallDirecX, wallLeap.y);
        
        isSliding = false;
        isJumping = true;
      }

      if (isGrounded)
      {
        isJumping = true;
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
      }
    }
    
    if (jumpHeld && isJumping)
    {
      if (jumpTimeCounter > 0)
      {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpTimeCounter -= Time.deltaTime;
      }
      else isJumping = false;
    }

    if (jumpRelease)
      isJumping = false;

    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + jumpGravity * Time.deltaTime);
  }

  void SlidingDetector()
  {
    if (!isGrounded && crouchHeld && nextToWall)
    {
      if (!isSliding)
        wallDirecX = transform.localScale.x;
      isSliding = true;
      isJumping = false;

      if (rb.velocity.y < -wallSlideSpeedMax)
      {
        rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeedMax);
      }
      
      if (slidingJumpTimeCounter > 0)
      {
        velocityXSmoothing = 0;
        rb.velocity = new Vector2(0, rb.velocity.y);

        if (horizontal != wallDirecX && horizontal != 0) 
          slidingJumpTimeCounter -= Time.deltaTime; 
        else
          slidingJumpTimeCounter = slidingJumpTime;
      }
      else
        slidingJumpTimeCounter = slidingJumpTime;
    }
  }

  void GroundCheck()
  {
    isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

    if (isGrounded) {
      jumpTimeCounter = jumpTime;
      isSliding = false;
      wallDirecX = 0;
    }
  }

  void CrouchCheck()
  {
    if (crouchHeld)
    {
      isCrouching = true;
      bodycollider.size = crouchingColliderSize;
      bodycollider.offset = crouchingColliderOffset;
    }
    else
    {
      isCrouching = false;
      bodycollider.size = standingColliderSize;
      bodycollider.offset = standingColliderOffset;
    }
  }

  void WallCheck()
  {
    nextToWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, groundLayer);
  }

  void Flip()
  {
    direction *= -1;
    Vector3 scale = transform.localScale;
    scale.x *= -1;
    transform.localScale = scale;
  }
}