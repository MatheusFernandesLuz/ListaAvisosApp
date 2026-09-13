namespace ListaAvisosApp.Models;

public class EventoReuniao : Evento
{
    public string Assunto { get; private set; }

    public EventoReuniao(DateTime dataHora, TipoEvento tipoEvento, string local, string atendimento, string assunto)
        : base(dataHora, tipoEvento, local, atendimento)
    {
        Assunto = string.IsNullOrWhiteSpace(assunto) ? "-" : assunto.ToUpper();
    }
}
