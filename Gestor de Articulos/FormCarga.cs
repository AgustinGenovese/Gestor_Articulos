using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using dominio;
using negocio;
using Negocio;
using System.Configuration;
using System.IO;

namespace winform
{
    public partial class FormCarga : Form
    {

        private Articulo articulo = null;
        private OpenFileDialog archivo = null;

        public FormCarga()
        {
            InitializeComponent();
        }

        public FormCarga(Articulo articulo)
        {
            InitializeComponent();
            this.articulo = articulo;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Evento click del botón principal (Aceptar)
            ArticulosData data = new ArticulosData();

            try
            {
                // Crear un nuevo artículo si es nulo
                if (articulo == null)
                {
                    articulo = new Articulo();
                }

                // Asignar los valores de los controles a las propiedades del artículo
                articulo.Codigo = textBox1.Text;
                articulo.Nombre = textBox3.Text;
                articulo.Precio = textBox5.Text;
                articulo.Descripcion = richTextBox1.Text;
                articulo.Imagen = textBox2.Text;

                articulo.Marca = (Marca)comboBox1.SelectedItem; ;

                articulo.Categoria = (Categoria)comboBox2.SelectedItem;

                // Determinar si se debe modificar o agregar el artículo
                if (articulo.Id != 0)
                {
                    data.modificar(articulo);
                    MessageBox.Show("Modificado exitosamente");
                }
                else
                {
                    data.agregar(articulo);
                    MessageBox.Show("Agregado exitosamente");

                }

                // Copiar la imagen al directorio especificado en la configuración
                if (archivo != null && !(textBox2.Text.ToUpper().Contains("HTTP")))
                {
                    File.Copy(archivo.FileName, ConfigurationManager.AppSettings["images-folder"] + archivo.SafeFileName);
                }

                this.Close();
            }
            catch (Exception)
            {
                this.Close();
            }
        }

        private void FormCarga_Load(object sender, EventArgs e)
        {
            CategoriaData categoriaData = new CategoriaData();
            MarcaData marcaData = new MarcaData();

            try
            {
                // Cargar datos en los combos de Categoría y Marca
                comboBox2.DataSource = categoriaData.listar();
                comboBox2.ValueMember = "id";
                comboBox2.DisplayMember = "descripcion";

                comboBox1.DataSource = marcaData.listar();
                comboBox1.ValueMember = "id";
                comboBox1.DisplayMember = "descripcion";

                if (articulo != null)
                {
                    // Mostrar datos del artículo en los controles si el artículo existe
                    textBox1.Text = articulo.Codigo.ToString();
                    textBox3.Text = articulo.Nombre;
                    textBox5.Text = articulo.Precio;
                    textBox2.Text = articulo.Imagen;
                    richTextBox1.Text = articulo.Descripcion;
                    cargarImagen(articulo.Imagen);
                    comboBox2.SelectedValue = articulo.Categoria.id;
                    comboBox1.SelectedValue = articulo.Marca.id;

                }
            }
            catch (Exception)
            {

            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            cargarImagen(textBox2.Text);
        }

        private void cargarImagen(string imagen)
        // Método para cargar la imagen en el PictureBox
        {
            try
            {
                pictureBox1.Load(imagen);
            }
            catch (Exception ex)
            {
                pictureBox1.Load("https://www.pulsecarshalton.co.uk/wp-content/uploads/2016/08/jk-placeholder-image.jpg");
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        // Evento click del botón para seleccionar una imagen
        {
            {
                archivo = new OpenFileDialog();
                archivo.Filter = "jpg|*.jpg;|png|*.png";
                if (archivo.ShowDialog() == DialogResult.OK)
                {
                    textBox2.Text = archivo.FileName;
                    cargarImagen(archivo.FileName);
                }
            }

        }
    }
}
