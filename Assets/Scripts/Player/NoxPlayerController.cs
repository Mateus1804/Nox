using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class NoxPlayerController : MonoBehaviour
{
    // Um controlador para input, vida e integração com o motor; colisões ficam no Rigidbody.
    public static NoxPlayerController Active;

    [FormerlySerializedAs("rigid")]
    public Rigidbody physicsBody;

    [FormerlySerializedAs("soundBossWater")]
    public AudioSource bossMusic;
    private bool bossEncounterStarted;

    [SerializeField, Min(1f)] private float maxHealth = 3f;
    [SerializeField, Min(0.1f)] private float jumpHeight = 3.5f;
    private Vector3 desiredMovement;
    private NoxPlayerMotor motor;
    private NoxPlayerCombat combat;
    private readonly NoxDamageWindow damageWindow = new NoxDamageWindow();
    [SerializeField, Min(0f)] private float damageProtectionSeconds = 0.6f;
    private bool jumpRequested;
    private bool isPushing;
    private NoxCharacterVisual visual;
    private float movementSpeed;
    public bool IsAlive => health > 0;
    public float AnimationSpeed => NoxSession.CanPlay && motor != null ? motor.LocomotionSpeed : 0;
    public void SetPushing(bool value)
    {
        isPushing = value;
    }

    [FormerlySerializedAs("mainCameraObj")]
    public GameObject cameraObject;

    [FormerlySerializedAs("newOffset")]
    public Vector3 bossCameraOffset;

    [FormerlySerializedAs("bossCanvas")]
    public GameObject bossHealthCanvas;

    [FormerlySerializedAs("invisibleWall")]
    public GameObject arenaBarrier;

    [FormerlySerializedAs("crosshair")]
    public GameObject attackOrigin;
    [FormerlySerializedAs("slash1")]
    [FormerlySerializedAs("firstSlash")]
    public GameObject firstAttackPrefab;
    [FormerlySerializedAs("slash2")]
    [FormerlySerializedAs("secondSlash")]
    public GameObject secondAttackPrefab;
    [FormerlySerializedAs("slash3")]
    [FormerlySerializedAs("thirdSlash")]
    public GameObject thirdAttackPrefab;
    [FormerlySerializedAs("slashCount")]
    public float comboStage = 0;
    [FormerlySerializedAs("startTime")]
    [HideInInspector]
    public float attackStartedAt = 0;
    [FormerlySerializedAs("currentTime")]
    [HideInInspector]
    public float attackElapsed = 0;
    [FormerlySerializedAs("shootTime")]
    [HideInInspector]
    public bool legacyShootFlag = false;

    [FormerlySerializedAs("WhatIsGround")]
    public LayerMask groundLayers;

    [FormerlySerializedAs("jumpForce")]
    [HideInInspector]
    public float legacyJumpForce; // Mantido para compatibilidade; o motor usa jumpHeight.

    [FormerlySerializedAs("grounded")]
    public bool isGrounded = false;

    [FormerlySerializedAs("hp")]
    public float health;

    [FormerlySerializedAs("healthBar")]
    public Image healthImage;

    [FormerlySerializedAs("healthBarObj")]
    [FormerlySerializedAs("healthBarOBj")]
    public GameObject healthBarObject;

    [FormerlySerializedAs("healthBar_Canvas")]
    public GameObject healthCanvas;

    public void ReadPushInput()
    {
        desiredMovement = ReadMovementDirection();
        movementSpeed = PushSpeed;
    }

    public void ReadMovementInput()
    {
        desiredMovement = ReadMovementDirection();
        movementSpeed = NormalSpeed;
        if (desiredMovement.sqrMagnitude > 0.001f && (combat == null || !combat.LocksFacing))
            transform.forward = desiredMovement;
    }

    public void ReadJumpInput()
    {
        if (!isPushing && Input.GetKeyDown(KeyCode.Space))
            jumpRequested = true;
    }

    public void CharacterSlash1()
    {
        QueueAttack();
    }
    public void CharacterSlash2()
    {
        QueueAttack();
    }
    public void CharacterSlash3()
    {
        QueueAttack();
    }
    public void QueueAttack()
    {
        // Métodos preservados para UnityEvents; o mesmo clique não entra duas vezes na fila.
        if (NoxSession.CanPlay && IsAlive && !isPushing)
            combat?.Request();
    }

    public float ApplyHealthDelta(float currentHealth, float delta, Image bar)
    {
        float next = NoxRules.AdjustHealth(currentHealth, delta, maxHealth);
        if (bar != null)
            bar.fillAmount = NoxRules.HealthRatio(next, maxHealth);
        return next;
    }

    public void RefreshHealthDisplay()
    {
        UpdateHealthBar(healthImage, health, maxHealth);
    }

    public void CloseArenaBarrier()
    {
        if (arenaBarrier != null)
            arenaBarrier.SetActive(true);
    }

    public bool TakeDamage(float amount)
    {
        if (!NoxSession.CanPlay || !IsAlive)
            return false;
        if (!damageWindow.TryApply(health, amount, maxHealth, Time.time, damageProtectionSeconds, out float next))
            return false;
        health = next;
        combat?.Cancel();
        if (visual != null)
            visual.PulseDamage();
        RefreshHealthDisplay();
        return true;
    }

    public bool Heal(float amount)
    {
        if (!NoxSession.CanPlay || !IsAlive || amount <= 0 || float.IsNaN(amount) || float.IsInfinity(amount))
            return false;
        float next = NoxRules.AdjustHealth(health, amount, maxHealth);
        if (next <= health)
            return false;
        health = next;
        RefreshHealthDisplay();
        return true;
    }

    private void ApplyContactDamage(Collider source)
    {
        if (CanReceiveContactDamage(source, GetInstanceID()))
            TakeDamage(1f);
    }

    protected virtual void OnCollisionStay(Collision collision)
    {
        ApplyContactDamage(collision.collider);
    }
    protected virtual void OnTriggerStay(Collider other)
    {
        ApplyContactDamage(other);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (!NoxSession.CanPlay || !IsAlive)
            return;
        ApplyContactDamage(collision.collider);
        if (collision.gameObject.CompareTag("Impulse") && physicsBody != null)
            physicsBody.AddForce(Vector3.up * ImpulseForce);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!NoxSession.CanPlay || !IsAlive)
            return;
        if (other.CompareTag("BulletTurret"))
        {
            NoxEnemyProjectile projectile = other.GetComponentInParent<NoxEnemyProjectile>();
            if (projectile != null)
                projectile.Hit(other: GetComponent<Collider>());
            else
            {
                other.enabled = false;
                TakeDamage(1f);
                Destroy(other.gameObject);
            }
        }
        if (other.CompareTag("Heal") && other.enabled && Heal(1f))
        {
            // Desabilitar antes de Destroy evita coleta duplicada no mesmo frame.
            other.enabled = false;
            other.gameObject.SetActive(false);
            Destroy(other.gameObject);
        }
        ApplyContactDamage(other);

        if (other.gameObject.tag == "AreaWin")
        {
            NoxSession.LoadNextScene();
            return;
        }

        if (other.gameObject.tag == "Limbo")
        {
            health = 0;
            NoxSession.SetState(NoxGameState.Lost);
            return;
        }

        if (other.gameObject.tag == "LifeCamera" && !bossEncounterStarted)
        {
            bossEncounterStarted = true;
            NoxFollowCamera follow = cameraObject != null ? cameraObject.GetComponent<NoxFollowCamera>() : null;
            if (follow != null)
                follow.offset = bossCameraOffset;
            if (bossHealthCanvas != null)
                bossHealthCanvas.SetActive(true);
            Invoke(nameof(CloseArenaBarrier), 2);

            PlayBossMusic();
        }

    }

    public void PlayBossMusic()
    {
        if (bossMusic != null)
        {
            if (NoxGameInterface.Singleton != null)
                NoxGameInterface.Singleton.StopMusic();
            bossMusic.Play();
        }
    }

    protected virtual void Awake()
    {
        if (Active == null)
        {
            Active = this;
        }

        else
        {
            Destroy(gameObject);
        }
    }

    protected virtual void Start()
    {
        if (physicsBody == null)
            physicsBody = GetComponent<Rigidbody>();
        if (physicsBody == null)
        {
            Debug.LogError("O jogador precisa de Rigidbody.", this);
            enabled = false;
            return;
        }
        visual = GetComponent<NoxCharacterVisual>();
        motor = new NoxPlayerMotor(physicsBody, GetComponent<Collider>());
        combat = new NoxPlayerCombat(transform, attackOrigin != null ? attackOrigin.transform : null,
            new[] { firstAttackPrefab, secondAttackPrefab, thirdAttackPrefab });
        healthBarObject = GameObject.FindGameObjectWithTag("HealthBar");
        if (healthBarObject != null)
            healthImage = healthBarObject.GetComponent<Image>();
        health = NoxRules.AdjustHealth(health > 0 ? health : maxHealth, 0, maxHealth);
        attackElapsed = 1f;
        attackStartedAt = Time.time - attackElapsed;
        RefreshHealthDisplay();
    }

    protected virtual void Update()
    {
        health = NoxRules.AdjustHealth(health, 0, maxHealth);
        if (!IsAlive)
        {
            ClearInput();
            if (NoxLevelResultScreen.PHP != null)
                NoxLevelResultScreen.PHP.PanelWinOrLose("Game Over");
            return;
        }
        if (!NoxSession.CanPlay)
        {
            ClearInput();
            return;
        }
        if (isPushing)
            ReadPushInput();
        else
            ReadMovementInput();
        combat.Update(IsAlive && !isPushing);
        comboStage = combat.Stage;
        ReadJumpInput();
        RefreshHealthDisplay();
    }

    protected virtual void FixedUpdate()
    {
        if (physicsBody == null || motor == null || !IsAlive || !NoxSession.CanPlay)
            return;
        motor.Step(desiredMovement, movementSpeed, jumpRequested && !isPushing, jumpHeight, groundLayers);
        isGrounded = motor.Grounded;
        jumpRequested = false;
    }

    private void ClearInput()
    {
        desiredMovement = Vector3.zero;
        jumpRequested = false;
        combat?.Cancel();
        motor?.ResetInput();
    }

    protected virtual void OnDestroy()
    {
        motor?.Dispose();
        if (Active == this)
            Active = null;
    }

    private const float NormalSpeed = 6f;
    private const float PushSpeed = 3f;
    private const float DefaultJumpHeight = 3.5f;
    private const float ImpulseForce = 1700f;

    private static Vector3 ReadMovementDirection()
    {
        return Vector3.ClampMagnitude(new Vector3(
            Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")), 1f);
    }

    private static bool CanReceiveContactDamage(Collider source, int playerId)
    {
        if (source == null)
            return false;
        NoxEnemyStrike strike = source.GetComponentInParent<NoxEnemyStrike>();
        if (strike != null)
            return strike.TryDamage(source, playerId);
        return source.CompareTag("Fogo") || source.CompareTag("Enemy") || source.CompareTag("Boss");
    }

    private static void UpdateHealthBar(Image bar, float health, float maximum)
    {
        if (bar == null)
            return;
        float ratio = NoxRules.HealthRatio(health, maximum);
        Color color = ratio > 2f / 3f ? Color.green : ratio > 1f / 3f ? Color.yellow : Color.red;
        if (bar.fillAmount != ratio)
            bar.fillAmount = ratio;
        if (bar.color != color)
            bar.color = color;
    }

    // Contratos antigos de cenas e UnityEvents; o código novo usa os membros descritivos acima.
    public Rigidbody rigid
    {
        get => physicsBody; set => physicsBody = value;
    }
    public AudioSource soundBossWater
    {
        get => bossMusic; set => bossMusic = value;
    }
    public GameObject mainCameraObj
    {
        get => cameraObject; set => cameraObject = value;
    }
    public Vector3 newOffset
    {
        get => bossCameraOffset; set => bossCameraOffset = value;
    }
    public GameObject bossCanvas
    {
        get => bossHealthCanvas; set => bossHealthCanvas = value;
    }
    public GameObject invisibleWall
    {
        get => arenaBarrier; set => arenaBarrier = value;
    }
    public GameObject crosshair
    {
        get => attackOrigin; set => attackOrigin = value;
    }
    public GameObject slash1
    {
        get => firstAttackPrefab; set => firstAttackPrefab = value;
    }
    public GameObject slash2
    {
        get => secondAttackPrefab; set => secondAttackPrefab = value;
    }
    public GameObject slash3
    {
        get => thirdAttackPrefab; set => thirdAttackPrefab = value;
    }
    public float slashCount
    {
        get => comboStage; set => comboStage = value;
    }
    public float startTime
    {
        get => attackStartedAt; set => attackStartedAt = value;
    }
    public float currentTime
    {
        get => attackElapsed; set => attackElapsed = value;
    }
    public bool shootTime
    {
        get => legacyShootFlag; set => legacyShootFlag = value;
    }
    public LayerMask WhatIsGround
    {
        get => groundLayers; set => groundLayers = value;
    }
    public float jumpForce
    {
        get => legacyJumpForce; set => legacyJumpForce = value;
    }
    public bool grounded
    {
        get => isGrounded; set => isGrounded = value;
    }
    public float hp
    {
        get => health; set => health = value;
    }
    public Image healthBar
    {
        get => healthImage; set => healthImage = value;
    }
    public GameObject healthBarObj
    {
        get => healthBarObject; set => healthBarObject = value;
    }
    public GameObject healthBar_Canvas
    {
        get => healthCanvas; set => healthCanvas = value;
    }
    public GameObject firstSlash
    {
        get => firstAttackPrefab; set => firstAttackPrefab = value;
    }
    public GameObject secondSlash
    {
        get => secondAttackPrefab; set => secondAttackPrefab = value;
    }
    public GameObject thirdSlash
    {
        get => thirdAttackPrefab; set => thirdAttackPrefab = value;
    }
    public GameObject healthBarOBj
    {
        get => healthBarObject; set => healthBarObject = value;
    }
    public void CharacterPushing() => ReadPushInput();
    public void CharacterMove() => ReadMovementInput();
    public void CharacterJump() => ReadJumpInput();
    public float BarCount(float value, float moreOrLess, float amount, Image bar) => ApplyHealthDelta(value, -moreOrLess, bar);
    public void ChangeColor() => RefreshHealthDisplay();
    public void InvisibleWall() => CloseArenaBarrier();
    public void SoundBossWater() => PlayBossMusic();
    public void CharacterSlash() => QueueAttack();
}
