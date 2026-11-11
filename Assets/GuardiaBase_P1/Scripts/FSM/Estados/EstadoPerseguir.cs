using UDIT.IA3.Guardia;
using UnityEngine;

public class EstadoPerseguir : State
{
    public EstadoPerseguir(StateMachine machine) : base(machine) { }

    internal override void Enter()
    {
        // Bark: se dice una vez al empezar a perseguir. Es presentacion, no mueve ni percibe
        GuardiaBarks bark = Agente.GetComponent<GuardiaBarks>();
        if (bark != null) bark.Decir("¡Alto ahí!");
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
        Pizarra.Set(ClavesBB.Destino, Pizarra.Get(ClavesBB.PosJugador, Vector3.zero));
        Pizarra.Set(ClavesBB.TieneDestino, true);
    }

    internal override void Exit()
    {
        // Apagar lo que empezo Enter
    }
}
