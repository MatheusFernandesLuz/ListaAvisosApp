using ListaAvisosApp.Models;

public class AvisoIrmandade : IItemRelatorio
{
    public TipoEvento TipoEvento { get; } = TipoEvento.AVISOS_GERAIS;

    public string Descricao { get; }

    public AvisoIrmandade(string descricao)
    {
        Descricao = string.IsNullOrWhiteSpace(descricao)
            ? "-"
            : descricao.Trim().ToUpper();
    }
}