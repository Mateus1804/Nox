using UnityEngine;

public class AimEnemy : MonoBehaviour
{
    public GameObject bullet;
    public GameObject mira;
    public float startTime;
    public float currentTime;
    public bool shootTime;
    private bool initialized;

    public void Shoot()
    {
        if (!NoxSession.CanPlay || bullet == null || mira == null) return;
        currentTime = Time.time - startTime;
        if (currentTime < 1) return;
        Instantiate(bullet, mira.transform.position, mira.transform.rotation);
        startTime = Time.time;
        currentTime = 0;
        shootTime = false;
    }
    private void OnEnable()
    {
        // Primeiro disparo imediato, independentemente do tempo gasto no menu.
        startTime = Time.time - 1;
        initialized = true;
    }
    private void Update() { if (initialized) Shoot(); }
}
