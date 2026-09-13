using ListaAvisosApp.Models;

public class EventoComEndereco : Evento
{
    public string Endereco { get; private set; }

    public EventoComEndereco(
        DateTime dataHora,
        TipoEvento tipoEvento,
        string local,
        string endereco,
        string atendimento)
        : base(dataHora, tipoEvento, local, atendimento)
    {
        Endereco = string.IsNullOrWhiteSpace(endereco)
            ? "-"
            : endereco.ToUpper();
    }
}