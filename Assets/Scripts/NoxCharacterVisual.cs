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
    private bool hasSpeed;
    private bool hasGrounded;
    private bool hasAttack;
    private bool warnedMissingAttack;

    private void Start()
    {
        visualRoot = transform.Find("NoxVisual");
        if (visualRoot != null) visualRotation = visualRoot.rotation;
        body = GetComponent<Rigidbody>();
        player = GetComponent<PlayerScript>();
        alternate = GetComponent<PlayerTwo>();
        animator = GetComponentInChildren<Animator>();
        if (animator == null) return;
        animator.applyRootMotion = false;
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
        if (animator == null || body == null) return;
        bool active = NoxSession.CanPlay;
        animator.speed = active ? 1 : 0;
        if (hasSpeed) animator.SetFloat(speedParameter,
            player != null ? player.AnimationSpeed : alternate != null ? alternate.AnimationSpeed : 0,
            0.1f, Time.deltaTime);
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
