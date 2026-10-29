using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    // Referência legada preservada para os componentes das cenas.
    public static PlayerScript PT;

    public Rigidbody rigid;

    public AudioSource soundBossWater;
    private bool bossEncounterStarted;

    [SerializeField, Min(1f)] private float maxHealth = 3f;
    [SerializeField, Min(0.1f)] private float jumpHeight = NoxPlayerControllerSupport.DefaultJumpHeight;
    private Vector3 desiredMovement;
    private NoxPlayerMotor motor;
    private NoxPlayerCombat combat;
    private readonly NoxDamageWindow damageWindow = new NoxDamageWindow();
    [SerializeField, Min(0f)] private float damageProtectionSeconds = 0.6f;
    private bool jumpRequested;
    private bool isPushing;
    private NoxCharacterVisual visual;
    private float movementSpeed;
    public bool IsAlive => hp > 0;
    public float AnimationSpeed => NoxSession.CanPlay && motor != null ? motor.LocomotionSpeed : 0;
    public void SetPushing(bool value)
    {
        isPushing = value;
    }

    public GameObject mainCameraObj;

    public Vector3 newOffset;

    public GameObject bossCanvas;

    public GameObject invisibleWall;

    public GameObject crosshair;
    public GameObject slash1;
    public GameObject slash2;
    public GameObject slash3;
    public float slashCount = 0;
    public float startTime = 0;
    public float currentTime = 0;
    public bool shootTime = false;

    public LayerMask WhatIsGround;

    public float jumpForce; // Mantido para compatibilidade; o motor usa jumpHeight.

    public bool grounded = false;

    public float hp;

    public Image healthBar;

    public GameObject healthBarObj;

    public GameObject healthBar_Canvas;

    public void CharacterPushing()
    {
        desiredMovement = NoxPlayerControllerSupport.ReadMovement();
        movementSpeed = NoxPlayerControllerSupport.PushSpeed;
    }

    public void CharacterMove()
    {
        desiredMovement = NoxPlayerControllerSupport.ReadMovement();
        movementSpeed = NoxPlayerControllerSupport.NormalSpeed;
        if (desiredMovement.sqrMagnitude > 0.001f && (combat == null || !combat.LocksFacing))
            transform.forward = desiredMovement;
    }

    public void CharacterJump()
    {
        if (!isPushing && Input.GetKeyDown(KeyCode.Space))
            jumpRequested = true;
    }

    public void CharacterSlash1()
    {
        RequestAttack();
    }
    public void CharacterSlash2()
    {
        RequestAttack();
    }
    public void CharacterSlash3()
    {
        RequestAttack();
    }
    private void RequestAttack()
    {
        // Métodos preservados para UnityEvents; o mesmo clique não entra duas vezes na fila.
        if (NoxSession.CanPlay && IsAlive && !isPushing)
            combat?.Request();
    }

    // amount integra a assinatura legada; a proporção atual deriva de hp / maxHealth.
    public float BarCount(float value, float moreOrLess, float amount, Image bar)
    {
        float next = NoxRules.AdjustHealth(value, -moreOrLess, maxHealth);
        if (bar != null)
            bar.fillAmount = NoxRules.HealthRatio(next, maxHealth);
        return next;
    }

    public void ChangeColor()
    {
        NoxPlayerControllerSupport.UpdateHealthBar(healthBar, hp, maxHealth);
    }

    public void InvisibleWall()
    {
        if (invisibleWall != null)
            invisibleWall.SetActive(true);
    }

    public bool TakeDamage(float amount)
    {
        if (!NoxSession.CanPlay || !IsAlive)
            return false;
        if (!damageWindow.TryApply(hp, amount, maxHealth, Time.time, damageProtectionSeconds, out float next))
            return false;
        hp = next;
        combat?.Cancel();
        if (visual != null)
            visual.PulseDamage();
        ChangeColor();
        return true;
    }

    public bool Heal(float amount)
    {
        if (!NoxSession.CanPlay || !IsAlive || amount <= 0 || float.IsNaN(amount) || float.IsInfinity(amount))
            return false;
        float next = NoxRules.AdjustHealth(hp, amount, maxHealth);
        if (next <= hp)
            return false;
        hp = next;
        ChangeColor();
        return true;
    }

    private void ApplyContactDamage(Collider source)
    {
        if (NoxPlayerControllerSupport.CanReceiveContactDamage(source, GetInstanceID()))
            TakeDamage(1f);
    }

    private void OnCollisionStay(Collision collision)
    {
        ApplyContactDamage(collision.collider);
    }
    private void OnTriggerStay(Collider other)
    {
        ApplyContactDamage(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!NoxSession.CanPlay || !IsAlive)
            return;
        ApplyContactDamage(collision.collider);
        if (collision.gameObject.CompareTag("Impulse") && rigid != null)
            rigid.AddForce(Vector3.up * NoxPlayerControllerSupport.ImpulseForce);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!NoxSession.CanPlay || !IsAlive)
            return;
        if (other.CompareTag("BulletTurret"))
        {
            BulletEnemy projectile = other.GetComponentInParent<BulletEnemy>();
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
            hp = 0;
            NoxSession.SetState(NoxGameState.Lost);
            return;
        }

        if (other.gameObject.tag == "LifeCamera" && !bossEncounterStarted)
        {
            bossEncounterStarted = true;
            CameraFollow follow = mainCameraObj != null ? mainCameraObj.GetComponent<CameraFollow>() : null;
            if (follow != null)
                follow.offset = newOffset;
            if (bossCanvas != null)
                bossCanvas.SetActive(true);
            Invoke(nameof(InvisibleWall), 2);

            SoundBossWater();
        }

    }

    public void SoundBossWater()
    {
        if (soundBossWater != null)
        {
            if (HUDAndSounds.Singleton != null)
                HUDAndSounds.Singleton.StopMusic();
            soundBossWater.Play();
        }
    }

    void Awake()
    {
        if (PT == null)
        {
            PT = this;
        }

        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (rigid == null)
            rigid = GetComponent<Rigidbody>();
        if (rigid == null)
        {
            Debug.LogError("O jogador precisa de Rigidbody.", this);
            enabled = false;
            return;
        }
        visual = GetComponent<NoxCharacterVisual>();
        motor = new NoxPlayerMotor(rigid, GetComponent<Collider>());
        combat = new NoxPlayerCombat(transform, crosshair != null ? crosshair.transform : null,
            new[] { slash1, slash2, slash3 });
        healthBarObj = GameObject.FindGameObjectWithTag("HealthBar");
        if (healthBarObj != null)
            healthBar = healthBarObj.GetComponent<Image>();
        hp = NoxRules.AdjustHealth(hp > 0 ? hp : maxHealth, 0, maxHealth);
        currentTime = 1f;
        startTime = Time.time - currentTime;
        ChangeColor();
    }

    void Update()
    {
        hp = NoxRules.AdjustHealth(hp, 0, maxHealth);
        if (!IsAlive)
        {
            desiredMovement = Vector3.zero;
            jumpRequested = false;
            combat?.Cancel();
            if (PlaceHolderPanel.PHP != null)
                PlaceHolderPanel.PHP.PanelWinOrLose("Game Over");
            return;
        }
        if (!NoxSession.CanPlay)
        {
            desiredMovement = Vector3.zero;
            jumpRequested = false;
            combat?.Cancel();
            motor?.ResetInput();
            return;
        }
        if (isPushing)
            CharacterPushing();
        else
            CharacterMove();
        combat.Update(IsAlive && !isPushing);
        slashCount = combat.Stage;
        CharacterJump();
        ChangeColor();
    }

    private void FixedUpdate()
    {
        if (rigid == null || motor == null || !IsAlive || !NoxSession.CanPlay)
            return;
        motor.Step(desiredMovement, movementSpeed, jumpRequested && !isPushing, jumpHeight, WhatIsGround);
        grounded = motor.Grounded;
        jumpRequested = false;
    }

    private void OnDestroy()
    {
        motor?.Dispose();
        if (PT == this)
            PT = null;
    }
}
