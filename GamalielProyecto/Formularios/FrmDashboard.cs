using System;
using System.Drawing;
using System.Windows.Forms;
using RefugioGamaliel.Utilidades;

namespace RefugioGamaliel.Formularios;

/// <summary>
/// Panel principal con el resumen del refugio. Se incrusta sin bordes en
/// FrmPrincipal.pnlContenedor. Solo interfaz: los datos de ejemplo se
/// reemplazan con MySQL llamando a MostrarResumen(...) y llenando el grid.
/// </summary>
public class FrmDashboard : Form
{
    // ───────────── Controles públicos (para conectar MySQL) ─────────────
    public Label lblTituloPagina = null!;
    public TableLayoutPanel tblTarjetas = null!;
    public Label lblTotalPerros = null!;
    public Label lblConsumoDiario = null!;
    public Label lblStockTotal = null!;
    public Label lblDiasAutonomia = null!;
    public Panel pnlAlertaStock = null!;
    public Label lblMensajeAlerta = null!;
    public Label lblTituloHistorial = null!;
    public DataGridView dgvUltimosCalculos = null!;

    // Internos de la maquetación
    private TableLayoutPanel tblRaiz = null!;
    private Panel pnlFranjaDias = null!;
    private const float AltoFilaAlerta = 72f;

    public FrmDashboard()
    {
        InicializarComponentes();
        CargarDatosDeEjemplo(); // TODO: quitar esta línea al conectar MySQL y usar CargarDatos()
    }

    // ═════════════════════════ CONSTRUCCIÓN DE LA INTERFAZ ═════════════════════════
    private void InicializarComponentes()
    {
        SuspendLayout();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Tema.AplicarFormulario(this, "Dashboard", 920, 700);
        FormBorderStyle = FormBorderStyle.None;

        // ───── Contenedor raíz: 5 filas ─────
        tblRaiz = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            BackColor = Tema.Fondo,
            ColumnCount = 1,
            RowCount = 5
        };
        tblRaiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tblRaiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));              // título
        tblRaiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 128));             // tarjetas
        tblRaiz.RowStyles.Add(new RowStyle(SizeType.Absolute, AltoFilaAlerta));  // alerta
        tblRaiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));              // título del historial
        tblRaiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));              // tabla

        // ───── Fila 0: título ─────
        lblTituloPagina = Tema.CrearTitulo("Panel Principal - Resumen del Refugio");
        lblTituloPagina.AutoSize = false;
        lblTituloPagina.Dock = DockStyle.Fill;
        lblTituloPagina.Margin = Padding.Empty;
        lblTituloPagina.TextAlign = ContentAlignment.MiddleLeft;
        tblRaiz.Controls.Add(lblTituloPagina, 0, 0);

        // ───── Fila 1: 4 tarjetas iguales (con separadores de 16 px) ─────
        tblTarjetas = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 16),
            BackColor = Tema.Fondo,
            ColumnCount = 7,
            RowCount = 1
        };
        for (int i = 0; i < 7; i++)
        {
            tblTarjetas.ColumnStyles.Add(i % 2 == 0
                ? new ColumnStyle(SizeType.Percent, 25)
                : new ColumnStyle(SizeType.Absolute, 16));
        }
        tblTarjetas.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        tblTarjetas.Controls.Add(CrearTarjetaResumen("Perros Alojados", "45", Tema.Primario, out lblTotalPerros, out _), 0, 0);
        tblTarjetas.Controls.Add(CrearTarjetaResumen("Consumo Diario", "12.5 kg/día", Tema.Secundario, out lblConsumoDiario, out _), 2, 0);
        tblTarjetas.Controls.Add(CrearTarjetaResumen("Stock Disponible", "120 kg", Tema.Exito, out lblStockTotal, out _), 4, 0);
        tblTarjetas.Controls.Add(CrearTarjetaResumen("Días de Autonomía", "9.6 días", Tema.Alerta, out lblDiasAutonomia, out pnlFranjaDias), 6, 0);
        tblRaiz.Controls.Add(tblTarjetas, 0, 1);

        // ───── Fila 2: panel de alerta ─────
        pnlAlertaStock = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 16),
            BackColor = Tema.Alerta
        };
        lblMensajeAlerta = new Label
        {
            Text = "ALERTA: Reserva de alimento menor a 7 días. Gestionar donaciones.",
            Dock = DockStyle.Fill,
            Padding = new Padding(20, 0, 20, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            Font = Tema.Fuente(12f, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent
        };
        pnlAlertaStock.Controls.Add(lblMensajeAlerta);
        tblRaiz.Controls.Add(pnlAlertaStock, 0, 2);

        // ───── Fila 3: subtítulo del historial ─────
        lblTituloHistorial = Tema.CrearLabel("Historial de últimos cálculos", 12f, true);
        lblTituloHistorial.AutoSize = false;
        lblTituloHistorial.Dock = DockStyle.Fill;
        lblTituloHistorial.Margin = Padding.Empty;
        lblTituloHistorial.TextAlign = ContentAlignment.MiddleLeft;
        tblRaiz.Controls.Add(lblTituloHistorial, 0, 3);

        // ───── Fila 4: tabla de cálculos ─────
        dgvUltimosCalculos = new DataGridView
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty
        };
        Tema.EstilizarGrid(dgvUltimosCalculos);

        dgvUltimosCalculos.Columns.Add("colFecha", "Fecha");
        dgvUltimosCalculos.Columns.Add("colPequenos", "Perros pequeños");
        dgvUltimosCalculos.Columns.Add("colMedianos", "Perros medianos");
        dgvUltimosCalculos.Columns.Add("colGrandes", "Perros grandes");
        dgvUltimosCalculos.Columns.Add("colStock", "Alimento (kg)");
        dgvUltimosCalculos.Columns.Add("colConsumo", "Consumo diario (kg)");
        dgvUltimosCalculos.Columns.Add("colDias", "Días de duración");
        dgvUltimosCalculos.Columns.Add("colAlerta", "Estado");
        dgvUltimosCalculos.CellFormatting += dgvUltimosCalculos_CellFormatting;
        tblRaiz.Controls.Add(dgvUltimosCalculos, 0, 4);

        Controls.Add(tblRaiz);

        ResumeLayout(false);
    }

    /// <summary>Crea una tarjeta blanca con franja superior de 5 px, título y valor grande.</summary>
    private static Panel CrearTarjetaResumen(string titulo, string valor, Color colorFranja,
        out Label lblValor, out Panel pnlFranja)
    {
        var tarjeta = Tema.CrearTarjeta(200, 112);
        tarjeta.Dock = DockStyle.Fill;
        tarjeta.Margin = Padding.Empty;
        tarjeta.Padding = Padding.Empty;

        pnlFranja = new Panel
        {
            Dock = DockStyle.Top,
            Height = 5,
            BackColor = colorFranja
        };

        var lblTitulo = Tema.CrearLabel(titulo, 10.5f);
        lblTitulo.Location = new Point(16, 24);

        lblValor = Tema.CrearLabel(valor, 20f, true);
        lblValor.Location = new Point(16, 52);

        tarjeta.Controls.Add(lblTitulo);
        tarjeta.Controls.Add(lblValor);
        tarjeta.Controls.Add(pnlFranja);
        return tarjeta;
    }

    // ═════════════════════════ MÉTODOS PARA EL EQUIPO ═════════════════════════

    /// <summary>Carga los datos reales del dashboard desde MySQL.</summary>
    public void CargarDatos()
    {
        // TODO: conexión MySQL
        // 1. Contar perros, calcular consumo diario, sumar stock y calcular días de autonomía.
        // 2. Llamar a MostrarResumen(totalPerros, consumoDiario, stockTotal, diasAutonomia);
        // 3. Llenar dgvUltimosCalculos con los últimos registros de la tabla Reporte.
    }

    /// <summary>
    /// Actualiza las 4 tarjetas y muestra u oculta la alerta (menos de 7 días de reserva).
    /// </summary>
    public void MostrarResumen(int totalPerros, decimal consumoDiarioKg, decimal stockKg, decimal diasAutonomia)
    {
        lblTotalPerros.Text = totalPerros.ToString();
        lblConsumoDiario.Text = $"{consumoDiarioKg:0.0} kg/día";
        lblStockTotal.Text = $"{stockKg:0.##} kg";
        lblDiasAutonomia.Text = $"{diasAutonomia:0.0} días";

        bool stockBajo = diasAutonomia < 7m;
        pnlFranjaDias.BackColor = stockBajo ? Tema.Alerta : Tema.Exito;
        pnlAlertaStock.Visible = stockBajo;
        tblRaiz.RowStyles[2].Height = stockBajo ? AltoFilaAlerta * DeviceDpi / 96f : 0;
    }

    // ═════════════════════════ EVENTOS ═════════════════════════

    /// <summary>Pinta la columna Estado: rojo si es "Stock bajo", verde si es "Normal".</summary>
    private void dgvUltimosCalculos_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.Value is null)
            return;

        if (dgvUltimosCalculos.Columns[e.ColumnIndex].Name != "colAlerta")
            return;

        bool stockBajo = e.Value.ToString() == "Stock bajo";
        e.CellStyle.ForeColor = stockBajo ? Tema.Alerta : Tema.Exito;
        e.CellStyle.Font = Tema.Fuente(10f, FontStyle.Bold);
    }

    // ═════════════════════════ DATOS DE EJEMPLO (BORRAR AL CONECTAR MYSQL) ═════════════════════════
    private void CargarDatosDeEjemplo()
    {
        dgvUltimosCalculos.Rows.Clear();
        dgvUltimosCalculos.Rows.Add("25/11/2026", 20, 15, 10, "120.00", "12.50", "9.6", "Normal");
        dgvUltimosCalculos.Rows.Add("24/11/2026", 20, 15, 10, "132.50", "12.50", "10.6", "Normal");
        dgvUltimosCalculos.Rows.Add("20/11/2026", 20, 15, 10, "80.00", "12.50", "6.4", "Stock bajo");
    }
}