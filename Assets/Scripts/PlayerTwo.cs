using UnityEngine;
using UnityEngine.UI;

public class PlayerTwo : MonoBehaviour
{

    // Referência legada preservada para os componentes das cenas.
    public static PlayerTwo PT;

    public Rigidbody rigid;

    [SerializeField, Min(1f)] private float maxHealth = 3f;
    [SerializeField, Min(0.1f)] private float jumpHeight = 3.5f;
    private Vector3 desiredMovement;
    private NoxPlayerMotor motor;
    private readonly NoxDamageWindow damageWindow = new NoxDamageWindow();
    [SerializeField, Min(0f)] private float damageProtectionSeconds = 0.6f;
    private bool jumpRequested;
    private bool isPushing;
    private float movementSpeed;
    public bool IsAlive => hp > 0;
    public float AnimationSpeed => NoxSession.CanPlay ? desiredMovement.magnitude * movementSpeed : 0;
    public void SetPushing(bool value) { isPushing = value; }

    #region Variáveis relacionadas a atirar
    public GameObject crosshair;
    public GameObject firstSlash;
    public GameObject secondSlash;
    public GameObject thirdSlash;
    public float slashCount = 0;
    public float startTime = 0;
    public float currentTime = 0;
    public bool shootTime = false;
    #endregion

    #region Apoio e salto

    public LayerMask WhatIsGround;

    public float jumpForce; // Mantido para compatibilidade; o motor usa jumpHeight.

    public bool grounded = false;

    #endregion

    #region Variáveis relacionadas a velocidade

    float pushingSpeed = 3f;

    float normalSpeed = 6f;

    #endregion

    #region Variáveis relacionadas ao HP

    public float hp;

    public Image healthBar;

    public GameObject healthBarOBj;
    #endregion

    #region Movimento
    public void CharacterPushing()
    {
        desiredMovement = Vector3.ClampMagnitude(new Vector3(
            Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")), 1f);
        movementSpeed = pushingSpeed;
    }

    public void CharacterMove()
    {
        desiredMovement = Vector3.ClampMagnitude(new Vector3(
            Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")), 1f);
        movementSpeed = normalSpeed;
        if (desiredMovement.sqrMagnitude > 0.001f)
            transform.forward = desiredMovement;
    }
    #endregion

    #region Metódo de pulo
    public void CharacterJump()
    {
        if (!isPushing && Input.GetKeyDown(KeyCode.Space))
            jumpRequested = true;
    }
    #endregion

    #region Ataque
    public void CharacterSlash() { TrySlash(firstSlash, 0.3f, 0); }
    public void CharacterSlash2() { TrySlash(secondSlash, 0.3f, 1); }
    public void CharacterSlash3() { TrySlash(thirdSlash, 0.5f, 2); }
    private void TrySlash(GameObject prefab, float cooldown, int stage)
    {
        currentTime = Time.time - startTime;
        if (!NoxSession.CanPlay || !IsAlive || isPushing || !Input.GetKey(KeyCode.Mouse0)
            || currentTime < cooldown || prefab == null || crosshair == null) return;
        Instantiate(prefab, crosshair.transform.position, crosshair.transform.rotation);
        startTime = Time.time;
        currentTime = 0;
        shootTime = false;
        slashCount = (stage + 1) % 3;
        NoxCharacterVisual visual = GetComponent<NoxCharacterVisual>();
        if (visual != null) visual.PlayAttack();
    }
    #endregion

    #region Metódo das barras
    public float BarCount(float value, float moreOrLess, float amount, Image bar)
    {
        float next = NoxRules.AdjustHealth(value, -moreOrLess, maxHealth);
        if (bar != null) bar.fillAmount = NoxRules.HealthRatio(next, maxHealth);
        return next;
    }
    #endregion

    #region Metódo de troca de cor da barra de vida
    public void ChangeColor()
    {
        if (healthBar == null) return;
        float ratio = NoxRules.HealthRatio(hp, maxHealth);
        healthBar.fillAmount = ratio;
        healthBar.color = ratio > 2f / 3f ? Color.green : ratio > 1f / 3f ? Color.yellow : Color.red;
    }
    #endregion

    public bool TakeDamage(float amount)
    {
        if (!NoxSession.CanPlay || !IsAlive) return false;
        if (!damageWindow.TryApply(hp, amount, maxHealth, Time.time, damageProtectionSeconds, out float next)) return false;
        hp = next;
        ChangeColor();
        return true;
    }

    public bool Heal(float amount)
    {
        if (!NoxSession.CanPlay || !IsAlive || amount <= 0 || float.IsNaN(amount) || float.IsInfinity(amount)) return false;
        float next = NoxRules.AdjustHealth(hp, amount, maxHealth);
        if (next <= hp) return false;
        hp = next;
        ChangeColor();
        return true;
    }

    private void ApplyContactDamage(GameObject source)
    {
        if (source.CompareTag("Fogo") || source.CompareTag("Enemy") || source.CompareTag("Boss")) TakeDamage(1f);
    }

    private void OnCollisionStay(Collision collision) { ApplyContactDamage(collision.gameObject); }
    private void OnTriggerStay(Collider other) { ApplyContactDamage(other.gameObject); }

    #region Colliders
    private void OnCollisionEnter(Collision collision)
    {
        if (!NoxSession.CanPlay || !IsAlive) return;
        ApplyContactDamage(collision.gameObject);
        if (collision.gameObject.CompareTag("Impulse") && rigid != null)
            rigid.AddForce(Vector3.up * 1700);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!NoxSession.CanPlay || !IsAlive) return;
        if (other.CompareTag("BulletTurret"))
        {
            BulletEnemy projectile = other.GetComponentInParent<BulletEnemy>();
            if (projectile != null) projectile.Hit(other: GetComponent<Collider>());
            else { other.enabled = false; TakeDamage(1f); Destroy(other.gameObject); }
        }
        if (other.CompareTag("Heal") && other.enabled && Heal(1f))
        {
            // Desabilitar antes de Destroy evita coleta duplicada no mesmo frame.
            other.enabled = false;
            other.gameObject.SetActive(false);
            Destroy(other.gameObject);
        }
        ApplyContactDamage(other.gameObject);

        if (other.CompareTag("Cristais")) Destroy(other.gameObject);
        if (other.gameObject.tag == "AreaWin")
        {
            NoxSession.LoadNextScene();
        }

        if (other.gameObject.tag == "Limbo")
        {
            hp = 0;
            NoxSession.SetState(NoxGameState.Lost);
        }
    }
    #endregion

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
        if (rigid == null) rigid = GetComponent<Rigidbody>();
        if (rigid == null)
        {
            Debug.LogError("O jogador precisa de Rigidbody.", this);
            enabled = false;
            return;
        }
        motor = new NoxPlayerMotor(rigid, GetComponent<Collider>());
        healthBarOBj = GameObject.FindGameObjectWithTag("HealthBar");
        if (healthBarOBj != null) healthBar = healthBarOBj.GetComponent<Image>();
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
            if (PlaceHolderPanel.PHP != null)
                PlaceHolderPanel.PHP.PanelWinOrLose("Game Over");
            return;
        }
        if (!NoxSession.CanPlay)
        {
            desiredMovement = Vector3.zero;
            jumpRequested = false;
            return;
        }
        if (isPushing) CharacterPushing(); else CharacterMove();
        switch ((int)slashCount)
        {
            case 0: CharacterSlash(); break;
            case 1: CharacterSlash2(); break;
            case 2: CharacterSlash3(); break;
            default: slashCount = 0; break;
        }
        if (currentTime > 2f) slashCount = 0;
        CharacterJump();
        ChangeColor();
    }

    private void FixedUpdate()
    {
        if (rigid == null || !IsAlive || !NoxSession.CanPlay) return;
        motor.Step(desiredMovement, movementSpeed, jumpRequested && !isPushing, jumpHeight, WhatIsGround);
        grounded = motor.Grounded;
        jumpRequested = false;
    }

    private void OnDestroy()
    {
        motor?.Dispose();
        if (PT == this) PT = null;
    }
}
