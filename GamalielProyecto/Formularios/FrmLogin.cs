using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using RefugioGamaliel.Utilidades;
using RefugioGamaliel.Logica;

namespace RefugioGamaliel.Formularios;

/// <summary>
/// Pantalla de inicio de sesión (RF9). Solo interfaz: la validación de
/// credenciales contra MySQL queda pendiente para el equipo.
/// </summary>
public class FrmLogin : Form
{
    // Mientras sea true, el botón abre FrmPrincipal sin validar campos (solo para probar navegación).
    // Cambiar a false cuando se conecte MySQL.
    private const bool ModoPrueba = false;

    // ───────────── Controles públicos (para conectar MySQL) ─────────────
    public Panel pnlEncabezado = null!;
    public Label lblTitulo = null!;
    public Panel pnlSombra = null!;
    public Panel pnlTarjeta = null!;
    public Label lblCorreo = null!;
    public TextBox txtCorreo = null!;
    public Label lblPassword = null!;
    public TextBox txtPassword = null!;
    public Label lblError = null!;
    public Button btnIniciarSesion = null!;

    public FrmLogin()
    {
        InicializarComponentes();
        ActiveControl = txtCorreo;
    }

    // ═════════════════════════ CONSTRUCCIÓN DE LA INTERFAZ ═════════════════════════
    private void InicializarComponentes()
    {
        SuspendLayout();

        // ───── Formulario (escalado DPI para pantallas 125% / 150%) ─────
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Tema.AplicarFormulario(this, "Refugio Gamaliel", 420, 520);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        // ───── Encabezado superior (100 px) con huella y título ─────
        pnlEncabezado = new PanelDobleBuffer
        {
            Dock = DockStyle.Top,
            Height = 100,
            BackColor = Tema.Primario
        };
        pnlEncabezado.Paint += (s, e) => DibujarHuella(e.Graphics, pnlEncabezado.Height);

        lblTitulo = new Label
        {
            Text = "Refugio Gamaliel",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = Tema.Fuente(22f),
            ForeColor = Color.White,
            BackColor = Color.Transparent
        };
        pnlEncabezado.Controls.Add(lblTitulo);

        // ───── Contenedor con sombra + tarjeta blanca ─────
        const int anchoTarjeta = 340;
        const int altoTarjeta = 282;
        const int margenSombra = 12;
        const int margenX = 20;
        const int anchoCampo = anchoTarjeta - (margenX * 2); // 300
        const int altoCampo = 40;

        int anchoContenedor = anchoTarjeta + margenSombra * 2;   // 364
        int altoContenedor = altoTarjeta + 24;                   // 306

        pnlSombra = new PanelDobleBuffer
        {
            Size = new Size(anchoContenedor, altoContenedor),
            Location = new Point(
                (ClientSize.Width - anchoContenedor) / 2,
                100 + (ClientSize.Height - 100 - altoContenedor) / 2),
            BackColor = Tema.Fondo
        };
        pnlSombra.Paint += PintarSombraTarjeta;

        pnlTarjeta = Tema.CrearTarjeta(anchoTarjeta, altoTarjeta);
        pnlTarjeta.Location = new Point(margenSombra, 10);
        pnlSombra.Controls.Add(pnlTarjeta);

        // ───── Campos ─────
        lblCorreo = Tema.CrearLabel("Correo Electrónico", 11.5f);
        lblCorreo.Location = new Point(margenX, 22);

        var campoCorreo = CrearCampoTexto(anchoCampo, altoCampo, false, out txtCorreo);
        campoCorreo.Location = new Point(margenX, 48);
        txtCorreo.TabIndex = 0;

        lblPassword = Tema.CrearLabel("Contraseña", 11.5f);
        lblPassword.Location = new Point(margenX, 104);

        var campoPassword = CrearCampoTexto(anchoCampo, altoCampo, true, out txtPassword);
        campoPassword.Location = new Point(margenX, 130);
        txtPassword.TabIndex = 1;

        lblError = new Label
        {
            Text = string.Empty,
            AutoSize = false,
            Size = new Size(anchoCampo, 26),
            Location = new Point(margenX, 176),
            Font = Tema.Fuente(9.5f),
            ForeColor = Tema.Alerta,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
            Visible = false
        };

        const int anchoBoton = 200;
        btnIniciarSesion = new BotonRedondeado
        {
            Text = "Iniciar Sesión",
            Size = new Size(anchoBoton, 46),
            Location = new Point((anchoTarjeta - anchoBoton) / 2, 212),
            BackColor = Tema.Secundario,
            ForeColor = Color.White,
            Font = Tema.Fuente(12f),
            Cursor = Cursors.Hand,
            TabIndex = 2
        };
        btnIniciarSesion.Click += btnIniciarSesion_Click;

        pnlTarjeta.Controls.AddRange(new Control[]
        {
            lblCorreo, campoCorreo,
            lblPassword, campoPassword,
            lblError,
            btnIniciarSesion
        });

        // ───── Composición del formulario ─────
        Controls.Add(pnlSombra);
        Controls.Add(pnlEncabezado);

        AcceptButton = btnIniciarSesion; // Enter inicia sesión

        ResumeLayout(false);
    }

    // ═════════════════════════ EVENTOS ═════════════════════════

    private void btnIniciarSesion_Click(object? sender, EventArgs e)
    {
        OcultarError();

        if (!ModoPrueba && !ValidarCampos())
            return;

        if (!ModoPrueba)
        {
            var usuario = LoginService.Validar(txtCorreo.Text, txtPassword.Text, out string error);
            if (usuario == null)
            {
                MostrarError(error);
                return;
            }
            Sesion.Actual = usuario;
        }

        // Navegación temporal para pruebas
        var principal = new FrmPrincipal();
        principal.FormClosed += (s, args) =>
        {
            if (principal.CerrandoSesion)
            {
                // "Cerrar Sesión": vuelve a mostrar el login limpio
                txtPassword.Clear();
                ActiveControl = txtCorreo;
                Show();
            }
            else
            {
                Close(); // cerró la ventana con la X: se cierra la app
            }
        };
        Hide();
        principal.Show();
    }
            
    

    // ═════════════════════════ MÉTODOS DE APOYO PARA EL EQUIPO ═════════════════════════

        /// <summary>Validación básica de campos vacíos (solo UI).</summary>
    public bool ValidarCampos()
    {
        if (string.IsNullOrWhiteSpace(txtCorreo.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            MostrarError("Ingrese su correo y contraseña.");
            return false;
        }
        if (!txtCorreo.Text.Contains('@'))
        {
            MostrarError("Ingrese un correo electrónico válido.");
            return false;
        }
        return true;
    }

    /// <summary>Muestra un mensaje de error en rojo bajo la contraseña.</summary>
    public void MostrarError(string mensaje)
    {
        lblError.Text = mensaje;
        lblError.Visible = true;
    }

    /// <summary>Oculta el mensaje de error.</summary>
    public void OcultarError()
    {
        lblError.Text = string.Empty;
        lblError.Visible = false;
    }

    // ═════════════════════════ ESTILO DE LA BARRA DE TÍTULO (OPCIONAL) ═════════════════════════

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int atributo, ref int valor, int tamaño);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    private const int DWMWA_CAPTION_COLOR = 35;
    private const int DWMWA_TEXT_COLOR = 36;

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        AplicarEstiloBarraTitulo();
    }

    /// <summary>
    /// En Windows 11 pinta la barra de título del mismo azul del encabezado.
    /// En versiones anteriores no hace nada.
    /// </summary>
    private void AplicarEstiloBarraTitulo()
    {
        try
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
                return;

            int esquinas = 2;
            int modoOscuro = 1;
            int colorBarra = ColorABgr(Tema.Primario);
            int colorTexto = ColorABgr(Color.White);

            DwmSetWindowAttribute(Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref esquinas, sizeof(int));
            DwmSetWindowAttribute(Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref modoOscuro, sizeof(int));
            DwmSetWindowAttribute(Handle, DWMWA_CAPTION_COLOR, ref colorBarra, sizeof(int));
            DwmSetWindowAttribute(Handle, DWMWA_TEXT_COLOR, ref colorTexto, sizeof(int));
        }
        catch
        {
            // Si falla, la ventana usa la barra de título estándar.
        }
    }

    private static int ColorABgr(Color c) => c.R | (c.G << 8) | (c.B << 16);

    // ═════════════════════════ DIBUJO PERSONALIZADO ═════════════════════════

    /// <summary>Dibuja la huella de perro (contorno blanco) a la izquierda del encabezado.</summary>
    private void DibujarHuella(Graphics g, int altoEncabezado)
    {
        float k = DeviceDpi / 96f;
        float tam = 44f * k;
        float x0 = 24f * k;
        float y0 = (altoEncabezado - tam) / 2f;

        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var pen = new Pen(Color.White, 2f * k) { LineJoin = LineJoin.Round };

        // Almohadilla central
        g.DrawEllipse(pen, x0 + tam * 0.20f, y0 + tam * 0.50f, tam * 0.60f, tam * 0.46f);

        // Cuatro dedos
        float w = tam * 0.22f, h = tam * 0.30f;
        g.DrawEllipse(pen, x0, y0 + tam * 0.34f, w, h);
        g.DrawEllipse(pen, x0 + tam * 0.22f, y0 + tam * 0.06f, w, h);
        g.DrawEllipse(pen, x0 + tam * 0.56f, y0 + tam * 0.06f, w, h);
        g.DrawEllipse(pen, x0 + tam * 0.78f, y0 + tam * 0.34f, w, h);
    }

    /// <summary>Sombra suave alrededor de la tarjeta blanca.</summary>
    private void PintarSombraTarjeta(object? sender, PaintEventArgs e)
    {
        float k = DeviceDpi / 96f;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = pnlTarjeta.Bounds;
        rect.Offset(0, (int)(2 * k));

        for (int i = 9; i >= 1; i--)
        {
            var r = rect;
            r.Inflate((int)(i * k), (int)(i * k));
            using var ruta = RutaRedondeada(r, (int)((i + 2) * k));
            using var pincel = new SolidBrush(Color.FromArgb(5, Tema.Primario));
            e.Graphics.FillPath(pincel, ruta);
        }
    }

    /// <summary>
    /// Campo de texto con borde gris claro que cambia a naranja al enfocarse.
    /// Devuelve el contenedor y entrega el TextBox interno por 'out'.
    /// </summary>
    private static Panel CrearCampoTexto(int ancho, int alto, bool esPassword, out TextBox textBox)
    {
        var contenedor = new PanelDobleBuffer
        {
            Size = new Size(ancho, alto),
            BackColor = Color.White,
            Cursor = Cursors.IBeam
        };

        var campo = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Font = Tema.Fuente(12f),
            ForeColor = Tema.TextoPrincipal,
            BackColor = Color.White,
            Width = ancho - 24,
            UseSystemPasswordChar = esPassword
        };
        campo.Location = new Point(12, (alto - campo.Height) / 2);

        var colorBorde = ControlPaint.Dark(Tema.Borde, 0.1f);

        contenedor.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var color = campo.Focused ? Tema.Secundario : colorBorde;
            using var pen = new Pen(color, campo.Focused ? 1.6f : 1f);
            using var ruta = RutaRedondeada(new Rectangle(0, 0, contenedor.Width - 1, contenedor.Height - 1), 4);
            e.Graphics.DrawPath(pen, ruta);
        };
        campo.Enter += (s, e) => contenedor.Invalidate();
        campo.Leave += (s, e) => contenedor.Invalidate();
        contenedor.Click += (s, e) => campo.Focus();

        contenedor.Controls.Add(campo);
        textBox = campo;
        return contenedor;
    }

    private static GraphicsPath RutaRedondeada(Rectangle r, int radio)
    {
        int d = Math.Max(1, radio * 2);
        var ruta = new GraphicsPath();
        ruta.AddArc(r.X, r.Y, d, d, 180, 90);
        ruta.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        ruta.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        ruta.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        ruta.CloseFigure();
        return ruta;
    }

    // ═════════════════════════ CONTROLES AUXILIARES ═════════════════════════

    /// <summary>Panel sin parpadeo al repintar.</summary>
    private class PanelDobleBuffer : Panel
    {
        public PanelDobleBuffer()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }
    }

    /// <summary>Botón plano con esquinas redondeadas y efecto hover.</summary>
    private sealed class BotonRedondeado : Button
    {
        private bool _hover;
        private bool _presionado;

        public BotonRedondeado()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _presionado = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs mevent) { _presionado = true; Invalidate(); base.OnMouseDown(mevent); }
        protected override void OnMouseUp(MouseEventArgs mevent) { _presionado = false; Invalidate(); base.OnMouseUp(mevent); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? Color.White);

            Color relleno = !Enabled ? ControlPaint.Light(BackColor, 0.5f)
                          : _presionado ? ControlPaint.Dark(BackColor, 0.15f)
                          : _hover ? ControlPaint.Dark(BackColor, 0.05f)
                          : BackColor;

            using var ruta = RutaRedondeada(new Rectangle(0, 0, Width - 1, Height - 1), 6);
            using var pincel = new SolidBrush(relleno);
            g.FillPath(pincel, ruta);

            TextRenderer.DrawText(g, Text, Font, ClientRectangle, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        }
    }
}
