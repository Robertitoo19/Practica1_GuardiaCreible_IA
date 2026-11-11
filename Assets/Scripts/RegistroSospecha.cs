using System.IO;
using System.Text;
using UnityEngine;
using UDIT.IA3.BT;

namespace UDIT.IA3.Guardia
{
    [RequireComponent(typeof(SensorVision))]
    public class RegistroSospecha : MonoBehaviour
    {
        [Header("Grabación")]
        public bool Grabar = false;
        [Tooltip("Ruta relativa al proyecto. Por defecto, la raíz.")]
        public string Fichero = "sospecha.csv";
        [Tooltip("Cada cuántos segundos escribe una fila. 0.1 = 10 Hz, suficiente.")]
        public float Intervalo = 0.1f;

        private SensorVision _vision;
        private GuardiaAgente _agente;
        private StreamWriter _sw;
        private float _proximo;

        private void Awake()
        {
            _vision = GetComponent<SensorVision>();
            _agente = GetComponent<GuardiaAgente>();
        }

        private void OnEnable()
        {
            if (!Grabar) return;

            string nombreGuardia = gameObject.name.Replace(" ", "_");
            string ficheroFinal = Fichero.Replace(".csv", "_" + nombreGuardia + ".csv");
            string ruta = Path.Combine(Application.dataPath, "..", ficheroFinal);
            _sw = new StreamWriter(ruta, false, Encoding.UTF8);
            _sw.WriteLine("t;nivel;sospecha;visible;raycasts;evidencia;destino");
            Debug.Log("[RegistroSospecha] Escribiendo en " + Path.GetFullPath(ruta));
        }

        private void Update()
        {
            if (!Grabar || _sw == null || _vision == null) return;
            if (Time.time < _proximo) return;
            _proximo = Time.time + Intervalo;

            string nivel = _vision.NivelActual.ToString();
            float sos = _vision.SospechaActual;
            bool vis = _vision.VisibleAhora;
            int rays = _vision.RaycastsLlegados;
            float ev = _vision.UltimaEvidencia;

            string destino = "-";
            if (_agente != null && _agente.Pizarra != null)
                destino = _agente.Pizarra.Get(ClavesBB.Destino, Vector3.zero).ToString("F2");

            _sw.WriteLine(string.Format(
                "{0:F3};{1};{2:F4};{3};{4};{5:F4};{6}",
                Time.time, nivel, sos, vis, rays, ev, destino));
        }

        private void OnDisable()
        {
            if (_sw != null) { _sw.Flush(); _sw.Close(); _sw = null; }
        }
    }
}