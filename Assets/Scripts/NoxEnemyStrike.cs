using UnityEngine;

public sealed class NoxEnemyStrike : MonoBehaviour
{
    public Collider damageArea;
    public float reach = 2;
    public float windup = 0.6f;
    public float recovery = 1.1f;
    public NoxThreatCue cue;
    private Transform target;
    private EnemyAir enemy;
    private Boss boss;
    private BossScript alternate;
    private NoxEnemyCycle cycle;
    private readonly NoxHitRegistry hits = new NoxHitRegistry();
    public bool Committed => cycle != null && cycle.Phase != NoxThreatPhase.Ready;
    private bool Alive => boss != null ? boss.hpBoss > 0 : alternate != null ? alternate.hp > 0 :
        enemy == null || enemy.hpTurtle > 0;
    public bool Active => cycle != null && cycle.Phase == NoxThreatPhase.Active;
    private void Awake()
    {
        cycle = new NoxEnemyCycle(Mathf.Max(0.1f, windup), 0.18, Mathf.Max(0.1f, recovery));
        enemy = GetComponent<EnemyAir>(); boss = GetComponent<Boss>(); alternate = GetComponent<BossScript>();
    }
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) target = player.transform;
        if (damageArea != null) damageArea.enabled = false;
    }
    private void Update()
    {
        if (NoxSession.State == NoxGameState.Paused)
        {
            if (damageArea != null) damageArea.enabled = false;
            if (cue != null) cue.Show(Vector3.zero, 0, NoxThreatPhase.Ready, 0);
            return;
        }
        bool alive = Alive;
        bool near = target != null && Vector3.Distance(transform.position, target.position) <= reach;
        bool visible = near && NoxPerception.CanSee(transform, target);
        NoxThreatPhase previous = cycle.Phase;
        cycle.Tick(Time.time, near, visible, alive && NoxSession.CanPlay && damageArea != null);
        if (cycle.Phase == NoxThreatPhase.Active && previous != NoxThreatPhase.Active) hits.Clear();
        if (damageArea != null) damageArea.enabled = Active && visible && NoxSession.CanPlay;
        if (cue != null)
        {
            Vector3 localCenter = damageArea is SphereCollider sphere ? sphere.center :
                damageArea is CapsuleCollider capsule ? capsule.center : Vector3.zero;
            Vector3 point = damageArea != null ? damageArea.transform.TransformPoint(localCenter) : transform.position;
            Collider body = GetComponent<Collider>();
            point.y = body != null && body.enabled ? body.bounds.min.y + 0.04f : transform.position.y + 0.04f;
            cue.Show(point, reach * 0.55f, cycle.Phase, cycle.Progress(Time.time));
        }
    }
    public bool TryDamage(Collider source, int playerId)
    {
        return NoxSession.CanPlay && Alive && Active && source == damageArea && damageArea.enabled && hits.TryHit(playerId);
    }
    public void Interrupt()
    {
        cycle?.Reset();
        if (cue != null) cue.Show(Vector3.zero, 0, NoxThreatPhase.Ready, 0);
        if (damageArea != null) damageArea.enabled = false;
    }
    private void OnDisable() { Interrupt(); }
}
