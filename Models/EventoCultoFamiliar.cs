using ListaAvisosApp.Models;

public class EventoCultoFamiliar : Evento
{
    public string Endereco { get; private set; }

    public EventoCultoFamiliar(
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