using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace FacturaQR
{
    public static class Utilidades
    {
        // Ruta del ejecutable SumatraPDF 
        static string rutaBase = AppDomain.CurrentDomain.BaseDirectory;
        static string rutaSumatra = Path.Combine(rutaBase, "SumatraPDF.exe");
        static string cacheSumatra = Path.Combine(rutaBase, "sumatrapdfcache");


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

        // Crea el documento en el que se insertara el QR o los textos
        public static PdfDocument Generardocumento(string rutaPdfEntrada)
        {
            try
            {
                // Genera el documento PDF para luego poder insertar las imagenes
                PdfDocument documento = PdfReader.Open(rutaPdfEntrada, PdfDocumentOpenMode.Modify);
                return documento;
            }

            catch(Exception ex)
            {
                throw new InvalidOperationException($"Se ha producido un error al procesar el PDF. {ex.Message}");
            }

        }

        // Comprueba que el codigo de color sea valido
        public static bool ColorValido(string colorHex)
        {
            return Regex.IsMatch(colorHex, @"^#(?:[0-9a-fA-F]{6})$");
        }

        // Genera la URL con los parámetros del QR UTF-8
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

        // Gestiona las acciones de abrir, imprimir o visualizar el PDF con SumatraPDF
        public static void GestionarAcciones()
        {
            var accionPDF = Configuracion.AccionPDF;

            // Si no se ha indicado el PDF de salida, se usa el de entrada
            var ficheroPDF = string.IsNullOrWhiteSpace(Configuracion.PdfSalida)
                ? Configuracion.PdfEntrada
                : Configuracion.PdfSalida;
            try
            {
                // Borrado de la carpeta de cache antes de la ejecucion
                if(Directory.Exists(cacheSumatra))
                {
                    Directory.Delete(cacheSumatra, true);
                }

                // Controla si esta disponible el programa para evitar excepciones
                if(!File.Exists(rutaSumatra))
                {
                    throw new InvalidOperationException("No se pudo lanzar la impresion del PDF.");
                }


                // Crea un proceso para ejecutar el programa SumatraPDF
                var psi = new ProcessStartInfo();
                psi.FileName = rutaSumatra;
                psi.WorkingDirectory = Path.GetDirectoryName(rutaSumatra);

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
                        psi.Arguments = $"{ficheroPDF}"; // Fichero PDF para abrir o visualizar
                        psi.CreateNoWindow = false; // Se crea la ventana del proceso
                        psi.WindowStyle = ProcessWindowStyle.Normal; // Estilo de la ventana del proceso
                        psi.UseShellExecute = true; // Usa el shell de Windows para abrir SumatraPDF normalmente (ventana visible)

                        // En la accion de visualizar no se espera a cerrar el visor
                        if(accionPDF == Configuracion.AccionesPDF.Visualizar)
                        {
                            espera = false;
                        }

                        break;

                }

                // Inicia el proceso configurado
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
                }

            }
            catch(Exception ex)
            {
                throw new InvalidOperationException($"Se ha producido un error con el visualizador del PDF. Mensaje: {ex.Message}");
            }
        }

        // Cierra todas las instancias del visor SumatraPDF que esten abiertas, matando la tarea del administrador de tareas (no implementado, lo dejo para futuras consultas)
        public static void ForzarCerrarVisor()
        {
            // Crea una lista con todos los procesos que hay abiertos de la aplicacion
            foreach(var proceso in Process.GetProcessesByName("SumatraPDF"))
            {
                // Forzar cierre si sigue activo
                proceso.Kill();
            }
        }

        // Cierra todas las instancias del visor SumatraPDF que esten abiertas, mandando un comando al propio programa para cerrarse.
        public static void CerrarVisor()
        {
            // Crea una lista con todos los procesos que hay abiertos de la aplicacion
            foreach(var proceso in Process.GetProcessesByName("SumatraPDF"))
            {
                string argumentos = $"-dde [CmdExit]"; // Comando para cerrar la aplicacion
                var psi = new ProcessStartInfo(rutaSumatra, argumentos); // Crea el proceso
                psi.CreateNoWindow = true; // No crea una ventana 
                psi.UseShellExecute = false; // No se manda como un comando de la Shell
                Process.Start(psi); // Lanza el comando
            }

        }

        // Inserta una marca de agua en la pagina PDF indicada
        public static PdfPage InsertaMarcaAgua(PdfPage pagina, XGraphics gfx, string marcaAgua)
        {
            try
            {
                // Fuente y pincel para dibujar el texto
                XFont fuenteMarca = new XFont("Arial", 20, XFontStyle.BoldItalic);
                XBrush pincelMarca = new XSolidBrush(XColor.FromArgb(0, 225, 225, 225)); // Gris muy claro (el primer cero es la transparencia pero no se puede aplicar a un PDF)

                // Ajusta el texto en varias lineas si es necesario
                List<string> lineas = new List<string>();
                string[] bloques = marcaAgua.Split(new string[] { "\n" }, StringSplitOptions.None);
                string linea = "";

                // Se define un cuadrado seguro de 210x210 mm para insertar la marca de agua
                double margenMm = 10;
                double margen = XUnit.FromMillimeter(margenMm).Point;
                double ladoCuadradoMm = 210;
                double ladoCuadrado = XUnit.FromMillimeter(ladoCuadradoMm).Point;

                // Calcula el centro del cuadrado
                double xInicioCuadrado = margen;
                double yInicioCuadrado = (pagina.Height.Point - ladoCuadrado) / 2;
                double centroX = xInicioCuadrado + ladoCuadrado / 2;
                double centroY = yInicioCuadrado + ladoCuadrado / 2;

                // Calculo del ancho maximo de la marca de agua aproximado a la diagonal del cuadrado seguro)
                double anchoMaximo = ladoCuadrado;

                // Se divide el texto en lineas que no sobrepasen el ancho maximo
                foreach(var bloque in bloques)
                {
                    foreach(var palabra in bloque.Split(' ')) // Separa por palabras
                    {
                        // Primera parte, añadir a la linea actual
                        string textoLinea = string.IsNullOrEmpty(linea) ? palabra : linea + " " + palabra;
                        XSize size = gfx.MeasureString(textoLinea, fuenteMarca);

                        // Si sobrepasa el ancho maximo, se guarda la linea actual y se inicia una nueva
                        if(size.Width > anchoMaximo)
                        {
                            if(!string.IsNullOrEmpty(linea))
                            {
                                lineas.Add(linea);
                            }
                            linea = palabra;
                        }
                        else
                        {
                            linea = textoLinea;
                        }
                    }

                    // Se añade la ultima linea calculada
                    if(!string.IsNullOrEmpty(linea))
                    {
                        lineas.Add(linea);
                        linea = "";
                    }
                }

                // Se guarda la configuracion para aplicarla solo a la marca de agua
                gfx.Save();

                // Rotacion 45 grados a la izquierda para poner la marca de agua
                gfx.RotateAtTransform(-45, new XPoint(centroX, centroY));

                // Posicion inicial del texto (centrado en el cuadro)
                double x = centroX;
                double y = centroY - (lineas.Count * fuenteMarca.Size / 2);


                // Se dibujan una a una las lineas de la marca de agua
                foreach(var l in lineas)
                {
                    gfx.DrawString(l, fuenteMarca, pincelMarca, new XPoint(x, y), XStringFormats.Center);

                    // Se recalcula la posicion del margen Y segun el tamaño de la fuente para desplazarlo hacia abajo
                    y += fuenteMarca.Size;
                }

                // Se restaura la configuracion para aplicar al resto del texto
                gfx.Restore();

                return pagina;
            }

            catch(InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Se ha producido un error al insertar la marca de agua. {ex.Message}");
            }
        }
    }
}
