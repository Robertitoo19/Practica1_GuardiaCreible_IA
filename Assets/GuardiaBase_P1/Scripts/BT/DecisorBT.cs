// ═══════════════════════════════════════════════════════════════════════════
//  DecisorBT.cs — Cerebro versión 2: árbol de comportamiento
//
//  Inteligencia Artificial (IA3) · GDDVV 3º · UDIT · Curso 2026/2027
//
//  APARTADO C DEL ENUNCIADO.
//
//  El runtime (BehaviorTree.cs) OS LO DOY. No lo modifiquéis.
//  Lo que hay aquí es LA FORMA DEL ÁRBOL. Los nodos hoja están en
//  NodosGuardia.cs.
//
//  El árbol produce el mismo comportamiento observable que DecisorFSM:
//    Alerta   -> Perseguir
//    Sospecha -> Investigar
//    Última posición conocida -> Buscar
//    Si no    -> Patrullar
//
//  No hay ningún Raycast ni SetDestination aquí: solo se lee la pizarra y
//  se escriben intenciones (destino / tieneDestino).
// ═══════════════════════════════════════════════════════════════════════════

using UDIT.IA3.BT;

namespace UDIT.IA3.Guardia
{
    public class DecisorBT : IDecisor
    {
        private readonly GuardiaAgente _agente;
        private BehaviorTree _arbol;

        public DecisorBT(GuardiaAgente agente) { _agente = agente; }

        public void Iniciar(Blackboard bb)
        {
            _arbol = new BehaviorTree(ConstruirArbol(), bb);
        }

        public void Decidir(Blackboard bb)
        {
            if (_arbol != null) _arbol.Tick();
        }

        public string Traza { get { return "BT: " + Trazador.Ultima(); } }

        // ───────────────────────────────────────────────────────────────────
        //  EL ÁRBOL
        //
        //  Orden de prioridad del ActiveSelector:
        //    1. Perseguir   (Monitor: nivel == Alerta)
        //    2. Investigar  (Monitor: nivel == Sospecha)
        //    3. Buscar      (Sequence: tiene última posición -> NodoBuscar)
        //    4. Patrullar   (acción de fondo)
        //
        //  Se usa Monitor en las dos primeras ramas porque la guarda tiene
        //  que comprobarse EN CADA TICK. Si se usara Sequence, el guardia
        //  seguiría persiguiendo a un jugador al que ya no ve.
        // ───────────────────────────────────────────────────────────────────

        private Node ConstruirArbol()
        {
            return new ActiveSelector("Guardia",

                // ── 1. ALERTA -> Perseguir ─────────────────────────────────
                new Monitor("Perseguir",
                    new Condition("¿Alerta?", bb =>
                        bb.Get(ClavesBB.Nivel, NivelSospecha.Tranquilo) == NivelSospecha.Alerta),
                    new NodoPerseguir(_agente)),

                // ── 2. SOSPECHA -> Investigar ──────────────────────────────
                new Monitor("Investigar",
                    new Condition("¿Sospecha?", bb =>
                        bb.Get(ClavesBB.Nivel, NivelSospecha.Tranquilo) == NivelSospecha.Sospecha),
                    new NodoInvestigar()),

                // ── 3. ÚLTIMA POSICIÓN CONOCIDA -> Buscar ──────────────────
                new Sequence("Buscar",
                    new Condition("¿Tengo última posición?", bb =>
                        bb.Get(ClavesBB.TieneUltimaPos, false)),
                    new NodoBuscar()),

                // ── 4. PATRULLAR ───────────────────────────────────────────
                new NodoPatrullar(_agente)
            );
        }
    }
}