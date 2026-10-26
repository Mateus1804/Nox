using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    // Variável que corresponde ao script "PlayerTwo", serve como "singleton"
    public static PlayerScript PT;

    // Variável que corresponde ao componente "Rigidbody" do Player
    public Rigidbody rigid;

    public AudioSource soundBossWater;


    [SerializeField, Min(1f)] private float maxHealth = 3f;
    [SerializeField, Min(0.1f)] private float jumpHeight = 3.5f;
    private Vector3 desiredMovement;
    private Collider bodyCollider;
    private bool jumpRequested;
    private bool isPushing;
    private float movementSpeed;
    public bool IsAlive => hp > 0;
    public void SetPushing(bool value) { isPushing = value; }

    #region Variáveis para a mudança do offset da camera, e a ativação do canvas da vida do Boss, e a parede invisivel
    // A câmera que segue o Player fica associada a essa variável
    public GameObject mainCameraObj;

    // Esse é o novo offset da camera, dá pra mudar o valor no Inspector do Player
    public Vector3 newOffset;

    // O canvas do Boss fica associado a essa variável
    public GameObject bossCanvas;

    public GameObject invisibleWall;
    #endregion

    #region Variáveis relacionadas a atacar
    public GameObject crosshair;
    public GameObject slash1;
    public GameObject slash2;
    public GameObject slash3;
    public float slashCount = 0;
    public float startTime = 0;
    public float currentTime = 0;
    public bool shootTime = false;
    #endregion

    #region Variáveis relacionadas ao pulo e raycast

    // Variável que corresponde a layer dentro da Unity
    public LayerMask WhatIsGround;

    // Variável que corresponde a força do pulo do jogador
    public float jumpForce;

    // Variável que informa se o jogador está no chão ou não
    public bool grounded = false;




    #endregion

    #region Variáveis relacionadas a velocidade

    // Variável que corresponde a velocidade ao empurrar objetos
    float pushingSpeed = 3f;

    // Variável que corresponde a velocidade normal do jogador
    float normalSpeed = 6f;

    #endregion

    #region Variáveis relacionadas ao HP

    // Variável que corresponde ao HP
    public float hp;

    // Variável que corresponde a "imagem" da barra de vida, a quantidade de vida
    public Image healthBar;

    // Variável que corresponde a barra de vida dentro da Unity
    public GameObject healthBarObj;

    public GameObject healthBar_Canvas;
    #endregion

    #region Metódos de movimentação: normal e empurrando
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

    #region Metódo de ataque
    // Metódo que invoca o primeiro Slash
    public void CharacterSlash1()
    {
        if (Input.GetKey(KeyCode.Mouse0) && currentTime >= 0.4f && Time.timeScale == 1)
        {
            Instantiate(slash1, crosshair.transform.position, crosshair.transform.rotation);

            startTime = Time.time;
            shootTime = false;
            slashCount++;
        }

        if (!shootTime)
        {
            currentTime = Time.time - startTime;
        }
    }

    // Metódo que invoca o segundo Slash
    public void CharacterSlash2()
    {
        if (Input.GetKey(KeyCode.Mouse0) && currentTime >= 0.2f && Time.timeScale == 1)
        {
            Instantiate(slash2, crosshair.transform.position, crosshair.transform.rotation);

            startTime = Time.time;
            shootTime = false;
            slashCount++;
        }

        if (!shootTime)
        {
            currentTime = Time.time - startTime;
        }
    }

    // Metódo que invoca o terceiro Slash
    public void CharacterSlash3()
    {
        if (Input.GetKey(KeyCode.Mouse0) && currentTime >= 0.4f && Time.timeScale == 1)
        {
            Instantiate(slash3, crosshair.transform.position, crosshair.transform.rotation);

            startTime = Time.time;
            shootTime = false;
            slashCount = 0;
        }

        if (!shootTime)
        {
            currentTime = Time.time - startTime;
        }
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

    #region Metódo da parede invisivel
    public void InvisibleWall()
    {
        if (invisibleWall != null) invisibleWall.SetActive(true);
    }
    #endregion


    #region Colliders
    private void OnCollisionEnter(Collision collision)
    {
        if (!NoxSession.CanPlay || !IsAlive) return;
        if (collision.gameObject.CompareTag("Fogo")) hp = BarCount(hp, 1, 0, healthBar);
        if (collision.gameObject.tag == "Impulse")
        {
            rigid.AddForce(Vector3.up * 1700);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!NoxSession.CanPlay || !IsAlive) return;
        if (other.CompareTag("BulletTurret"))
        {
            hp = BarCount(hp, 1, 0, healthBar);
            Destroy(other.gameObject);
        }
        if (other.gameObject.tag == "Heal")
        {
            hp = BarCount(hp, -1, -0.25f, healthBar);
            Destroy(other.gameObject);
        }

        if (other.gameObject.tag == "Enemy")
        {
            hp = BarCount(hp, 1, 0.25f, healthBar);
            // O feedback visual é apresentado pela barra de vida.
        }

        if (other.gameObject.tag == "AreaWin")
        {
            NoxSession.LoadNextScene();
        }

        if (other.gameObject.tag == "Limbo")
        {
            hp = 0;
            NoxSession.SetState(NoxGameState.Lost);
        }

        if (other.gameObject.tag == "LifeCamera")
        {
            CameraFollow follow = mainCameraObj != null ? mainCameraObj.GetComponent<CameraFollow>() : null;
            if (follow != null) follow.offset = newOffset;
            if (bossCanvas != null) bossCanvas.SetActive(true);
            Invoke("InvisibleWall", 2);

            SoundBossWater();
        }

        if (other.gameObject.tag == "Boss")
        {
            hp = BarCount(hp, 1, 0.25f, healthBar);

        }
    }


    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            // Não sobrescrever a cor original do material do personagem.
        }
    }
    #endregion

    public void SoundBossWater()
    {
        if (soundBossWater != null) soundBossWater.Play();
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
        if (rigid == null) rigid = GetComponent<Rigidbody>();
        if (rigid == null)
        {
            Debug.LogError("O jogador precisa de Rigidbody.", this);
            enabled = false;
            return;
        }
        bodyCollider = GetComponent<Collider>();
        rigid.interpolation = RigidbodyInterpolation.Interpolate;
        rigid.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        healthBarObj = GameObject.FindGameObjectWithTag("HealthBar");
        if (healthBarObj != null) healthBar = healthBarObj.GetComponent<Image>();
        hp = NoxRules.AdjustHealth(hp > 0 ? hp : maxHealth, 0, maxHealth);
        currentTime = 1f;
        startTime = Time.time - currentTime;
        ChangeColor();
    }


    void Update()
    {
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
            case 0: CharacterSlash1(); break;
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
        // A consulta usa os pés do collider, em vez de um deslocamento global fixo.
        Vector3 origin = bodyCollider != null
            ? bodyCollider.bounds.center : transform.position + Vector3.up * 0.5f;
        float length = bodyCollider != null ? bodyCollider.bounds.extents.y + 0.12f : 0.7f;
        grounded = rigid.velocity.y <= 0.1f && Physics.Raycast(origin, Vector3.down,
            length, WhatIsGround, QueryTriggerInteraction.Ignore);
        Vector3 velocity = desiredMovement * movementSpeed;
        velocity.y = rigid.velocity.y;
        if (jumpRequested && grounded && !isPushing)
        {
            velocity.y = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * jumpHeight);
            grounded = false;
        }
        jumpRequested = false;
        rigid.velocity = velocity;
    }

    private void OnDestroy()
    {
        if (PT == this) PT = null;
    }
}
