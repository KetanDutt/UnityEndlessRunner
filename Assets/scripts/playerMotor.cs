using UnityEngine;

/// <summary>
/// Player controller: forward movement with progressive acceleration, jump with
/// buffering + coyote time + variable height, three-lane switching, and
/// squash &amp; stretch feedback.
///
/// Architecture note: the root GameObject holds ONLY the CharacterController
/// (Unity forbids rotating or scaling a CharacterController's transform). All
/// visual polish (lean, pitch, squash/stretch, run bob) is applied to a child
/// "Visual" object instead.
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
    public float jumpStretch = 1.22f;

    /// <summary>The live player transform, used by coins/powerups for magnet pulls.</summary>
    public static Transform Player { get; private set; }

    private CharacterController controller;
    private Transform visual;
    private Vector3 visualBaseScale = Vector3.one;
    private Vector3 visualBaseLocalPos = Vector3.zero;

    private float verticalVelocity;
    private float targetLaneX;
    private bool grounded;
    private bool wasGrounded;
    private bool jumpQueued;
    private bool jumpHeld;
    private float jumpBufferTimer;
    private float jumpGraceTimer;
    private float landScaleTimer;

    // Distance accumulated for score (meters).
    private float distanceAccumulator;
    private float lastZ;

    // Touch state (swipe/tap detection).
    private bool touchActive;
    private int touchId;
    private Vector2 touchStart;
    private float touchStartTime;

    void Start()
    {
        Player = transform;

        controller = GetComponent<CharacterController>();
        if (controller == null) controller = gameObject.AddComponent<CharacterController>();

        controller.height = 2f;
        controller.radius = 0.55f;
        controller.center = new Vector3(0f, 1f, 0f);
        controller.slopeLimit = 45f;
        controller.stepOffset = 0.3f;

        // Remove colliders/rigidbodies that would fight the CharacterController.
        // IMPORTANT: the CharacterController itself is a Collider subclass, so
        // it is excluded here — destroying it would break movement entirely.
        Collider[] colliders = GetComponents<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != controller)
                Destroy(colliders[i]);
        }
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);

        EnsureVisual();

        targetLaneX = transform.position.x;
        lastZ = transform.position.z;
        Game.Speed = speed;
    }

    void OnDestroy()
    {
        if (Player == transform) Player = null;
    }

    /// <summary>Finds or builds the child object that carries the visuals.</summary>
    void EnsureVisual()
    {
        visual = transform.Find("Visual");
        if (visual != null)
        {
            visualBaseScale = visual.localScale;
            visualBaseLocalPos = visual.localPosition;
            return;
        }

        // Fallback (should never happen with PlayerSpawner): build a fresh
        // capsule child so the controller object itself stays untouched.
        Renderer rootRenderer = GetComponent<Renderer>();
        Material mat = rootRenderer != null ? rootRenderer.material : null;

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "Visual";
        Collider[] cs = go.GetComponents<Collider>();
        for (int i = 0; i < cs.Length; i++) Destroy(cs[i]);

        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 1f, 0f);
        go.transform.localScale = Vector3.one;
        if (mat != null)
        {
            Renderer r = go.GetComponent<Renderer>();
            if (r != null) r.material = mat;
        }

        visual = go.transform;
        visualBaseScale = visual.localScale;
        visualBaseLocalPos = visual.localPosition;
    }

    void Update()
    {
        if (Game.GameOver) return;

        // Physics (gravity + grounding) always runs so the player settles onto
        // the track during the countdown. Everything else waits for the run.
        TickVertical();

        if (!Game.IsRunning || Game.IsPaused) return;

        HandleInput();
        UpdateMovement();
        UpdateScoreDistance();
        CollectNearbyCoins();
        CollectPowerups();
        TickMagnet();
        UpdateVisuals();
    }

    // ------------------------------------------------------------------
    // Input.
    // ------------------------------------------------------------------

    void HandleInput()
    {
        jumpBufferTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.W) || Input.GetMouseButtonDown(0))
        {
            Jump();
        }

        jumpHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow) ||
                   Input.GetKey(KeyCode.W) || Input.GetMouseButton(0);

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            MoveLane(-1);
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            MoveLane(1);

        HandleTouch();
    }

    void HandleTouch()
    {
        if (Input.touchCount == 0)
        {
            touchActive = false;
            return;
        }

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.phase == TouchPhase.Began)
            {
                touchActive = true;
                touchId = touch.fingerId;
                touchStart = touch.position;
                touchStartTime = Time.time;
            }
            else if (touchActive && touch.fingerId == touchId && touch.phase == TouchPhase.Ended)
            {
                touchActive = false;
                Vector2 delta = touch.position - touchStart;
                float elapsed = Time.time - touchStartTime;

                if (elapsed < 0.4f && delta.magnitude < 40f)
                {
                    // Tap: switch lane on the tapped side.
                    MoveLane(touch.position.x < Screen.width * 0.5f ? -1 : 1);
                }
                else if (delta.y > 60f)
                {
                    Jump();
                }
                else if (Mathf.Abs(delta.x) > 40f)
                {
                    MoveLane(delta.x > 0f ? 1 : -1);
                }
            }
        }
    }

    void Jump()
    {
        jumpQueued = true;
        jumpBufferTimer = 0.12f;
    }

    public void MoveLane(int direction)
    {
        if (direction == 0) return;
        float next = Mathf.Clamp(targetLaneX + direction * Game.LaneWidth,
                                 -Game.LaneWidth, Game.LaneWidth);
        if (Mathf.Abs(next - targetLaneX) < 0.001f) return;
        targetLaneX = next;
        Sfx.LaneChange();
    }

    // ------------------------------------------------------------------
    // Movement.
    // ------------------------------------------------------------------

    void TickVertical()
    {
        wasGrounded = grounded;
        grounded = controller.isGrounded;

        if (grounded && !wasGrounded)
        {
            verticalVelocity = 0f;
            landScaleTimer = landSquashTime;
            Vfx.LandDust(transform.position);
            Sfx.Land();
            Sfx.Haptic();
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
            verticalVelocity -= gravity * (1f / variableJumpMultiplier - 1f) * Time.deltaTime;

        verticalVelocity -= gravity * Time.deltaTime;
        verticalVelocity = Mathf.Clamp(verticalVelocity, -45f, 45f);
    }

    void UpdateMovement()
    {
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
                if (Score.Current != null) Score.Current.AddDistanceScore(meters);
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
                if (Score.Current != null)
                {
                    Score.Current.AddCoin();
                    GameUI.SpawnCoinPopup("+" + Score.Current.scorePerCoin, coin.transform.position);
                }

                Vfx.CoinSparkle(coin.transform.position);
                Sfx.Coin();
                coin.Collect();
            }
        }
    }

    void CollectPowerups()
    {
        var list = Powerup.Active;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            Powerup p = list[i];
            if (p == null) continue;

            Vector3 delta = p.transform.position - transform.position;
            delta.y = 0f;
            if (delta.sqrMagnitude < p.collectRadius * p.collectRadius)
            {
                Game.MagnetTime = 4.5f;
                Vfx.MagnetBurst(p.transform.position);
                Sfx.PowerUp();
                Sfx.Haptic();
                if (GameUI.Current != null) GameUI.Current.ShowBanner("MAGNET!");
                p.Collect();
            }
        }
    }

    void TickMagnet()
    {
        if (Game.MagnetTime > 0f)
            Game.MagnetTime = Mathf.Max(0f, Game.MagnetTime - Time.deltaTime);
    }

    // ------------------------------------------------------------------
    // Visual polish (applied to the Visual child only).
    // ------------------------------------------------------------------

    void UpdateVisuals()
    {
        if (visual == null) return;

        // Squash & stretch.
        Vector3 target = visualBaseScale;
        if (landScaleTimer > 0f)
        {
            landScaleTimer -= Time.deltaTime;
            float p = 1f - Mathf.Clamp01(landScaleTimer / landSquashTime);
            float squash = Easing.QuadOut(p);
            float xz = Mathf.Lerp(1f, 1f + (1f - landSquash) * 0.6f, squash);
            float y = Mathf.Lerp(1f, landSquash, squash);
            target = new Vector3(visualBaseScale.x * xz, visualBaseScale.y * y, visualBaseScale.z * xz);
        }
        else if (!grounded && verticalVelocity > 0.5f)
        {
            // Stretch while rising.
            float xz = 2f - jumpStretch;
            target = new Vector3(visualBaseScale.x * xz, visualBaseScale.y * jumpStretch, visualBaseScale.z * xz);
        }

        visual.localScale = Vector3.Lerp(visual.localScale, target, Easing.Damp(16f, Time.deltaTime));

        // Lane lean (visual only) + a gentle pitch while airborne.
        float lean = Mathf.Clamp((targetLaneX - transform.position.x) * 4f, -1f, 1f);
        float pitch = grounded ? 0f : Mathf.Clamp(verticalVelocity * 0.6f, -16f, 12f);
        Quaternion targetRot = Quaternion.Euler(pitch, lean * 14f, -lean * 10f);
        visual.localRotation = Quaternion.Slerp(visual.localRotation, targetRot, Easing.Damp(12f, Time.deltaTime));

        // Subtle run bob.
        Vector3 bobPos = visualBaseLocalPos + Vector3.up * Mathf.Sin(distanceAccumulator * 11f) * 0.03f;
        visual.localPosition = Vector3.Lerp(visual.localPosition, bobPos, Easing.Damp(14f, Time.deltaTime));
    }

    // ------------------------------------------------------------------
    // Death.
    // ------------------------------------------------------------------

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

        // Make sure we are not stuck paused / time-scaled when dying.
        Game.IsPaused = false;
        Time.timeScale = 1f;

        Vfx.DeathExplosion(transform.position);
        Sfx.Death();
        Sfx.Haptic();
        cameraMotor.Shake(0.45f);

        if (Score.Current != null) Score.Current.FinalizeScore();

        DeathMenu menu = FindObjectOfType<DeathMenu>();
        if (menu != null) menu.Show();
    }
}
