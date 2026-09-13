namespace ListaAvisosApp.Models;

public static class TipoEventoExtension
{
    private static Dictionary<string, TipoEvento> Dicionario = new()
    {
        { "BATISMO", TipoEvento.BATISMO },
        { "SANTA CEIA", TipoEvento.SANTA_CEIA },
        { "REUNIÃO DE MOCIDADE", TipoEvento.REUNIAO_MOCIDADE },
        { "ENSAIO REGIONAL", TipoEvento.ENSAIO_REGIONAL },
        { "CULTOS DIVERSOS", TipoEvento.CULTOS_DIVERSOS },
        { "CULTO FAMILIAR", TipoEvento.CULTO_FAMILIAR },
        { "CULTO COM A MOCIDADE", TipoEvento.CULTO_COM_MOCIDADE },
        { "REUNIÕES DIVERSAS", TipoEvento.REUNIOES_DIVERSAS },
        { "AVISOS GERAIS", TipoEvento.AVISOS_GERAIS },
        { string.Empty, TipoEvento.INDEFINIDO },
    };

    public static TipoEvento ToEnum(this string tipoEvento)
    {
        return Dicionario[tipoEvento];
    }

    public static string ToText(this TipoEvento tipoEvento)
    {
        if (tipoEvento == TipoEvento.AVISOS_GERAIS)
            return "AVISOS PARA IRMANDADE";

        return Dicionario.FirstOrDefault(d => d.Value == tipoEvento).Key;
    }
}
