using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimEnemy : MonoBehaviour
{
    #region Variaveis > Balas   
    //Declarando a variável das balas
    public GameObject bullet;
    public GameObject mira;
    #endregion

    #region Variaveis > FireRate
    //Declarando as variáveis do fire rate
    public float startTime;
    public float currentTime;
    public bool shootTime;
    #endregion

    #region Shoot()
    public void Shoot()
    {
        if (currentTime >= 1)
        {
            Instantiate(bullet, mira.transform.position, mira.transform.rotation);

            startTime = Time.time;
            shootTime = false;
        }
     

        if (!shootTime)
        {
            currentTime = Time.time - startTime;
        }
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        currentTime = 1;
        startTime = currentTime;
    }

    // Update is called once per frame
    void Update()
    {
        Shoot();
    }
}
