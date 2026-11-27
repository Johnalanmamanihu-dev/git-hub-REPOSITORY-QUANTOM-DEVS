using System;
using System.Drawing;
using System.Windows.Forms;
using RefugioGamaliel.Utilidades;
using RefugioGamaliel.Logica;

namespace RefugioGamaliel.Formularios;

/// <summary>
/// Control de inventario de alimentos en bodega (kg).
/// Solo interfaz gráfica, sin consultas SQL.
/// </summary>
public class FrmInventario : Form
{
    // ───────────── Controles públicos (para conectar MySQL) ─────────────
    public TextBox txtNombreAlimento = null!;
    public TextBox txtDescripcion = null!;
    public TextBox txtStockInicial = null!;

    public Button btnAgregarAlimento = null!;
    public Button btnActualizarStock = null!;
    public Button btnLimpiar = null!;

    public DataGridView dgvInventario = null!;

    public FrmInventario()
    {
        InicializarComponentes();
        btnAgregarAlimento.Click += BtnAgregarAlimento_Click;
        btnActualizarStock.Click += BtnActualizarStock_Click;
        CargarInventario();
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
            Text = "Control de Inventario de Alimentos",
            Font = Tema.Fuente(16f, FontStyle.Bold),
            ForeColor = Tema.Primario,
            Location = new Point(25, 20),
            AutoSize = true
        };
        Controls.Add(lblTitulo);

        // ══════════════════════════════════════════════════════════════
        // PARTE SUPERIOR: TARJETA BLANCA HORIZONTAL DE GESTIÓN
        // ══════════════════════════════════════════════════════════════
        var pnlSuperior = new Panel
        {
            BackColor = Color.White,
            Location = new Point(25, 65),
            Size = new Size(850, 165),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        var pnlFranja = new Panel
        {
            BackColor = Tema.Primario,
            Dock = DockStyle.Top,
            Height = 5
        };
        pnlSuperior.Controls.Add(pnlFranja);

        var lblSubtitulo = new Label
        {
            Text = "Registro y Ajuste de Existencias en Bodega",
            Font = Tema.Fuente(11.5f, FontStyle.Bold),
            ForeColor = Tema.TextoPrincipal,
            Location = new Point(20, 16),
            AutoSize = true
        };

        // Campo 1: Marca o Tipo de Alimento
        var lblNombre = new Label
        {
            Text = "Marca o Tipo de Alimento:",
            ForeColor = Tema.TextoPrincipal,
            Location = new Point(20, 48),
            AutoSize = true
        };

        txtNombreAlimento = new TextBox
        {
            Location = new Point(20, 72),
            Size = new Size(210, 28),
            Font = Tema.Fuente(10f),
            PlaceholderText = "Ej: Croquetas Adulto"
        };

        // Campo 2: Descripción / Etapa
        var lblDescripcion = new Label
        {
            Text = "Descripción / Etapa:",
            ForeColor = Tema.TextoPrincipal,
            Location = new Point(245, 48),
            AutoSize = true
        };

        txtDescripcion = new TextBox
        {
            Location = new Point(245, 72),
            Size = new Size(240, 28),
            Font = Tema.Fuente(10f),
            PlaceholderText = "Ej: Saco 25 kg - Perro adulto"
        };

        // Campo 3: Stock (kg)
        var lblStock = new Label
        {
            Text = "Stock (kg):",
            ForeColor = Tema.TextoPrincipal,
            Location = new Point(500, 48),
            AutoSize = true
        };

        txtStockInicial = new TextBox
        {
            Location = new Point(500, 72),
            Size = new Size(105, 28),
            Font = Tema.Fuente(10f),
            PlaceholderText = "0.00"
        };

        // Botones de acción
        btnAgregarAlimento = new Button
        {
            Text = "Agregar Alimento",
            BackColor = Tema.Secundario, // Naranja #E67E22
            ForeColor = Color.White,
            Location = new Point(620, 45),
            Size = new Size(205, 36),
            FlatStyle = FlatStyle.Flat,
            Font = Tema.Fuente(9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnAgregarAlimento.FlatAppearance.BorderSize = 0;

        btnActualizarStock = new Button
        {
            Text = "Actualizar Stock",
            BackColor = Tema.Primario, // Azul #1E3A5F
            ForeColor = Color.White,
            Location = new Point(620, 88),
            Size = new Size(140, 36),
            FlatStyle = FlatStyle.Flat,
            Font = Tema.Fuente(9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnActualizarStock.FlatAppearance.BorderSize = 0;

        btnLimpiar = new Button
        {
            Text = "Limpiar",
            BackColor = Color.FromArgb(224, 230, 237),
            ForeColor = Tema.TextoPrincipal,
            Location = new Point(765, 88),
            Size = new Size(60, 36),
            FlatStyle = FlatStyle.Flat,
            Font = Tema.Fuente(9f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnLimpiar.FlatAppearance.BorderSize = 0;
        btnLimpiar.Click += (s, e) => LimpiarFormulario();

        // Nota visual de unidad estándar
        var lblNotaUnidad = new Label
        {
            Text = "* Todas las existencias se contabilizan en kilogramos (kg) para el cálculo automático de autonomía.",
            ForeColor = Color.FromArgb(127, 140, 141),
            Font = Tema.Fuente(8.5f, FontStyle.Italic),
            Location = new Point(20, 118),
            AutoSize = true
        };

        pnlSuperior.Controls.AddRange(new Control[]
        {
            lblSubtitulo,
            lblNombre, txtNombreAlimento,
            lblDescripcion, txtDescripcion,
            lblStock, txtStockInicial,
            btnAgregarAlimento, btnActualizarStock, btnLimpiar,
            lblNotaUnidad
        });

        // ══════════════════════════════════════════════════════════════
        // PARTE INFERIOR: TABLA DE INVENTARIO DE ANCHO COMPLETO
        // ══════════════════════════════════════════════════════════════
        var lblTablaTitulo = new Label
        {
            Text = "Existencias Actuales en Bodega",
            Font = Tema.Fuente(12f, FontStyle.Bold),
            ForeColor = Tema.TextoPrincipal,
            Location = new Point(25, 250),
            AutoSize = true
        };
        Controls.Add(lblTablaTitulo);

        var pnlTabla = new Panel
        {
            BackColor = Color.White,
            Location = new Point(25, 280),
            Size = new Size(850, 325),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        dgvInventario = new DataGridView
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
            RowTemplate = { Height = 36 }
        };

        // Cabecera #1E3A5F
        dgvInventario.ColumnHeadersDefaultCellStyle.BackColor = Tema.Primario;
        dgvInventario.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvInventario.ColumnHeadersDefaultCellStyle.Font = Tema.Fuente(10f, FontStyle.Bold);
        dgvInventario.ColumnHeadersHeight = 40;

        // Filas alternas
        dgvInventario.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        dgvInventario.DefaultCellStyle.SelectionBackColor = Color.FromArgb(214, 234, 248);
        dgvInventario.DefaultCellStyle.SelectionForeColor = Tema.TextoPrincipal;

        // Columnas solicitadas
        dgvInventario.Columns.Add("id_alimento", "ID");
        dgvInventario.Columns.Add("nombre", "Alimento");
        dgvInventario.Columns.Add("descripcion", "Descripción");
        dgvInventario.Columns.Add("stock_kg", "Stock Disponible (kg)");
        dgvInventario.Columns.Add("unidad_medida", "Unidad");

        dgvInventario.Columns["id_alimento"].FillWeight = 10;
        dgvInventario.Columns["nombre"].FillWeight = 25;
        dgvInventario.Columns["descripcion"].FillWeight = 35;
        dgvInventario.Columns["stock_kg"].FillWeight = 18;
        dgvInventario.Columns["unidad_medida"].FillWeight = 12;

        // Al hacer clic en una fila, carga los datos arriba para facilitar "Actualizar Stock"
        dgvInventario.CellClick += DgvInventario_CellClick;

        pnlTabla.Controls.Add(dgvInventario);

        Controls.Add(pnlSuperior);
        Controls.Add(pnlTabla);

        ResumeLayout(false);
    }

    private void DgvInventario_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;

        var fila = dgvInventario.Rows[e.RowIndex]; _idSeleccionado = Convert.ToInt32(fila.Cells["id_alimento"].Value);
        txtNombreAlimento.Text = fila.Cells["nombre"].Value?.ToString() ?? string.Empty;
        txtDescripcion.Text = fila.Cells["descripcion"].Value?.ToString() ?? string.Empty;
        txtStockInicial.Text = fila.Cells["stock_kg"].Value?.ToString() ?? string.Empty;
    }

    public void LimpiarFormulario()
    {
        txtNombreAlimento.Clear();
        txtDescripcion.Clear();
        txtStockInicial.Clear();
        txtNombreAlimento.Focus();
    }

    private int _idSeleccionado = 0;

    private void CargarInventario()
    {
        var lista = InventarioService.ListarAlimentos(out string error);
        if (error != "")
        {
            MessageBox.Show(error, "Inventario", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        dgvInventario.Rows.Clear();
        foreach (var it in lista)
        {
            dgvInventario.Rows.Add(it.IdAlimento, it.Nombre, it.Descripcion,
                it.StockKg.ToString("0.00"), it.UnidadMedida);
        }
    }
    private void BtnAgregarAlimento_Click(object? sender, EventArgs e)
    {
        bool ok = InventarioService.RegistrarAlimento(
            txtNombreAlimento.Text, txtDescripcion.Text, "kg", out string error);
        if (!ok)
        {
            MessageBox.Show(error, "Inventario", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        MessageBox.Show("Alimento agregado con stock 0. Registre una donación para sumar existencias.",
            "Inventario", MessageBoxButtons.OK, MessageBoxIcon.Information);
        LimpiarFormulario();
        CargarInventario();
    }

    private void BtnActualizarStock_Click(object? sender, EventArgs e)
    {
        if (_idSeleccionado <= 0)
        {
            MessageBox.Show("Seleccione primero un alimento de la tabla.", "Inventario",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (!decimal.TryParse(txtStockInicial.Text.Trim().Replace(',', '.'),
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out decimal cantidad))
        {
            MessageBox.Show("Escriba una cantidad válida en kg.", "Inventario",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var r = MessageBox.Show($"¿Registrar una salida de {cantidad:0.00} kg de este alimento?",
            "Confirmar salida", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (r != DialogResult.Yes) return;

        if (!InventarioService.RegistrarSalida(_idSeleccionado, cantidad, out string error))
        {
            MessageBox.Show(error, "Inventario", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        _idSeleccionado = 0;
        LimpiarFormulario();
        CargarInventario();
    }
}