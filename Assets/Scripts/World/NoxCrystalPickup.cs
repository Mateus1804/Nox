using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom(true, sourceNamespace: null, sourceAssembly: "Assembly-CSharp", sourceClassName: "Cristais")]
public class NoxCrystalPickup : MonoBehaviour
{
    [FormerlySerializedAs("breakCristals")]
    public AudioSource pickupSound;
    private bool collected;

    public void PlayPickupSound()
    {
        if (pickupSound == null || pickupSound.clip == null)
            return;
        if (pickupSound.transform.IsChildOf(transform))
        {
            // Um som pertencente ao objeto coletado precisa sobreviver ao seu descarte.
            AudioSource.PlayClipAtPoint(pickupSound.clip, transform.position, pickupSound.volume);
        }
        else
            pickupSound.PlayOneShot(pickupSound.clip);
    }
    public void OnTriggerEnter(Collider other)
    {
        if (collected || !NoxSession.CanPlay || other == null || !other.CompareTag("Player"))
            return;
        collected = true;
        foreach (Collider item in GetComponentsInChildren<Collider>())
            item.enabled = false;
        PlayPickupSound();
        Destroy(gameObject);
    }
    // API de compatibilidade para os dados e callbacks recuperados.
    public AudioSource breakCristals
    {
        get => pickupSound; set => pickupSound = value;
    }

    // Nomes serializados nos UnityEvents de cenas anteriores.
    public void BreakCristal() => PlayPickupSound();
}
