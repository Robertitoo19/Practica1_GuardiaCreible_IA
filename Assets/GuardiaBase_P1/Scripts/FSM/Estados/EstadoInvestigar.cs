using UDIT.IA3.Guardia;
using UnityEngine;

public class EstadoInvestigar : State
{
    public EstadoInvestigar(StateMachine machine) : base(machine) { }

    internal override void Enter()
    {
        // si hay posicion del estimulo guardada en la pizarra, se acerca sin comprometerse
        if (Pizarra.Has(ClavesBB.PosJugador))
        {
            //Escribe la intencion de ir .No se mueve : LocomocionNavMesh lo lee esto y hace el SetDestination.
            Pizarra.Set(ClavesBB.Destino, Pizarra.Get(ClavesBB.PosJugador, Vector3.zero));
            //Hay un destino válido
            Pizarra.Set(ClavesBB.TieneDestino, true);
        }
        else
        {
            //si no hay pos, quieto
            Pizarra.Set(ClavesBB.TieneDestino, false); // solo se queda quieto
        }
    }

    internal override void Execute()
    {
        //Leer nivel sospecha
        NivelSospecha nivel = Pizarra.Get(ClavesBB.Nivel, NivelSospecha.Tranquilo);

        if (nivel == NivelSospecha.Alerta)
        {
            //(DESCOMENTAR CUANDO HAYA PERSEGUIR)
            StateMachine.ChangeState(new EstadoPerseguir(StateMachine));
            return;
        }

        if (nivel == NivelSospecha.Tranquilo)
        {
            StateMachine.ChangeState(new EstadoPatrullar(StateMachine));
            return;
        }

        // Sigue en Sospecha, mantiene el destino actualizado hacia el estímulo
        if (Pizarra.Has(ClavesBB.PosJugador))
        {
            Pizarra.Set(ClavesBB.Destino, Pizarra.Get(ClavesBB.PosJugador, Vector3.zero));
            Pizarra.Set(ClavesBB.TieneDestino, true);
        }
    }

    internal override void Exit()
    {
        //Ya no hay intecion
        Pizarra.Set(ClavesBB.TieneDestino, false);
    }
}
