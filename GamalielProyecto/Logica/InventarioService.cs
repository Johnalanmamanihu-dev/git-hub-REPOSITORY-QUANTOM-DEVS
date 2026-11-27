using MySqlConnector;
using RefugioGamaliel.Datos;

namespace RefugioGamaliel.Logica;

public class ItemInventario
{
    public int IdAlimento { get; set; }
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public decimal StockKg { get; set; }
    public string UnidadMedida { get; set; } = "kg";
}

public static class InventarioService
{
    public static List<ItemInventario> ListarAlimentos(out string error)
    {
        error = "";
        var lista = new List<ItemInventario>();
        var sesion = Sesion.Actual;
        if (sesion == null) { error = "No hay una sesión iniciada."; return lista; }

        try
        {
            using var con = Conexion.ObtenerConexion();
            con.Open();
            const string sql = @"SELECT id_alimento, nombre, descripcion, stock_kg, unidad_medida
                                 FROM alimento
                                 WHERE id_albergue = @alb
                                 ORDER BY nombre";
            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@alb", sesion.IdAlbergue);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                lista.Add(new ItemInventario
                {
                    IdAlimento = rd.GetInt32(0),
                    Nombre = rd.GetString(1),
                    Descripcion = rd.IsDBNull(2) ? "" : rd.GetString(2),
                    StockKg = rd.GetDecimal(3),
                    UnidadMedida = rd.GetString(4)
                });
            }
        }
        catch (Exception ex)
        {
            error = "No se pudo cargar el inventario: " + ex.Message;
        }
        return lista;
    }

    public static bool RegistrarAlimento(string nombre, string descripcion, string unidadMedida, out string error)
    {
        error = "";
        var sesion = Sesion.Actual;
        if (sesion == null) { error = "No hay una sesión iniciada."; return false; }
        if (string.IsNullOrWhiteSpace(nombre)) { error = "Escriba el nombre del alimento."; return false; }

        try
        {
            using var con = Conexion.ObtenerConexion();
            con.Open();
            const string sql = @"INSERT INTO alimento (id_albergue, nombre, descripcion, stock_kg, unidad_medida)
                                 VALUES (@alb, @nombre, @desc, 0, @unidad)";
            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@alb", sesion.IdAlbergue);
            cmd.Parameters.AddWithValue("@nombre", nombre.Trim());
            cmd.Parameters.AddWithValue("@desc",
                string.IsNullOrWhiteSpace(descripcion) ? DBNull.Value : descripcion.Trim());
            cmd.Parameters.AddWithValue("@unidad",
                string.IsNullOrWhiteSpace(unidadMedida) ? "kg" : unidadMedida.Trim());
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception ex)
        {
            error = "No se pudo registrar el alimento: " + ex.Message;
            return false;
        }
    }

    // Salida de alimento (resta del stock). Nunca deja el stock en negativo.
    public static bool RegistrarSalida(int idAlimento, decimal cantidadKg, out string error)
    {
        error = "";
        var sesion = Sesion.Actual;
        if (sesion == null) { error = "No hay una sesión iniciada."; return false; }
        if (cantidadKg <= 0) { error = "La cantidad debe ser mayor a 0."; return false; }

        try
        {
            using var con = Conexion.ObtenerConexion();
            con.Open();
            const string sql = @"UPDATE alimento
                                 SET stock_kg = stock_kg - @cant
                                 WHERE id_alimento = @id AND id_albergue = @alb AND stock_kg >= @cant";
            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@cant", cantidadKg);
            cmd.Parameters.AddWithValue("@id", idAlimento);
            cmd.Parameters.AddWithValue("@alb", sesion.IdAlbergue);
            if (cmd.ExecuteNonQuery() == 0)
            {
                error = "No hay existencias suficientes (el stock no puede quedar negativo).";
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            error = "No se pudo registrar la salida: " + ex.Message;
            return false;
        }
    }
}
}