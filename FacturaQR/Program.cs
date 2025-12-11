using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace FacturaQR
{
    public class Program
    {
        static void Main(string[] args)
        {
            StringBuilder resultado = new StringBuilder();
            try
            {
                // Cargar configuración
                resultado = Configuracion.CargarParametros(args);

                // Valida parametros obligatorios en caso de que haya que añadir el QR
                if(Configuracion.InsertarQR == true)
                {
                    resultado = Configuracion.ValidarParametros(resultado);

                    // Insertar QR si no hay errores de configuración
                    if(resultado.Length == 0)
                    {
                        resultado = InsertaQR.InsertarQR();
                    }
                }

                if(resultado.Length == 0)
                {
                    // Ejecuta las acciones adicionales que se hayan solicitado
                    Utilidades.GestionarAcciones();
                }
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
