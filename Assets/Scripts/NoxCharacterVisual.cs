using UnityEngine;

// Apenas apresentação. O Rigidbody e o collider continuam no jogador original.
public sealed class NoxCharacterVisual : MonoBehaviour
{
    public GameObject modelPrefab;
    [SerializeField] private string speedParameter = "Speed";
    [SerializeField] private string groundedParameter = "Grounded";
    [SerializeField] private string attackParameter = "Attack";
    [SerializeField, Min(0f)] private float turnSpeed = 720f;
    private Transform visualRoot;
    private Quaternion visualRotation;
    private Animator animator;
    private Rigidbody body;
    private PlayerScript player;
    private PlayerTwo alternate;
    private Renderer[] modelRenderers = new Renderer[0];
    private MaterialPropertyBlock[] savedBlocks = new MaterialPropertyBlock[0];
    private readonly MaterialPropertyBlock flashBlock = new MaterialPropertyBlock();
    private float flashUntil;
    private bool flashVisible;
    private bool hasCadence;
    private bool hasSpeed;
    private bool hasGrounded;
    private bool hasAttack;
    private bool warnedMissingAttack;

    private void Start()
    {
        visualRoot = transform.Find("NoxVisual");
        if (visualRoot != null) visualRotation = visualRoot.rotation;
        if (visualRoot != null) modelRenderers = visualRoot.GetComponentsInChildren<Renderer>();
        savedBlocks = new MaterialPropertyBlock[modelRenderers.Length];
        for (int i = 0; i < savedBlocks.Length; i++) savedBlocks[i] = new MaterialPropertyBlock();
        body = GetComponent<Rigidbody>();
        player = GetComponent<PlayerScript>();
        alternate = GetComponent<PlayerTwo>();
        animator = GetComponentInChildren<Animator>();
        if (animator == null) return;
        animator.applyRootMotion = false;
        hasCadence = HasParameter("Cadence", AnimatorControllerParameterType.Float);
        hasSpeed = HasParameter(speedParameter, AnimatorControllerParameterType.Float);
        hasGrounded = HasParameter(groundedParameter, AnimatorControllerParameterType.Bool);
        hasAttack = HasParameter(attackParameter, AnimatorControllerParameterType.Trigger);
    }

    private bool HasParameter(string name, AnimatorControllerParameterType type)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
            if (parameter.name == name && parameter.type == type) return true;
        return false;
    }

    private void Update()
    {
        UpdateDamageFlash();
        if (animator == null || body == null) return;
        bool active = NoxSession.CanPlay;
        animator.speed = active ? 1 : 0;
        float speed = player != null ? player.AnimationSpeed : alternate != null ? alternate.AnimationSpeed : 0;
        if (hasSpeed) animator.SetFloat(speedParameter, speed, 0.08f, Time.deltaTime);
        if (hasCadence) animator.SetFloat("Cadence", Mathf.Clamp(speed / 4f, 0.6f, 1.8f));
        if (hasGrounded) animator.SetBool(groundedParameter,
            player != null ? player.grounded : alternate != null && alternate.grounded);
    }

    private void LateUpdate()
    {
        if (visualRoot == null || !NoxSession.CanPlay) return;
        // Suaviza somente a apresentação; direção de movimento/mira e colisões são preservadas.
        visualRotation = Quaternion.RotateTowards(visualRotation, transform.rotation, turnSpeed * Time.deltaTime);
        visualRoot.rotation = visualRotation;
    }

    public void PulseDamage()
    {
        if (!flashVisible)
            for (int i = 0; i < modelRenderers.Length; i++)
                if (modelRenderers[i] != null) modelRenderers[i].GetPropertyBlock(savedBlocks[i]);
        flashUntil = Time.time + 0.16f;
        flashVisible = true;
    }

    private void UpdateDamageFlash()
    {
        if (!flashVisible) return;
        bool visible = Time.time < flashUntil;
        for (int i = 0; i < modelRenderers.Length; i++)
        {
            Renderer renderer = modelRenderers[i];
            if (renderer == null) continue;
            // PropertyBlock conserva os materiais compartilhados e as texturas do modelo.
            renderer.GetPropertyBlock(flashBlock);
            if (visible) flashBlock.SetColor("_Color", new Color(1f, 0.55f, 0.35f));
            renderer.SetPropertyBlock(visible ? flashBlock : savedBlocks[i]);
        }
        if (!visible) flashVisible = false;
    }

    public void PlayAttack()
    {
        if (animator == null) return;
        if (hasAttack) animator.SetTrigger(attackParameter);
        else if (!warnedMissingAttack)
        {
            warnedMissingAttack = true;
            Debug.LogWarning("O visual do Nox não possui o gatilho de ataque configurado. O golpe físico continua disponível.", this);
        }
    }
}
