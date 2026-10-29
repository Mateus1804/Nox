using UnityEngine;

public class Cristais : MonoBehaviour
{
    public AudioSource breakCristals;
    private bool collected;

    public void BreakCristal()
    {
        if (breakCristals == null || breakCristals.clip == null)
            return;
        if (breakCristals.transform.IsChildOf(transform))
        {
            // Um som pertencente ao objeto coletado precisa sobreviver ao seu descarte.
            AudioSource.PlayClipAtPoint(breakCristals.clip, transform.position, breakCristals.volume);
        }
        else
            breakCristals.PlayOneShot(breakCristals.clip);
    }
    public void OnTriggerEnter(Collider other)
    {
        if (collected || !NoxSession.CanPlay || other == null || !other.CompareTag("Player"))
            return;
        collected = true;
        foreach (Collider item in GetComponentsInChildren<Collider>())
            item.enabled = false;
        BreakCristal();
        Destroy(gameObject);
    }
}
