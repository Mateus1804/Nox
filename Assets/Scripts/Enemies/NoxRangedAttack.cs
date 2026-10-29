using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom(true, sourceNamespace: null, sourceAssembly: "Assembly-CSharp", sourceClassName: "AimEnemy")]
public class NoxRangedAttack : MonoBehaviour
{
    [FormerlySerializedAs("bullet")]
    public GameObject projectilePrefab;
    [FormerlySerializedAs("mira")]
    public GameObject aimObject;
    [FormerlySerializedAs("startTime")]
    public float lastShotAt;
    [FormerlySerializedAs("currentTime")]
    public float elapsedSinceShot;
    [FormerlySerializedAs("shootTime")]
    [HideInInspector]
    public bool legacyReadyState;
    public float detectionRange = 24f;
    public NoxThreatCue cue;
    private Transform target;
    private readonly NoxEnemyCycle cycle = new NoxEnemyCycle(0.45, 0.10, 0.85);
    public void Shoot()
    {
        if (NoxSession.State == NoxGameState.Paused)
        {
            if (cue != null)
                cue.Show(Vector3.zero, 0, NoxThreatPhase.Ready, 0);
            return;
        }
        bool near = target != null && Vector3.Distance(transform.position, target.position) <= detectionRange;
        NoxThreatPhase before = cycle.Phase;
        cycle.Tick(Time.time, near, near && NoxPerception.CanSee(transform, target),
            NoxSession.CanPlay && projectilePrefab != null && aimObject != null);
        if (before != NoxThreatPhase.Active && cycle.Phase == NoxThreatPhase.Active)
        {
            GameObject shot = Instantiate(projectilePrefab, aimObject.transform.position, aimObject.transform.rotation);
            NoxEnemyProjectile projectile = shot.GetComponent<NoxEnemyProjectile>();
            if (projectile != null)
                projectile.Configure(transform);
            lastShotAt = Time.time;
            legacyReadyState = false;
        }
        if (cycle.Phase == NoxThreatPhase.Windup && aimObject != null && target != null)
        {
            Vector3 direction = target.position - aimObject.transform.position;
            if (direction.sqrMagnitude > 0.001f)
                aimObject.transform.rotation = Quaternion.LookRotation(direction);
        }
        elapsedSinceShot = Time.time - lastShotAt;
        if (cue != null)
            cue.Show(transform.position + Vector3.up * 0.05f, 0.65f, cycle.Phase, cycle.Progress(Time.time));
    }
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            target = player.transform;
    }
    private void OnEnable()
    {
        cycle.Reset();
        lastShotAt = Time.time;
    }
    private void Update()
    {
        Shoot();
    }
    // API de compatibilidade para os dados e callbacks recuperados.
    public GameObject bullet
    {
        get => projectilePrefab; set => projectilePrefab = value;
    }
    public GameObject mira
    {
        get => aimObject; set => aimObject = value;
    }
    public float startTime
    {
        get => lastShotAt; set => lastShotAt = value;
    }
    public float currentTime
    {
        get => elapsedSinceShot; set => elapsedSinceShot = value;
    }
    public bool shootTime
    {
        get => legacyReadyState; set => legacyReadyState = value;
    }
}
