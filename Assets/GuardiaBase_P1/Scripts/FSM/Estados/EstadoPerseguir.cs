using UDIT.IA3.Guardia;
using UnityEngine;

public class EstadoPerseguir : State
{
    public EstadoPerseguir(StateMachine machine) : base(machine) { }

    internal override void Enter()
    {
        // qui va un bark (Alto ahí) y cambio de velocidad
    }

    internal override void Execute()
    {
        // Si deja de verlo, va a la ultima posicion conocida (lee en pizarra)
        if (!Pizarra.Get(ClavesBB.Visible, false))
        {
            StateMachine.ChangeState(new EstadoBuscar(StateMachine));
            return;
        }

        // Lo vemos, el destino es donde esta ahora (escribe el sensor)
        Pizarra.Set(ClavesBB.Destino, Pizarra.Get(ClavesBB.PosJugador, UnityEngine.Vector3.zero));
        Pizarra.Set(ClavesBB.TieneDestino, true);
    }

    internal override void Exit()
    {
        // Apagar lo que empezo Enter
    }
}
