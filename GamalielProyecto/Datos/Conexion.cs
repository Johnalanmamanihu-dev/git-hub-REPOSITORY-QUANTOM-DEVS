using MySqlConnector;

namespace RefugioGamaliel.Datos;

public static class Conexion
{
    private static readonly string cadena = new MySqlConnectionStringBuilder
    {
        Server = "mysql-48065c8-unifranz-c583.k.aivencloud.com",
        Port = 14243,
        UserID = "avnadmin",
        Password = Secretos.Password,
        Database = "albergue_db",
        SslMode = MySqlSslMode.Required
    }.ConnectionString;

    public static MySqlConnection ObtenerConexion() => new MySqlConnection(cadena);

    public static bool Probar(out string error)
    {
        try
        {
            using var con = ObtenerConexion();
            con.Open();
            error = "";
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
}