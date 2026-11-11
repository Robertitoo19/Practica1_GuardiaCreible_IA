// ═══════════════════════════════════════════════════════════════════════════
//  NodosGuardia.cs — Nodos hoja del árbol del guardia
//
//  Inteligencia Artificial (IA3) · GDDVV 3º · UDIT · Curso 2026/2027
//
//  Cada nodo hace SOLO una cosa:
//    · lee la pizarra
//    · escribe intenciones (destino / tieneDestino)
//    · devuelve Running / Success / Failure
//
//  Ninguno llama a SetDestination ni a Physics.Raycast.
// ═══════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UDIT.IA3.BT;

namespace UDIT.IA3.Guardia
{
    // ───────────────────────────────────────────────────────────────────────
    //  PERSEGUIR
    //
    //  Equivale a EstadoPerseguir:
    //    · si NO se le ve, falla para que el selector pase a Buscar
    //    · si se le ve, pide ir a la posición actual del jugador
    // ───────────────────────────────────────────────────────────────────────
    public class NodoPerseguir : Node
    {
        private readonly GuardiaAgente _agente;

        public NodoPerseguir(GuardiaAgente agente) : base("Perseguir")
        {
            _agente = agente;
        }

        // Equivalente a EstadoPerseguir.Enter(): una vez, al empezar a perseguir
        protected override void OnInitialize(Blackboard bb)
        {
            if (_agente == null) return;

            GuardiaBarks bark = _agente.GetComponent<GuardiaBarks>();
            if (bark != null) bark.Decir("¡Alto ahí!");
        }

        protected override Status Update(Blackboard bb)
        {
            if (!bb.Get(ClavesBB.Visible, false))
                return Status.Failure;

            Vector3 pos = bb.Get(ClavesBB.PosJugador,
                                 bb.Get(ClavesBB.UltimaPos, Vector3.zero));

            bb.Set(ClavesBB.Destino, pos);
            bb.Set(ClavesBB.TieneDestino, true);

            return Status.Running;
        }
    }

    // ───────────────────────────────────────────────────────────────────────
    //  INVESTIGAR
    //
    //  Equivale a EstadoInvestigar:
    //    · si hay PosJugador, se acerca sin comprometerse
    //    · si no, se queda quieto
    //
    //  No limpia nada en OnTerminate: el ActiveSelector aborta la rama
    //  antigua DESPUÉS de tickear la nueva, así que borrar aquí podría
    //  pisar el destino que acaba de escribir Perseguir.
    // ───────────────────────────────────────────────────────────────────────
    public class NodoInvestigar : Node
    {
        public NodoInvestigar() : base("Investigar") { }

        protected override void OnInitialize(Blackboard bb)
        {
            ActualizarDestino(bb);
        }

        protected override Status Update(Blackboard bb)
        {
            ActualizarDestino(bb);
            return Status.Running;
        }

        private static void ActualizarDestino(Blackboard bb)
        {
            if (bb.Has(ClavesBB.PosJugador))
            {
                bb.Set(ClavesBB.Destino,
                       bb.Get(ClavesBB.PosJugador, Vector3.zero));
                bb.Set(ClavesBB.TieneDestino, true);
            }
            else
            {
                bb.Set(ClavesBB.TieneDestino, false);
            }
        }
    }

    // ───────────────────────────────────────────────────────────────────────
    //  BUSCAR
    //
    //  Equivale a EstadoBuscar:
    //    · al entrar, pide ir a la última posición conocida
    //    · durante 8 s sigue buscando
    //    · al terminar, invalida el recuerdo y vuelve a patrullar
    //
    //  El temporizador vive en el nodo, no en la pizarra. Es estado de la
    //  ACCIÓN, no «en qué estado estoy».
    // ───────────────────────────────────────────────────────────────────────
    public class NodoBuscar : Node
    {
        private const float TiempoBusqueda = 8f;

        private float _inicio;

        public NodoBuscar() : base("Buscar") { }

        protected override void OnInitialize(Blackboard bb)
        {
            _inicio = bb.Tiempo;

            if (bb.Get(ClavesBB.TieneUltimaPos, false))
            {
                bb.Set(ClavesBB.Destino,
                       bb.Get(ClavesBB.UltimaPos, Vector3.zero));
                bb.Set(ClavesBB.TieneDestino, true);
            }
            else
            {
                bb.Set(ClavesBB.TieneDestino, false);
            }
        }

        protected override Status Update(Blackboard bb)
        {
            if (bb.Tiempo - _inicio >= TiempoBusqueda)
                return Status.Success;

            return Status.Running;
        }

        protected override void OnTerminate(Blackboard bb, Status resultado)
        {
            // Solo se limpia cuando la búsqueda termina de verdad.
            // Si la aborta Perseguir, no borramos TieneUltimaPos: el sensor
            // puede haberlo actualizado en este mismo frame.
            if (resultado == Status.Success)
            {
                bb.Set(ClavesBB.TieneUltimaPos, false);
                bb.Set(ClavesBB.TieneDestino, false);
            }
        }
    }

    // ───────────────────────────────────────────────────────────────────────
    //  PATRULLAR
    //
    //  Equivale a EstadoPatrullar:
    //    · al entrar, pide el punto actual de la ruta
    //    · cuando llega, avanza al siguiente
    //    · nunca termina: devuelve Running
    // ───────────────────────────────────────────────────────────────────────
    public class NodoPatrullar : Node
    {
        private readonly GuardiaAgente _agente;
        private RutaPatrulla _ruta;

        public NodoPatrullar(GuardiaAgente agente) : base("Patrullar")
        {
            _agente = agente;
        }

        protected override void OnInitialize(Blackboard bb)
        {
            _ruta = _agente != null ? _agente.GetComponent<RutaPatrulla>() : null;

            if (_ruta == null || !_ruta.TieneRuta)
            {
                bb.Set(ClavesBB.TieneDestino, false);

                Debug.LogWarning(
                    "[NodoPatrullar] El guardia no tiene RutaPatrulla con puntos. " +
                    "Revisad el montaje de la escena.", _agente);
                return;
            }

            bb.Set(ClavesBB.Destino, _ruta.PuntoActual);
            bb.Set(ClavesBB.TieneDestino, true);
        }

        protected override Status Update(Blackboard bb)
        {
            if (_ruta == null || !_ruta.TieneRuta)
                return Status.Running;

            if (bb.Get(ClavesBB.HaLlegado, false))
            {
                bb.Set(ClavesBB.Destino, _ruta.Avanzar());
                bb.Set(ClavesBB.TieneDestino, true);
            }

            return Status.Running;
        }
    }
}