using System;
using System.IO;
using System.Linq;
using System.Text;

namespace FacturaQR
{
    public static class Configuracion
    {
        // Rutas de los ficheros
        public static string PdfEntrada { get; private set; }
        public static string PdfSalida { get; set; }
        public static string RutaFicheros { get; private set; } = Directory.GetCurrentDirectory();
        public static string FicheroSalida { get; set; } // Fichero de control para gestionar cuando termina el programa.


        // Datos de control para generar el QR
        public static bool? UsarQrExterno = false; // Indica si se usa un fichero de QR externo
        public static bool? InsertarQR = false; // Control para incluir o no el QR en el PDF


        // Datos para generar el QR
        public static string NombreFicheroQR { get; private set; }

        // Datos base de la URL para generar el QR
        public static string UrlPruebasBase { get; set; } = @"https://prewww2.aeat.es/wlpl/TIKE-CONT/";
        public static string UrlProduccionBase { get; set; } = @"https://www2.agenciatributaria.gob.es/wlpl/TIKE-CONT/";
        public static string UrlEnvio { get; set; } // URL completa con parámetros


        // Datos de control para utilizar el entorno de pruebas o producción y el uso de VeriFactu
        public static bool EntornoProduccion { get; set; } = true; // Defecto entorno producción
        public static bool VeriFactu { get; private set; } = false; // Defecto sistema no VeriFactu


        // Datos de la factura que se insertarán en el QR
        public static string NifEmisor { get; set; }
        public static string NumeroFactura { get; set; }
        public static DateTime FechaFactura { get; set; }
        public static decimal TotalFactura { get; set; }


        // Texto adiconal a insertar en el QR
        public static string TextoArriba { get; private set; } = "QR Tributario";
        public static string TextoAbajo { get; private set; } = "";


        // Posición tamaño y color del QR
        public static double PosX { get; private set; } = 10;
        public static double PosY { get; private set; } = 10;
        public static double Ancho { get; private set; } = 30;
        public static double Alto { get; private set; } = 30;
        public static string ColorQR { get; private set; } = "#000000"; // Por defecto negro


        // Texto de la marca de agua
        public static string MarcaAgua { get; private set; }


        // Lista de acciones adicionales a realizar con el PDF
        public enum AccionesPDF
        {
            Ninguna,
            Imprimir,
            Abrir,
            Visualizar
        }


        // Acción a realizar con el PDF
        public static AccionesPDF AccionPDF { get; private set; }


        // Controla si hay que realizar alguna accion con el PDF
        public static bool EjecutarAcciones { get; set; } = false;


        // Control para cerrar el visor
        public static bool CerrarVisor {get; set;} = false;


        // Carga los parámetros desde el archivo de guion
        public static StringBuilder CargarParametros(string[] args)
        {
            StringBuilder resultado = new StringBuilder();

            // Validar los parámetros de entrada
            if(args.Length < 2)
            {
                resultado.AppendLine("Parámetros insuficientes.");
            }
            if(args[0] != "ds123456")
            {
                resultado.AppendLine("Clave de inicio incorrecta.");
                return resultado;
            }

            // Asignar el archivo de guion
            string guion = args[1];

            if(!File.Exists(guion))
            {
                resultado.AppendLine("El archivo de guion no existe.");
            }


            // Leer el archivo de guion y asignar los parámetros
            foreach(string linea in File.ReadAllLines(guion))
            {
                // Salta las lineas vacias
                if(string.IsNullOrWhiteSpace(linea))
                {
                    continue;
                }

                // Separa las lineas del guion en clave y valor
                string[] partes = linea
                    .Split(new char[] { '=' }, 2)
                    .Select(p => p.Trim())
                    .ToArray();

                // Chequea que tenga dos partes (clave y valor) antes de asignar los parametros
                if(partes.Length == 2)
                {
                    AsignaParametros(partes[0], partes[1]);
                }
                else if(string.Equals(partes[0], "cerrarvisor", StringComparison.OrdinalIgnoreCase))
                {
                    // El parametro 'cerrarvisor' no tiene dos partes y se trata de forma independiente
                    CerrarVisor = true;
                }
            }

            return resultado;
        }


        // Asigna los parámetros según la clave y valor proporcionados
        public static void AsignaParametros(string clave, string valor)
        {
            switch(clave.ToLower())
            {
                case "pdfentrada":
                    PdfEntrada = Path.GetFullPath(valor.Trim('"'));

                    // Chequea si el fichero existe para asignar la ruta de ficheros
                    if(File.Exists(PdfEntrada))
                    {
                        RutaFicheros = Path.GetDirectoryName(PdfEntrada);
                    }
                    break;

                case "pdfsalida":
                    // Asigna el PDF de salida eliminando las comillas si las tiene
                    if(!string.IsNullOrEmpty(valor))
                    {
                        PdfSalida = Path.GetFullPath(valor.Trim('"'));
                    }
                    break;

                case "url":
                    // Si se pasa la URL, se usa esa directamente
                    UrlEnvio = valor;
                    InsertarQR = true; // Al pasar la url hay que insertar el QR en el PDF
                    break;

                case "ficheroqr":
                    // Si se pasa un fichero de QR, se usa ese directamente
                    if(!string.IsNullOrEmpty(valor))
                    {
                        NombreFicheroQR = Path.GetFullPath(valor.Trim('"'));
                        UsarQrExterno = true; // Se indica que se usará un fichero externo
                        InsertarQR = true; // Si se pasa un fichero con el QR hay que insertarlo en el PDF
                    }
                    break;

                case "entorno":
                    // Define el entorno de pruebas o producción
                    if(string.Equals(valor, "pruebas", StringComparison.OrdinalIgnoreCase))
                    {
                        EntornoProduccion = false;
                    }
                    break;

                case "verifactu":
                    // Define si se usa el sistema VeriFactu
                    if(string.Equals(valor, "si", StringComparison.OrdinalIgnoreCase))
                    {
                        VeriFactu = true;
                        TextoAbajo = "VERI*FACTU"; // Si es VeriFactu, se pone el texto abajo
                    }
                    break;

                case "nifemisor":
                    // Asigna el NIF del emisor
                    NifEmisor = valor;
                    if(!string.IsNullOrEmpty(NifEmisor))
                    {
                        // Si se ha pasado el NIF del emisor, se insertara el QR
                        InsertarQR = true;
                    }
                    break;

                case "numerofactura":
                    // Asigna el número de la factura
                    NumeroFactura = valor;
                    break;

                case "fechafactura":
                    // Define los formatos de fecha válidos
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
                    // Asigna el total de la factura
                    if(!decimal.TryParse(valor, out decimal total)) // Evita una excepcion si no se pasa el total correcto
                    {
                        total = 0m;
                    }
                    TotalFactura = total;
                    break;

                case "posicionx":
                    // Asigna la posición X del QR
                    PosX = double.Parse(valor);
                    break;

                case "posiciony":
                    // Asigna la posición Y del QR
                    PosY = double.Parse(valor);
                    break;

                case "ancho":
                    // Asigna el ancho y alto del QR
                    Ancho = double.Parse(valor);
                    Alto = Ancho; // Mantener proporción cuadrada
                    break;

                case "color":
                    // Asigna el color del QR
                    ColorQR = valor;
                    break;

                case "marcaagua":
                    // Asigna la marca de agua, reemplazando \n por saltos de línea
                    MarcaAgua = valor.Replace("\\n", "\n");
                    break;

                case "accionpdf":
                    // Define distintas acciones a realizar con el visor SumatraPDF que permite imprimir, abrir o visualizar el PDF
                    switch(valor.ToLower())
                    {
                        case "imprimir":
                            AccionPDF = AccionesPDF.Imprimir;
                            EjecutarAcciones = true;
                            break;

                        case "abrir":
                            AccionPDF = AccionesPDF.Abrir;
                            EjecutarAcciones = true;
                            break;

                        case "visualizar":
                            AccionPDF = AccionesPDF.Visualizar;
                            EjecutarAcciones = true;
                            break;

                    }
                    break;

                case "ficherosalida":
                    // Fichero para controlar si se ha terminado el proceso
                    FicheroSalida = valor;
                    // Revisa si existe el fichero para borrarlo antes
                    if(File.Exists(FicheroSalida))
                    {
                        File.Delete(FicheroSalida);
                    }
                    break;
            }
        }


        // Valida los parámetros cargados y devuelve los errores encontrados
        public static StringBuilder ValidarParametros(StringBuilder resultado)
        {
            // Validar parámetros obligatorios
            if(string.IsNullOrEmpty(PdfEntrada))
            {
                resultado.AppendLine("El parámetro 'pdfEntrada' es obligatorio.");
            }

            if(!File.Exists(PdfEntrada))
            {
                resultado.AppendLine("El PDF de entrada no existe.");
            }

            // Chequea si se no ha pasado un fichero QR externo para validar los parametros necesarios para generarlo
            if(UsarQrExterno == false)
            {
                // Genera la URL de envío del QR si no se ha pasado segun el resto de parametros 
                if(string.IsNullOrEmpty(UrlEnvio))
                {
                    UrlEnvio = Utilidades.ObtenerUrl(EntornoProduccion, VeriFactu);
                }

                // Valida que se haya pasado el numero de factura
                if(string.IsNullOrEmpty(NumeroFactura))
                {
                    resultado.AppendLine("El parámetro 'numeroFactura' es obligatorio.");
                }

                // Valida que se haya pasado la fecha de la factura
                if(FechaFactura == DateTime.MinValue)
                {
                    resultado.AppendLine("El parámetro 'fechaFactura' es obligatorio.");
                }

                // Valida que se haya pasado el total de la factura
                if(TotalFactura == 0)
                {
                    resultado.AppendLine("El parámetro 'totalFactura' es obligatorio.");
                }

                // Valida si el color pasado es valido
                if(!Utilidades.ColorValido(ColorQR))
                {
                    resultado.AppendLine("El codigo de color del QR no es valido");
                }

                // Con los datos anteriores correctos se genera la URL de envio del QR
                Utilidades.GenerarURL();
            }

            // En caso de que se pase un fichero con el QR, valida que exista
            else
            {
                // Chequea que el fichero del QR existe
                if(!File.Exists(NombreFicheroQR))
                {
                    resultado.AppendLine("El fichero del código QR no existe.");
                }
            }

            return resultado;
        }
    }
}
