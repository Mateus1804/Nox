using UnityEngine;

// Adaptador de cenas recuperadas; toda a lógica está em NoxPlayerController.
public class PlayerTwo : NoxPlayerController
{
    public static PlayerTwo PT;

    protected override void Awake()
    {
        base.Awake();
        if (Active == this)
            PT = this;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (PT == this)
            PT = null;
    }
}
