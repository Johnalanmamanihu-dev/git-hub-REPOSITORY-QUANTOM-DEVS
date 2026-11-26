using MySqlConnector;
using RefugioGamaliel.Datos;

namespace RefugioGamaliel.Logica;

public class Perro
{
    public int IdPerro { get; set; }
    public string Nombre { get; set; } = "";
    public string Tamano { get; set; } = "";
    public DateTime FechaIngreso { get; set; }
}

public static class PerroService
{
    public static bool Registrar(string nombre, string tamano, DateTime fechaIngreso, out string error)
    {
        error = "";
        var sesion = Sesion.Actual;
        if (sesion == null) { error = "No hay una sesión iniciada."; return false; }
        if (string.IsNullOrWhiteSpace(nombre)) { error = "Escriba el nombre del perro."; return false; }

        try
        {
            using var con = Conexion.ObtenerConexion();
            con.Open();
            const string sql = @"INSERT INTO perro (id_albergue, nombre, tamano, fecha_ingreso)
                                 VALUES (@alb, @nombre, @tamano, @fecha)";
            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@alb", sesion.IdAlbergue);
            cmd.Parameters.AddWithValue("@nombre", nombre.Trim());
            cmd.Parameters.AddWithValue("@tamano", tamano);
            cmd.Parameters.AddWithValue("@fecha", fechaIngreso.Date);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception ex)
        {
            error = "No se pudo guardar: " + ex.Message;
            return false;
        }
    }

    public static List<Perro> Listar(out string error)
    {
        error = "";
        var lista = new List<Perro>();
        var sesion = Sesion.Actual;
        if (sesion == null) { error = "No hay una sesión iniciada."; return lista; }

        try
        {
            using var con = Conexion.ObtenerConexion();
            con.Open();
            const string sql = @"SELECT id_perro, nombre, tamano, fecha_ingreso
                                 FROM perro
                                 WHERE id_albergue = @alb
                                 ORDER BY id_perro";
            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@alb", sesion.IdAlbergue);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                lista.Add(new Perro
                {
                    IdPerro = rd.GetInt32(0),
                    Nombre = rd.GetString(1),
                    Tamano = rd.GetString(2),
                    FechaIngreso = rd.GetDateTime(3)
                });
            }
        }
        catch (Exception ex)
        {
            error = "No se pudo cargar la lista: " + ex.Message;
        }
        return lista;
    }

    public static bool Modificar(int idPerro, string nombre, string tamano, DateTime fechaIngreso, out string error)
    {
        error = "";
        var sesion = Sesion.Actual;
        if (sesion == null) { error = "No hay una sesión iniciada."; return false; }
        if (string.IsNullOrWhiteSpace(nombre)) { error = "Escriba el nombre del perro."; return false; }

        try
        {
            using var con = Conexion.ObtenerConexion();
            con.Open();
            const string sql = @"UPDATE perro
                                 SET nombre = @nombre, tamano = @tamano, fecha_ingreso = @fecha
                                 WHERE id_perro = @id AND id_albergue = @alb";
            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@nombre", nombre.Trim());
            cmd.Parameters.AddWithValue("@tamano", tamano);
            cmd.Parameters.AddWithValue("@fecha", fechaIngreso.Date);
            cmd.Parameters.AddWithValue("@id", idPerro);
            cmd.Parameters.AddWithValue("@alb", sesion.IdAlbergue);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception ex)
        {
            error = "No se pudo modificar: " + ex.Message;
            return false;
        }
    }

    public static bool Eliminar(int idPerro, out string error)
    {
        error = "";
        var sesion = Sesion.Actual;
        if (sesion == null) { error = "No hay una sesión iniciada."; return false; }

        try
        {
            using var con = Conexion.ObtenerConexion();
            con.Open();
            const string sql = "DELETE FROM perro WHERE id_perro = @id AND id_albergue = @alb";
            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@id", idPerro);
            cmd.Parameters.AddWithValue("@alb", sesion.IdAlbergue);
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (MySqlException ex) when (ex.Number == 1451)
        {
            error = "No se puede eliminar: el perro tiene consumos de alimento registrados.";
            return false;
        }
        catch (Exception ex)
        {
            error = "No se pudo eliminar: " + ex.Message;
            return false;
        }
    }
}