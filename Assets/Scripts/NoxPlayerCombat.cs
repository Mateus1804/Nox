using UnityEngine;
using UnityEngine.EventSystems;

// Integra o relógio testável aos dois controladores, mantendo seus pontos públicos legados.
public sealed class NoxPlayerCombat
{
    private readonly Transform owner, aim;
    private readonly GameObject[] prefabs;
    private readonly NoxCharacterVisual visual;
    private readonly NoxAttackCycle cycle = new NoxAttackCycle();
    private int inputFrame = -1;
    private Quaternion direction;
    public bool LocksFacing => cycle.Phase(Time.time) == NoxAttackPhase.Windup ||
        cycle.Phase(Time.time) == NoxAttackPhase.Active;
    public int Stage => cycle.Stage;
    public NoxPlayerCombat(Transform owner, Transform aim, GameObject[] prefabs)
    {
        this.owner = owner;
        this.aim = aim;
        this.prefabs = prefabs;
        visual = owner.GetComponent<NoxCharacterVisual>();
    }
    public void Request()
    {
        if (inputFrame == Time.frameCount || !Input.GetMouseButtonDown(0))
            return;
        inputFrame = Time.frameCount;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;
        cycle.Request(Time.time);
    }
    public void Update(bool allowed)
    {
        if (allowed)
            Request();
        else
        {
            Cancel();
            return;
        }
        cycle.Tick(Time.time, allowed && aim != null, out bool started, out bool strike);
        if (started)
        {
            direction = aim.rotation;
            if (visual != null)
                visual.PlayAttack();
        }
        if (!strike)
            return;
        GameObject prefab = prefabs[cycle.Stage];
        if (prefab == null)
        {
            Debug.LogWarning("Prefab do golpe ausente: " + (cycle.Stage + 1), owner);
            return;
        }
        GameObject effect = Object.Instantiate(prefab, aim.position, direction);
        NoxAttackHitbox hitbox = effect.GetComponent<NoxAttackHitbox>();
        if (hitbox == null)
            hitbox = effect.AddComponent<NoxAttackHitbox>();
        // O terceiro golpe é o projétil original; conserva a janela de voo de 1,5 s.
        hitbox.Configure(owner, cycle.Stage == 2 ? 1.5f : (float)NoxAttackCycle.Active);
    }
    public void Cancel()
    {
        if (cycle.Running && visual != null)
            visual.CancelAttack();
        cycle.Cancel();
    }
}
