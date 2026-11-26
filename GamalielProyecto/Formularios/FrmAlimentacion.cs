using System;
using System.Drawing;
using System.Windows.Forms;
using RefugioGamaliel.Utilidades;

namespace RefugioGamaliel.Formularios;

/// <summary>
/// Configuración de raciones por tamaño, resumen de consumo diario
/// e historial de descuentos automáticos. Solo interfaz gráfica, sin consultas SQL.
/// </summary>
public class FrmAlimentacion : Form
{
    // ───────────── Controles públicos (para conectar MySQL) ─────────────
    public NumericUpDown nudRacionPequeno = null!;
    public NumericUpDown nudRacionMediano = null!;
    public NumericUpDown nudRacionGrande = null!;
    public Button btnCalcular = null!;

    // Etiquetas del panel resumen (se actualizan desde BD o al recalcular)
    public Label lblDesglosePequenos = null!;
    public Label lblDesgloseMedianos = null!;
    public Label lblDesgloseGrandes = null!;
    public Label lblConsumoTotalDiario = null!;
    public Label lblAutonomiaEstimada = null!;

    public DataGridView dgvConsumoDiario = null!;

    // Conteos de referencia visual (luego los compañeros los reemplazan con COUNT de MySQL)
    private readonly int _cantPequenosEjemplo = 20;
    private readonly int _cantMedianosEjemplo = 15;
    private readonly int _cantGrandesEjemplo = 10;
    private readonly decimal _stockActualEjemplo = 120.0m;

    public FrmAlimentacion()
    {
        InicializarComponentes();
        RecalcularVistaPrevia();
        CargarDatosEjemplo();
    }

    private void InicializarComponentes()
    {
        SuspendLayout();

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Tema.Fondo;
        Font = Tema.Fuente(10f);

        // ───── Título superior ─────
        var lblTitulo = new Label
        {
            Text = "Cálculo de Raciones y Consumo Diario",
            Font = Tema.Fuente(16f, FontStyle.Bold),
            ForeColor = Tema.Primario,
            Location = new Point(25, 20),
            AutoSize = true
        };
        Controls.Add(lblTitulo);

        // ══════════════════════════════════════════════════════════════
        // SUPERIOR IZQUIERDA: PANEL 'PARÁMETROS DE RACIÓN'
        // ══════════════════════════════════════════════════════════════
        var pnlParametros = new Panel
        {
            BackColor = Color.White,
            Location = new Point(25, 65),
            Size = new Size(390, 240),
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };

        var pnlFranjaIzq = new Panel
        {
            BackColor = Tema.Primario,
            Dock = DockStyle.Top,
            Height = 5
        };
        pnlParametros.Controls.Add(pnlFranjaIzq);

        var lblSubParametros = new Label
        {
            Text = "Parámetros de Ración (g / día)",
            Font = Tema.Fuente(12f, FontStyle.Bold),
            ForeColor = Tema.TextoPrincipal,
            Location = new Point(20, 18),
            AutoSize = true
        };

        // Ración Pequeño
        var lblPeq = new Label
        {
            Text = "Perro Pequeño (g):",
            ForeColor = Tema.TextoPrincipal,
            Location = new Point(20, 62),
            AutoSize = true
        };
        nudRacionPequeno = CrearNumericRacion(150, new Point(235, 58));

        // Ración Mediano
        var lblMed = new Label
        {
            Text = "Perro Mediano (g):",
            ForeColor = Tema.TextoPrincipal,
            Location = new Point(20, 102),
            AutoSize = true
        };
        nudRacionMediano = CrearNumericRacion(300, new Point(235, 98));

        // Ración Grande
        var lblGra = new Label
        {
            Text = "Perro Grande (g):",
            ForeColor = Tema.TextoPrincipal,
            Location = new Point(20, 142),
            AutoSize = true
        };
        nudRacionGrande = CrearNumericRacion(500, new Point(235, 138));

        // Botón Recalcular Consumo (#E67E22)
        btnCalcular = new Button
        {
            Text = "Recalcular Consumo",
            BackColor = Tema.Secundario,
            ForeColor = Color.White,
            Location = new Point(20, 184),
            Size = new Size(345, 40),
            FlatStyle = FlatStyle.Flat,
            Font = Tema.Fuente(10f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnCalcular.FlatAppearance.BorderSize = 0;
        btnCalcular.Click += (s, e) => RecalcularVistaPrevia();

        pnlParametros.Controls.AddRange(new Control[]
        {
            lblSubParametros,
            lblPeq, nudRacionPequeno,
            lblMed, nudRacionMediano,
            lblGra, nudRacionGrande,
            btnCalcular
        });

        // ══════════════════════════════════════════════════════════════
        // SUPERIOR DERECHA: PANEL RESUMEN DE CONSUMO POR TAMAÑO
        // ══════════════════════════════════════════════════════════════
        var pnlResumen = new Panel
        {
            BackColor = Color.White,
            Location = new Point(435, 65),
            Size = new Size(440, 240),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        var pnlFranjaDer = new Panel
        {
            BackColor = Tema.Secundario,
            Dock = DockStyle.Top,
            Height = 5
        };
        pnlResumen.Controls.Add(pnlFranjaDer);

        var lblSubResumen = new Label
        {
            Text = "Desglose de Consumo Diario Estimado",
            Font = Tema.Fuente(12f, FontStyle.Bold),
            ForeColor = Tema.TextoPrincipal,
            Location = new Point(20, 18),
            AutoSize = true
        };

        lblDesglosePequenos = new Label
        {
            Text = "• Pequeños (20 × 0.15 kg): 3.00 kg/día",
            ForeColor = Tema.TextoPrincipal,
            Font = Tema.Fuente(10f),
            Location = new Point(20, 58),
            AutoSize = true
        };

        lblDesgloseMedianos = new Label
        {
            Text = "• Medianos (15 × 0.30 kg): 4.50 kg/día",
            ForeColor = Tema.TextoPrincipal,
            Font = Tema.Fuente(10f),
            Location = new Point(20, 88),
            AutoSize = true
        };

        lblDesgloseGrandes = new Label
        {
            Text = "• Grandes (10 × 0.50 kg): 5.00 kg/día",
            ForeColor = Tema.TextoPrincipal,
            Font = Tema.Fuente(10f),
            Location = new Point(20, 118),
            AutoSize = true
        };

        var pnlSeparador = new Panel
        {
            BackColor = Color.FromArgb(224, 230, 237),
            Location = new Point(20, 152),
            Size = new Size(395, 1),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        lblConsumoTotalDiario = new Label
        {
            Text = "Consumo Total Diario: 12.50 kg/día",
            Font = Tema.Fuente(12.5f, FontStyle.Bold),
            ForeColor = Tema.Primario,
            Location = new Point(20, 165),
            AutoSize = true
        };

        lblAutonomiaEstimada = new Label
        {
            Text = "Autonomía con stock actual (120 kg): 9.6 días",
            Font = Tema.Fuente(9.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(39, 174, 96), // Verde esmeralda
            Location = new Point(20, 198),
            AutoSize = true
        };

        pnlResumen.Controls.AddRange(new Control[]
        {
            lblSubResumen,
            lblDesglosePequenos,
            lblDesgloseMedianos,
            lblDesgloseGrandes,
            pnlSeparador,
            lblConsumoTotalDiario,
            lblAutonomiaEstimada
        });

        // ══════════════════════════════════════════════════════════════
        // PARTE INFERIOR: HISTORIAL DE DESCUENTOS DIARIOS
        // ══════════════════════════════════════════════════════════════
        var lblTablaTitulo = new Label
        {
            Text = "Registro de Descuentos Diarios Automáticos",
            Font = Tema.Fuente(12f, FontStyle.Bold),
            ForeColor = Tema.TextoPrincipal,
            Location = new Point(25, 322),
            AutoSize = true
        };
        Controls.Add(lblTablaTitulo);

        var pnlTabla = new Panel
        {
            BackColor = Color.White,
            Location = new Point(25, 352),
            Size = new Size(850, 255),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        dgvConsumoDiario = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            EnableHeadersVisualStyles = false,
            RowTemplate = { Height = 34 }
        };

        dgvConsumoDiario.ColumnHeadersDefaultCellStyle.BackColor = Tema.Primario;
        dgvConsumoDiario.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvConsumoDiario.ColumnHeadersDefaultCellStyle.Font = Tema.Fuente(10f, FontStyle.Bold);
        dgvConsumoDiario.ColumnHeadersHeight = 40;

        dgvConsumoDiario.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        dgvConsumoDiario.DefaultCellStyle.SelectionBackColor = Color.FromArgb(214, 234, 248);
        dgvConsumoDiario.DefaultCellStyle.SelectionForeColor = Tema.TextoPrincipal;

        // Columnas solicitadas
        dgvConsumoDiario.Columns.Add("fecha", "Fecha");
        dgvConsumoDiario.Columns.Add("pequenos", "Perros Pequeños");
        dgvConsumoDiario.Columns.Add("medianos", "Medianos");
        dgvConsumoDiario.Columns.Add("grandes", "Grandes");
        dgvConsumoDiario.Columns.Add("total_consumido", "Total Consumido (kg)");
        dgvConsumoDiario.Columns.Add("saldo_restante", "Saldo Restante (kg)");

        pnlTabla.Controls.Add(dgvConsumoDiario);

        Controls.Add(pnlParametros);
        Controls.Add(pnlResumen);
        Controls.Add(pnlTabla);

        ResumeLayout(false);
    }

    private NumericUpDown CrearNumericRacion(decimal valorInicial, Point ubicacion)
    {
        return new NumericUpDown
        {
            Location = ubicacion,
            Size = new Size(130, 28),
            Minimum = 50,
            Maximum = 2000,
            Increment = 25,
            Value = valorInicial,
            Font = Tema.Fuente(10f),
            TextAlign = HorizontalAlignment.Right
        };
    }

    /// <summary>
    /// Simulación visual de la fórmula CT = (P * rP) + (M * rM) + (G * rG)
    /// para comprobar el funcionamiento de la interfaz antes de conectar MySQL.
    /// </summary>
    public void RecalcularVistaPrevia()
    {
        decimal kgPeq = nudRacionPequeno.Value / 1000m;
        decimal kgMed = nudRacionMediano.Value / 1000m;
        decimal kgGra = nudRacionGrande.Value / 1000m;

        decimal subPeq = _cantPequenosEjemplo * kgPeq;
        decimal subMed = _cantMedianosEjemplo * kgMed;
        decimal subGra = _cantGrandesEjemplo * kgGra;
        decimal totalDiario = subPeq + subMed + subGra;

        lblDesglosePequenos.Text = $"• Pequeños ({_cantPequenosEjemplo} × {kgPeq:0.00} kg): {subPeq:0.00} kg/día";
        lblDesgloseMedianos.Text = $"• Medianos ({_cantMedianosEjemplo} × {kgMed:0.00} kg): {subMed:0.00} kg/día";
        lblDesgloseGrandes.Text = $"• Grandes ({_cantGrandesEjemplo} × {kgGra:0.00} kg): {subGra:0.00} kg/día";
        lblConsumoTotalDiario.Text = $"Consumo Total Diario: {totalDiario:0.00} kg/día";

        if (totalDiario > 0)
        {
            decimal dias = _stockActualEjemplo / totalDiario;
            lblAutonomiaEstimada.Text = $"Autonomía con stock de referencia ({_stockActualEjemplo:0.0} kg): {dias:0.1} días";
            lblAutonomiaEstimada.ForeColor = dias < 7m ? Tema.Alerta : Color.FromArgb(39, 174, 96);
        }
    }

    private void CargarDatosEjemplo()
    {
        // Ejemplo tomado del modelo matemático del informe (120 kg iniciales, 12.5 kg/día)
        dgvConsumoDiario.Rows.Add("21/11/2026", "20", "15", "10", "12.50 kg", "107.50 kg");
        dgvConsumoDiario.Rows.Add("22/11/2026", "20", "15", "10", "12.50 kg", "95.00 kg");
        dgvConsumoDiario.Rows.Add("23/11/2026", "20", "15", "10", "12.50 kg", "82.50 kg");
        dgvConsumoDiario.Rows.Add("24/11/2026", "20", "15", "10", "12.50 kg", "70.00 kg");
        dgvConsumoDiario.Rows.Add("25/11/2026", "20", "15", "10", "12.50 kg", "57.50 kg");
    }
}