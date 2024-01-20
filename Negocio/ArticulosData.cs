using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using dominio;
using Negocio;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace negocio
{
    public class ArticulosData
    {
        public List<Articulo> listar()
        // Método para obtener una lista de todos los artículos
        {
            List<Articulo> lista = new List<Articulo>();

            using (SqlConnection conexion = new SqlConnection("server=.\\SQLEXPRESS; database=CATALOGO_DB; integrated security=true"))
            using (SqlCommand comando = new SqlCommand("SELECT A.id, A.Codigo, A.Nombre, A.Descripcion, A.ImagenUrl, A.Precio, C.Descripcion AS Categoria, C.id AS CategoriaId, M.Descripcion AS Marca, M.id AS MarcaId FROM ARTICULOS A JOIN CATEGORIAS C ON A.IdCategoria = C.Id JOIN MARCAS M ON A.IdMarca = M.Id;", conexion))
            {
                try
                {
                    conexion.Open();
                    using (SqlDataReader lector = comando.ExecuteReader())
                    {
                        while (lector.Read())
                        // Iterar sobre los resultados y crear objetos Articulo
                        {
                            Articulo aux = new Articulo
                            {
                                Id = (int)lector["Id"],
                                Codigo = (string)lector["Codigo"],
                                Marca = new Marca { id = (int)lector["MarcaId"], descripcion = (string)lector["Marca"] },
                                Nombre = (string)lector["Nombre"],
                                Precio = Convert.ToString((decimal)lector["Precio"]),
                                Categoria = new Categoria { id = (int)lector["CategoriaId"], descripcion = (string)lector["Categoria"] },
                                Descripcion = (string)lector["Descripcion"],
                                Imagen = lector.IsDBNull(lector.GetOrdinal("ImagenUrl")) ? null : (string)lector["ImagenUrl"]
                            };

                            lista.Add(aux);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return lista;
        }

        public void agregar(Articulo nuevo)
        // Método para agregar un nuevo artículo
        {
            AccesoDatos datos = new AccesoDatos();

            try
            {
                // Configuración de la consulta SQL para insertar un nuevo artículo
                datos.setearConsulta("INSERT INTO ARTICULOS (Codigo, Nombre, Descripcion, Precio, idMarca, idCategoria, ImagenUrl) VALUES (@Codigo, @Nombre, @Descripcion, @Precio, @idMarca, @idCategoria, @ImagenUrl);");
                datos.setearParametro("@Codigo", nuevo.Codigo);
                datos.setearParametro("@Nombre", nuevo.Nombre);
                datos.setearParametro("@Descripcion", nuevo.Descripcion);
                datos.setearParametro("@Precio", nuevo.Precio);
                datos.setearParametro("@idMarca", nuevo.Marca.id);
                datos.setearParametro("@idCategoria", nuevo.Categoria.id);
                datos.setearParametro("@ImagenUrl", nuevo.Imagen);
                datos.ejecutarAccion();

                // Consulta para obtener la descripción de la marca y categoría asociadas al nuevo artículo
                datos.setearConsulta("SELECT M.Descripcion as MarcaDescripcion, C.Descripcion as CategoriaDescripcion FROM ARTICULOS A INNER JOIN MARCAS M ON A.idMarca = M.Id INNER JOIN CATEGORIAS C ON A.idCategoria = C.Id WHERE A.idMarca = @idMarca AND A.idCategoria = @idCategoria");
                datos.setearParametro("@idMarca", nuevo.Marca.id);
                datos.setearParametro("@idCategoria", nuevo.Categoria.id);
                datos.ejecutarLectura();

                if (datos.Lector.Read())
                {
                    nuevo.Marca.descripcion = datos.Lector["MarcaDescripcion"].ToString();
                    nuevo.Categoria.descripcion = datos.Lector["CategoriaDescripcion"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void modificar(Articulo modificado)
        {
            // Método para modificar un artículo existente
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("UPDATE ARTICULOS SET Codigo = @codigo, Nombre = @nombre, Descripcion = @desc, ImagenUrl = @img, Precio = @precio, IdMarca = @idMarca, IdCategoria = @idCategoria WHERE Id = @id");
                datos.setearParametro("@codigo", modificado.Codigo);
                datos.setearParametro("@nombre", modificado.Nombre);
                datos.setearParametro("@desc", modificado.Descripcion);
                datos.setearParametro("@img", modificado.Imagen);
                datos.setearParametro("@precio", modificado.Precio);
                datos.setearParametro("@idMarca", modificado.Marca.id);
                datos.setearParametro("@idCategoria", modificado.Categoria.id);
                datos.setearParametro("@id", modificado.Id);

                datos.ejecutarAccion();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void eliminar(int id)
        // Método para eliminar un artículo por su ID
        {
            try
            {
                AccesoDatos datos = new AccesoDatos();
                datos.setearConsulta("delete from ARTICULOS where id = @id");
                datos.setearParametro("@id", id);
                datos.ejecutarAccion();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Articulo> filtrar(string campo, string criterio, string filtro)
        {
            // Método para filtrar artículos según ciertos criterios
            List<Articulo> lista = new List<Articulo>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                string consulta = "SELECT A.id, A.Codigo, A.Nombre, A.Descripcion, A.ImagenUrl, A.Precio, C.Descripcion AS Categoria, C.id AS CategoriaId, M.Descripcion AS Marca, M.id AS MarcaId FROM ARTICULOS A JOIN CATEGORIAS C ON A.IdCategoria = C.Id JOIN MARCAS M ON A.IdMarca = M.Id And ";

                if (campo == "Precio")
                {
                    switch (criterio)
                    {
                        case "Mayor a":
                            consulta += "A.Precio > " + filtro;
                            break;
                        case "Menor a":
                            consulta += "A.Precio < " + filtro;
                            break;
                        default:
                            consulta += "A.Precio = " + filtro;
                            break;
                    }
                }
                else if (campo == "Codigo")
                {
                    switch (criterio)
                    {
                        case "Comienza con":
                            consulta += "A.Codigo like '" + filtro + "%' ";
                            break;
                        case "Termina con":
                            consulta += "A.Codigo like '%" + filtro + "'";
                            break;
                        default:
                            consulta += "A.Codigo like '%" + filtro + "%'";
                            break;
                    }
                }
                else if (campo == "Marca")
                {
                    switch (criterio)
                    {
                        case "Comienza con":
                            consulta += "M.Descripcion like '" + filtro + "%' ";
                            break;
                        case "Termina con":
                            consulta += "M.Descripcion like '%" + filtro + "'";
                            break;
                        default:
                            consulta += "M.Descripcion like '%" + filtro + "%'";
                            break;
                    }
                } 
                else if (campo == "Categoria")
                {
                    switch (criterio)
                    {
                        case "Comienza con":
                            consulta += "C.Descripcion like '" + filtro + "%' ";
                            break;
                        case "Termina con":
                            consulta += "C.Descripcion like '%" + filtro + "'";
                            break;
                        default:
                            consulta += "C.Descripcion like '%" + filtro + "%'";
                            break;
                    }
                }
                else if (campo == "Nombre")
                {
                    switch (criterio)
                    {
                        case "Comienza con":
                            consulta += "A.Nombre like '" + filtro + "%' ";
                            break;
                        case "Termina con":
                            consulta += "A.Nombre like '%" + filtro + "'";
                            break;
                        default:
                            consulta += "A.Nombre like '%" + filtro + "%'";
                            break;
                    }
                }

                datos.setearConsulta(consulta);
                datos.ejecutarLectura();
                while (datos.Lector.Read())
                {
                    Articulo aux = new Articulo
                    {
                        Id = (int)datos.Lector["Id"],
                        Codigo = (string)datos.Lector["Codigo"],
                        Marca = new Marca { id = (int)datos.Lector["MarcaId"], descripcion = (string)datos.Lector["Marca"] },
                        Nombre = (string)datos.Lector["Nombre"],
                        Precio = Convert.ToString((decimal)datos.Lector["Precio"]),
                        Categoria = new Categoria { id = (int)datos.Lector["CategoriaId"], descripcion = (string)datos.Lector["Categoria"] },
                        Descripcion = (string)datos.Lector["Descripcion"],
                        Imagen = datos.Lector.IsDBNull(datos.Lector.GetOrdinal("ImagenUrl")) ? null : (string)datos.Lector["ImagenUrl"]
                    };

                    lista.Add(aux);
                }

                return lista;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

}
