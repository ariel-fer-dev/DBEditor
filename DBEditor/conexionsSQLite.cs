using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace DBEditor {
    class conexionsSQLite {

        public static async Task testDeConexion(string dbNombre) {
            dbNombre = "Data Source=" + dbNombre;

            using (var conexion = new SQLiteConnection(dbNombre)) {
                try {
                    await conexion.OpenAsync();
                    Console.WriteLine("Conexion exitosa con " + dbNombre);
                } catch (Exception) {
                    System.Windows.Forms.MessageBox.Show("La ruta es erronea" + Environment.NewLine,
                "Error", System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Error);
                } finally {
                    conexion.Close();
                }
            }
        }

        public static async Task<Int32> cantidadFilas(string dbNombre, string tabla) {
            dbNombre = "Data Source=" + dbNombre;
            string sql = "SELECT count(*) FROM '" + tabla + "';";
            int filas = 0;

            using (var con = new SQLiteConnection(dbNombre)) {
                await con.OpenAsync();
                var comand = new SQLiteCommand(sql, con);

                var reader = await comand.ExecuteReaderAsync();

                while (reader.Read()) {
                    filas = reader.GetInt32(0);
                }
            }
            return filas;
        }

        public static async Task<List<string>> tablasNombre(string dbNombre) {

            var tablas = new List<string>();
            dbNombre = "Data Source=" + dbNombre;
            string sql = "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY name; ";

            using (var con = new SQLiteConnection(dbNombre)) {
                await con.OpenAsync();
                using (var command = new SQLiteCommand(sql, con)) {
                    using (var reader = await command.ExecuteReaderAsync()) {
                        if (reader.HasRows) {
                            while (reader.Read()) {
                                if (!reader[0].ToString().Equals("android_metadata") && 
                                    !reader[0].ToString().Equals("sqlite_sequence")) {
                                    tablas.Add(reader[0].ToString());
                                }
                            }
                        }
                    }
                }
                con.Close();
            }
            return tablas;
        }

        public static async Task<DataTable> getDatos(string dbNombre, string tabla) {
            var dt = new DataTable();
            dbNombre = "Data Source=" + dbNombre;
            string sql = "SELECT * FROM '" + tabla + "'; ";

            try {
                using (var con = new SQLiteConnection(dbNombre)) {
                    await con.OpenAsync();
                    using (var command = new SQLiteCommand(sql, con)) {
                        using (var da = new SQLiteDataAdapter()) {
                            da.SelectCommand = command;
                            da.Fill(dt);
                        }
                    }
                    con.Close();
                }
            } catch (Exception ex) {
                System.Windows.Forms.MessageBox.Show("Excepcion al obtener datos de la tabla \"" + tabla + "\"" +
                    Environment.NewLine + ex.Message, "Error SQLite", System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Stop);
            }

            return dt;
        }

        public static async Task guardarCambios(string dbNombre, string tabla, DataTable dt) {
            dbNombre = "Data Source=" + dbNombre;

            try {
                using (var con = new SQLiteConnection(dbNombre)) {
                    await con.OpenAsync();
                    var cmd = new SQLiteCommand();

                    cmd = con.CreateCommand();
                    cmd.CommandText = string.Format("SELECT * FROM {0}", tabla);
                    var adapter = new SQLiteDataAdapter(cmd);
                    SQLiteCommandBuilder builder = new SQLiteCommandBuilder(adapter);

                    builder.ConflictOption = ConflictOption.OverwriteChanges;

                    adapter.Update(dt);
                    con.Close();

                    System.Windows.Forms.MessageBox.Show("Los datos se almacenaron satisfactoriamente", "Aviso",
                               System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Information);
                    adapter.Dispose();
                    cmd.Dispose();
                }
            } catch (SQLiteException ex) {
                System.Windows.Forms.MessageBox.Show("Error al guardar los datos" + Environment.NewLine +
                 "Exepción: " + ex.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK,
                 System.Windows.Forms.MessageBoxIcon.Error);
            } catch (Exception ex) {
                System.Windows.Forms.MessageBox.Show("Error al guardar los datos" + Environment.NewLine +
                "Exepción: " + ex.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }
}
