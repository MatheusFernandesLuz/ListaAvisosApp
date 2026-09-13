using ListaAvisosApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ListaAvisosApp.Reports;

/// <summary>
/// Layout da "Lista de Avisos Diversos" em PDF. O QuestPDF cuida da quebra
/// de página automaticamente — não é preciso ajustar nada manualmente
/// como acontecia no Excel.
/// </summary>
public class ListaAvisosReport : IDocument
{
    private readonly List<IItemRelatorio> _eventos;
    private const string CCB = "CONGREGAÇÃO CRISTÃ NO BRASIL";
    private const string TITULO = "JI-PARANÁ/RO - LISTA DE AVISOS DIVERSOS";

    public ListaAvisosReport(List<IItemRelatorio> eventos)
    {
        _eventos = eventos;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A5);
            page.Margin(1, Unit.Millimetre);
            page.DefaultTextStyle(x => x.FontSize(7).FontFamily("Calisto MT"));

            page.Header().ShowOnce().Column(col =>
            {
                col.Item()
                    .Text($"{CCB} \n {TITULO} - {GetMes()}")
                    .FontSize(10)
                    .Italic()
                    .SemiBold()
                    .AlignCenter();
            });

            page.Content().PaddingTop(5).Column(col =>
            {
                var grupos = _eventos
                    .GroupBy(e => e.TipoEvento)
                    .OrderBy(g => (int)g.Key);

                foreach (var grupo in grupos)
                {
                    col.Item()
                        .PaddingBottom(1)
                        .PaddingTop(5)
                        .Text($"{grupo.Key.ToText()}")
                        .FontSize(9)
                        .Bold()
                        .Italic()
                        .Underline()
                        .AlignCenter();

                    switch (grupo.First())
                    {
                        case EventoReuniao:
                            col.Item().Table(table =>
                                MontaTabelaEventoReuniao(
                                    table,
                                    grupo.OfType<EventoReuniao>().ToList()));
                            break;

                        case EventoCultoFamiliar:
                            col.Item().Table(table =>
                                MontaTabelaCultoFamiliar(
                                    table,
                                    grupo.OfType<EventoCultoFamiliar>().ToList()));
                            break;

                        case EventoComEndereco:
                            col.Item().Table(table =>
                                MontaTabelaEventoComEndereco(
                                    table,
                                    grupo.OfType<EventoComEndereco>().ToList()));
                            break;

                        case AvisoIrmandade:
                            col.Item().Table(table =>
                                MontaTabelaAvisosIrmandade(
                                    table,
                                    grupo.OfType<AvisoIrmandade>().ToList()));
                            break;

                        default:
                            col.Item().Table(table =>
                                MontaTabelaPadrao(
                                    table,
                                    grupo.OfType<Evento>().ToList()));
                            break;
                    }
                }
            });
        });
    }

    private static void MontaTabelaEventoComEndereco(TableDescriptor table, List<EventoComEndereco> eventos)
    {
        table.ColumnsDefinition(columns =>
        {
            columns.ConstantColumn(30);  // Data
            columns.ConstantColumn(30);  // Hora
            columns.ConstantColumn(35);  // Dia
            columns.RelativeColumn(400);  // Endereço
            columns.RelativeColumn(150);  // Atendimento
        });

        table.Header(header =>
        {
            header.Cell().Element(CellHeader).Text("DATA");
            header.Cell().Element(CellHeader).Text("HORA");
            header.Cell().Element(CellHeader).Text("DIA");
            header.Cell().Element(CellHeader).Text("ENDEREÇO");
            header.Cell().Element(CellHeader).Text("ATENDIMENTO");
        });

        foreach (var evento in eventos.OrderBy(e => e.Data).ThenBy(e => e.Hora))
        {
            table.Cell().Element(CellBody).Text(evento.DataFormatada);
            table.Cell().Element(CellBody).Text(evento.Hora);
            table.Cell().Element(CellBody).Text(evento.Dia);
            table.Cell().Element(CellBody).Text(evento.Endereco);
            table.Cell().Element(CellBody).Text(evento.Atendimento);
        }
    }

    private static void MontaTabelaCultoFamiliar(TableDescriptor table, List<EventoCultoFamiliar> eventos)
    {
        table.ColumnsDefinition(columns =>
        {
            columns.RelativeColumn(40);  // Data
            columns.RelativeColumn(40);  // Hora
            columns.RelativeColumn(45);  // Dia
            columns.RelativeColumn(400);  // Endereço
            columns.RelativeColumn(100);  // Atendimento
        });

        // HEADER — aparece somente uma vez
        // Cabeçalho — aparece apenas uma vez
        table.Cell().Element(CellHeader).Text("DATA");
        table.Cell().Element(CellHeader).Text("HORA");
        table.Cell().Element(CellHeader).Text("DIA");
        table.Cell().Element(CellHeader).Text("ENDEREÇO");
        table.Cell().Element(CellHeader).Text("ATENDIMENTO");

        var gruposPorIgreja = eventos
            .GroupBy(e => e.Local)
            .OrderBy(g => g.Key);

        foreach (var grupo in gruposPorIgreja)
        {
            // Linha da igreja
            table.Cell()
                .ColumnSpan(5)
                .Element(CellIgreja)
                .Text(grupo.Key);

            // Eventos daquela igreja
            foreach (var evento in grupo
                .OrderBy(e => e.Data)
                .ThenBy(e => e.Hora))
            {
                table.Cell().Element(CellBody).Text(evento.DataFormatada);
                table.Cell().Element(CellBody).Text(evento.Hora);
                table.Cell().Element(CellBody).Text(evento.Dia);
                table.Cell().Element(CellBody).Text(evento.Endereco);
                table.Cell().Element(CellBody).Text(evento.Atendimento);
            }
        }
    }

    private static string GetMes()
    {
        return DateTime.Now.ToString("MMMM/yyyy").ToUpper();
    }

    private static void MontaTabelaPadrao(TableDescriptor table, List<Evento> eventos)
    {
        table.ColumnsDefinition(columns =>
        {
            columns.ConstantColumn(30);  // Data
            columns.ConstantColumn(30);  // Hora
            columns.RelativeColumn(35);  // Dia
            columns.RelativeColumn(150);  // Local
            columns.RelativeColumn(150);  // Atendimento
        });

        table.Header(header =>
        {
            header.Cell().Element(CellHeader).Text("DATA");
            header.Cell().Element(CellHeader).Text("HORA");
            header.Cell().Element(CellHeader).Text("DIA");
            header.Cell().Element(CellHeader).Text("LOCAL");
            header.Cell().Element(CellHeader).Text("ATENDIMENTO");
        });

        foreach (var evento in eventos.OrderBy(e => e.Data).ThenBy(e => e.Hora))
        {
            table.Cell().Element(CellBody).Text(evento.DataFormatada);
            table.Cell().Element(CellBody).Text(evento.Hora);
            table.Cell().Element(CellBody).Text(evento.Dia);
            table.Cell().Element(CellBody).Text(evento.Local);
            table.Cell().Element(CellBody).Text(evento.Atendimento);
        }
    }

    private static void MontaTabelaEventoReuniao(TableDescriptor table, List<EventoReuniao> eventos)
    {
        table.ColumnsDefinition(columns =>
        {
            columns.RelativeColumn(40);  // Data
            columns.RelativeColumn(40);  // Hora
            columns.RelativeColumn(45);  // Dia
            columns.RelativeColumn(300);  // Assunto
            columns.RelativeColumn(120);  // Local
        });

        table.Header(header =>
        {
            header.Cell().Element(CellHeader).Text("DATA");
            header.Cell().Element(CellHeader).Text("HORA");
            header.Cell().Element(CellHeader).Text("DIA");
            header.Cell().Element(CellHeader).Text("ASSUNTO");
            header.Cell().Element(CellHeader).Text("LOCAL");
        });

        foreach (var evento in eventos.OrderBy(e => e.Data).ThenBy(e => e.Hora))
        {
            table.Cell().Element(CellBody).Text(evento.DataFormatada);
            table.Cell().Element(CellBody).Text(evento.Hora);
            table.Cell().Element(CellBody).Text(evento.Dia);
            table.Cell().Element(CellBody).Text(evento.Assunto);
            table.Cell().Element(CellBody).Text(evento.Local);
        }
    }

    private static void MontaTabelaAvisosIrmandade(
    TableDescriptor table,
    List<AvisoIrmandade> avisos)
    {
        table.ColumnsDefinition(columns =>
        {
            columns.RelativeColumn();
        });

        table.Header(header =>
        {
            header.Cell()
                .Element(CellHeader)
                .Text("DESCRIÇÃO");
        });

        foreach (var aviso in avisos)
        {
            table.Cell()
                .Element(CellBody)
                .Text(aviso.Descricao);
        }
    }

    private static IContainer CellHeader(IContainer container)
    {
        return container
            .Background(Colors.Grey.Lighten3)
            .Border(2, Unit.Mil)
            .BorderColor(Colors.Grey.Darken1)
            .Padding(2)
            .DefaultTextStyle(x => x
                .FontSize(7)
                .Bold());
    }

    private static IContainer CellBody(IContainer container)
    {
        return container
            .ShowEntire()
            .Border(2, Unit.Mil)
            .BorderColor(Colors.Grey.Darken1)
            .Padding(2)
            .DefaultTextStyle(x => x.FontSize(9));

    }

    private static IContainer CellIgreja(IContainer container) =>
    container
        .Background(Colors.Grey.Lighten3)
        .Border(2, Unit.Mil)
        .Padding(2)
        .DefaultTextStyle(x => x
            .FontSize(7)
            .Bold());
}
