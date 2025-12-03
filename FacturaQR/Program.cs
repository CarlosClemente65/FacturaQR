using System;
using System.IO;

namespace FacturaQR
{
    public class Program
    {
        public static string RutaFicheros = Directory.GetCurrentDirectory();
        static void Main(string[] args)
        {
            // Cargar configuración
            string resultado = Configuracion.CargarParametros(args);

            // Insertar QR si no hay errores de configuración
            if(string.IsNullOrEmpty(resultado))
            {
                resultado += InsertaQR.InsertarQR();
            }

            // Guardar resultados en errores.txt si hay errores
            if(!string.IsNullOrEmpty(resultado))
            {
                File.WriteAllText(Path.Combine(Configuracion.RutaFicheros, "errores.txt"), resultado);
            }
        }
    }
}
