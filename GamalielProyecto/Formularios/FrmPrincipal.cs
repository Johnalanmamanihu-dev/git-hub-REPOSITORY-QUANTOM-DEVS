using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RefugioGamaliel.Utilidades;

namespace RefugioGamaliel.Formularios;

/// <summary>
/// Ventana principal: menú lateral fijo (230 px) + contenedor donde se
/// incrustan los demás formularios. Solo interfaz, sin lógica de base de datos.
/// </summary>
public class FrmPrincipal : Form
{
    // ───────────── Controles públicos (para conectar MySQL) ─────────────
    public Panel pnlMenuLateral = null!;
    public Panel pnlLogo = null!;
    public Label lblTituloMenu = null!;
    public Button btnDashboard = null!;
    public Button btnPerros = null!;
    public Button btnDonaciones = null!;
    public Button btnInventario = null!;
    public Button btnAlimentacion = null!;
    public Button btnCerrarSesion = null!;
    public Panel pnlContenedor = null!;

    private Form? _formularioActual;

    /// <summary>True cuando el usuario eligió "Cerrar Sesión" (FrmLogin lo usa para volver a mostrarse).</summary>
    public bool CerrandoSesion { get; private set; }

    public FrmPrincipal()
    {
        InicializarComponentes();
    }

    // ═════════════════════════ CONSTRUCCIÓN DE LA INTERFAZ ═════════════════════════
    private void InicializarComponentes()
    {
        SuspendLayout();

        // ───── Formulario ─────
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Tema.AplicarFormulario(this, "Refugio Gamaliel - Control de alimento y donaciones", 1150, 700);
        MinimumSize = new Size(1000, 640);

        // ───── Panel derecho: contenedor de formularios hijos ─────
        pnlContenedor = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Tema.Fondo
        };

        // ───── Panel lateral izquierdo (230 px) ─────
        pnlMenuLateral = new Panel
        {
            Dock = DockStyle.Left,
            Width = 230,
            BackColor = Tema.Primario
        };

        // Encabezado del menú: huella + título
        pnlLogo = new PanelDobleBuffer
        {
            Dock = DockStyle.Top,
            Height = 90,
            BackColor = Tema.Primario
        };
        pnlLogo.Paint += PintarLogo;

        lblTituloMenu = new Label
        {
            Text = "Refugio Gamaliel",
            AutoSize = false,
            Location = new Point(62, 0),
            Size = new Size(160, 90),
            TextAlign = ContentAlignment.MiddleLeft,
            Font = Tema.Fuente(14f, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent
        };
        pnlLogo.Controls.Add(lblTituloMenu);

        // Botones del menú (planos, ancho completo, texto a la izquierda)
        btnDashboard = Tema.BotonMenu("Dashboard");
        btnPerros = Tema.BotonMenu("Registro de Perros");
        btnDonaciones = Tema.BotonMenu("Donaciones");
        btnInventario = Tema.BotonMenu("Inventario");
        btnAlimentacion = Tema.BotonMenu("Alimentación");

        btnCerrarSesion = Tema.BotonMenu("Cerrar Sesión");
        btnCerrarSesion.Dock = DockStyle.Bottom;
        btnCerrarSesion.Height = 54;
        btnCerrarSesion.FlatAppearance.MouseOverBackColor = Tema.Alerta;
        btnCerrarSesion.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(Tema.Alerta, 0.1f);

        btnDashboard.Click += btnDashboard_Click;
        btnPerros.Click += btnPerros_Click;
        btnDonaciones.Click += btnDonaciones_Click;
        btnInventario.Click += btnInventario_Click;
        btnAlimentacion.Click += btnAlimentacion_Click;
        btnCerrarSesion.Click += btnCerrarSesion_Click;
  

        pnlMenuLateral.Controls.AddRange(new Control[]
        {
            btnCerrarSesion,
            btnAlimentacion,
            btnInventario,
            btnDonaciones,
            btnPerros,
            btnDashboard,
            pnlLogo
        });

        // ───── Composición del formulario ─────
        Controls.Add(pnlContenedor);   // Fill: se agrega primero
        Controls.Add(pnlMenuLateral);  // Left: se agrega después

        // Al abrir, muestra el Dashboard
        Load += (s, e) => btnDashboard.PerformClick();

        ResumeLayout(false);
    }

    // ═════════════════════════ NAVEGACIÓN ═════════════════════════

    /// <summary>
    /// Incrusta cualquier formulario dentro de pnlContenedor (reemplaza al anterior).
    /// Uso: AbrirFormularioHijo(new FrmPerros());
    /// </summary>
    public void AbrirFormularioHijo(Form formHijo)
    {
        if (_formularioActual != null)
        {
            pnlContenedor.Controls.Remove(_formularioActual);
            _formularioActual.Dispose();
        }

        _formularioActual = formHijo;

        formHijo.TopLevel = false;
        formHijo.FormBorderStyle = FormBorderStyle.None;
        formHijo.Dock = DockStyle.Fill;

        pnlContenedor.Controls.Add(formHijo);
        formHijo.BringToFront();
        formHijo.Show();
    }

    private void MarcarActivo(Button boton)
    {
        Tema.MarcarMenuActivo(boton, btnDashboard, btnPerros, btnDonaciones, btnInventario, btnAlimentacion);
    }

    // ═════════════════════════ EVENTOS DE LOS BOTONES ═════════════════════════
    // Cada botón usa un aviso temporal (FrmPendiente) hasta que exista su formulario real.
    // Cuando lo creen, reemplacen la línea indicada en cada método.

    private void btnDashboard_Click(object? sender, EventArgs e)
    {
        MarcarActivo(btnDashboard);
        // Cuando exista FrmDashboard: AbrirFormularioHijo(new FrmDashboard());
        AbrirFormularioHijo(new FrmDashboard());
    }

    private void btnPerros_Click(object? sender, EventArgs e)
    {
        MarcarActivo(btnPerros);
        // Reemplaza FrmPendiente por FrmPerros:
        AbrirFormularioHijo(new FrmPerros());
    }

    private void btnDonaciones_Click(object? sender, EventArgs e)
    {
        MarcarActivo(btnDonaciones);
        AbrirFormularioHijo(new FrmDonaciones());
    }

    private void btnInventario_Click(object? sender, EventArgs e)
    {
        MarcarActivo(btnInventario);
        AbrirFormularioHijo(new FrmInventario());
    }

    private void btnAlimentacion_Click(object? sender, EventArgs e)
    {
        MarcarActivo(btnAlimentacion);
        AbrirFormularioHijo(new FrmAlimentacion());
    }

    private void btnCerrarSesion_Click(object? sender, EventArgs e)
    {
        var respuesta = MessageBox.Show(this, "¿Desea cerrar la sesión?", "Cerrar sesión",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (respuesta != DialogResult.Yes)
            return;

        RefugioGamaliel.Logica.Sesion.Actual = null;
        CerrandoSesion = true;
        Close(); // FrmLogin detecta CerrandoSesion y vuelve a mostrarse
    }
    // ═════════════════════════ DIBUJO PERSONALIZADO ═════════════════════════

    private void PintarLogo(object? sender, PaintEventArgs e)
    {
        float k = DeviceDpi / 96f;
        float tam = 34f * k;
        DibujarHuella(e.Graphics, 18f * k, (pnlLogo.Height - tam) / 2f, tam, k);

        // Línea divisoria fina bajo el encabezado
        using var pen = new Pen(ControlPaint.Light(Tema.Primario, 0.2f));
        e.Graphics.DrawLine(pen, 16f * k, pnlLogo.Height - 1, pnlLogo.Width - 16f * k, pnlLogo.Height - 1);
    }

    private static void DibujarHuella(Graphics g, float x0, float y0, float tam, float k)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var pen = new Pen(Color.White, 1.8f * k) { LineJoin = LineJoin.Round };

        // Almohadilla central
        g.DrawEllipse(pen, x0 + tam * 0.20f, y0 + tam * 0.50f, tam * 0.60f, tam * 0.46f);

        // Cuatro dedos
        float w = tam * 0.22f, h = tam * 0.30f;
        g.DrawEllipse(pen, x0, y0 + tam * 0.34f, w, h);
        g.DrawEllipse(pen, x0 + tam * 0.22f, y0 + tam * 0.06f, w, h);
        g.DrawEllipse(pen, x0 + tam * 0.56f, y0 + tam * 0.06f, w, h);
        g.DrawEllipse(pen, x0 + tam * 0.78f, y0 + tam * 0.34f, w, h);
    }

    // ═════════════════════════ CLASES AUXILIARES ═════════════════════════

    /// <summary>Panel sin parpadeo al repintar.</summary>
    private class PanelDobleBuffer : Panel
    {
        public PanelDobleBuffer()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }
    }

    /// <summary>Formulario temporal que se muestra mientras el real no exista.</summary>
    private sealed class FrmPendiente : Form
    {
        public FrmPendiente(string nombre)
        {
            BackColor = Tema.Fondo;
            Controls.Add(new Label
            {
                Text = nombre + "\n\nEste formulario aún no ha sido creado.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = Tema.Fuente(14f),
                ForeColor = Tema.TextoPrincipal
            });
        }
    }
}