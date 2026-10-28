using UnityEngine;

public class Player : MonoBehaviour
{
    public float velocidad = 5f;

    private CharacterController controller;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 movimiento = new Vector3(horizontal, 0f, vertical).normalized;

        controller.Move(movimiento * velocidad * Time.deltaTime);
    }
}
