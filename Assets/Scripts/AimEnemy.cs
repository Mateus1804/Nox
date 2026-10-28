using UnityEngine;

public class AimEnemy : MonoBehaviour
{
    public GameObject bullet;
    public GameObject mira;
    public float startTime;
    public float currentTime;
    public bool shootTime;
    public float detectionRange = 24f;
    public NoxThreatCue cue;
    private Transform target;
    private readonly NoxEnemyCycle cycle = new NoxEnemyCycle(0.45, 0.10, 0.85);
    public void Shoot()
    {
        if (NoxSession.State == NoxGameState.Paused)
        { if (cue != null) cue.Show(Vector3.zero, 0, NoxThreatPhase.Ready, 0); return; }
        bool near = target != null && Vector3.Distance(transform.position, target.position) <= detectionRange;
        NoxThreatPhase before = cycle.Phase;
        cycle.Tick(Time.time, near, near && NoxPerception.CanSee(transform, target),
            NoxSession.CanPlay && bullet != null && mira != null);
        if (before != NoxThreatPhase.Active && cycle.Phase == NoxThreatPhase.Active)
        {
            GameObject shot = Instantiate(bullet, mira.transform.position, mira.transform.rotation);
            BulletEnemy projectile = shot.GetComponent<BulletEnemy>();
            if (projectile != null) projectile.Configure(transform);
            startTime = Time.time;
            shootTime = false;
        }
        if (cycle.Phase == NoxThreatPhase.Windup && mira != null && target != null)
        {
            Vector3 direction = target.position - mira.transform.position;
            if (direction.sqrMagnitude > 0.001f) mira.transform.rotation = Quaternion.LookRotation(direction);
        }
        currentTime = Time.time - startTime;
        if (cue != null) cue.Show(transform.position + Vector3.up * 0.05f, 0.65f, cycle.Phase, cycle.Progress(Time.time));
    }
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) target = player.transform;
    }
    private void OnEnable() { cycle.Reset(); startTime = Time.time; }
    private void Update() { Shoot(); }
}
