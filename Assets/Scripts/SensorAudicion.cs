using UnityEngine;
using UDIT.IA3.BT;

namespace UDIT.IA3.Guardia
{
    public class SensorAudicion : MonoBehaviour, ISensor
    {
        [Header("Oído")]
        public float AlcanceAuditivo = 12f;
        [Range(0f, 1f)] public float UmbralMinimo = 0.05f;
        public float VelocidadDecaimiento = 1.2f;

        [Header("Gizmo")]
        public bool DibujarGizmo = true;
        public bool DibujarFuentesEnRango = true;

        public float IntensidadActual { get; private set; }
        public Vector3 UltimaFuenteRuido { get; private set; }

        public void Percibir(Blackboard bb)
        {
            float maxInt = 0f;
            Vector3 mejorPos = Vector3.zero;
            bool hayFuente = false;

            for (int i = 0; i < FuenteRuido.Activas.Count; i++)
            {
                FuenteRuido f = FuenteRuido.Activas[i];
                if (f == null || !f.EstaSonando) continue;

                Vector3 pos = f.transform.position;
                float dist = Vector3.Distance(transform.position, pos);
                if (dist > AlcanceAuditivo) continue;

                float atenuacion = 1f - (dist / AlcanceAuditivo);
                float recibida = f.IntensidadActual * atenuacion;

                if (recibida > maxInt)
                {
                    maxInt = recibida;
                    mejorPos = pos;
                    hayFuente = true;
                }
            }

            if (hayFuente && maxInt > UmbralMinimo)
            {
                IntensidadActual = Mathf.Max(IntensidadActual, maxInt);
                UltimaFuenteRuido = mejorPos;
            }
            else
            {
                IntensidadActual = Mathf.Max(0f, IntensidadActual - VelocidadDecaimiento * Time.deltaTime);
            }

            bool tieneRuido = IntensidadActual > UmbralMinimo;
            bb.Set(ClavesBB.RuidoIntensidad, tieneRuido ? IntensidadActual : 0f);
            bb.Set(ClavesBB.RuidoPos, UltimaFuenteRuido);
            bb.Set(ClavesBB.TieneRuido, tieneRuido);

            if (tieneRuido && !bb.Get(ClavesBB.Visible, false))
            {
                bb.Set(ClavesBB.PosJugador, UltimaFuenteRuido);
                bb.Set(ClavesBB.UltimaPos, UltimaFuenteRuido);
                bb.Set(ClavesBB.TieneUltimaPos, true);
            }
        }

        private void OnDrawGizmos()
        {
            if (!DibujarGizmo) return;

            Vector3 centro = transform.position + Vector3.up * 0.9f;

            // Esfera de alcance auditivo (siempre visible, también en edit mode)
            Gizmos.color = new Color(0.4f, 0.7f, 1f, 0.35f);
            Gizmos.DrawWireSphere(centro, AlcanceAuditivo);

            if (!Application.isPlaying) return;

            // Cuando está oyendo algo, se rellena con naranja
            if (IntensidadActual > UmbralMinimo)
            {
                Gizmos.color = new Color(1f, 0.55f, 0.1f, 0.06f);
                Gizmos.DrawSphere(centro, AlcanceAuditivo);

                // Marca del foco de sonido
                Gizmos.color = new Color(1f, 0.4f, 0f, 0.9f);
                Gizmos.DrawLine(centro, UltimaFuenteRuido + Vector3.up * 0.2f);
                Gizmos.DrawWireCube(UltimaFuenteRuido + Vector3.up * 0.1f,
                                    new Vector3(0.4f, 0.4f, 0.4f));
            }

            // Líneas a todas las fuentes de ruido dentro del alcance
            if (DibujarFuentesEnRango && FuenteRuido.Activas != null)
            {
                for (int i = 0; i < FuenteRuido.Activas.Count; i++)
                {
                    FuenteRuido f = FuenteRuido.Activas[i];
                    if (f == null || !f.EstaSonando) continue;

                    float dist = Vector3.Distance(transform.position, f.transform.position);
                    if (dist > AlcanceAuditivo) continue;

                    float at = 1f - (dist / AlcanceAuditivo);
                    float recibida = f.IntensidadActual * at;

                    Gizmos.color = recibida > UmbralMinimo
                        ? new Color(1f, 0.3f, 0.1f, 0.8f)   // oído: rojo
                        : new Color(1f, 1f, 0.3f, 0.35f);   // en rango pero flojo: amarillo

                    Gizmos.DrawLine(centro, f.transform.position + Vector3.up * 0.9f);
                }
            }
        }
    }
}