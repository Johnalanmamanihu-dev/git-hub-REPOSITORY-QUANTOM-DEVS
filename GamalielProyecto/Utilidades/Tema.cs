using System.Drawing;
using System.Windows.Forms;

namespace RefugioGamaliel.Utilidades;

/// <summary>
/// Tema visual único del sistema. Todos los formularios usan estas
/// constantes y fábricas para mantener un estilo consistente.
/// Solo UI: no contiene lógica de base de datos.
/// </summary>
public static class Tema
{
    // ───────────── Paleta oficial ─────────────
    public static readonly Color Primario = Color.FromArgb(30, 58, 95);    // #1E3A5F  menú lateral y encabezados de tabla
    public static readonly Color Secundario = Color.FromArgb(230, 126, 34);  // #E67E22  botones principales
    public static readonly Color Fondo = Color.FromArgb(244, 246, 249); // #F4F6F9  fondo general
    public static readonly Color Tarjeta = Color.White;                   // tarjetas / paneles
    public static readonly Color TextoPrincipal = Color.FromArgb(44, 62, 80);   // #2C3E50
    public static readonly Color Exito = Color.FromArgb(39, 174, 96);   // #27AE60
    public static readonly Color Alerta = Color.FromArgb(231, 76, 60);   // #E74C3C

    // Neutro auxiliar solo para bordes finos y filas alternas (no es color de marca)
    public static readonly Color Borde = Color.FromArgb(222, 226, 232);
    public static readonly Color FilaAlterna = Color.FromArgb(249, 250, 252);

    // ───────────── Tipografía (solo Segoe UI) ─────────────
    public const string NombreFuente = "Segoe UI";

    public static Font Fuente(float tamaño = 10f, FontStyle estilo = FontStyle.Regular)
        => new Font(NombreFuente, tamaño, estilo, GraphicsUnit.Point);

    // ───────────── Formulario base ─────────────
    /// <summary>Aplica fondo, fuente y color de texto a un formulario.</summary>
    public static void AplicarFormulario(Form form, string titulo, int ancho, int alto)
    {
        form.Text = titulo;
        form.ClientSize = new Size(ancho, alto);
        form.BackColor = Fondo;
        form.ForeColor = TextoPrincipal;
        form.Font = Fuente(10f);
        form.StartPosition = FormStartPosition.CenterScreen;
    }

    // ───────────── Labels ─────────────
    public static Label CrearLabel(string texto, float tamaño = 10f, bool negrita = false, Color? color = null)
    {
        return new Label
        {
            Text = texto,
            AutoSize = true,
            Font = Fuente(tamaño, negrita ? FontStyle.Bold : FontStyle.Regular),
            ForeColor = color ?? TextoPrincipal,
            BackColor = Color.Transparent
        };
    }

    public static Label CrearTitulo(string texto) => CrearLabel(texto, 18f, true);
    public static Label CrearSubtitulo(string texto) => CrearLabel(texto, 11f, false, Color.FromArgb(44, 62, 80));

    // ───────────── Paneles / tarjetas ─────────────
    public static Panel CrearTarjeta(int ancho, int alto)
    {
        var panel = new Panel
        {
            Size = new Size(ancho, alto),
            BackColor = Tarjeta,
            Padding = new Padding(16)
        };
        panel.Paint += (s, e) =>
        {
            using var pen = new Pen(Borde);
            e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
        };
        return panel;
    }

    // ───────────── Botones ─────────────
    private static Button CrearBoton(string texto, Color fondo, int ancho, int alto)
    {
        var btn = new Button
        {
            Text = texto,
            Size = new Size(ancho, alto),
            BackColor = fondo,
            ForeColor = Color.White,
            Font = Fuente(10f, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(fondo, 0.05f);
        btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(fondo, 0.15f);
        return btn;
    }

    /// <summary>Botón principal (naranja).</summary>
    public static Button BotonPrincipal(string texto, int ancho = 150, int alto = 38)
        => CrearBoton(texto, Secundario, ancho, alto);

    /// <summary>Botón de confirmación / guardado (verde).</summary>
    public static Button BotonExito(string texto, int ancho = 150, int alto = 38)
        => CrearBoton(texto, Exito, ancho, alto);

    /// <summary>Botón de acción destructiva (rojo).</summary>
    public static Button BotonPeligro(string texto, int ancho = 150, int alto = 38)
        => CrearBoton(texto, Alerta, ancho, alto);

    /// <summary>Botón neutro (azul oscuro) para acciones secundarias: Cancelar, Limpiar, etc.</summary>
    public static Button BotonNeutro(string texto, int ancho = 150, int alto = 38)
        => CrearBoton(texto, Primario, ancho, alto);

    /// <summary>Botón del menú lateral.</summary>
    public static Button BotonMenu(string texto)
    {
        var btn = new Button
        {
            Text = "   " + texto,
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = Primario,
            ForeColor = Color.White,
            Font = Fuente(11f),
            FlatStyle = FlatStyle.Flat,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(16, 0, 0, 0),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(Primario, 0.15f);
        btn.FlatAppearance.MouseDownBackColor = ControlPaint.Light(Primario, 0.25f);
        return btn;
    }

    /// <summary>Resalta el botón de menú activo y restaura el resto.</summary>
    public static void MarcarMenuActivo(Button activo, params Button[] todos)
    {
        foreach (var b in todos)
        {
            b.BackColor = Primario;
            b.Font = Fuente(11f);
        }
        activo.BackColor = ControlPaint.Light(Primario, 0.2f);
        activo.Font = Fuente(11f, FontStyle.Bold);
    }

    // ───────────── Campos de entrada ─────────────
    public static TextBox CrearTextBox(int ancho = 240)
    {
        return new TextBox
        {
            Width = ancho,
            Font = Fuente(10.5f),
            ForeColor = TextoPrincipal,
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
    }

    public static ComboBox CrearComboBox(int ancho = 240)
    {
        return new ComboBox
        {
            Width = ancho,
            Font = Fuente(10.5f),
            ForeColor = TextoPrincipal,
            BackColor = Color.White,
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat
        };
    }

    public static NumericUpDown CrearNumerico(int ancho = 120, decimal max = 100000, int decimales = 0)
    {
        return new NumericUpDown
        {
            Width = ancho,
            Font = Fuente(10.5f),
            ForeColor = TextoPrincipal,
            Maximum = max,
            DecimalPlaces = decimales
        };
    }

    public static DateTimePicker CrearFecha(int ancho = 160)
    {
        return new DateTimePicker
        {
            Width = ancho,
            Font = Fuente(10.5f),
            Format = DateTimePickerFormat.Short
        };
    }

    // ───────────── DataGridView ─────────────
    public static void EstilizarGrid(DataGridView grid)
    {
        grid.BackgroundColor = Tarjeta;
        grid.BorderStyle = BorderStyle.None;
        grid.GridColor = Borde;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.EnableHeadersVisualStyles = false;
        grid.RowHeadersVisible = false;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.ReadOnly = true;
        grid.MultiSelect = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersHeight = 40;
        grid.RowTemplate.Height = 36;

        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Primario,
            ForeColor = Color.White,
            Font = Fuente(10f, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(8, 0, 0, 0),
            SelectionBackColor = Primario,
            SelectionForeColor = Color.White
        };
        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.White,
            ForeColor = TextoPrincipal,
            Font = Fuente(10f),
            Padding = new Padding(8, 0, 0, 0),
            SelectionBackColor = ControlPaint.Light(Secundario, 0.7f),
            SelectionForeColor = TextoPrincipal
        };
        grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = FilaAlterna
        };
    }

    // ───────────── Alertas visuales ─────────────
    /// <summary>Etiqueta tipo "chip" para estados (ej. días de reserva, alerta de stock bajo).</summary>
    public static Label CrearChip(string texto, bool critico)
    {
        return new Label
        {
            Text = texto,
            AutoSize = false,
            Size = new Size(160, 30),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = Fuente(10f, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = critico ? Alerta : Exito
        };
    }
}