using UnityEngine;

/// <summary>
/// Player controller: forward movement with progressive acceleration, jump with
/// buffering + coyote time + variable height, three-lane switching, squash &amp;
/// stretch feedback, and death handling.
///
/// The player is created and configured by PlayerSpawner at runtime, so the game
/// works regardless of what the scene serializes.
/// </summary>
public class playerMotor : MonoBehaviour
{
    [Header("Forward speed")]
    public float speed = 9f;
    public float minSpeed = 7f;
    public float maxSpeed = 26f;
    public float acceleration = 0.45f;   // world units per second, per second

    [Header("Jump")]
    public float jumpForce = 11.5f;
    public float gravity = 28f;
    public float variableJumpMultiplier = 0.55f;

    [Header("Lane switching")]
    public float laneChangeSpeed = 12f;

    [Header("Squash & stretch")]
    public float landSquash = 0.72f;
    public float landSquashTime = 0.12f;

    private CharacterController controller;

    private float verticalVelocity;
    private float targetLaneX;
    private bool grounded;
    private bool jumpQueued;
    private bool jumpHeld;
    private float jumpBufferTimer;
    private float jumpGraceTimer;
    private float landScaleTimer;
    private Vector3 visualBaseScale = Vector3.one;
    private bool scaleDirty;

    // Distance accumulated for score (meters).
    private float distanceAccumulator;
    private float lastZ;
    private bool lastZValid;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null) controller = gameObject.AddComponent<CharacterController>();

        controller.height = 2f;
        controller.radius = 0.55f;
        controller.center = new Vector3(0f, 1f, 0f);
        controller.slopeLimit = 45f;
        controller.stepOffset = 0.3f;

        // Remove colliders/rigidbodies that would fight the CharacterController.
        Collider[] colliders = GetComponents<Collider>();
        for (int i = 0; i < colliders.Length; i++) Destroy(colliders[i]);
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);

        targetLaneX = transform.position.x;
        visualBaseScale = transform.localScale;
        lastZ = transform.position.z;
        lastZValid = true;
    }

    void Update()
    {
        if (!Game.IsRunning || Game.GameOver) return;

        HandleInput();
        UpdateMovement();
        UpdateScoreDistance();
        CollectNearbyCoins();
        UpdateVisuals();
    }

    void HandleInput()
    {
        jumpBufferTimer -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            jumpQueued = true;
            jumpBufferTimer = 0.12f;
        }

        jumpHeld = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            MoveLane(-1);
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            MoveLane(1);

        HandleTouch();
    }

    void HandleTouch()
    {
        if (Input.touchCount == 0) return;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.phase != TouchPhase.Began) continue;

            if (touch.position.x < Screen.width * 0.5f)
                MoveLane(-1);
            else
                MoveLane(1);
        }
    }

    public void MoveLane(int direction)
    {
        targetLaneX = Mathf.Clamp(targetLaneX + direction * Game.LaneWidth,
                                  -Game.LaneWidth, Game.LaneWidth);
    }

    void UpdateMovement()
    {
        bool wasGrounded = grounded;
        grounded = controller.isGrounded;

        if (grounded && !wasGrounded)
        {
            verticalVelocity = 0f;
            landScaleTimer = landSquashTime;
            Vfx.LandDust(transform.position);
            Sfx.Land();
        }

        // Coyote time: allow a jump briefly after running off a ledge.
        if (grounded) jumpGraceTimer = 0.1f;
        else jumpGraceTimer -= Time.deltaTime;

        if (jumpQueued && jumpBufferTimer > 0f && jumpGraceTimer > 0f)
        {
            verticalVelocity = jumpForce;
            grounded = false;
            jumpGraceTimer = 0f;
            jumpBufferTimer = 0f;
            jumpQueued = false;
            Vfx.JumpDust(transform.position);
            Sfx.Jump();
        }
        else if (jumpQueued && jumpBufferTimer <= 0f)
        {
            jumpQueued = false;
        }

        // Variable jump height: releasing early cancels part of the ascent.
        if (!jumpHeld && verticalVelocity > 0f)
            verticalVelocity += Physics.gravity.y * (1f / variableJumpMultiplier - 1f) * Time.deltaTime;

        verticalVelocity += Physics.gravity.y * Time.deltaTime;
        verticalVelocity = Mathf.Clamp(verticalVelocity, -45f, 45f);

        // Lane switching: horizontal motion goes through CharacterController.Move
        // so the controller never desyncs from its transform.
        float currentX = transform.position.x;
        float newX = Mathf.MoveTowards(currentX, targetLaneX, laneChangeSpeed * Time.deltaTime);
        float vx = (newX - currentX) / Mathf.Max(Time.deltaTime, 0.0001f);

        Vector3 velocity = new Vector3(vx, verticalVelocity, speed);
        controller.Move(velocity * Time.deltaTime);

        speed = Mathf.MoveTowards(speed, maxSpeed, acceleration * Time.deltaTime);
        if (speed < minSpeed) speed = minSpeed;
        Game.Speed = speed;
    }

    void UpdateScoreDistance()
    {
        float dz = transform.position.z - lastZ;
        lastZ = transform.position.z;
        if (dz > 0f)
        {
            distanceAccumulator += dz;
            if (distanceAccumulator >= 1f)
            {
                int meters = (int)distanceAccumulator;
                distanceAccumulator -= meters;
                Score score = FindObjectOfType<Score>();
                if (score != null) score.AddDistanceScore(meters);
            }
        }
    }

    void CollectNearbyCoins()
    {
        var coins = CoinSpin.Active;
        for (int i = coins.Count - 1; i >= 0; i--)
        {
            CoinSpin coin = coins[i];
            if (coin == null) continue;

            Vector3 delta = coin.transform.position - transform.position;
            delta.y = 0f;
            if (delta.sqrMagnitude < coin.collectRadius * coin.collectRadius)
            {
                Score score = FindObjectOfType<Score>();
                if (score != null) score.AddCoin();

                Vfx.CoinSparkle(coin.transform.position);
                Sfx.Coin();
                coin.Collect();
            }
        }
    }

    void UpdateVisuals()
    {
        float scaleY = visualBaseScale.y;
        float scaleXZ = visualBaseScale.x;

        if (landScaleTimer > 0f)
        {
            landScaleTimer -= Time.deltaTime;
            float p = 1f - Mathf.Clamp01(landScaleTimer / landSquashTime);
            float squash = Easing.QuadOut(p);
            scaleY = visualBaseScale.y * Mathf.Lerp(1f, landSquash, squash);
            scaleXZ = visualBaseScale.x * Mathf.Lerp(1f, 1f + (1f - landSquash) * 0.6f, squash);
            scaleDirty = true;
        }

        if (scaleDirty)
        {
            transform.localScale = new Vector3(scaleXZ, scaleY, scaleXZ);
            if (landScaleTimer <= 0f)
            {
                transform.localScale = visualBaseScale;
                scaleDirty = false;
            }
        }

        // Lean into lane changes (visual only).
        float lean = Mathf.Clamp((targetLaneX - transform.position.x) * 5f, -1f, 1f);
        Quaternion targetRotation = Quaternion.Euler(0f, 180f + lean * 12f, -lean * 8f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                                              Easing.Damp(12f, Time.deltaTime));
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!Game.IsRunning || Game.GameOver) return;

        if (hit.collider.CompareTag("Enemy"))
            Death();
    }

    public void Death()
    {
        if (Game.GameOver) return;
        Game.GameOver = true;
        Game.IsRunning = false;

        Vfx.DeathExplosion(transform.position);
        Sfx.Death();

        Score score = FindObjectOfType<Score>();
        if (score != null) score.FinalizeScore();

        DeathMenu menu = FindObjectOfType<DeathMenu>();
        if (menu != null) menu.Show();
    }
}
