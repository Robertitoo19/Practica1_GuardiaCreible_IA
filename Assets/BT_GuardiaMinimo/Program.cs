// ═══════════════════════════════════════════════════════════════════════════
//  Program.cs — Arnés de consola del árbol del guardia
//
//  Inteligencia Artificial · GDDVV 3º · UDIT · Curso 2026/2027
//
//  Ejecuta el árbol contra un mundo simulado e imprime la traza de cada tick.
//  Sirve para tres cosas:
//    1. Ver la semántica del árbol sin abrir Unity (en clase, sesión 9).
//    2. Depurar VUESTRO árbol aislado del motor: si aquí no se comporta, el
//       problema no es Unity.
//    3. Enseñar un error clásico y su arreglo: se ejecutan DOS versiones del
//       mismo árbol, una con Sequence y otra con Monitor.
//
//  Compilar y ejecutar:      dotnet run
// ═══════════════════════════════════════════════════════════════════════════

using System;
using UDIT.IA3.BT;

namespace UDIT.IA3.BT.Demo
{
    static class Program
    {
        // Guion fijo de percepción, para que la traza sea siempre la misma y se
        // pueda comentar en clase tick a tick.
        static readonly bool[] Vision =
        {
        //   0      1      2      3      4      5      6      7
            false, false, true,  true,  false, false, false, false,
        //   8      9      10     11     12     13
            false, true,  false, false, false, false
        };

        static int Main()
        {
            Trazador.Activo = true;

            Ejecutar("VERSIÓN 1 — con Sequence (la intuitiva… y equivocada)",
                     ConstruirConSequence());

            Ejecutar("VERSIÓN 2 — con Monitor (la correcta)",
                     ConstruirConMonitor());

            Console.WriteLine();
            Console.WriteLine("Comparad el tick 4 en las dos versiones: es donde el");
            Console.WriteLine("jugador desaparece de la vista.");
            return 0;
        }

        // ───────────────────────────────────────────────────────────────────
        static void Ejecutar(string titulo, Node raiz)
        {
            var bb = new Blackboard();
            bb.Set("visible", false);
            bb.Set("tieneUltimaPos", false);
            bb.Set("pasosBusqueda", 0);

            var arbol = new BehaviorTree(raiz, bb);

            Console.WriteLine();
            Console.WriteLine("══════════════════════════════════════════════════════════");
            Console.WriteLine(" " + titulo);
            Console.WriteLine("══════════════════════════════════════════════════════════");

            for (int t = 0; t < Vision.Length; t++)
            {
                // ── Percepción: esto NO lo hace el árbol, lo hace el sensor ──
                bool veAhora   = Vision[t];
                bool veiaAntes = bb.Get("visible", false);

                if (veiaAntes && !veAhora)          // acaba de perderlo de vista
                {
                    bb.Set("tieneUltimaPos", true);
                    bb.Set("pasosBusqueda", 0);
                }

                bb.Set("visible", veAhora);
                bb.Tiempo = t * 0.2f;               // 5 Hz

                // ── Un ciclo de pensamiento ──────────────────────────────────
                Status s = arbol.Tick();

                Console.WriteLine("t={0,2}  ve={1,-5}  hace: {2,-22}  raíz: {3}",
                                  t, veAhora, HojaActiva(), s);
            }
        }

        /// <summary>
        /// La hoja que de verdad se está ejecutando. El trazador registra en
        /// postorden (primero los hijos, luego los padres), así que el PRIMER
        /// Running de la traza es el nodo más profundo que sigue vivo.
        /// </summary>
        static string HojaActiva()
        {
            foreach (var paso in Trazador.Tick)
                if (paso.EndsWith(":Running"))
                    return paso.Replace(":Running", "");
            return "(nada en curso)";
        }

        // ───────────────────────────────────────────────────────────────────
        //  VERSIÓN 1: condición + acción dentro de una Sequence
        //
        //  Parece correcto y no lo es. Cuando la secuencia devuelve Running,
        //  el siguiente tick RETOMA en la acción: la condición se queda atrás
        //  y no se vuelve a comprobar nunca. El guardia persigue a un jugador
        //  al que ya no ve.
        // ───────────────────────────────────────────────────────────────────
        static Node ConstruirConSequence()
        {
            return new ActiveSelector("Guardia",
                new Sequence("Cazar",
                    VeAlJugador(),
                    Perseguir()),
                Buscar(),
                Patrullar());
        }

        // ───────────────────────────────────────────────────────────────────
        //  VERSIÓN 2: el mismo árbol con un Monitor
        //
        //  La guarda se comprueba EN CADA TICK. En cuanto deja de cumplirse,
        //  el cuerpo se aborta y la rama falla, así que el selector activo pasa
        //  a la siguiente. Es el patrón que el capítulo 6 del Game AI Pro llama
        //  «probablemente el más útil de todos».
        // ───────────────────────────────────────────────────────────────────
        static Node ConstruirConMonitor()
        {
            return new ActiveSelector("Guardia",
                new Monitor("Cazar",
                    VeAlJugador(),
                    Perseguir()),
                Buscar(),
                Patrullar());
        }

        // ── Hojas del dominio, compartidas por las dos versiones ────────────

        static Node VeAlJugador()
            => new Condition("¿Lo ve?", bb => bb.Get("visible", false));

        static Node Perseguir()
            => new ActionNode("Perseguir", bb => Status.Running);

        static Node Patrullar()
            => new ActionNode("Patrullar", bb => Status.Running);

        static Node Buscar()
        {
            return new Sequence("Buscar",
                new Condition("¿Hay última posición?",
                              bb => bb.Get("tieneUltimaPos", false)),

                new ActionNode("Ir al último punto", bb =>
                {
                    // Simula un desplazamiento que dura 3 ticks
                    int pasos = bb.Get("pasosBusqueda", 0) + 1;
                    bb.Set("pasosBusqueda", pasos);
                    return pasos < 3 ? Status.Running : Status.Success;
                }),

                // Esbozo que acaba AL INSTANTE: por eso, en el tick en que se
                // llega al último punto, la traza dice «(nada en curso)» y la
                // raíz devuelve Success. Uno de verdad devolvería Running
                // mientras gira.
                new ActionNode("Mirar alrededor", bb =>
                {
                    bb.Set("tieneUltimaPos", false);   // se acabó la pista
                    return Status.Success;
                }));
        }
    }
}
