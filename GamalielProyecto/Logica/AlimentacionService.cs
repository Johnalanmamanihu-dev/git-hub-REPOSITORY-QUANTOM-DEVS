using MySqlConnector;
using RefugioGamaliel.Datos;

namespace RefugioGamaliel.Logica;

public class ConsumoRegistro
{
    public DateTime Fecha { get; set; }
    public string Perro { get; set; } = "";
    public string Alimento { get; set; } = "";
    public decimal CantidadKg { get; set; }
}

public static class AlimentacionService
{
    // Ración diaria ORIENTATIVA en gramos. No sustituye la indicación de un veterinario.
    // kcalPorKg: energía del alimento según su etiqueta. Si es 0 o menor, se usa la referencia (3500).
    public static decimal CalcularRacionGramos(string tamano, decimal kcalPorKg = 3500m)
    {
        decimal baseGramos = tamano switch
        {
            "Pequeño" => 150m,
            "Mediano" => 300m,
            "Grande" => 500m,
            _ => 0m
        };
        if (baseGramos == 0m) return 0m;
        if (kcalPorKg <= 0m) kcalPorKg = 3500m;
        return Math.Round(baseGramos * 3500m / kcalPorKg, 0);
    }

    // Registra lo que comió un perro y descuenta del inventario. cantidadGramos se guarda en kg.
    public static bool RegistrarConsumo(int idPerro, int idAlimento, decimal cantidadGramos,
        DateTime fecha, out string error)
    {
        error = "";
        var sesion = Sesion.Actual;
        if (sesion == null) { error = "No hay una sesión iniciada."; return false; }
        if (idPerro <= 0) { error = "Seleccione un perro."; return false; }
        if (idAlimento <= 0) { error = "Seleccione un alimento."; return false; }
        if (cantidadGramos <= 0) { error = "La cantidad debe ser mayor a 0."; return false; }

        decimal cantidadKg = Math.Round(cantidadGramos / 1000m, 3);

        try
        {
            using var con = Conexion.ObtenerConexion();
            con.Open();
            using var tx = con.BeginTransaction();
            try
            {
                // 1) El perro debe ser de este albergue
                const string sqlPerro = "SELECT COUNT(*) FROM perro WHERE id_perro = @p AND id_albergue = @alb";
                using (var cmdPerro = new MySqlCommand(sqlPerro, con, tx))
                {
                    cmdPerro.Parameters.AddWithValue("@p", idPerro);
                    cmdPerro.Parameters.AddWithValue("@alb", sesion.IdAlbergue);
                    if (Convert.ToInt32(cmdPerro.ExecuteScalar()) == 0)
                    {
                        tx.Rollback();
                        error = "El perro seleccionado no existe.";
                        return false;
                    }
                }

                // 2) Restar del stock sin dejarlo en negativo
                const string sqlStock = @"UPDATE alimento SET stock_kg = stock_kg - @cant
                                          WHERE id_alimento = @ali AND id_albergue = @alb AND stock_kg >= @cant";
                using (var cmdStock = new MySqlCommand(sqlStock, con, tx))
                {
                    cmdStock.Parameters.AddWithValue("@cant", cantidadKg);
                    cmdStock.Parameters.AddWithValue("@ali", idAlimento);
                    cmdStock.Parameters.AddWithValue("@alb", sesion.IdAlbergue);
                    if (cmdStock.ExecuteNonQuery() == 0)
                    {
                        tx.Rollback();
                        error = "No hay existencias suficientes de ese alimento.";
                        return false;
                    }
                }

                // 3) Registrar el consumo; si ya hay uno del mismo día, se suma
                const string sqlBuscar = @"SELECT id_consumo FROM consumo_diario
                                           WHERE id_perro = @p AND id_alimento = @ali AND fecha = @f
                                           LIMIT 1";
                object? existente;
                using (var cmdBuscar = new MySqlCommand(sqlBuscar, con, tx))
                {
                    cmdBuscar.Parameters.AddWithValue("@p", idPerro);
                    cmdBuscar.Parameters.AddWithValue("@ali", idAlimento);
                    cmdBuscar.Parameters.AddWithValue("@f", fecha.Date);
                    existente = cmdBuscar.ExecuteScalar();
                }

                if (existente != null && existente != DBNull.Value)
                {
                    const string sqlSuma = @"UPDATE consumo_diario
                                             SET cantidad_total_kg = cantidad_total_kg + @cant
                                             WHERE id_consumo = @id";
                    using var cmdSuma = new MySqlCommand(sqlSuma, con, tx);
                    cmdSuma.Parameters.AddWithValue("@cant", cantidadKg);
                    cmdSuma.Parameters.AddWithValue("@id", Convert.ToInt32(existente));
                    cmdSuma.ExecuteNonQuery();
                }
                else
                {
                    const string sqlNuevo = @"INSERT INTO consumo_diario (id_perro, id_alimento, fecha, cantidad_total_kg)
                                              VALUES (@p, @ali, @f, @cant)";
                    using var cmdNuevo = new MySqlCommand(sqlNuevo, con, tx);
                    cmdNuevo.Parameters.AddWithValue("@p", idPerro);
                    cmdNuevo.Parameters.AddWithValue("@ali", idAlimento);
                    cmdNuevo.Parameters.AddWithValue("@f", fecha.Date);
                    cmdNuevo.Parameters.AddWithValue("@cant", cantidadKg);
                    cmdNuevo.ExecuteNonQuery();
                }

                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            error = "No se pudo registrar el consumo: " + ex.Message;
            return false;
        }
    }

    // Historial de consumo del albergue, del más reciente al más antiguo.
    public static List<ConsumoRegistro> ListarConsumos(out string error)
    {
        error = "";
        var lista = new List<ConsumoRegistro>();
        var sesion = Sesion.Actual;
        if (sesion == null) { error = "No hay una sesión iniciada."; return lista; }

        try
        {
            using var con = Conexion.ObtenerConexion();
            con.Open();
            const string sql = @"SELECT c.fecha, p.nombre, a.nombre, c.cantidad_total_kg
                                 FROM consumo_diario c
                                 JOIN perro p ON p.id_perro = c.id_perro
                                 JOIN alimento a ON a.id_alimento = c.id_alimento
                                 WHERE p.id_albergue = @alb
                                 ORDER BY c.fecha DESC, c.id_consumo DESC";
            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@alb", sesion.IdAlbergue);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                lista.Add(new ConsumoRegistro
                {
                    Fecha = rd.GetDateTime(0),
                    Perro = rd.GetString(1),
                    Alimento = rd.GetString(2),
                    CantidadKg = rd.GetDecimal(3)
                });
            }
        }
        catch (Exception ex)
        {
            error = "No se pudo cargar el historial: " + ex.Message;
        }
        return lista;
    }

    // Cantidad de perros por tamaño: [0]=Pequeño, [1]=Mediano, [2]=Grande
    public static int[] ContarPerrosPorTamano(out string error)
    {
        error = "";
        var r = new int[3];
        var sesion = Sesion.Actual;
        if (sesion == null) { error = "No hay una sesión iniciada."; return r; }
        try
        {
            using var con = Conexion.ObtenerConexion();
            con.Open();
            const string sql = @"SELECT tamano, COUNT(*) FROM perro
                                 WHERE id_albergue = @alb GROUP BY tamano";
            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@alb", sesion.IdAlbergue);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                int n = Convert.ToInt32(rd.GetValue(1));
                switch (rd.GetString(0))
                {
                    case "Pequeño": r[0] = n; break;
                    case "Mediano": r[1] = n; break;
                    case "Grande": r[2] = n; break;
                }
            }
        }
        catch (Exception ex) { error = "No se pudo contar los perros: " + ex.Message; }
        return r;
    }

    public static decimal StockTotalKg(out string error)
    {
        error = "";
        var sesion = Sesion.Actual;
        if (sesion == null) { error = "No hay una sesión iniciada."; return 0m; }
        try
        {
            using var con = Conexion.ObtenerConexion();
            con.Open();
            const string sql = "SELECT COALESCE(SUM(stock_kg), 0) FROM alimento WHERE id_albergue = @alb";
            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@alb", sesion.IdAlbergue);
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }
        catch (Exception ex) { error = "No se pudo leer el stock: " + ex.Message; return 0m; }
    }
}