using MySqlConnector;
using RefugioGamaliel.Datos;

namespace RefugioGamaliel.Logica;

public static class DonacionService
{
    // tipoDonante: texto corto, por ejemplo "Persona" o "Empresa".
    public static bool RegistrarDonacion(string nombreDonante, string tipoDonante,
        DateTime fecha, int idAlimento, decimal cantidadKg, out string error)
    {
        error = "";
        var sesion = Sesion.Actual;
        if (sesion == null) { error = "No hay una sesión iniciada."; return false; }
        if (string.IsNullOrWhiteSpace(nombreDonante)) { error = "Escriba el nombre del donante."; return false; }
        if (idAlimento <= 0) { error = "Seleccione un alimento."; return false; }
        if (cantidadKg <= 0) { error = "La cantidad debe ser mayor a 0."; return false; }

        if (string.IsNullOrWhiteSpace(tipoDonante)) tipoDonante = "Persona";

        try
        {
            using var con = Conexion.ObtenerConexion();
            con.Open();
            using var tx = con.BeginTransaction();
            try
            {
                // 1) Buscar el donante; si no existe, crearlo
                int idDonante;
                const string sqlBuscar = @"SELECT id_donante FROM donante
                                           WHERE id_albergue = @alb AND nombre_razon_social = @nombre
                                           LIMIT 1";
                using (var cmdBuscar = new MySqlCommand(sqlBuscar, con, tx))
                {
                    cmdBuscar.Parameters.AddWithValue("@alb", sesion.IdAlbergue);
                    cmdBuscar.Parameters.AddWithValue("@nombre", nombreDonante.Trim());
                    var r = cmdBuscar.ExecuteScalar();
                    if (r != null && r != DBNull.Value)
                    {
                        idDonante = Convert.ToInt32(r);
                    }
                    else
                    {
                        const string sqlNuevo = @"INSERT INTO donante (id_albergue, nombre_razon_social, tipo_donante)
                                                  VALUES (@alb, @nombre, @tipo)";
                        using var cmdNuevo = new MySqlCommand(sqlNuevo, con, tx);
                        cmdNuevo.Parameters.AddWithValue("@alb", sesion.IdAlbergue);
                        cmdNuevo.Parameters.AddWithValue("@nombre", nombreDonante.Trim());
                        cmdNuevo.Parameters.AddWithValue("@tipo", tipoDonante.Trim());
                        cmdNuevo.ExecuteNonQuery();
                        idDonante = (int)cmdNuevo.LastInsertedId;
                    }
                }

                // 2) Registrar la donación
                const string sqlDon = @"INSERT INTO donacion (id_donante, id_alimento, id_usuario, fecha_donacion, cantidad_donada_kg)
                                        VALUES (@don, @ali, @usr, @fecha, @cant)";
                using (var cmdDon = new MySqlCommand(sqlDon, con, tx))
                {
                    cmdDon.Parameters.AddWithValue("@don", idDonante);
                    cmdDon.Parameters.AddWithValue("@ali", idAlimento);
                    cmdDon.Parameters.AddWithValue("@usr", sesion.IdUsuario);
                    cmdDon.Parameters.AddWithValue("@fecha", fecha.Date);
                    cmdDon.Parameters.AddWithValue("@cant", cantidadKg);
                    cmdDon.ExecuteNonQuery();
                }

                // 3) Sumar al stock del alimento
                const string sqlStock = @"UPDATE alimento SET stock_kg = stock_kg + @cant
                                          WHERE id_alimento = @ali AND id_albergue = @alb";
                using (var cmdStock = new MySqlCommand(sqlStock, con, tx))
                {
                    cmdStock.Parameters.AddWithValue("@cant", cantidadKg);
                    cmdStock.Parameters.AddWithValue("@ali", idAlimento);
                    cmdStock.Parameters.AddWithValue("@alb", sesion.IdAlbergue);
                    if (cmdStock.ExecuteNonQuery() == 0)
                    {
                        tx.Rollback();
                        error = "El alimento seleccionado no existe.";
                        return false;
                    }
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
            error = "No se pudo registrar la donación: " + ex.Message;
            return false;
        }
    }
}