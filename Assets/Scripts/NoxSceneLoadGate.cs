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
