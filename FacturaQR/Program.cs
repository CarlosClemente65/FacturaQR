using System;
using System.IO;

namespace FacturaQR
{
    public class Program
    {
        public static string RutaFicheros = Directory.GetCurrentDirectory();
        static void Main(string[] args)
        {
            string resultado = Configuracion.CargarParametros(args);

            if(string.IsNullOrEmpty(resultado))
            {
                resultado += InsertaQR.InsertarQR();
            }

            if(!string.IsNullOrEmpty(resultado))
            {
                File.WriteAllText(Path.Combine(Configuracion.RutaFicheros, "errores.txt"), resultado);
            }
        }
    }
}
