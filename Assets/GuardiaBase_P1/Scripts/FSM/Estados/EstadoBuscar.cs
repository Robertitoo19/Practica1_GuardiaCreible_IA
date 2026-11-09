using UDIT.IA3.Guardia;
using UnityEngine;

public class EstadoBuscar : State
{
    private const float TiempoBusqueda = 8f;   // ajustable
    private float _inicio;

    public EstadoBuscar(StateMachine machine) : base(machine) { }
    internal override void Enter()
    {
        _inicio = Pizarra.Tiempo;

        if (Pizarra.Get(ClavesBB.TieneUltimaPos, false))
        {
            Pizarra.Set(ClavesBB.Destino, Pizarra.Get(ClavesBB.UltimaPos, Vector3.zero));
            Pizarra.Set(ClavesBB.TieneDestino, true);
        }
        else
        {
            Pizarra.Set(ClavesBB.TieneDestino, false);
        }
    }

    internal override void Execute()
    {
        // Te ven o la alerta sigue alta, a perseguir
        if (Pizarra.Get(ClavesBB.Visible, false) ||
            Pizarra.Get(ClavesBB.Nivel, NivelSospecha.Tranquilo) == NivelSospecha.Alerta)
        {
            //(DESCOMENTAR CUANDO HAYA PERSEGUIR)
            //StateMachine.ChangeState(new EstadoPerseguir(StateMachine));
            return;
        }

        // Se acaba el tiempo de búsqueda, vuelta a la ruta.
        if (Pizarra.Tiempo - _inicio >= TiempoBusqueda)
        {
            StateMachine.ChangeState(new EstadoPatrullar(StateMachine));
        }

        // Mejora (extra, 7 "consulta de posición"): al llegar a ultimaPos, elegir puntos candidatos alrededor en lugar de quedarse quieto.
    }

    internal override void Exit()
    {
        Pizarra.Set(ClavesBB.TieneUltimaPos, false);
        Pizarra.Set(ClavesBB.TieneDestino, false);
    }
}