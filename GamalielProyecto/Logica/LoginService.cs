using MySqlConnector;
using RefugioGamaliel.Datos;

namespace RefugioGamaliel.Logica;

public class UsuarioSesion
{
    public int IdUsuario { get; set; }
    public int IdAlbergue { get; set; }
    public string NombreCompleto { get; set; } = "";
    public string Rol { get; set; } = "";
}

public static class Sesion
{
    public static UsuarioSesion? Actual { get; set; }
}

public static class LoginService
{
    public static UsuarioSesion? Validar(string correo, string password, out string error)
    {
        error = "";
        try
        {
            using var con = Conexion.ObtenerConexion();
            con.Open();

            const string sql = @"SELECT id_usuario, id_albergue, nombre_completo, password_hash, rol
                                 FROM usuario
                                 WHERE correo = @correo AND activo = 1
                                 LIMIT 1";

            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@correo", correo.Trim());

            using var rd = cmd.ExecuteReader();
            if (!rd.Read() || !BCrypt.Net.BCrypt.Verify(password, rd.GetString(3)))
            {
                error = "Correo o contraseña incorrectos.";
                return null;
            }

            return new UsuarioSesion
            {
                IdUsuario = rd.GetInt32(0),
                IdAlbergue = rd.GetInt32(1),
                NombreCompleto = rd.GetString(2),
                Rol = rd.GetString(4)
            };
        }
        catch (Exception ex)
        {
            error = "No se pudo conectar con la base de datos: " + ex.Message;
            return null;
        }
    }
}