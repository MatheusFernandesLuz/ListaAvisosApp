using ListaAvisosApp.Reports;
using ListaAvisosApp.Services;
using QuestPDF.Fluent;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace ListaAvisosApp;

public partial class MainWindow : Window
{
    private readonly EventoService _service = new();
    private readonly ObservableCollection<IItemRelatorio> _itens = new();
    private AppConfig _config = new();

    public MainWindow()
    {
        InitializeComponent();
        GridEventos.ItemsSource = _itens;
        CarregarConfig();        
    }

    private void CarregarConfig()
    {
        try
        {
            var caminho = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            var json = File.ReadAllText(caminho);
            _config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();

            TxtAviso.Text = "Obs: Os dados podem demorar até alguns minutos para sincronizar";
        }
        catch (Exception ex)
        {
            TxtErro.Text = $"Não foi possível ler appsettings.json: {ex.Message}";
        }
    }

    private async void BtnAtualizar_Click(object sender, RoutedEventArgs e)
    {
        TxtErro.Text = "";
        TxtStatus.Text = "Atualizando...";
        BtnAtualizar.IsEnabled = false;
        BtnGerar.IsEnabled = false;

        try
        {
            var lista = await _service.BuscarEventosAsync(_config.PlanilhaCsvUrl);
            _itens.Clear();

            foreach (var ev in lista)
                _itens.Add(ev);

            TxtStatus.Text = $"{_itens.Count} registro(s) carregado(s).";
            BtnGerar.IsEnabled = _itens.Count > 0;
        }
        catch (Exception ex)
        {
            TxtErro.Text = "Não foi possível buscar os dados. Verifique sua conexão com a internet " +
                           $"e a URL configurada em appsettings.json. Detalhe técnico: {ex.Message}";
            TxtStatus.Text = "";
        }
        finally
        {
            BtnAtualizar.IsEnabled = true;
        }
    }

    private void BtnGerar_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var doc = new ListaAvisosReport(_itens.ToList());
            var caminhoPdf = Path.Combine(Path.GetTempPath(), $"ListaDeAvisos_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
            doc.GeneratePdf(caminhoPdf);

            // Abre o PDF no visualizador padrão do Windows, de onde o
            // secretário já pode imprimir com Ctrl+P.
            var psi = new System.Diagnostics.ProcessStartInfo(caminhoPdf) { UseShellExecute = true };
            System.Diagnostics.Process.Start(psi);
        }
        catch (Exception ex)
        {
            TxtErro.Text = $"Erro ao gerar relatório: {ex.Message}";
        }
    }
}

public class AppConfig
{
    public string PlanilhaCsvUrl { get; set; } = "";
}
