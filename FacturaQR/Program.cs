using System;
using System.Diagnostics;
using System.IO;

namespace FacturaQR
{
    public class Program
    {
        public static string RutaFicheros = Directory.GetCurrentDirectory();
        static void Main(string[] args)
        {
            string resultado = string.Empty;
            try
            {
                // Cargar configuración
                resultado = Configuracion.CargarParametros(args);

                // Insertar QR si no hay errores de configuración
                if(string.IsNullOrEmpty(resultado))
                {
                    if (Configuracion.AccionPDF == Configuracion.AccionesPDF.Visualizar)
                    {
                        Utilidades.GestionarSalidaPDF();
                    }
                    else
                    {
                        resultado += InsertaQR.InsertarQR();
                    }
                }
            }

            catch(Exception ex)
            {
                resultado += $"Se ha producido un error al procesar el fichero. Mensaje: {ex.Message}";
            }

            // Guardar resultados en errores.txt si hay errores
            if(!string.IsNullOrEmpty(resultado))
            {
                File.WriteAllText(Path.Combine(Configuracion.RutaFicheros, "errores.txt"), resultado);
            }
        }
    }
}
