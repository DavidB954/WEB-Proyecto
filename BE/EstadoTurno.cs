namespace BE
{
    // Los valores coinciden con los IdEstadoTurno que carga Scripts/TurnosPacientes.sql.
    public enum EstadoTurno
    {
        Reservado = 1,
        Cancelado = 2,
        Atendido = 3,
        Ausente = 4
    }
}
