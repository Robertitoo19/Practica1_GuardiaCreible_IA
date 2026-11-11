using UnityEngine;

public class GuardiaBarks : MonoBehaviour
{
    public AudioSource Audio;
    public AudioClip ClipAlerta;
    // segundos mínimos entre barks
    public float Cooldown = 4f;      
    private float _ultimo = -999f;

    public void Decir(string frase)
    {
        // evita el spam
        if (Time.time - _ultimo < Cooldown) return;   
        _ultimo = Time.time;

        Debug.Log("[" + name + "] " + frase, this);
        if (Audio != null && ClipAlerta != null) Audio.PlayOneShot(ClipAlerta);
        Debug.Log("sonido");
    }
}
