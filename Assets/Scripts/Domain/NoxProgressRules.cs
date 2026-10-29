

public static class NoxProgressRules
{
    // Apenas as fases presentes na build são retomadas; fogo exige desbloqueio.
    public static bool CanResume(int scene, int sceneCount, bool fireUnlocked)
    {
        return scene > 0 && scene < sceneCount && (scene != 4 || fireUnlocked);
    }
}

public sealed class NoxSceneLoadGate
{
    public bool Busy
    {
        get; private set;
    }
    public bool TryBegin(int index, int count)
    {
        if (Busy || !NoxRules.IsValidIndex(index, count))
            return false;
        Busy = true;
        return true;
    }
    public void Complete()
    {
        Busy = false;
    }
}
