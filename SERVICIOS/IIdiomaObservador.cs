namespace SERVICIOS
{
    /// <summary>
    /// Observer del patron: lo implementa cualquier componente que deba
    /// re-aplicar sus textos cuando cambia el idioma activo (ver GestorIdioma,
    /// el Subject, en el proyecto Presentacion).
    /// </summary>
    public interface IIdiomaObservador
    {
        void ActualizarIdioma();
    }
}
