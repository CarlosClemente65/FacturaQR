using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FacturaQR
{
    public static class Utilidades
    {
        // Establece la ruta para insertar el QR en funcion del entorno y si aplica Verifactu
        public static string ObtenerUrl(bool produccion, bool verifactu)
        {
            string urlBase = produccion ? Configuracion.UrlProduccionBase : Configuracion.UrlPruebasBase;

            if(verifactu)
            {
                return urlBase + "ValidarQR";
            }
            else
            {
                return urlBase + "ValidarQRNoVerifactu";
            }
        }


        // Comprueba que el codigo de color sea valido
        public static bool ColorValido(string colorHex)
        {
            return Regex.IsMatch(colorHex, @"^#(?:[0-9a-fA-F]{6})$");
        }

        public static void GenerarURL()
        {
            // Genera la URL con los parámetros del QR UTF-8
            StringBuilder urlBuilder = new StringBuilder();
            urlBuilder.Append(Configuracion.UrlEnvio).Append("?");
            urlBuilder.Append("nif=").Append(Uri.EscapeUriString(Configuracion.NifEmisor)).Append("&");
            urlBuilder.Append("numserie=").Append(Uri.EscapeUriString(Configuracion.NumeroFactura)).Append("&");
            urlBuilder.Append("fecha=").Append(Configuracion.FechaFactura.ToString("dd-MM-yyyy")).Append("&");
            urlBuilder.Append("importe=").Append(Configuracion.TotalFactura.ToString("F2").Replace(',', '.')); // Asegurar que el decimal es punto

            // Construir la URL completa
            Configuracion.UrlEnvio = urlBuilder.ToString();
        }

        public static void GestionarAcciones()
        {
            var accionPDF = Configuracion.AccionPDF;
            var ficheroPDF = string.IsNullOrWhiteSpace(Configuracion.PdfSalida)
                ? Configuracion.PdfEntrada
                : Configuracion.PdfSalida;
            try
            {
                if (accionPDF == Configuracion.AccionesPDF.CerrarVisor)
                {
                    CerrarVisor();
                    return; // Si la accion es cerrar el visor, no hay que ejecutar nada mas
                }


                // Ruta del ejecutable SumatraPDF 
                string rutaBase = AppDomain.CurrentDomain.BaseDirectory;
                string sumatraExe = Path.Combine(rutaBase, "SumatraPDF.exe");
                string rutaCache = Path.Combine(rutaBase, "sumatrapdfcache");

                // Borrado de la carpeta de cache antes de la ejecucion
                if(Directory.Exists(rutaCache))
                {
                    Directory.Delete(rutaCache, true);
                }

                // Controla si esta disponible el programa para evitar excepciones
                if(!File.Exists(sumatraExe))
                {
                    throw new InvalidOperationException("No se pudo lanzar la impresion del PDF.");
                }

                // Crea un proceso para ejecutar el programa SumatraPDF
                var psi = new ProcessStartInfo();
                psi.FileName = sumatraExe;
                psi.WorkingDirectory = Path.GetDirectoryName(sumatraExe);

                bool espera = true; // Indica si hay que esperar al cierre del visor

                //Configura los parametros segun si se va a imprimir, abrir o visualizar el PDF
                switch(accionPDF)
                {
                    // Configura el proceso para lanzar la impresion silenciosa en la impresora predeterminada
                    case Configuracion.AccionesPDF.Imprimir:
                        psi.Arguments = $"-print-to-default -silent \"{ficheroPDF}\""; // Imprime el PDF en la impresora predeterminada
                        psi.CreateNoWindow = true; // No crea ninguna ventana
                        psi.WindowStyle = ProcessWindowStyle.Hidden; // El proceso esta oculto
                        psi.UseShellExecute = false; // Ejecuta el proceso directamente sin usar la shell de windows
                        break;

                    case Configuracion.AccionesPDF.Abrir:
                    case Configuracion.AccionesPDF.Visualizar:
                        psi.Arguments = $"{ficheroPDF}"; // Abrir el PDF
                        psi.CreateNoWindow = false;
                        psi.WindowStyle = ProcessWindowStyle.Normal; // Estilo de la ventana del proceso
                        psi.UseShellExecute = true; // Usa el shell de Windows para abrir SumatraPDF normalmente (ventana visible)

                        // En el modo de visualizar se cierra el visor sin espera
                        if(accionPDF == Configuracion.AccionesPDF.Visualizar)
                        {
                            espera = false; 
                        }

                        break;

                }

                // Inicia el proceso de impresion
                using(var proceso = Process.Start(psi))
                {
                    if(espera)
                    {
                        proceso.WaitForExit();

                        // Comprueba el código de salida
                        if(proceso.ExitCode != 0)
                        {
                            throw new InvalidOperationException($"La impresión del PDF falló. Código de salida: {proceso.ExitCode}");
                        }
                    }

                    if(Configuracion.FicheroSalida != null)
                    {
                        // Genera el fichero de control de salida una vez termine la ejecucion
                        File.WriteAllText(Configuracion.FicheroSalida, "OK");
                    }
                }

            }
            catch(Exception ex)
            {
                throw new InvalidOperationException($"Se ha producido un error con el visualizador del PDF. Mensaje: {ex.Message}");
            }
        }

        public static void CerrarVisor()
        {
            var listaProcesos = Process.GetProcessesByName("SumatraPDF");
            foreach(var proc in listaProcesos)
            {
                // Cierre normal del proceso
                proc.CloseMainWindow();

                // Forzar cierre si sigue activo
                if(!proc.WaitForExit(2000))
                {
                    proc.Kill();
                }
            }
        }
    }
}
