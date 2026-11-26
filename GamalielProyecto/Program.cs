namespace GamalielProyecto;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        MessageBox.Show(RefugioGamaliel.Datos.Conexion.Probar(out var err) ? "Conectado" : err);
        Application.Run(new RefugioGamaliel.Formularios.FrmLogin());
    }
}