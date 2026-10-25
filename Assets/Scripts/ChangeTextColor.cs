using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeTextColor : MonoBehaviour
{
    /*
     * Script que contém a fórmula para trocar a cor do texto quando o mouse tá em cima. Para funcionar precisa de:

    • Associar esse script ao texto que vai mudar de cor.
    • Um componente "Event Trigger" associado ao texto. O "Event Trigger" precisa de 3 "Event Type":

           1. Um "PointerEnter" com esse script, e o metódo "ChangeTheColor" associados; 
           2. Um "PointerExit" com esse script, e o metódo "ResetTheColor" associados;
           3. Um "PointerClick" com o GameADM, e o metódo do botão do qual o texto é filho; 
           4. Outro "PointerClick" com esse script, e o metódo ResetTheColor associados;
      


    * Explicando os requisitos acima ↑:

     • O script precisa estar associado ao texto para associarmos a variável "theText" individualmente a cada texto ;
     • O componente "Event Trigger" serve para acionar os metódos de troca e reset de cor;
     • O primeiro "EventType" serve para acionar o metódo de troca de cor, ele é ativado quando o mouse fica em cima do texto;
     • O segundo "EventType" serve para acionar o metódo de reset de cor, ele é ativado quando o mouse sai de cima do texto;
     • O terceiro "EventType" serve para acionar o efeito do botão que é "pai" do texto. Por algum motivo, o "OnClick" do botão NÃO funciona se o texto tiver um "Event Trigger", para substituir o botão, adicionamos esse "Event Type" que é acionado no click do mouse.
     • O quarto "EventType" serve para resetar a cor quando clicamos no texto, pois quando vamos de um painel para outro, o texto continua com a cor da variável "secondColor", para evitar esse problema, adicionamos esse "Event Type".
     */


    //Variável que armazena a cor original do texto.
    public Color originalColor;

    //Variável que armazena a cor da troca. A cor é BRANCA pois o botão fica PRETO quando o mouse tá em cima, ela ser BRANCA dá um destaque maior ao texto.
    public Color secondColor = Color.white;

    //Variável que armazena o texto, ela é "TextMeshProUGUI" pois estamos usando o sistema "TextMeshPro", ao invés do "Text".
    public TextMeshProUGUI theText;


    //Metódo que troca a cor do texto
    public void ChangeTheColor()
    {
        //Declaramos que a cor do texto armazenado na variável "theText", é igual a cor armazenada na variável "secondColor".
        theText.color = secondColor;
    
    }

    //Metódo que reseta a cor
    public void ResetTheColor()
    {
        //Declaramos que a cor do texto armazenado na variável "theText", é igual a cor armazenada na variável "originalColor", retornando o texto à sua cor original.
        theText.color = originalColor;

    }


    
    void Start()
    {
        //Declaramos que a cor armazenada na variável "originalColor", é igual a cor atual do texto armazenado na variável "theText". Assim, usamos a variável "originalColor" para "guardar" a cor original, e usaremos a variável para resetar a cor.
        originalColor = theText.color;
    }

    
    void Update()
    {
        
    }
}
