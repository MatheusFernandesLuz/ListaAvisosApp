using CsvHelper;
using CsvHelper.Configuration;
using ListaAvisosApp.Models;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;

namespace ListaAvisosApp.Services;

/// <summary>
/// Busca as respostas do Google Forms (publicadas como CSV) e converte
/// os eventos permitidos em uma lista de eventos ordenada.
/// </summary>
public class EventoService
{
    private readonly HttpClient _http = new()
    {
        Timeout = TimeSpan.FromSeconds(5)
    };


    public async Task<List<IItemRelatorio>> BuscarEventosAsync(string csvUrl)
    {
        if (string.IsNullOrWhiteSpace(csvUrl))
            throw new InvalidOperationException(
                "A URL da planilha não foi configurada em appsettings.json.");

        var csvTexto = await _http.GetStringAsync(csvUrl);

        using var reader = new StringReader(csvTexto);

        var config = new CsvConfiguration(
            CultureInfo.GetCultureInfo("pt-BR"))
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null,
            BadDataFound = null,
            Delimiter = ","
        };

        using var csv = new CsvReader(reader, config);

        csv.Read();
        csv.ReadHeader();

        var headers = csv.HeaderRecord ?? Array.Empty<string>();

        var colDataHora = ColunaQueContem(headers, "Data e hora");
        var colTipo = ColunaQueContem(headers, "aviso para");
        var colLocal = ColunaQueContem(headers, "Casa de oração:");
        var colAtendimento = ColunaQueContem(headers,"Irmão que irá atender");
        var colAssunto = ColunaQueContem(headers,"Nome ou assunto da reunião:");
        var colNomeLocal = ColunaQueContem(headers,"Informe o nome do local:");
        var colReferenciaLocal = ColunaQueContem(headers,"Descreva a referência, se necessário:");
        var colCasaIrmao = ColunaQueContem(headers,"Casa do irmão/irmã:");
        var colBairro = ColunaQueContem(headers,"Nome do bairro:");
        var colRua = ColunaQueContem(headers,"Nome da rua:");
        var colNumero = ColunaQueContem(headers,"Número da casa:");
        var colAviso = ColunaQueContem(headers, "Descrição do aviso:");

        var eventos = new List<IItemRelatorio>();

        while (await csv.ReadAsync())
        {
            var dataHoraTexto = ObterCampo(csv, colDataHora);

            if (!DateTime.TryParse(
                    dataHoraTexto,
                    CultureInfo.GetCultureInfo("pt-BR"),
                    DateTimeStyles.None,
                    out var dataHora))
            {
                dataHora = DateTime.Now;
            }

            var tipo = ObterCampo(csv, colTipo).Trim().ToEnum();

            var evento = CriarEvento(
                csv,
                dataHora,
                tipo,
                colLocal,
                colAtendimento,
                colAssunto,
                colNomeLocal,
                colReferenciaLocal,
                colCasaIrmao,
                colBairro,
                colRua,
                colNumero,
                colAviso);

            eventos.Add(evento);
        }

        return eventos
            .OrderBy(e => e.TipoEvento)
            .ToList();
    }

    private int ColunaQueContem(string[] headers, string palavraChave)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            if (headers[i].Contains(palavraChave, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    private static string ObterCampo(CsvReader csv, int indiceColuna, string valorPadrao = "")
    {
        if (indiceColuna < 0) return valorPadrao;
        return csv.GetField(indiceColuna)?.Trim() ?? valorPadrao;
    }

    private static IItemRelatorio CriarEvento(
    CsvReader csv,
    DateTime dataHora,
    TipoEvento tipo,
    int colLocal,
    int colAtendimento,
    int colAssunto,
    int colNomeLocal,
    int colReferenciaLocal,
    int colCasaIrmao,
    int colBairro,
    int colRua,
    int colNumero,
    int colAviso)
    {
        var atendimento = ObterCampo(csv, colAtendimento);

        return tipo switch
        {
            TipoEvento.REUNIOES_DIVERSAS =>
                new EventoReuniao(
                    dataHora,
                    tipo,
                    ObterCampo(csv, colLocal),
                    atendimento,
                    ObterCampo(csv, colAssunto)),

            TipoEvento.CULTOS_DIVERSOS =>
                new EventoComEndereco(
                    dataHora,
                    tipo,
                    ObterCampo(csv, colLocal),
                    ComporEnderecoCultoDiverso(
                        csv,
                        colNomeLocal,
                        colReferenciaLocal),
                    atendimento),

            TipoEvento.CULTO_FAMILIAR =>
                new EventoCultoFamiliar(
                    dataHora,
                    tipo,
                    ObterCampo(csv, colLocal),
                    ComporEnderecoCultoFamiliar(
                        csv,
                        colCasaIrmao,
                        colBairro,
                        colRua,
                        colNumero,
                        colReferenciaLocal),
                    atendimento),

            TipoEvento.AVISOS_GERAIS =>
                new AvisoIrmandade(ObterCampo(csv, colAviso)),

            _ =>
                new Evento(
                    dataHora,
                    tipo,
                    ObterCampo(csv, colLocal),
                    atendimento)
        };
    }

    private static string ComporEnderecoCultoDiverso(
    CsvReader csv,
    int colNomeLocal,
    int colReferencia)
    {
        var local = ObterCampo(csv, colNomeLocal);
        var referencia = ObterCampo(csv, colReferencia);

        return string.Join(
            " - ",
            new[] { local, referencia }
                .Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private static string ComporEnderecoCultoFamiliar(
    CsvReader csv,
    int colCasa,
    int colBairro,
    int colRua,
    int colNumero,
    int colReferencia)
    {
        var casa = LimparTexto(ObterCampo(csv, colCasa), palavras: ["irmã", "irmão"]);
        var bairro = LimparTexto(ObterCampo(csv, colBairro), caracteres: []);
        var rua = LimparTexto(ObterCampo(csv, colRua), palavras: ["rua", "avenida"]);
        var numero = LimparTexto(ObterCampo(csv, colNumero), palavras: ["nº"]);
        var referencia = LimparTexto(ObterCampo(csv, colReferencia), palavras: []);

        StringBuilder sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(casa))
            sb.Append($"IR {casa}");

        if (!string.IsNullOrWhiteSpace(rua))
            sb.Append($" - R. {rua}");

        if (!string.IsNullOrWhiteSpace(numero))
            sb.Append($", {numero}");

        if (!string.IsNullOrWhiteSpace(bairro))
            sb.Append($" - {bairro}");

        if(!string.IsNullOrWhiteSpace(referencia))
            sb.Append($" - {referencia}");

        return sb.ToString();
    }

    private static string LimparTexto(
    string texto,
    IEnumerable<string>? palavras = null,
    IEnumerable<char>? caracteres = null)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return string.Empty;

        if (palavras is not null)
        {
            foreach (var palavra in palavras)
            {
                texto = texto.Replace(
                    palavra,
                    string.Empty,
                    StringComparison.OrdinalIgnoreCase);
            }
        }

        if (caracteres is not null)
        {
            foreach (var caractere in caracteres)
            {
                texto = texto.Replace(
                    caractere.ToString(),
                    string.Empty);
            }
        }

        // Remove espaços duplicados
        texto = string.Join(
            ' ',
            texto.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries));

        return texto.Trim();
    }
}