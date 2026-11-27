using System;
using System.Drawing;
using System.Windows.Forms;
using RefugioGamaliel.Utilidades;
using RefugioGamaliel.Logica;

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
    public ComboBox cmbPerro = null!;
    public ComboBox cmbAlimentoConsumo = null!;
    public NumericUpDown nudGramos = null!;
    public Button btnRegistrarConsumo = null!;

    // Conteos de referencia visual (luego los compañeros los reemplazan con COUNT de MySQL)
    private int _cantPequenosEjemplo = 0;
    private int _cantMedianosEjemplo = 0;
    private int _cantGrandesEjemplo = 0;
    private decimal _stockActualEjemplo = 0m;

    public FrmAlimentacion()
    {
        InicializarComponentes();
        CargarDatosReales();
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
            Location = new Point(25, 415),
            AutoSize = true
        };
        Controls.Add(lblTablaTitulo);

        var pnlTabla = new Panel
        {
            BackColor = Color.White,
            Location = new Point(25, 445),
            Size = new Size(850, 190),
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
        dgvConsumoDiario.Columns.Add("perro", "Perro");
        dgvConsumoDiario.Columns.Add("alimento", "Alimento");
        dgvConsumoDiario.Columns.Add("espacio", "");
        dgvConsumoDiario.Columns.Add("total_consumido", "Consumido (kg)");
        dgvConsumoDiario.Columns.Add("espacio2", "");

        var pnlConsumo = new Panel
        {
            BackColor = Color.White,
            Location = new Point(25, 322),
            Size = new Size(850, 80),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        pnlConsumo.Controls.Add(new Panel { BackColor = Color.FromArgb(39, 174, 96), Dock = DockStyle.Top, Height = 4 });

        pnlConsumo.Controls.Add(new Label { Text = "Perro:", Location = new Point(15, 14), AutoSize = true, ForeColor = Tema.TextoPrincipal });
        cmbPerro = new ComboBox { Location = new Point(15, 38), Size = new Size(200, 28), DropDownStyle = ComboBoxStyle.DropDownList, Font = Tema.Fuente(10f) };

        pnlConsumo.Controls.Add(new Label { Text = "Alimento:", Location = new Point(230, 14), AutoSize = true, ForeColor = Tema.TextoPrincipal });
        cmbAlimentoConsumo = new ComboBox { Location = new Point(230, 38), Size = new Size(200, 28), DropDownStyle = ComboBoxStyle.DropDownList, Font = Tema.Fuente(10f) };

        pnlConsumo.Controls.Add(new Label { Text = "Gramos:", Location = new Point(445, 14), AutoSize = true, ForeColor = Tema.TextoPrincipal });
        nudGramos = new NumericUpDown { Location = new Point(445, 38), Size = new Size(110, 28), Minimum = 1, Maximum = 5000, Value = 300, Increment = 25, Font = Tema.Fuente(10f), TextAlign = HorizontalAlignment.Right };

        btnRegistrarConsumo = new Button
        {
            Text = "Registrar Consumo",
            BackColor = Color.FromArgb(39, 174, 96),
            ForeColor = Color.White,
            Location = new Point(580, 32),
            Size = new Size(180, 38),
            FlatStyle = FlatStyle.Flat,
            Font = Tema.Fuente(10f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnRegistrarConsumo.FlatAppearance.BorderSize = 0;
        btnRegistrarConsumo.Click += BtnRegistrarConsumo_Click;

        pnlConsumo.Controls.AddRange(new Control[] { cmbPerro, cmbAlimentoConsumo, nudGramos, btnRegistrarConsumo });
        Controls.Add(pnlConsumo);
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
            lblAutonomiaEstimada.Text = $"Autonomía con stock de referencia ({_stockActualEjemplo:0.0} kg): {dias:0.0} días";
            lblAutonomiaEstimada.ForeColor = dias < 7m ? Tema.Alerta : Color.FromArgb(39, 174, 96);
        }
    }

    private void CargarDatosReales()
    {
        CargarCombos();
        var conteo = AlimentacionService.ContarPerrosPorTamano(out string e1);
        if (e1 != "") { MessageBox.Show(e1, "Alimentación", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        _cantPequenosEjemplo = conteo[0];
        _cantMedianosEjemplo = conteo[1];
        _cantGrandesEjemplo = conteo[2];

        _stockActualEjemplo = AlimentacionService.StockTotalKg(out string e2);
        if (e2 != "") { MessageBox.Show(e2, "Alimentación", MessageBoxButtons.OK, MessageBoxIcon.Warning); }

        RecalcularVistaPrevia();

        var lista = AlimentacionService.ListarConsumos(out string e3);
        if (e3 != "") { MessageBox.Show(e3, "Alimentación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

        dgvConsumoDiario.Rows.Clear();
        foreach (var c in lista)
        {
            dgvConsumoDiario.Rows.Add(c.Fecha.ToString("dd/MM/yyyy"), c.Perro, c.Alimento,
                "", c.CantidadKg.ToString("0.000") + " kg", "");
        }
    }
    private void CargarCombos()
    {
        var perros = PerroService.Listar(out string e1);
        if (e1 != "") MessageBox.Show(e1, "Alimentación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        cmbPerro.DataSource = null;
        cmbPerro.DisplayMember = "Nombre";
        cmbPerro.ValueMember = "IdPerro";
        cmbPerro.DataSource = perros;

        var alimentos = InventarioService.ListarAlimentos(out string e2);
        if (e2 != "") MessageBox.Show(e2, "Alimentación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        cmbAlimentoConsumo.DataSource = null;
        cmbAlimentoConsumo.DisplayMember = "Nombre";
        cmbAlimentoConsumo.ValueMember = "IdAlimento";
        cmbAlimentoConsumo.DataSource = alimentos;
    }

    private void BtnRegistrarConsumo_Click(object? sender, EventArgs e)
    {
        if (cmbPerro.SelectedValue is not int idPerro)
        {
            MessageBox.Show("Seleccione un perro.", "Alimentación", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (cmbAlimentoConsumo.SelectedValue is not int idAlimento)
        {
            MessageBox.Show("Seleccione un alimento.", "Alimentación", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!AlimentacionService.RegistrarConsumo(idPerro, idAlimento, nudGramos.Value,
                DateTime.Now, out string error))
        {
            MessageBox.Show(error, "Alimentación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        MessageBox.Show("Consumo registrado y existencias actualizadas.", "Alimentación",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        CargarDatosReales();
    }
}