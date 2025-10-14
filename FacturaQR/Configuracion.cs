using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace FacturaQR
{
    public static class Configuracion
    {
        public static string PdfEntrada { get; private set; }
        public static string PdfSalida { get; private set; }

        public static string RutaFicheros { get; private set; } = Program.RutaFicheros;

        // Datos para el texto del QR

        public static bool QRValido = false; // Control para incluir o no el QR en el PDF

        // Base de la URL del QR
        private static string UrlPruebasBase { get; set; } = @"https://prewww2.aeat.es/wlpl/TIKE-CONT/";
        private static string UrlProduccionBase { get; set; } = @"https://www2.agenciatributaria.gob.es/wlpl/TIKE-CONT/";
        public static string UrlEnvio { get; private set; } // URL completa con parámetros

        // Define si se usa el entorno de pruebas o producción y si se usa VeriFactu o no
        private static bool EntornoProduccion { get; set; } = true; // Defecto entorno producción
        public static bool VeriFactu { get; private set; } = true; // Defecto sistema VeriFactu

        // Datos de la factura
        private static string NifEmisor { get; set; }
        private static string NumeroFactura { get; set; }
        private static DateTime FechaFactura { get; set; }
        private static decimal TotalFactura { get; set; }

        // Texto adiconal del QR
        public static string TextoArriba { get; private set; } = "QR Tributario";
        public static string TextoAbajo { get; private set; } = "VERI*FACTU";

        // Posición del QR
        public static double PosX { get; private set; } = 10;
        public static double PosY { get; private set; } = 10;

        // Tamaño del QR
        public static double Ancho { get; private set; } = 30;
        public static double Alto { get; private set; } = 30;

        // Color del QR
        public static string ColorQR { get; private set; } = "#000000"; // Por defecto negro

        // Marca de agua
        public static string MarcaAgua { get; private set; }


        public static string CargarParametros(string[] args)
        {
            StringBuilder resultado = new StringBuilder();
            if(args.Length < 2)
            {
                resultado.AppendLine("Parámetros insuficientes.");
            }
            if(args[0] != "ds123456")
            {
                resultado.AppendLine("Clave de inicio incorrecta.");
                return resultado.ToString();
            }

            string guion = args[1];

            if(!File.Exists(guion))
            {
                resultado.AppendLine("El archivo de guion no existe.");
            }

            foreach(string linea in File.ReadAllLines(guion))
            {
                if(string.IsNullOrWhiteSpace(linea)) ;
                string[] partes = linea.Split(new char[] { '=' }, 2);
                if(partes.Length == 2)
                {
                    AsignaParametros(partes[0], partes[1]);
                }
            }

            // Validar parámetros obligatorios
            if(string.IsNullOrEmpty(PdfEntrada))
            {
                resultado.AppendLine("El parámetro 'pdfEntrada' es obligatorio.");
                throw new ArgumentException("El parámetro 'pdfEntrada' es obligatorio.");
            }

            if(File.Exists(PdfEntrada) == false)
            {
                resultado.AppendLine("El PDF de entrada no existe.");
            }

            // Chequea si se han pasado los valores del QR
            if(QRValido)
            {
                if(string.IsNullOrEmpty(UrlEnvio))
                {
                    UrlEnvio = ObtenerUrl(EntornoProduccion, VeriFactu); // Si no se ha pasado, se genera segun el resto de parametros (por defecto entorno producción y VeriFactu)
                }

                // Se comenta el chequeo del Nif porque si no se pasa, no se inserta el QR
                //if(string.IsNullOrEmpty(NifEmisor))
                //{
                //    resultado.AppendLine("El parámetro 'nifEmisor' es obligatorio.");
                //}

                if(string.IsNullOrEmpty(NumeroFactura))
                {
                    resultado.AppendLine("El parámetro 'numeroFactura' es obligatorio.");
                }

                if(FechaFactura == DateTime.MinValue)
                {
                    resultado.AppendLine("El parámetro 'fechaFactura' es obligatorio.");
                }

                if(TotalFactura == 0)
                {
                    resultado.AppendLine("El parámetro 'totalFactura' es obligatorio.");
                }

                // Valida si el color pasado es valido
                if(!ColorValido(ColorQR))
                {
                    resultado.AppendLine("El codigo de color del QR no es valido");
                }

                // Codificar los parámetros para la URL en UTF-8
                StringBuilder urlBuilder = new StringBuilder();
                urlBuilder.Append(UrlEnvio).Append("?");
                urlBuilder.Append("nif=").Append(Uri.EscapeUriString(NifEmisor)).Append("&");
                urlBuilder.Append("numserie=").Append(Uri.EscapeUriString(NumeroFactura)).Append("&");
                urlBuilder.Append("fecha=").Append(FechaFactura.ToString("dd-MM-yyyy")).Append("&");
                urlBuilder.Append("importe=").Append(TotalFactura.ToString("F2").Replace(',', '.')); // Asegurar que el decimal es punto

                // Construir la URL completa
                UrlEnvio = urlBuilder.ToString();
            }

            return resultado.ToString();
        }

        private static void AsignaParametros(string clave, string valor)
        {
            switch(clave.ToLower())
            {
                case "pdfentrada":
                    PdfEntrada = Path.GetFullPath(valor.Trim('"'));

                    if(File.Exists(PdfEntrada))
                    {
                        Program.RutaFicheros = Path.GetDirectoryName(PdfEntrada);
                    }

                    PdfSalida = Path.Combine(Program.RutaFicheros, Path.GetFileNameWithoutExtension(PdfEntrada) + "salida.pdf"); // Se asigna un valor por defecto al PDF de salida

                    break;

                case "pdfsalida":
                    if(!string.IsNullOrEmpty(valor))
                    {
                        PdfSalida = Path.GetFullPath(valor.Trim('"')); ;
                    }
                    break;

                case "url":
                    UrlEnvio = valor;
                    break;

                case "entorno":
                    if(valor.ToLower() == "pruebas")
                    {
                        EntornoProduccion = false;
                    }
                    break;

                case "verifactu":
                    if(valor.ToLower() == "no")
                    {
                        VeriFactu = false;
                        TextoAbajo = ""; // Si no es VeriFactu, no se pone texto abajo
                    }
                    break;

                case "nifemisor":
                    NifEmisor = valor;
                    if(!string.IsNullOrEmpty(NifEmisor))
                    {
                        // Si se ha pasado el NIF del emisor, se insertara el QR
                        QRValido = true;
                    }
                    break;

                case "numerofactura":
                    NumeroFactura = valor;
                    break;

                case "fechafactura":
                    string[] formatosValidos = { "dd-MM-yyyy", "dd/MM/yyyy", "dd.MM.yyyy" };

                    // Intentar parsear la fecha con los formatos válidos
                    if(DateTime.TryParseExact(valor, formatosValidos, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime fecha))
                    {
                        FechaFactura = fecha;
                    }
                    else
                    {
                        FechaFactura = DateTime.MinValue; // Valor inválido
                    }
                    break;

                case "totalfactura":
                    if(!decimal.TryParse(valor, out decimal total)) // Evita una excepcion si no se pasa el total correcto
                    {
                        total = 0m;
                    }
                    TotalFactura = total;
                    break;

                case "posicionx":
                    PosX = double.Parse(valor);
                    break;

                case "posiciony":
                    PosY = double.Parse(valor);
                    break;

                case "ancho":
                    Ancho = double.Parse(valor);
                    Alto = Ancho; // Mantener proporción cuadrada
                    break;

                case "color":
                    ColorQR = valor;
                    break;

                case "marcaagua":
                    MarcaAgua = valor.Replace("\\n", "\n");
                    break;
            }
        }

        private static string ObtenerUrl(bool produccion, bool verifactu)
        {
            string urlBase = produccion ? UrlProduccionBase : UrlPruebasBase;

            if(verifactu)
            {
                return urlBase + "ValidarQR";
            }
            else
            {
                return urlBase + "ValidarQRNoVerifactu";
            }
        }

        private static bool ColorValido(string colorHex)
        {
            return Regex.IsMatch(colorHex, @"^#(?:[0-9a-fA-F]{6})$");
        }
    }


}
