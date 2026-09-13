namespace ListaAvisosApp.Models;

/// <summary>
/// Representa um item da "lista de avisos diversos": um culto, batismo,
/// reunião ministerial, ensaio musical, etc.
/// </summary>
public class Evento : IItemRelatorio
{
    public TipoEvento TipoEvento { get; private set; }
    public DateOnly Data { get; private set; }
    public string DataFormatada { get; private set; }
    public string Hora { get; private set; }
    public string Dia { get; private set; }
    public string Local { get; private set; }
    public string Atendimento { get; private set; }

    public Evento(DateTime dataHora, TipoEvento tipoEvento, string local, string atendimento)
    {
        Data = DateOnly.FromDateTime(dataHora);
        DataFormatada = dataHora.ToString("dd-MM");
        TipoEvento = tipoEvento;
        Hora = dataHora.ToString("HH:mm");
        Dia = dataHora.ToString("ddd").ToUpper();
        Local = string.IsNullOrWhiteSpace(local) ? "N/A"  : local.ToUpper();
        Atendimento = string.IsNullOrWhiteSpace(atendimento) ? "-" : atendimento.ToUpper();
    }
}