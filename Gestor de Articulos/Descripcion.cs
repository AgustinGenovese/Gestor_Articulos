using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using dominio;

namespace winform
{
    public partial class Descripcion : Form
    {
        private string descripcion;
        public Descripcion(string descripcion)
        {
            InitializeComponent();
            richTextBox1.Text = descripcion;
        }
    }
}
