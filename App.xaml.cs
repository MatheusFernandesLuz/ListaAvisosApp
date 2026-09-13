using System.Windows;
using QuestPDF.Infrastructure;

namespace ListaAvisosApp;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // QuestPDF é gratuito para uso comunitário / organizações pequenas
        // (licença Community). Confira as condições em questpdf.com/license
        // caso o uso da igreja mude de porte no futuro.
        QuestPDF.Settings.License = LicenseType.Community;
    }
}
