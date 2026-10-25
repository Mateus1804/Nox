
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;


public class MovableObjectsScript : MonoBehaviour
{
    /* MOVABLE OBJECTS
    Esse é o script com o código de "mover" os objetos da cena, pra que ele funcione você vai precisar:

      • Precisa de um gameObject, com um objeto vazio (empty) como filho, esse empty tem a tag "InRange", um "box collider" com a opção "Is Trigget" ligada;
      • Associar esse script ao Player, e associar o script do Player à opção "playerScript";
    
    Qualquer "Debug.Log" não precisa estar na versão final, só servem para feedback do código no console.

    (João)
    */

    #region Variáveis do noJump
    //Váriavel que armazena a classe que contém as variáveis relacionadas ao player
    public PlayerScript playerScript;

    //Váriavel que armazena o valor original do JumpForce.
    float original_jumpForce;
    #endregion

    #region Variáveis para mover objetos
    //Variavel que armazena uma contagem, serve como "condição" de um IF para soltar o objeto.
    public float countdown = 0;

    //Variável que verifica se o objeto está na range, depende de um box collider para ser validada.
    bool movable = false;

    //Variável que armazena o objeto que vai ser movido, o objeto é armazenado na variável pelo metódo de colisão.
    GameObject movableObject;
    #endregion


    #region Metódo de Colisão
    private void OnTriggerEnter(Collider other)
    {
        //Instrução que verifica se o "objeto" da colisão tem a tag "InRange".
        if (other.gameObject.tag == "InRange")
        {
            //Se o objeto tiver a tag, dizemos que a variável "movable" é true.
            movable = true;

            //Armazenamos o objeto da colisão na variável "movableObject".
            movableObject = other.transform.parent.gameObject;

            //Printamos no console uma mensagem, para verificar se o processo deu certo.
            Debug.Log("A box tá na range");
        }
    }
    #endregion

    #region Metódo para mover objetos
    public void Player_GrabDrop()
    {

        //Instrução para PEGAR o objeto, que verifica três parametros: se a tecla E foi apertada, e se a variável "movable" é true, e se a variável "isGrounded" é true
        if (Input.GetKeyDown(KeyCode.E) && movable == true && playerScript.grounded == true)
        {

            //Se as três condições forem verdadeiras, transformamos o objeto armazenado na variável "movableObject", em filho do "Player". Agora, toda vez que o "Player" se move, o objeto se move junto.
            movableObject.transform.parent = this.transform;

            //Altera o valor do "jumpForce" para 0, para que o Player não possa pular.
            playerScript.jumpForce = 0;

            //Aumenta a contagem toda vez que a tecla E é pressionada.
            countdown++;

            //A tag do Player é trocada por "Pushing", e assim a forma de movimentação também é trocada.
            this.gameObject.tag = "Pushing";

            //Printa no console uma mensagem para verificar se o objeto foi pego.
            Debug.Log("A box foi pega, empurra crlh");
        }

        //Instrução para SOLTAR o objeto, que verifica dois parametros: se a tecla E foi apertada, se a variável "movable" é true, e se a contagem é igual 2.
        if (Input.GetKeyDown(KeyCode.E) && movable == true && countdown == 2)
        {

            //Se as três condições forem verdadeiras, declaramos que o objeto armazenado na variável "movableObject" não tem parentes. Agora, toda vez que o "Player" se move, o objeto não se move junto.
            movableObject.transform.parent = null;

            //Resetamos o "jumpForce" para seu valor original.
            playerScript.jumpForce = original_jumpForce;

            //Resetamos a contagem.
            countdown = 0;

            //A tag do Player é resetada para "Player", e assim a forma de movimentação volta a ser a padrão.
            this.gameObject.tag = "Player";

            //Printa no console uma mensagem para verificar se o objeto foi solto.
            Debug.Log("A box foi solta");
        }

    }
    #endregion



    void Start()
    {
        //Armazenamos o valor original do "jumpForce" em uma variável, para que possamos acessá-lo depois.
        original_jumpForce = playerScript.jumpForce;
    }


    void Update()
    {
      //Chamamos o metódo no Update, pra ele poder ser checado a todo momento, sem isso o metódo não funciona.
        Player_GrabDrop();
    
    }
}
