using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerTwo : MonoBehaviour
{
    //public Animator animin;

    //public GameObject painelGameOver;

    // Variável que corresponde ao script "PlayerTwo", serve como "singleton"
    public static PlayerTwo PT;

    // Variável que corresponde ao componente "Rigidbody" do Player
    public Rigidbody rigid;

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

    #region Variáveis relacionadas ao pulo e raycast

    // Variável que corresponde a layer dentro da Unity
    public LayerMask WhatIsGround;

    // Variável que corresponde a força do pulo do jogador
    public float jumpForce;

    // Variável que informa se o jogador está no chão ou não
    public bool grounded = false;

    // Variável que corresponde ao comprimento do raio
    float rayDistance = 2f;

    // Variável que corresponde a distância que existe do jogador e o ponto onde o raio é criado
    Vector3 rayOffset = new Vector3(0, 0.3f, 1);
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
    public GameObject healthBarOBj;
    #endregion


    #region Metódos de movimentação: normal e empurrando
    public void CharacterPushing()
    {
        float vertical = Input.GetAxis("Vertical") * pushingSpeed;
        float horizontal = Input.GetAxis("Horizontal") * pushingSpeed;


        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
        {
            rigid.velocity = new Vector3(0, 0, -vertical);
        }


        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            rigid.velocity = new Vector3(-horizontal, 0, 0);
        }
    }

    public void CharacterMove()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (transform.eulerAngles != Vector3.zero)
            {
                transform.eulerAngles = Vector3.zero;
            }
        }

        else if (Input.GetKeyDown(KeyCode.S))
        {
            Vector3 backAxis = new Vector3(0, 180, 0);
            if (transform.eulerAngles != backAxis)
            {
                transform.eulerAngles = backAxis;
            }
        }


        else if (Input.GetKeyDown(KeyCode.D))
        {
            Vector3 rightAxis = new Vector3(0, 90, 0);
            if (transform.eulerAngles != rightAxis)
            {
                transform.eulerAngles = rightAxis;
            }
        }


        else if (Input.GetKeyDown(KeyCode.A))
        {
            Vector3 leftAxis = new Vector3(0, -90, 0);
            if (transform.eulerAngles != leftAxis)
            {
                transform.eulerAngles = leftAxis;
            }
        }


        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            transform.position = transform.position + transform.forward * normalSpeed * Time.deltaTime;
            //animin.SetBool("IsWalk", true);
        }
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
        {
            //animin.SetBool("IsWalk", false);
        }
    }
    #endregion

    #region Metódo de pulo
   public void CharacterJump()
    {
        if (Input.GetKey(KeyCode.Space) && grounded == true)
        {
            rigid.AddForce(Vector3.up * jumpForce);
            //animin.SetBool("IsJump", true);
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            // animin.SetBool("IsJump", false);
        }
    }
    #endregion

    #region Metódo de ataque
    public void CharacterSlash()
    {
        if (Input.GetKey(KeyCode.Mouse0) && currentTime >= 0.3f && Time.timeScale == 1)
        {
            Instantiate(firstSlash, crosshair.transform.position, crosshair.transform.rotation);

            startTime = Time.time;
            shootTime = false;
            slashCount += 1;
        }

        if (!shootTime)
        {
            currentTime = Time.time - startTime;
        }

    }

    public void CharacterSlash2()
    {
        if (Input.GetKey(KeyCode.Mouse0) && currentTime >= 0.3f && Time.timeScale == 1)
        {
            Instantiate(secondSlash, crosshair.transform.position, crosshair.transform.rotation);

            startTime = Time.time;
            shootTime = false;
            slashCount += 1;
        }

        if (!shootTime)
        {
            currentTime = Time.time - startTime;
        }

    }

    public void CharacterSlash3()
    {
        if (Input.GetKey(KeyCode.Mouse0) && currentTime >= 0.5f && Time.timeScale == 1)
        {
            Instantiate(thirdSlash, crosshair.transform.position, crosshair.transform.rotation);

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
        value -= moreOrLess;

        bar.fillAmount -= amount;

        return value;
    }
    #endregion

    #region Metódo de troca de cor da barra de vida
    public void ChangeColor()
    {
        if (hp == 3)
        {
            healthBar.color = Color.green;
        }

        if (hp == 2)
        {
            healthBar.color = Color.yellow;
        }

        if (hp == 1)
        {
            healthBar.color = Color.red;
        }
    }
    #endregion


    #region Colliders
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Fogo")
        {
            hp = BarCount(hp, 1, 0.25f, healthBar);
            Destroy(collision.gameObject, 0.5f);
            
        }

        if (collision.gameObject.tag == "Impulse")
        {
            rigid.AddForce(Vector3.up * 1700);
        }

    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "BulletTurret")
        {
            hp = BarCount(hp, 1, 0.25f, healthBar);
            Destroy(other.gameObject);
        }

        if (other.gameObject.tag == "Heal")
        {
            hp = BarCount(hp, -1, -0.25f, healthBar);
            Destroy(other.gameObject);
        }

        if (other.gameObject.tag == "Cristais")
        {
            //4 de mana são 100% do fillamount, 1 de mana é igual a 25% e 0.10 de mana é igual a 0.025 de fillamount que significa 2,5% do valor total (que é 4). 
            //Quando for atribuir um valor ao fillamount, dividir o valor perdido ou recebido pelo valor total. Esse será o valor do fillamount.
            Destroy(other.gameObject);
        }

        if (other.gameObject.tag == "Enemy")
        {
            hp = BarCount(hp, 1, 0.25f, healthBar);
        }

        if (other.gameObject.tag == "AreaWin")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        if (other.gameObject.tag == "Limbo")
        {
            hp = 0;
            Time.timeScale = 0;
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
        //animin = gameObject.GetComponent<Animator>();
        healthBarOBj = GameObject.FindGameObjectWithTag("HealthBar");
        healthBar = healthBarOBj.GetComponent<Image>();

        currentTime = 1;
        startTime = currentTime;
    }


    void Update()
    {

        if (this.gameObject.tag == "Player")
        {
            CharacterMove();
        }

        if (this.gameObject.tag == "Pushing")
        {
            CharacterPushing();
        }

        if (slashCount == 0)
        {
            CharacterSlash();
        }

        if (slashCount == 1)
        {
            CharacterSlash2();
        }

        if (slashCount == 2)
        {
            CharacterSlash3();
        }

        if (currentTime >= 2)
        {
            slashCount = 0;
        }

        CharacterJump();
        ChangeColor();
        
        if (Physics.Raycast(transform.position + rayOffset, Vector3.down, rayDistance, WhatIsGround))
        {
            Debug.DrawRay(transform.position + rayOffset, Vector3.down * rayDistance, Color.red);
            grounded = true;
        }
        else
        {
            grounded = false;
        }

        if (hp <= 0)
        {
            healthBarOBj.SetActive(false);
            PlaceHolderPanel.PHP.PanelWinOrLose("Game Over");
        }
    }
}
