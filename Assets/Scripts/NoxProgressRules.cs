public static class NoxProgressRules
{
    // Apenas as fases presentes na build são retomadas; fogo exige desbloqueio.
    public static bool CanResume(int scene, int sceneCount, bool fireUnlocked)
    {
        return scene > 0 && scene < sceneCount && (scene != 4 || fireUnlocked);
    }
}
