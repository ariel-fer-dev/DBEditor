using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBEditor {
    public partial class Form1 : Form {
        public Form1() {

            InitializeComponent();

            chekearRecientes();
            registrosCount.Text = "0";

        }

        private void chekearRecientes() {
            try {
                if (!File.Exists(Environment.CurrentDirectory + "\\reciente")) {
                    File.Create(Environment.CurrentDirectory + "\\reciente");
                    Console.WriteLine("Archivo \"recientes\" creado!!");
                } else {
                    //cbRecientes.Items.AddRange(File.ReadAllLines(Environment.CurrentDirectory + "\\reciente"));
                    var recientes = File.ReadAllLines(Environment.CurrentDirectory + "\\reciente").ToList();
                    //recientes.ForEach(x => Console.WriteLine(!File.Exists(x)));
                    recientes.RemoveAll(x => !File.Exists(x));
                    cbRecientes.Items.AddRange(recientes.ToArray());
                }
            } catch (Exception e) {
                System.Windows.Forms.MessageBox.Show("Error " + Environment.NewLine +
               "Exepción: " + e.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK,
               System.Windows.Forms.MessageBoxIcon.Exclamation);
            }
        }

        private async void btSource_Click(object sender, EventArgs e) { //Botón abrir

            if (textBox1.TextLength > 0) {

                try {
                    await conexionsSQLite.testDeConexion(textBox1.Text);
                    comboBox1.Items.AddRange(conexionsSQLite.tablasNombre(textBox1.Text).Result.ToArray());
                    comboBox1.SelectedIndex = 0;
                    guardarPath(textBox1.Text);
                    registrosCount.Text = Convert.ToString(conexionsSQLite.cantidadFilas(textBox1.Text, comboBox1.Text).Result);
                    MessageBox.Show("tabla: " + comboBox1.Text);

                } catch (Exception) {
                    textBox1.Text = "";
                }
            } else {

                textBox1.Enabled = true;
                comboBox1.Items.Clear();
                comboBox1.Text = "";
                dataGridView1.DataSource = "";
                button3.Enabled = false;
                //button4.Enabled = false;

                if (cbRecientes.Items.Count > 0) {

                    // Busco la ruta al ultimo archivo abierto y lo establezco como inicial de "openFileDialog"
                    FileInfo reciente = new FileInfo(cbRecientes.Items[0].ToString());
                    openFileDialog1.InitialDirectory = reciente.DirectoryName;
                    //Console.WriteLine("Ruta: " + reciente.DirectoryName);
                }

                //openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                openFileDialog1.FileName = "";
                openFileDialog1.Filter = "SQLite (*.db; *.sqlite3; *.sqlite; *db3)|*.db; *.sqlite3; *.sqlite; *.db3|Cualquier archivo (*.*)|*.*"; //Filtro del openFileDialog

                if (openFileDialog1.ShowDialog() == DialogResult.OK) { //Si se selecciono un archivo 
                    try {
                        textBox1.Text = openFileDialog1.FileName;
                        await conexionsSQLite.testDeConexion(textBox1.Text);
                        comboBox1.Items.AddRange(conexionsSQLite.tablasNombre(textBox1.Text).Result.ToArray());
                        comboBox1.SelectedIndex = 0;
                        guardarPath(textBox1.Text);
                        registrosCount.Text = Convert.ToString(conexionsSQLite.cantidadFilas(textBox1.Text, comboBox1.Text).Result);
                    } catch (Exception ex) {
                        MessageBox.Show("Excepcion:" + Environment.NewLine + ex.Message, " Error");
                        textBox1.Text = "";
                    }
                    textBox1.Enabled = false;
                }
            }
        }

        private async void guardarPath(string path) {  //Guardo los paths de los últimos 5 archivos abiertos en un archivo

            try {
                cbRecientes.Items.Clear();

                var recientes = File.ReadAllLines(Environment.CurrentDirectory + "\\reciente").ToList();

                var lista = new LinkedList<string>(recientes);

                //if (lista.Contains(path)) {
                //    lista.Remove(path);
                //}
                lista.AddFirst(path);

                if (lista.Count > 7) lista.RemoveLast();

                var items = from item in lista
                            group item.First() by item into g
                            where File.Exists(g.Key)
                            select g.Key;

                await Task.Run(() => {
                    File.Delete(Environment.CurrentDirectory + "\\reciente");
                    File.WriteAllLines(Environment.CurrentDirectory + "\\rec", items.ToArray());
                    File.Move(Environment.CurrentDirectory + "\\rec", Environment.CurrentDirectory + "\\reciente");
                });
                cbRecientes.Items.AddRange(File.ReadAllLines(Environment.CurrentDirectory + "\\reciente"));

            } catch (Exception) {
            }
        }

        private async void button2_Click(object sender, EventArgs e) { 
          
            if (!textBox1.Text.Equals("")) {
                dataGridView1.Columns.Clear();

                DataTable source;
                source = await conexionsSQLite.getDatos(textBox1.Text, comboBox1.Text);

                dataGridView1.DataSource = source;
                labelImagen.Visible = false;
                dataGridView1.ReadOnly = true;
                button1.Enabled = true;
                registrosCount.Text = Convert.ToString(conexionsSQLite.cantidadFilas(textBox1.Text, comboBox1.Text).Result);
            }
        } //Boton actualizar datos del DGV segun la tabla de la DB seleccionada

        private async void button3_Click(object sender, EventArgs e) { 

            DataTable dt = (DataTable)dataGridView1.DataSource;
            await conexionsSQLite.guardarCambios(textBox1.Text, comboBox1.Text, dt);
            button3.Enabled = false;
            button4.Enabled = false;
            dataGridView1.ReadOnly = true;
            button2.PerformClick();
        } //Boton guardar modificaciones

        private void button4_Click(object sender, EventArgs e) { 

              DialogResult select =  MessageBox.Show("La fila será borrada, seguro?", "Aviso", MessageBoxButtons.YesNo, MessageBoxIcon.Question );
              if (select == DialogResult.Yes)  dataGridView1.Rows.RemoveAt(dataGridView1.CurrentCell.RowIndex);
       
            dataGridView1.Refresh();
            dataGridView1.Parent.Refresh();
        } //Botón borrar fila

        private void button5_Click(object sender, EventArgs e) { 

            if (dataGridView1.Rows.Count > 0) {
                button3.Enabled = true;
                button4.Enabled = true;
                dataGridView1.ReadOnly = false;
                dataGridView1.Columns[0].ReadOnly = true;
            }
        } //Botón habilitar edición del DGV

        private void button1_Click(object sender, EventArgs e) { //Botón cerrar DB

            textBox1.Enabled = true;
            textBox1.Text = "";
            textBox1.Focus();
            comboBox1.Items.Clear();
            comboBox1.Text = "";
            dataGridView1.DataSource = "";
            dataGridView1.Refresh();
            button3.Enabled = false;
            button4.Enabled = false;
            button1.Enabled = false;
            labelImagen.Visible = true;
            registrosCount.Text = "0";

        }  //Botón cerrar DB

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) { //Evento en combo nombre de Tablas
            button2.PerformClick();
        }

        private async void Form1_DragDrop(object sender, DragEventArgs e) { // Evento arrastrar y soltar dentro del Form

            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0 && !files[0].StartsWith("\\")) {
                textBox1.Text = files[0];

                if (textBox1.Text.Length > 0) {
                    textBox1.Enabled = false;
                    try {
                        await conexionsSQLite.testDeConexion(textBox1.Text);
                        comboBox1.Items.Clear();
                        comboBox1.Items.AddRange(conexionsSQLite.tablasNombre(textBox1.Text).Result.ToArray());
                        comboBox1.SelectedIndex = 0;
                        button2.PerformClick();
                        guardarPath(textBox1.Text);
                    } catch (Exception ex) {
                        MessageBox.Show("Excepcion:" + Environment.NewLine + ex.Message, " Error");
                    }
                }
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e) { //Eventro arrastrar esta dentro del Form

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            //Console.WriteLine(files[0]);

            if (e.Data.GetDataPresent(DataFormats.FileDrop) && files[0].EndsWith(".db")) {
                e.Effect = DragDropEffects.Copy;
            } else {
                e.Effect = DragDropEffects.None;
            }
        }

        private void cbRecientes_SelectedIndexChanged(object sender, EventArgs e) {

            button1.PerformClick();
            textBox1.Text = cbRecientes.Text;
            textBox1.Enabled = false;

            if (!File.Exists(textBox1.Text)) {
                MessageBox.Show("Error " + Environment.NewLine +
               "El archivo ya no se encuentra ahí, revisa la ruta", "Error", MessageBoxButtons.OK,
               MessageBoxIcon.Exclamation);

                cbRecientes.Items.Clear();
                //chekearRecientes();
                return;
            }

            comboBox1.Items.AddRange(conexionsSQLite.tablasNombre(textBox1.Text).Result.ToArray());
            if (comboBox1.Items.Count > 0) comboBox1.SelectedIndex = 0;
            button2.PerformClick();
            cbRecientes.Text = "";
            guardarPath(textBox1.Text);
            registrosCount.Text = Convert.ToString(conexionsSQLite.cantidadFilas(textBox1.Text, comboBox1.Text).Result);
            //cbRecientes.Items.Clear();
            //cbRecientes.Items.AddRange(File.ReadAllLines(Environment.CurrentDirectory + "\\reciente"));

        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e) {

            DataGridViewCell celda = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];

            dataGridView1.CurrentCell = celda;
            dataGridView1.BeginEdit(true);

        }

        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e) {

            if (dataGridView1.SelectedCells.Count > 0) {
                DataGridViewCell celda = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];

                dataGridView1.CurrentCell = celda;
                dataGridView1.BeginEdit(true);
            }
        }

        private void Form1_Load(object sender, EventArgs e) {

        }
    }
}
