using System.Collections.Generic;
using UnityEngine;

public class FuenteRuido : MonoBehaviour
{
    public static readonly List<FuenteRuido> Activas = new List<FuenteRuido>();

    [Header("Emisión")]
    [Range(0f, 1f)] public float Intensidad = 0.8f;
    public float DuracionSegundos = 0.4f;
    public bool EmiteConstantemente = false;

    private float _finEmision = -1f;

    private void OnEnable() { if (!Activas.Contains(this)) Activas.Add(this); }
    private void OnDisable() { Activas.Remove(this); }

    public void Activar() { _finEmision = Time.time + DuracionSegundos; }

    public float IntensidadActual
    {
        get
        {
            if (EmiteConstantemente) return Intensidad;
            return Time.time < _finEmision ? Intensidad : 0f;
        }
    }

    public bool EstaSonando { get { return IntensidadActual > 0f; } }
}