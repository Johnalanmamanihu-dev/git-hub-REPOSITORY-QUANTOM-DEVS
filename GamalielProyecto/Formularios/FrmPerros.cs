using System;
using System.Drawing;
using System.Windows.Forms;
using RefugioGamaliel.Logica;

namespace RefugioGamaliel.Formularios
{
    public class FrmPerros : Form
    {
        // Paleta de colores oficial - Refugio Gamaliel
        private readonly Color colorPrimario = Color.FromArgb(30, 58, 95);     // #1E3A5F
        private readonly Color colorSecundario = Color.FromArgb(230, 126, 34); // #E67E22
        private readonly Color colorFondo = Color.FromArgb(244, 246, 249);     // #F4F6F9
        private readonly Color colorTexto = Color.FromArgb(44, 62, 80);        // #2C3E50
        private readonly Color colorAdvertencia = Color.FromArgb(241, 196, 15);// #F1C40F
        private readonly Color colorPeligro = Color.FromArgb(231, 76, 60);     // #E74C3C

        // Controles públicos para que tus compañeros conecten MySQL fácilmente
        public TextBox txtNombrePerro;
        public ComboBox cmbTamano;
        public DateTimePicker dtpFechaIngreso;
        public Label lblRacionEstimada;

        public Button btnGuardar;
        public Button btnEditar;
        public Button btnEliminar;
        public Button btnLimpiar;

        public DataGridView dgvPerros;

        public FrmPerros()
        {
            ConfigurarFormulario();
            InicializarComponentes();
            ConectarEventos();
            CargarPerros();
        }

        private void ConfigurarFormulario()
        {
            this.Text = "Registro de Perros - Refugio Gamaliel";
            this.BackColor = colorFondo;
            this.FormBorderStyle = FormBorderStyle.None; // Listo para incrustar en FrmPrincipal
            this.Size = new Size(920, 650);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
        }

        private void InicializarComponentes()
        {
            // Título superior de la vista
            Label lblTituloVista = new Label
            {
                Text = "Gestión de Población Canina",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = colorPrimario,
                Location = new Point(25, 20),
                AutoSize = true
            };
            this.Controls.Add(lblTituloVista);

            // ==========================================
            // PANEL IZQUIERDO: FORMULARIO DE REGISTRO
            // ==========================================
            Panel pnlFormulario = new Panel
            {
                BackColor = Color.White,
                Location = new Point(25, 65),
                Size = new Size(320, 540),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            };

            // Franja superior decorativa en la tarjeta
            Panel pnlFranja = new Panel
            {
                BackColor = colorPrimario,
                Dock = DockStyle.Top,
                Height = 5
            };
            pnlFormulario.Controls.Add(pnlFranja);

            Label lblSubtitulo = new Label
            {
                Text = "Datos del Can",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = colorTexto,
                Location = new Point(20, 20),
                AutoSize = true
            };
            pnlFormulario.Controls.Add(lblSubtitulo);

            // Campo: Nombre
            Label lblNombre = new Label
            {
                Text = "Nombre del Perro:",
                ForeColor = colorTexto,
                Location = new Point(20, 60),
                AutoSize = true
            };
            txtNombrePerro = new TextBox
            {
                Location = new Point(20, 85),
                Size = new Size(275, 28),
                Font = new Font("Segoe UI", 10F)
            };

            // Campo: Tamaño (según CHECK de BD: Pequeño, Mediano, Grande)
            Label lblTamano = new Label
            {
                Text = "Tamaño:",
                ForeColor = colorTexto,
                Location = new Point(20, 128),
                AutoSize = true
            };
            cmbTamano = new ComboBox
            {
                Location = new Point(20, 153),
                Size = new Size(275, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            cmbTamano.Items.AddRange(new object[] { "Pequeño", "Mediano", "Grande" });
            cmbTamano.SelectedIndex = 0;
            cmbTamano.SelectedIndexChanged += CmbTamano_SelectedIndexChanged;

            // Etiqueta informativa de ración estándar
            lblRacionEstimada = new Label
            {
                Text = "Ración diaria asignada: 150 g (0.15 kg)",
                ForeColor = colorSecundario,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(20, 188),
                AutoSize = true
            };

            // Campo: Fecha de Ingreso
            Label lblFecha = new Label
            {
                Text = "Fecha de Ingreso:",
                ForeColor = colorTexto,
                Location = new Point(20, 222),
                AutoSize = true
            };
            dtpFechaIngreso = new DateTimePicker
            {
                Location = new Point(20, 247),
                Size = new Size(275, 28),
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10F)
            };

            // Botones de acción CRUD
            btnGuardar = CrearBoton("Guardar Registro", colorSecundario, Color.White, new Point(20, 310), new Size(275, 40));
            btnEditar = CrearBoton("Modificar Seleccionado", colorAdvertencia, colorTexto, new Point(20, 360), new Size(275, 38));
            btnEliminar = CrearBoton("Dar de Baja / Eliminar", colorPeligro, Color.White, new Point(20, 408), new Size(275, 38));
            btnLimpiar = CrearBoton("Limpiar Campos", Color.FromArgb(224, 230, 237), colorTexto, new Point(20, 456), new Size(275, 35));

            btnLimpiar.Click += (s, e) => LimpiarFormulario();

            pnlFormulario.Controls.AddRange(new Control[] {
                lblNombre, txtNombrePerro,
                lblTamano, cmbTamano, lblRacionEstimada,
                lblFecha, dtpFechaIngreso,
                btnGuardar, btnEditar, btnEliminar, btnLimpiar
            });

            // ==========================================
            // PANEL DERECHO: TABLA DE PERROS REGISTRADOS
            // ==========================================
            Panel pnlTabla = new Panel
            {
                BackColor = Color.White,
                Location = new Point(365, 65),
                Size = new Size(530, 540),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            dgvPerros = new DataGridView
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

            // Estilo de cabecera azul petróleo
            dgvPerros.ColumnHeadersDefaultCellStyle.BackColor = colorPrimario;
            dgvPerros.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvPerros.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvPerros.ColumnHeadersHeight = 40;

            // Estilo de filas alternas
            dgvPerros.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvPerros.DefaultCellStyle.SelectionBackColor = Color.FromArgb(214, 234, 248);
            dgvPerros.DefaultCellStyle.SelectionForeColor = colorTexto;

            // Columnas alineadas a la tabla Perro de MySQL
            dgvPerros.Columns.Add("id_perro", "ID");
            dgvPerros.Columns.Add("nombre", "Nombre");
            dgvPerros.Columns.Add("tamano", "Tamaño");
            dgvPerros.Columns.Add("racion", "Ración (g)");
            dgvPerros.Columns.Add("fecha_ingreso", "Fecha Ingreso");

            dgvPerros.Columns["id_perro"].FillWeight = 12;

            // Evento visual al hacer clic en una fila para pasar datos a los TextBox
            dgvPerros.CellClick += DgvPerros_CellClick;

            pnlTabla.Controls.Add(dgvPerros);

            this.Controls.Add(pnlFormulario);
            this.Controls.Add(pnlTabla);
        }

        private Button CrearBoton(string texto, Color fondo, Color textoColor, Point ubicacion, Size tamano)
        {
            Button btn = new Button
            {
                Text = texto,
                BackColor = fondo,
                ForeColor = textoColor,
                Location = ubicacion,
                Size = tamano,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void CmbTamano_SelectedIndexChanged(object? sender, EventArgs e)
        {
            switch (cmbTamano.SelectedItem?.ToString())
            {
                case "Pequeño":
                    lblRacionEstimada.Text = "Ración diaria asignada: 150 g (0.15 kg)";
                    break;
                case "Mediano":
                    lblRacionEstimada.Text = "Ración diaria asignada: 300 g (0.30 kg)";
                    break;
                case "Grande":
                    lblRacionEstimada.Text = "Ración diaria asignada: 500 g (0.50 kg)";
                    break;
            }
        }

        private void DgvPerros_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow fila = dgvPerros.Rows[e.RowIndex];
                txtNombrePerro.Text = fila.Cells["nombre"].Value?.ToString();
                cmbTamano.SelectedItem = fila.Cells["tamano"].Value?.ToString();
                if (DateTime.TryParse(fila.Cells["fecha_ingreso"].Value?.ToString(), out DateTime fecha))
                {
                    dtpFechaIngreso.Value = fecha;
                }
            }
        }

        public void LimpiarFormulario()
        {
            txtNombrePerro.Clear();
            cmbTamano.SelectedIndex = 0;
            dtpFechaIngreso.Value = DateTime.Now;
            txtNombrePerro.Focus();
        }

        private void CargarDatosEjemploVisual()
        {
            // Filas de muestra para probar el diseño (borrar cuando se conecte MySQL)
            dgvPerros.Rows.Add("1", "Toby", "Pequeño", "150 g", "10/11/2026");
            dgvPerros.Rows.Add("2", "Canela", "Mediano", "300 g", "12/11/2026");
            dgvPerros.Rows.Add("3", "Max", "Grande", "500 g", "15/11/2026");
        }
        private void ConectarEventos()
        {
            btnGuardar.Click += BtnGuardar_Click;
            btnEditar.Click += BtnEditar_Click;
            btnEliminar.Click += BtnEliminar_Click;
        }

        private static string RacionTexto(string tamano) => tamano switch
        {
            "Pequeño" => "150 g",
            "Mediano" => "300 g",
            "Grande" => "500 g",
            _ => ""
        };

        public void CargarPerros()
        {
            var lista = PerroService.Listar(out string error);
            if (error != "")
            {
                MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            dgvPerros.Rows.Clear();
            foreach (var p in lista)
            {
                dgvPerros.Rows.Add(p.IdPerro, p.Nombre, p.Tamano, RacionTexto(p.Tamano),
                                   p.FechaIngreso.ToString("dd/MM/yyyy"));
            }
        }

        private int? IdSeleccionado()
        {
            var valor = dgvPerros.CurrentRow?.Cells["id_perro"].Value;
            return valor == null ? null : Convert.ToInt32(valor);
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            string tamano = cmbTamano.SelectedItem?.ToString() ?? "";
            if (!PerroService.Registrar(txtNombrePerro.Text, tamano, dtpFechaIngreso.Value, out string error))
            {
                MessageBox.Show(error, "No se pudo guardar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            MessageBox.Show("Perro registrado correctamente.", "Listo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LimpiarFormulario();
            CargarPerros();
        }

        private void BtnEditar_Click(object? sender, EventArgs e)
        {
            int? id = IdSeleccionado();
            if (id == null)
            {
                MessageBox.Show("Seleccione un perro de la tabla.", "Modificar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string tamano = cmbTamano.SelectedItem?.ToString() ?? "";
            if (!PerroService.Modificar(id.Value, txtNombrePerro.Text, tamano, dtpFechaIngreso.Value, out string error))
            {
                MessageBox.Show(error, "No se pudo modificar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            MessageBox.Show("Datos actualizados.", "Listo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LimpiarFormulario();
            CargarPerros();
        }

        private void BtnEliminar_Click(object? sender, EventArgs e)
        {
            int? id = IdSeleccionado();
            if (id == null)
            {
                MessageBox.Show("Seleccione un perro de la tabla.", "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var respuesta = MessageBox.Show("¿Eliminar este perro? Esta acción no se puede deshacer.",
                                            "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (respuesta != DialogResult.Yes) return;

            if (!PerroService.Eliminar(id.Value, out string error))
            {
                MessageBox.Show(error, "No se pudo eliminar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            LimpiarFormulario();
            CargarPerros();
        }
    }
}