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

        // Controles públicos
        public TextBox txtNombrePerro = null!;
        public NumericUpDown nudEdad = null!;
        public ComboBox cmbUnidadEdad = null!;
        public ComboBox cmbTamano = null!;
        public DateTimePicker dtpFechaIngreso = null!;
        public Label lblRacionEstimada = null!;

        public Button btnGuardar = null!;
        public Button btnEditar = null!;
        public Button btnEliminar = null!;
        public Button btnLimpiar = null!;

        public DataGridView dgvPerros = null!;

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
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(920, 650);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
        }

        private void InicializarComponentes()
        {
            Label lblTituloVista = new Label
            {
                Text = "Gestión de Población Canina",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = colorPrimario,
                Location = new Point(25, 20),
                AutoSize = true
            };
            this.Controls.Add(lblTituloVista);

            // PANEL IZQUIERDO
            Panel pnlFormulario = new Panel
            {
                BackColor = Color.White,
                Location = new Point(25, 65),
                Size = new Size(320, 540),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            };

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

            Label lblEdad = new Label
            {
                Text = "Edad:",
                ForeColor = colorTexto,
                Location = new Point(20, 128),
                AutoSize = true
            };
            nudEdad = new NumericUpDown
            {
                Location = new Point(20, 153),
                Size = new Size(130, 28),
                Minimum = 0,
                Maximum = 30,
                Value = 1,
                Font = new Font("Segoe UI", 10F)
            };
            cmbUnidadEdad = new ComboBox
            {
                Location = new Point(160, 153),
                Size = new Size(135, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            cmbUnidadEdad.Items.AddRange(new object[] { "Años", "Meses" });
            cmbUnidadEdad.SelectedIndex = 0;

            Label lblTamano = new Label
            {
                Text = "Tamaño:",
                ForeColor = colorTexto,
                Location = new Point(20, 196),
                AutoSize = true
            };
            cmbTamano = new ComboBox
            {
                Location = new Point(20, 221),
                Size = new Size(275, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            cmbTamano.Items.AddRange(new object[] { "Pequeño", "Mediano", "Grande" });
            cmbTamano.SelectedIndex = 0;
            cmbTamano.SelectedIndexChanged += CmbTamano_SelectedIndexChanged;

            lblRacionEstimada = new Label
            {
                Text = "Ración diaria asignada: 150 g (0.15 kg)",
                ForeColor = colorSecundario,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(20, 256),
                AutoSize = true
            };

            Label lblFecha = new Label
            {
                Text = "Fecha de Ingreso:",
                ForeColor = colorTexto,
                Location = new Point(20, 286),
                AutoSize = true
            };
            dtpFechaIngreso = new DateTimePicker
            {
                Location = new Point(20, 311),
                Size = new Size(275, 28),
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10F)
            };

            btnGuardar = CrearBoton("Guardar Registro", colorSecundario, Color.White, new Point(20, 360), new Size(275, 40));
            btnEditar = CrearBoton("Modificar Seleccionado", colorAdvertencia, colorTexto, new Point(20, 408), new Size(275, 38));
            btnEliminar = CrearBoton("Dar de Baja / Eliminar", colorPeligro, Color.White, new Point(20, 454), new Size(275, 38));
            btnLimpiar = CrearBoton("Limpiar Campos", Color.FromArgb(224, 230, 237), colorTexto, new Point(20, 500), new Size(275, 30));

            btnLimpiar.Click += (s, e) => LimpiarFormulario();

            pnlFormulario.Controls.AddRange(new Control[] {
                lblNombre, txtNombrePerro,
                lblEdad, nudEdad, cmbUnidadEdad,
                lblTamano, cmbTamano, lblRacionEstimada,
                lblFecha, dtpFechaIngreso,
                btnGuardar, btnEditar, btnEliminar, btnLimpiar
            });

            // PANEL DERECHO
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

            dgvPerros.ColumnHeadersDefaultCellStyle.BackColor = colorPrimario;
            dgvPerros.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvPerros.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvPerros.ColumnHeadersHeight = 40;

            dgvPerros.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvPerros.DefaultCellStyle.SelectionBackColor = Color.FromArgb(214, 234, 248);
            dgvPerros.DefaultCellStyle.SelectionForeColor = colorTexto;

            dgvPerros.Columns.Add("id_perro", "ID");
            dgvPerros.Columns.Add("nombre", "Nombre");
            dgvPerros.Columns.Add("edad", "Edad");
            dgvPerros.Columns.Add("tamano", "Tamaño");
            dgvPerros.Columns.Add("racion", "Ración (g)");
            dgvPerros.Columns.Add("fecha_ingreso", "Fecha Ingreso");

            dgvPerros.Columns["id_perro"].FillWeight = 10;
            dgvPerros.Columns["edad"].FillWeight = 15;

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

        public string ObtenerEdadTexto()
        {
            int valor = (int)nudEdad.Value;
            bool esAnios = cmbUnidadEdad.SelectedItem?.ToString() == "Años";
            if (esAnios) return valor == 1 ? "1 año" : $"{valor} años";
            return valor == 1 ? "1 mes" : $"{valor} meses";
        }

        public int ObtenerEdadEnMeses()
        {
            int valor = (int)nudEdad.Value;
            return cmbUnidadEdad.SelectedItem?.ToString() == "Años" ? valor * 12 : valor;
        }

        private void DgvPerros_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow fila = dgvPerros.Rows[e.RowIndex];
            txtNombrePerro.Text = fila.Cells["nombre"].Value?.ToString() ?? string.Empty;

            var partes = fila.Cells["edad"].Value?.ToString()?.Split(' ');
            if (partes != null && partes.Length == 2 && int.TryParse(partes[0], out int edad))
            {
                nudEdad.Value = Math.Min(edad, nudEdad.Maximum);
                cmbUnidadEdad.SelectedIndex = partes[1].StartsWith("mes", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            }

            cmbTamano.SelectedItem = fila.Cells["tamano"].Value?.ToString();

            if (DateTime.TryParse(fila.Cells["fecha_ingreso"].Value?.ToString(), out DateTime fecha))
            {
                dtpFechaIngreso.Value = fecha;
            }
        }

        public void LimpiarFormulario()
        {
            txtNombrePerro.Clear();
            nudEdad.Value = 1;
            cmbUnidadEdad.SelectedIndex = 0;
            cmbTamano.SelectedIndex = 0;
            dtpFechaIngreso.Value = DateTime.Now;
            txtNombrePerro.Focus();
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
                // Muestra "Pendiente BD" mientras tus compañeros agregan la columna en MySQL
                dgvPerros.Rows.Add(p.IdPerro, p.Nombre, "Pendiente BD", p.Tamano, RacionTexto(p.Tamano),
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
