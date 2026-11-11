using UnityEngine;

[RequireComponent(typeof(FuenteRuido))]
[RequireComponent(typeof(CharacterController))]
public class PlayerRuido : MonoBehaviour
{
    public float VelocidadParaRuidoMaximo = 5f;
    [Range(0f, 1f)] public float IntensidadMaxima = 0.8f;

    [Header("Suavizado")]
    [Tooltip("Cuánto tarda la intensidad en seguir a la velocidad real. " +
             "Alto = responde rápido, bajo = cambios suaves.")]
    public float Suavizado = 10f;

    [Header("Gizmo")]
    public bool DibujarGizmo = true;
    public int Anillos = 3;
    public float VelocidadPulso = 3f;
    public float RadioMaximo = 2.5f;

    private FuenteRuido _fuente;
    private CharacterController _controller;
    private Vector3 _posAnterior;
    private float _velocidadSuavizada;

    private void Awake()
    {
        _fuente = GetComponent<FuenteRuido>();
        _controller = GetComponent<CharacterController>();
        _posAnterior = transform.position;
    }

    private void Update()
    {
        // Velocidad horizontal real = desplazamiento entre frames / dt
        Vector3 delta = transform.position - _posAnterior;
        delta.y = 0f;
        float v = delta.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        _posAnterior = transform.position;

        // Suavizado exponencial: la intensidad no salta, sube y baja progresivamente
        _velocidadSuavizada = Mathf.Lerp(_velocidadSuavizada, v,
                                         1f - Mathf.Exp(-Suavizado * Time.deltaTime));

        float factor = Mathf.Clamp01(_velocidadSuavizada / VelocidadParaRuidoMaximo);
        _fuente.EmiteConstantemente = true;
        _fuente.Intensidad = factor * IntensidadMaxima;
    }

    private void OnDrawGizmos()
    {
        if (!DibujarGizmo || !Application.isPlaying) return;
        if (_fuente == null) _fuente = GetComponent<FuenteRuido>();
        if (_fuente == null) return;

        float intensidad = _fuente.IntensidadActual;
        if (intensidad < 0.02f) return;

        Color c = Color.Lerp(Color.white, new Color(1f, 0.45f, 0f, 1f), intensidad);

        float fase = Time.time * VelocidadPulso;
        Vector3 centro = transform.position + Vector3.up * 0.9f;

        for (int i = 0; i < Anillos; i++)
        {
            float t = Mathf.Repeat(fase + (float)i / Anillos, 1f);
            float radio = Mathf.Lerp(0.2f, RadioMaximo * intensidad, t);
            float alpha = (1f - t) * intensidad;

            Gizmos.color = new Color(c.r, c.g, c.b, alpha);
            Gizmos.DrawWireSphere(centro, radio);
        }

        Gizmos.color = new Color(c.r, c.g, c.b, 0.15f);
        Gizmos.DrawSphere(centro, RadioMaximo * intensidad);
    }
}