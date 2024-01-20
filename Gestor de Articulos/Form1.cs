using Gestor_de_Articulos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using dominio;
using negocio;
using Negocio;
using System.Xml.Linq;

namespace winform
{
    public partial class Form1 : Form
    {
        private List<Articulo> listaArticulos;
        public Form1()
        {
            InitializeComponent();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Evento Click del botón Eliminar
            ArticulosData data = new ArticulosData();
            Articulo seleccionado;
            try
            {
                DialogResult respuesta = MessageBox.Show("¿De verdad querés eliminarlo?", "Eliminando", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (respuesta == DialogResult.Yes)
                {
                    seleccionado = (Articulo)dataGridView1.CurrentRow.DataBoundItem;
                    data.eliminar(seleccionado.Id);
                    cargar();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }  
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Obtener el artículo seleccionado y mostrar su descripción en un cuadro de diálogo
            if (dataGridView1.CurrentRow != null)
            {
                Articulo articuloSeleccionado = (Articulo)dataGridView1.CurrentRow.DataBoundItem;
                Descripcion cuadroDescripcion = new Descripcion(articuloSeleccionado.Descripcion);
                cuadroDescripcion.ShowDialog();
            }
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {  
            cargar();
            // Inicializar opciones del ComboBox de filtros
            comboBox1.Items.Add("Codigo");
            comboBox1.Items.Add("Nombre");
            comboBox1.Items.Add("Marca");
            comboBox1.Items.Add("Categoria");
            comboBox1.Items.Add("Precio");
        }

        private void cargar()
        {
            // Método para cargar la lista de artículos y configurar el DataGridView
            try
            {
                ArticulosData articulosData = new ArticulosData();
                listaArticulos = articulosData.listar();
                dataGridView1.DataSource = listaArticulos;
                dataGridView1.Columns["Imagen"].Visible = false;
                dataGridView1.Columns["Descripcion"].Visible = false;
                dataGridView1.Columns["Id"].Visible = false;
                cargarImagen(listaArticulos[0].Imagen);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            // Abrir el formulario de carga para agregar un nuevo artículo
            FormCarga FormularioCarga = new FormCarga();
            FormularioCarga.ShowDialog();

            // Recargar la lista de artículos después de agregar
            cargar();
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            // Obtener el artículo seleccionado y abrir el formulario de carga para modificar
            if (dataGridView1.CurrentRow != null)
            {
                Articulo articuloSeleccionado = (Articulo)dataGridView1.CurrentRow.DataBoundItem;
                FormCarga FormularioCarga = new FormCarga(articuloSeleccionado);
                FormularioCarga.ShowDialog();
                cargar();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cargar();
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            // Obtener el artículo seleccionado y cargar su imagen en el PictureBox
            if (dataGridView1.CurrentRow != null)
            {
                Articulo articuloSeleccionado = (Articulo)dataGridView1.CurrentRow.DataBoundItem;
                cargarImagen(articuloSeleccionado.Imagen);
            }
        }

        private void cargarImagen(string imagen)
        {
            try
            {
                boxImagen.Load(imagen);
            }
            catch (Exception ex)
            {
                boxImagen.Load("https://www.pulsecarshalton.co.uk/wp-content/uploads/2016/08/jk-placeholder-image.jpg");
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Evento TextChanged del TextBox de búsqueda rapida
            List<Articulo> listaFiltrada;
            string filtro = textBox1.Text;

            if (filtro.Length >= 3)
            {
                // Filtrar la lista de artículos según el texto ingresado en el TextBox de búsqueda
                listaFiltrada = listaArticulos.FindAll(x =>
                x.Codigo.ToString().ToUpper().Contains(filtro.ToUpper()) ||
                x.Nombre.ToUpper().Contains(filtro.ToUpper()) ||
                x.Categoria.descripcion.ToUpper().Contains(filtro.ToUpper()) ||
                x.Marca.descripcion.ToUpper().Contains(filtro.ToUpper())
                );
            }
            else
            {
                listaFiltrada = listaArticulos;
            }

            dataGridView1.DataSource = null;
            dataGridView1.DataSource = listaFiltrada;
            dataGridView1.Columns["Imagen"].Visible = false;
            dataGridView1.Columns["Descripcion"].Visible = false;
            dataGridView1.Columns["Id"].Visible = false;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Evento SelectedIndexChanged del ComboBox de campos de filtro
            string opcion = comboBox1.SelectedItem.ToString();

            // Configurar las opciones del ComboBox de criterios de filtro según el campo seleccionado
            if (opcion == "Precio")
            {
                comboBox2.Items.Clear();
                comboBox2.Items.Add("Mayor a");
                comboBox2.Items.Add("Menor a");
                comboBox2.Items.Add("Igual a");
            }
            else
            {
                comboBox2.Items.Clear();
                comboBox2.Items.Add("Comienza con");
                comboBox2.Items.Add("Termina con");
                comboBox2.Items.Add("Contiene");
            }
        }

        private bool validarFiltro()
        // Método para validar los campos de filtro antes de realizar la búsqueda
        {
            if (comboBox1.SelectedIndex < 0)
            {
                MessageBox.Show("Por favor, seleccione el campo para filtrar.");
                return true;
            }
            if (comboBox2.SelectedIndex < 0)
            {
                MessageBox.Show("Por favor, seleccione el criterio para filtrar.");
                return true;
            }
            if (comboBox1.SelectedItem.ToString() == "Precio")
            {
                if (string.IsNullOrEmpty(textBox2.Text))
                {
                    MessageBox.Show("Debes cargar el filtro para numéricos...");
                    return true;
                }
                if (!(soloNumeros(textBox2.Text)))
                {
                    MessageBox.Show("Solo nros para filtrar por un campo numérico...");
                    return true;
                }
            }
            return false;
        }

        private bool soloNumeros(string cadena)
        {
            // Método para validar que una cadena contenga solo números

            foreach (char caracter in cadena)
            {
                if (!(char.IsNumber(caracter)))
                    return false;
            }
            return true;
        }

        private void Buscar_Click(object sender, EventArgs e)
        {
            // Evento Click del botón Buscar
            ArticulosData negocio = new ArticulosData();
            try
            {
                // Validar campos de filtro antes de realizar la búsqueda
                if (validarFiltro())
                    return;

                // Obtener campos de filtro y realizar la búsqueda
                string campo = comboBox1.SelectedItem.ToString();
                string criterio = comboBox2.SelectedItem.ToString();
                string filtro = textBox2.Text;
                dataGridView1.DataSource = negocio.filtrar(campo, criterio, filtro);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}




