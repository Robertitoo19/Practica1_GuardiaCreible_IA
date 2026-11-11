using UnityEngine;

public class Player : MonoBehaviour
{
    public float Velocidad = 5f;
    public float VelocidadRotacion = 12f;
    public float Gravedad = -20f;

    private CharacterController _controller;
    private CamaraTerceraPersona _camara;
    private float _velocidadY;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _camara = FindAnyObjectByType<CamaraTerceraPersona>();
    }

    private void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 adelante = _camara != null ? _camara.DireccionAdelante : Vector3.forward;
        Vector3 derecha = _camara != null ? _camara.DireccionDerecha : Vector3.right;
        Vector3 movimiento = adelante * v + derecha * h;

        if (movimiento.sqrMagnitude > 1f) movimiento.Normalize();

        _controller.Move(movimiento * Velocidad * Time.deltaTime);

        if (movimiento.sqrMagnitude > 0.01f)
        {
            Quaternion objetivo = Quaternion.LookRotation(movimiento);
            transform.rotation = Quaternion.Slerp(transform.rotation, objetivo,
                                                  VelocidadRotacion * Time.deltaTime);
        }

        if (_controller.isGrounded && _velocidadY < 0f) _velocidadY = -2f;
        _velocidadY += Gravedad * Time.deltaTime;
        _controller.Move(Vector3.up * _velocidadY * Time.deltaTime);
    }
}