using UnityEngine;

public class CamaraTerceraPersona : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform Objetivo;
    public Vector3 OffsetObjetivo = new Vector3(0f, 1.5f, 0f);

    [Header("Órbita")]
    public float Distancia = 5f;
    public float DistanciaMin = 2f;
    public float SensibilidadRaton = 2f;
    public float PitchMin = -20f;
    public float PitchMax = 70f;

    [Header("Colisión")]
    public LayerMask CapasColision;
    public float RadioColision = 0.3f;
    public float VelocidadRecuperacion = 20f; // solo al alejarse tras una colisión

    [Header("Cursor")]
    public bool BloquearCursor = true;
    public KeyCode TeclaDesbloquear = KeyCode.Escape;

    private float _yaw;
    private float _pitch = 20f;
    private float _distanciaActual;

    private void Start()
    {
        if (Objetivo != null) _yaw = Objetivo.eulerAngles.y;
        _distanciaActual = Distancia;
        if (BloquearCursor) Bloquear(true);
    }

    private void Update()
    {
        if (BloquearCursor)
        {
            if (Input.GetKeyDown(TeclaDesbloquear)) Bloquear(false);
            else if (Input.GetMouseButtonDown(0) &&
                     Cursor.lockState != CursorLockMode.Locked) Bloquear(true);
        }

        if (Objetivo == null) return;

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            _yaw += Input.GetAxisRaw("Mouse X") * SensibilidadRaton;
            _pitch -= Input.GetAxisRaw("Mouse Y") * SensibilidadRaton;
            _pitch = Mathf.Clamp(_pitch, PitchMin, PitchMax);
        }
    }

    private void LateUpdate()
    {
        if (Objetivo == null) return;

        Vector3 pivot = Objetivo.position + OffsetObjetivo;
        Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 direccion = rot * Vector3.back;
        float distanciaDeseada = Distancia;

        if (Physics.SphereCast(pivot, RadioColision, direccion,
                               out RaycastHit hit, Distancia, CapasColision))
            distanciaDeseada = Mathf.Max(hit.distance - RadioColision, DistanciaMin);

        // Se acerca al instante (no atraviesa paredes); se aleja suave para evitar saltos
        if (distanciaDeseada < _distanciaActual)
            _distanciaActual = distanciaDeseada;
        else
            _distanciaActual = Mathf.MoveTowards(_distanciaActual, distanciaDeseada,
                                                  VelocidadRecuperacion * Time.deltaTime);

        transform.SetPositionAndRotation(pivot + direccion * _distanciaActual, rot);
    }

    private void Bloquear(bool bloquear)
    {
        Cursor.lockState = bloquear ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !bloquear;
    }

    // Basadas en el yaw (estables), no en el transform de la cámara
    public Vector3 DireccionAdelante => Quaternion.Euler(0f, _yaw, 0f) * Vector3.forward;
    public Vector3 DireccionDerecha => Quaternion.Euler(0f, _yaw, 0f) * Vector3.right;
}