// ═══════════════════════════════════════════════════════════════════════════
//  SensorVision.cs — LA PIEZA PRINCIPAL DE LA PRÁCTICA
//
//  Inteligencia Artificial (IA3) · GDDVV 3º · UDIT · Curso 2026/2027
//  Dr. Luis Peña Sánchez
//
//  ⚠️  LO QUE OS DOY AQUÍ ESTÁ DELIBERADAMENTE MAL.
//
//  Tal y como está, este sensor detecta por DISTANCIA Y NADA MÁS. Es decir,
//  es exactamente el primero de los errores típicos de la sesión 4: el guardia
//  os ve a través de las paredes y de espaldas.
//
//  Está así a propósito. Arrancad el proyecto, jugad treinta segundos y veréis
//  el problema con vuestros propios ojos. Vuestro trabajo en el apartado A del
//  enunciado es convertir esto en un sensor que se pueda defender:
//
//     ETAPA 1  alcance   ✅ hecho (es lo único que hay)
//     ETAPA 2  ángulo    ❌ TODO
//     ETAPA 3  oclusión  ❌ TODO   ← y con VARIOS puntos, no solo el centro
//     MEDIDOR  sospecha  ❌ TODO   ← 3 factores, decaimiento, histéresis
//
//  Filtrar barato antes de filtrar caro (sesión 4): el orden de las tres
//  etapas no es casual. La comprobación de distancia es un producto escalar;
//  el raycast es una consulta al motor de físicas. Poner el raycast primero
//  «funciona» y cuesta veinte veces más.
// ═══════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UDIT.IA3.BT;

namespace UDIT.IA3.Guardia
{
    public class SensorVision : MonoBehaviour, ISensor
    {
        [Header("Objetivo")]
        public Transform Objetivo;
        public string TagObjetivo = "Player";

        [Header("Cono de visión")]
        public float Alcance = 15f;
        [Range(0f, 180f)] public float AnguloVision = 60f;
        public Transform Ojos;
        public LayerMask CapasQueOcluyen;

        [Header("Puntos de raycast")]
        public float[] AlturasRaycast = { 0.8f, 0f, -0.9f };

        [Header("Medidor de sospecha")]
        public float VelocidadSubida = 0.8f;
        public float VelocidadBajada = 0.08f;
        [Range(0f, 1f)] public float UmbralSospecha = 0.35f;
        [Range(0f, 1f)] public float UmbralAlerta = 0.8f;
        [Range(0f, 0.5f)] public float Histeresis = 0.1f;

        [Header("Depuración")]
        public bool DibujarRaycasts = true;

        private float _sospecha;
        private NivelSospecha _nivel = NivelSospecha.Tranquilo;
        private bool _visiblePrevio;

        public bool VisibleAhora { get; private set; }
        public float SospechaActual { get { return _sospecha; } }
        public NivelSospecha NivelActual { get { return _nivel; } }
        public int RaycastsLlegados { get; private set; }
        public float UltimaEvidencia { get; private set; }

        private void Awake()
        {
            if (Ojos == null) Ojos = transform;

            if (Objetivo == null && !string.IsNullOrEmpty(TagObjetivo))
            {
                GameObject go = GameObject.FindGameObjectWithTag(TagObjetivo);
                if (go != null) Objetivo = go.transform;
            }
        }

        public void Percibir(Blackboard bb)
        {
            if (Objetivo == null)
            {
                float evAudioSolo = bb.Get(ClavesBB.RuidoIntensidad, 0f);
                ActualizarSospecha(evAudioSolo, Time.deltaTime);
                VisibleAhora = false;
                _visiblePrevio = false;
                bb.Set(ClavesBB.Visible, false);
                bb.Set(ClavesBB.Sospecha, _sospecha);
                bb.Set(ClavesBB.Nivel, _nivel);
                return;
            }

            int llegados;
            bool visible = ComprobarVision(out llegados);
            VisibleAhora = visible;
            RaycastsLlegados = llegados;

            float evidenciaVisual = visible ? CalcularEvidencia(llegados) : 0f;
            float evidenciaAudio = bb.Get(ClavesBB.RuidoIntensidad, 0f);
            float evidencia = Mathf.Max(evidenciaVisual, evidenciaAudio);
            UltimaEvidencia = evidencia;
            ActualizarSospecha(evidencia, Time.deltaTime);

            if (!visible && _nivel == NivelSospecha.Alerta)
                _nivel = NivelSospecha.Sospecha;

            bb.Set(ClavesBB.Visible, visible);
            bb.Set(ClavesBB.Sospecha, _sospecha);
            bb.Set(ClavesBB.Nivel, _nivel);

            if (visible)
            {
                bb.Set(ClavesBB.UltimaPos, Objetivo.position);
                bb.Set(ClavesBB.TieneUltimaPos, true);
                bb.Set(ClavesBB.PosJugador, Objetivo.position);
            }
            else if (_visiblePrevio)
            {
                bb.Set(ClavesBB.UltimaPos, Objetivo.position);
                bb.Set(ClavesBB.TieneUltimaPos, true);
                bb.Set(ClavesBB.PosJugador, Objetivo.position);
                bb.Set(ClavesBB.VecesPerdido, bb.Get(ClavesBB.VecesPerdido, 0) + 1);
            }

            _visiblePrevio = visible;
        }

        private bool ComprobarVision(out int llegados)
        {
            llegados = 0;

            Vector3 haciaObjetivo = Objetivo.position - Ojos.position;

            if (haciaObjetivo.sqrMagnitude > Alcance * Alcance) return false;

            float angulo = Vector3.Angle(Ojos.forward, haciaObjetivo);
            if (angulo > AnguloVision * 0.5f) return false;

            if (AlturasRaycast == null || AlturasRaycast.Length == 0)
            {
                Debug.LogWarning(
                    "[SensorVision] AlturasRaycast vacío en " + name +
                    ": no se está comprobando oclusión multi-punto.", this);
                llegados = 1;
                return true;
            }

            for (int i = 0; i < AlturasRaycast.Length; i++)
            {
                Vector3 punto = Objetivo.position + Vector3.up * AlturasRaycast[i];
                Vector3 dir = punto - Ojos.position;
                float dist = dir.magnitude;

                if (!Physics.Raycast(Ojos.position, dir.normalized, dist, CapasQueOcluyen))
                    llegados++;
            }

            return llegados > 0;
        }

        private float CalcularEvidencia(int llegados)
        {
            Vector3 haciaObjetivo = Objetivo.position - Ojos.position;

            float dist = haciaObjetivo.magnitude;
            float fDist = Mathf.Clamp01(1f - dist / Alcance);

            float angulo = Vector3.Angle(Ojos.forward, haciaObjetivo);
            float semiCono = AnguloVision * 0.5f;
            float fAng = semiCono > 0.0001f
                                ? Mathf.Clamp01(1f - angulo / semiCono)
                                : 1f;

            int total = (AlturasRaycast != null && AlturasRaycast.Length > 0)
                            ? AlturasRaycast.Length
                            : 1;
            float fVis = (float)llegados / total;

            return fDist * fAng * fVis;
        }

        private void ActualizarSospecha(float evidencia, float dt)
        {
            if (evidencia > 0.01f)
                _sospecha += evidencia * VelocidadSubida * dt;
            else
                _sospecha -= VelocidadBajada * dt;

            _sospecha = Mathf.Clamp01(_sospecha);
            _nivel = ActualizarNivel(_sospecha, _nivel);
        }

        private NivelSospecha ActualizarNivel(float s, NivelSospecha actual)
        {
            switch (actual)
            {
                case NivelSospecha.Tranquilo:
                    if (s >= UmbralSospecha) return NivelSospecha.Sospecha;
                    break;

                case NivelSospecha.Sospecha:
                    if (s >= UmbralAlerta) return NivelSospecha.Alerta;
                    if (s < UmbralSospecha - Histeresis) return NivelSospecha.Tranquilo;
                    break;

                case NivelSospecha.Alerta:
                    if (s < UmbralAlerta - Histeresis) return NivelSospecha.Sospecha;
                    break;
            }
            return actual;
        }

        private void OnDrawGizmos()
        {
            if (!DibujarRaycasts) return;
            if (Objetivo == null) return;
            if (AlturasRaycast == null || AlturasRaycast.Length == 0) return;

            Transform ojos = Ojos != null ? Ojos : transform;

            for (int i = 0; i < AlturasRaycast.Length; i++)
            {
                Vector3 punto = Objetivo.position + Vector3.up * AlturasRaycast[i];
                Vector3 dir = punto - ojos.position;
                float dist = dir.magnitude;
                if (dist < 0.001f) continue;

                bool llega = !Physics.Raycast(ojos.position, dir.normalized,
                                              dist, CapasQueOcluyen);

                Gizmos.color = llega
                    ? new Color(0.2f, 1f, 0.2f, 0.85f)
                    : new Color(1f, 0.2f, 0.2f, 0.85f);

                Gizmos.DrawLine(ojos.position, punto);
                Gizmos.DrawWireSphere(punto, 0.07f);
            }
        }
    }
}
