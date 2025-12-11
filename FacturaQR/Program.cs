using System;
using System.IO;
using System.Text;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace FacturaQR
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Objeto para almacenar el resutlado de las operaciones
            StringBuilder resultado = new StringBuilder();

            // Objeto con el documento para insertar las imagenes
            PdfDocument documento = new PdfDocument();

            // Objeto con la pagina del PDF para añadir las imagenes (QR y marca de agua)
            PdfPage pagina = new PdfPage();

            // Objeto que representa un recuadro donde se incluira el QR y los textos
            XGraphics gfx = null;

            try
            {
                // Cargar configuración
                resultado = Configuracion.CargarParametros(args);

                // Si se ha solicitado cerrar el visor, se cierra antes de iniciar el proceso
                if(Configuracion.CerrarVisor)
                {
                    Utilidades.CerrarVisor();
                }

                // Si no hay errores, se continua con el proceso
                if(resultado.Length == 0)
                {
                    // Valida parametros obligatorios en caso de que haya que añadir el QR
                    if(Configuracion.InsertarQR == true)
                    {
                        resultado = Configuracion.ValidarParametros(resultado);

                        // Insertar QR si no hay errores de configuración
                        if(resultado.Length == 0)
                        {
                            // Si no se ha pasado el fichero de salida, se asigna un valor por defecto
                            if(string.IsNullOrEmpty(Configuracion.PdfSalida))
                            {
                                Configuracion.PdfSalida = Path.Combine(Configuracion.RutaFicheros, Path.GetFileNameWithoutExtension(Configuracion.PdfEntrada) + "_salida.pdf");
                            }

                            // Carga el documento con el PDF de entrada
                            documento = Utilidades.Generardocumento(Configuracion.PdfEntrada);

                            // Establece la pagina 1 para insertar el QR y las imagenes
                            pagina = documento.Pages[0];

                            // Añade el recuadro a la pagina
                            gfx = XGraphics.FromPdfPage(pagina);

                            // Proceso para insertar el QR en el documento
                            resultado = InsertaQR.InsertarQR(pagina, gfx, resultado);

                            if(resultado.Length == 0)
                            {
                                // Guarda el PDF modificado en la ruta de salida
                                documento.Save(Configuracion.PdfSalida);
                            }
                        }
                    }
                    else
                    {
                        // Si no hay que insertar el QR se revisa si hay que añadir la marca de agua
                        if(!string.IsNullOrEmpty(Configuracion.MarcaAgua))
                        {
                            // Carga en el documento el PDF de entrada
                            documento = Utilidades.Generardocumento(Configuracion.PdfEntrada);

                            // Establece la pagina 1 para insertar el QR y las imagenes
                            pagina = documento.Pages[0];

                            // añade el recuadro a la pagina
                            gfx = XGraphics.FromPdfPage(pagina);

                            // Inserta la marca de agua en el PDF
                            Utilidades.InsertaMarcaAgua(pagina, gfx, Configuracion.MarcaAgua);

                            // Asigna el nombre del fichero de salida si no se ha pasado
                            if(string.IsNullOrEmpty(Configuracion.PdfSalida))
                            {
                                Configuracion.PdfSalida = Path.Combine(Configuracion.RutaFicheros, Path.GetFileNameWithoutExtension(Configuracion.PdfEntrada) + "_salida.pdf");
                            }

                            // Guarda el PDF modificado en la ruta de salida
                            documento.Save(Configuracion.PdfSalida);
                        }
                    }

                    // Revisa si hay que ejecutar acciones adicionales
                    if(Configuracion.EjecutarAcciones)
                    {
                        // Ejecuta las acciones adicionales que se hayan solicitado
                        Utilidades.GestionarAcciones();
                    }

                    // Si se ha especificado un fichero de salida, se genera el fichero de control
                    if(Configuracion.FicheroSalida != null)
                    {
                        // Genera el fichero de control de salida una vez termine la ejecucion
                        File.WriteAllText(Configuracion.FicheroSalida, "OK");
                    }
                }
            }

            catch(InvalidOperationException ex)
            {
                resultado.AppendLine(ex.Message);
            }

            catch(Exception ex)
            {
                resultado.AppendLine($"Se ha producido un error al procesar el fichero. Mensaje: {ex.Message}");
            }

            // Guardar resultados en errores.txt si hay errores
            if(resultado.Length > 0)
            {
                File.WriteAllText(Path.Combine(Configuracion.RutaFicheros, "errores.txt"), resultado.ToString());
            }
        }
    }
}
