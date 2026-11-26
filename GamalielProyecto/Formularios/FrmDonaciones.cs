using System;
using System.Drawing;
using System.Windows.Forms;
using RefugioGamaliel.Utilidades;

namespace RefugioGamaliel.Formularios;

/// <summary>
/// Registro de aportes y donaciones de alimento en kg.
/// Solo interfaz gráfica, sin consultas SQL.
/// </summary>
public class FrmDonaciones : Form
{
    // Colores semánticos específicos de esta vista
    private readonly Color colorExito = Color.FromArgb(39, 174, 96); // #27AE60 Verde Esmeralda

    // ───────────── Controles públicos (para conectar MySQL) ─────────────
    public TextBox txtDonante = null!;
    public CheckBox chkAnonimo = null!;
    public ComboBox cmbTipoDonante = null!;
    public ComboBox cmbAlimento = null!;
    public TextBox txtCantidadKg = null!;
    public DateTimePicker dtpFechaDonacion = null!;

    public Button btnRegistrarDonacion = null!;
    public Button btnLimpiar = null!;

    public DataGridView dgvDonaciones = null!;

    public FrmDonaciones()
    {
        InicializarComponentes();
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
            Text = "Registro y Control de Donaciones",
            Font = Tema.Fuente(16f, FontStyle.Bold),
            ForeColor = Tema.Primario,
            Location = new Point(25, 20),
            AutoSize = true
        };
        Controls.Add(lblTitulo);

        // ══════════════════════════════════════════════════════════════
        // MITAD SUPERIOR: TARJETA DE REGISTRO (2 FILAS DE CAMPOS)
        // ══════════════════════════════════════════════════════════════
        var pnlFormulario = new Panel
        {
            BackColor = Color.White,
            Location = new Point(25, 65),
            Size = new Size(850, 235),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        var pnlFranja = new Panel
        {
            BackColor = colorExito,
            Dock = DockStyle.Top,
            Height = 5
        };
        pnlFormulario.Controls.Add(pnlFranja);

        var lblSubtitulo = new Label
        {
            Text = "Nueva Entrega de Alimento",
            Font = Tema.Fuente(12f, FontStyle.Bold),
            ForeColor = Tema.TextoPrincipal,
            Location = new Point(20, 18),
            AutoSize = true
        };

        // ───── FILA 1: Donante | CheckBox Anónimo | Tipo de Donante ─────
        var lblDonante = new Label
        {
            Text = "Nombre o Razón Social:",
            ForeColor = Tema.TextoPrincipal,
            Location = new Point(20, 55),
            AutoSize = true
        };

        txtDonante = new TextBox
        {
            Location = new Point(20, 80),
            Size = new Size(340, 28),
            Font = Tema.Fuente(10f)
        };

        chkAnonimo = new CheckBox
        {
            Text = "Donante Anónimo",
            ForeColor = Tema.TextoPrincipal,
            Location = new Point(380, 82),
            AutoSize = true,
            Cursor = Cursors.Hand,
            Font = Tema.Fuente(9.5f, FontStyle.Bold)
        };
        chkAnonimo.CheckedChanged += ChkAnonimo_CheckedChanged;

        var lblTipoDonante = new Label
        {
            Text = "Tipo de Donante:",
            ForeColor = Tema.TextoPrincipal,
            Location = new Point(555, 55),
            AutoSize = true
        };

        cmbTipoDonante = new ComboBox
        {
            Location = new Point(555, 80),
            Size = new Size(265, 28),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = Tema.Fuente(10f)
        };
        cmbTipoDonante.Items.AddRange(new object[] { "Persona", "Empresa", "Institución" });
        cmbTipoDonante.SelectedIndex = 0;

        // ───── FILA 2: Tipo de Alimento | Cantidad (kg) | Fecha | Botones ─────
        var lblAlimento = new Label
        {
            Text = "Tipo de Alimento:",
            ForeColor = Tema.TextoPrincipal,
            Location = new Point(20, 128),
            AutoSize = true
        };

        cmbAlimento = new ComboBox
        {
            Location = new Point(20, 153),
            Size = new Size(230, 28),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = Tema.Fuente(10f)
        };
        // Valores de referencia hasta que MySQL cargue la tabla Alimento
        cmbAlimento.Items.AddRange(new object[] { "Croquetas Adulto", "Alimento Balanceado", "Croquetas Cachorro", "Otros Donativos" });
        cmbAlimento.SelectedIndex = 0;

        var lblCantidad = new Label
        {
            Text = "Cantidad (kg):",
            ForeColor = Tema.TextoPrincipal,
            Location = new Point(270, 128),
            AutoSize = true
        };

        txtCantidadKg = new TextBox
        {
            Location = new Point(270, 153),
            Size = new Size(125, 28),
            Font = Tema.Fuente(10f),
            PlaceholderText = "Ej: 25.5"
        };

        var lblFecha = new Label
        {
            Text = "Fecha de Recepción:",
            ForeColor = Tema.TextoPrincipal,
            Location = new Point(415, 128),
            AutoSize = true
        };

        dtpFechaDonacion = new DateTimePicker
        {
            Location = new Point(415, 153),
            Size = new Size(140, 28),
            Format = DateTimePickerFormat.Short,
            Font = Tema.Fuente(10f)
        };

        btnRegistrarDonacion = new Button
        {
            Text = "Registrar Donación",
            BackColor = colorExito,
            ForeColor = Color.White,
            Location = new Point(575, 146),
            Size = new Size(165, 40),
            FlatStyle = FlatStyle.Flat,
            Font = Tema.Fuente(10f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnRegistrarDonacion.FlatAppearance.BorderSize = 0;

        btnLimpiar = new Button
        {
            Text = "Limpiar",
            BackColor = Color.FromArgb(224, 230, 237),
            ForeColor = Tema.TextoPrincipal,
            Location = new Point(750, 146),
            Size = new Size(70, 40),
            FlatStyle = FlatStyle.Flat,
            Font = Tema.Fuente(9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnLimpiar.FlatAppearance.BorderSize = 0;
        btnLimpiar.Click += (s, e) => LimpiarFormulario();

        pnlFormulario.Controls.AddRange(new Control[]
        {
            lblSubtitulo,
            lblDonante, txtDonante,
            chkAnonimo,
            lblTipoDonante, cmbTipoDonante,
            lblAlimento, cmbAlimento,
            lblCantidad, txtCantidadKg,
            lblFecha, dtpFechaDonacion,
            btnRegistrarDonacion, btnLimpiar
        });

        // ══════════════════════════════════════════════════════════════
        // MITAD INFERIOR: HISTORIAL DE DONACIONES (DATAGRIDVIEW)
        // ══════════════════════════════════════════════════════════════
        var lblHistorial = new Label
        {
            Text = "Historial de Donaciones Recibidas",
            Font = Tema.Fuente(12f, FontStyle.Bold),
            ForeColor = Tema.TextoPrincipal,
            Location = new Point(25, 318),
            AutoSize = true
        };
        Controls.Add(lblHistorial);

        var pnlTabla = new Panel
        {
            BackColor = Color.White,
            Location = new Point(25, 348),
            Size = new Size(850, 260),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        dgvDonaciones = new DataGridView
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

        dgvDonaciones.ColumnHeadersDefaultCellStyle.BackColor = Tema.Primario;
        dgvDonaciones.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvDonaciones.ColumnHeadersDefaultCellStyle.Font = Tema.Fuente(10f, FontStyle.Bold);
        dgvDonaciones.ColumnHeadersHeight = 40;

        dgvDonaciones.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        dgvDonaciones.DefaultCellStyle.SelectionBackColor = Color.FromArgb(214, 234, 248);
        dgvDonaciones.DefaultCellStyle.SelectionForeColor = Tema.TextoPrincipal;

        // Columnas solicitadas
        dgvDonaciones.Columns.Add("id_donacion", "ID");
        dgvDonaciones.Columns.Add("donante", "Donante");
        dgvDonaciones.Columns.Add("tipo_donante", "Tipo");
        dgvDonaciones.Columns.Add("alimento", "Alimento");
        dgvDonaciones.Columns.Add("cantidad_kg", "Cantidad (kg)");
        dgvDonaciones.Columns.Add("fecha_donacion", "Fecha");
        dgvDonaciones.Columns.Add("validado_por", "Validado por");

        dgvDonaciones.Columns["id_donacion"].FillWeight = 10;
        dgvDonaciones.Columns["cantidad_kg"].FillWeight = 16;

        pnlTabla.Controls.Add(dgvDonaciones);

        Controls.Add(pnlFormulario);
        Controls.Add(pnlTabla);

        ResumeLayout(false);
    }

    private void ChkAnonimo_CheckedChanged(object? sender, EventArgs e)
    {
        if (chkAnonimo.Checked)
        {
            txtDonante.Text = "Anónimo";
            txtDonante.Enabled = false;
            cmbTipoDonante.SelectedItem = "Persona";
            cmbTipoDonante.Enabled = false;
        }
        else
        {
            txtDonante.Clear();
            txtDonante.Enabled = true;
            cmbTipoDonante.Enabled = true;
            txtDonante.Focus();
        }
    }

    public void LimpiarFormulario()
    {
        chkAnonimo.Checked = false;
        txtDonante.Clear();
        txtDonante.Enabled = true;
        cmbTipoDonante.SelectedIndex = 0;
        cmbTipoDonante.Enabled = true;
        if (cmbAlimento.Items.Count > 0) cmbAlimento.SelectedIndex = 0;
        txtCantidadKg.Clear();
        dtpFechaDonacion.Value = DateTime.Now;
        txtDonante.Focus();
    }

    private void CargarDatosEjemplo()
    {
        dgvDonaciones.Rows.Add("1", "Veterinaria San Roque", "Empresa", "Croquetas Adulto", "50.00 kg", "18/11/2026", "John Alan Mamani");
        dgvDonaciones.Rows.Add("2", "Anónimo", "Persona", "Alimento Balanceado", "15.50 kg", "20/11/2026", "Emilio Salguero");
        dgvDonaciones.Rows.Add("3", "Colegio Bolivia", "Institución", "Croquetas Cachorro", "30.00 kg", "22/11/2026", "John David Castillo");
    }
}
