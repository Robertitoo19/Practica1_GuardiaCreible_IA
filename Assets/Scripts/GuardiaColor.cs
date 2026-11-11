using UnityEngine;
using UDIT.IA3.BT;

namespace UDIT.IA3.Guardia
{
    [RequireComponent(typeof(SensorVision))]
    public class GuardiaColor : MonoBehaviour
    {
        [Header("Renderers a teñir")]
        [Tooltip("Si está vacío se cogen todos los Renderer de los hijos.")]
        public Renderer[] Renderers;

        [Header("Colores por nivel")]
        public Color ColorTranquilo = new Color(0.4f, 0.9f, 0.4f);
        public Color ColorSospecha = new Color(1f, 0.85f, 0.2f);
        public Color ColorAlerta = new Color(1f, 0.2f, 0.2f);

        [Header("Suavizado")]
        [Tooltip("0 = cambio instantáneo. 0.15–0.25 se ve bien en vídeo.")]
        [Range(0f, 1f)] public float Suavizado = 0.18f;

        private SensorVision _vision;
        private MaterialPropertyBlock _mpb;
        private static readonly int _idColor = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            _vision = GetComponent<SensorVision>();
            if (Renderers == null || Renderers.Length == 0)
                Renderers = GetComponentsInChildren<Renderer>();
            _mpb = new MaterialPropertyBlock();
        }

        private void Update()
        {
            if (_vision == null || Renderers == null) return;

            Color objetivo = ColorDe(_vision.NivelActual);

            for (int i = 0; i < Renderers.Length; i++)
            {
                if (Renderers[i] == null) continue;

                Renderers[i].GetPropertyBlock(_mpb);
                Color actual = _mpb.GetColor(_idColor);
                if (actual.a <= 0f) actual = objetivo;

                Color nuevo = Color.Lerp(actual, objetivo,
                                         1f - Mathf.Pow(0.001f, Suavizado * Time.deltaTime));
                _mpb.SetColor(_idColor, nuevo);
                Renderers[i].SetPropertyBlock(_mpb);
            }
        }

        private Color ColorDe(NivelSospecha n)
        {
            switch (n)
            {
                case NivelSospecha.Alerta: return ColorAlerta;
                case NivelSospecha.Sospecha: return ColorSospecha;
                default: return ColorTranquilo;
            }
        }
    }
}