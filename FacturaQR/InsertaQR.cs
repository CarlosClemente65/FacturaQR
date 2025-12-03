using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Documents;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using QRCoder;

namespace FacturaQR
{
    public class InsertaQR
    {
        public static string InsertarQR()
        {
            // Asignacion de propiedades para usarlas en la clase
            string resultado = string.Empty;
            string rutaPdfOriginal = Configuracion.PdfEntrada;
            string rutaPdfSalida = Configuracion.PdfSalida;
            string textoQr = Configuracion.UrlEnvio ?? string.Empty;
            string textoArriba = Configuracion.TextoArriba;
            string textoAbajo = Configuracion.TextoAbajo;

            // Convierte las posiciones X e Y, y el tamaño del QR a unidades de punto (1/72 pulgadas)
            double posX = XUnit.FromMillimeter(Configuracion.PosX).Point;
            double posY = XUnit.FromMillimeter(Configuracion.PosY).Point;
            double ancho = XUnit.FromMillimeter(Configuracion.Ancho).Point;
            double alto = XUnit.FromMillimeter(Configuracion.Alto).Point;


            // Convierte el color hexadecimal para usarlo en el QR
            Color colorQR = ColorTranslator.FromHtml(Configuracion.ColorQR);

            try
            {
                // Abre el PDF al que insertar las imagenes
                PdfDocument documento = PdfReader.Open(rutaPdfOriginal, PdfDocumentOpenMode.Modify);

                // Crea el objeto del QR para luego leerlo del fichero o generarlo
                XImage qrImage = null;

                // Carga o genera el código QR
                if(Configuracion.UsarQrExterno == true)
                {
                    // Si se pasa un fichero externo, se carga la imagen en el objeto QR
                    qrImage = XImage.FromFile(Configuracion.NombreFicheroQR);
                }
                else
                {
                    // Si no, se genera el código QR a partir del texto proporcionado
                    using(QRCodeGenerator qrGenerator = new QRCodeGenerator())
                    using(QRCodeData qrCodeData = qrGenerator.CreateQrCode(textoQr, QRCodeGenerator.ECCLevel.Q))
                    using(QRCode qrCode = new QRCode(qrCodeData))
                    using(Bitmap qrBitmap = qrCode.GetGraphic(20))
                    {
                        using(var ms = new System.IO.MemoryStream())
                        {
                            qrBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            ms.Position = 0;
                            qrImage = XImage.FromStream(ms);
                        }
                    }
                }

                // Se estable la pagina 1 del PDF para añadir las imagenes (QR y marca de agua)
                PdfPage pagina = documento.Pages[0];

                // Ajuste de la posicion del QR por si hay desbordamiento a la derecha
                double desbordaDerecha = posX + ancho - pagina.Width;
                if(desbordaDerecha > 0)
                {
                    posX -= desbordaDerecha + 10;
                }

                // Se crea un recuadro donde se incluira el QR y los textos
                XGraphics gfx = XGraphics.FromPdfPage(pagina);

                // Primero se inserta la marca de agua (si tiene contenido) para que quede debajo del todo
                string marcaAgua = Configuracion.MarcaAgua;
                if(!string.IsNullOrEmpty(marcaAgua))
                {
                    pagina = InsertaMarcaAgua(pagina, gfx);
                }

                double altoFuente = 8; // Altura aproximada del texto en puntos

                // Fuente para los textos
                XFont font = new XFont("Arial", altoFuente, XFontStyle.Bold);

                // Color a aplicar a los textos igual al del QR
                XBrush brocha = new XSolidBrush(XColor.FromArgb(colorQR.A, colorQR.R, colorQR.G, colorQR.B));

                // Primero se inserta el texto arriba del QR
                gfx.DrawString(textoArriba, font, brocha, new XRect(posX, posY - altoFuente, ancho, altoFuente), XStringFormats.Center);

                // Despues se inserta el QR
                gfx.DrawImage(qrImage, posX, posY, ancho, alto);

                // Por ultimo se inserta el texto debajo del QR y centrado
                gfx.DrawString(textoAbajo, font, brocha, new XRect(posX, posY + alto, ancho, altoFuente), XStringFormats.Center);

                qrImage.Dispose();

                // Guarda el PDF modificado en la ruta de salida
                documento.Save(rutaPdfSalida);

                // Si se ha pasado el parametro de impresion, se lanza la impresion del PDF generado
                if(Configuracion.Imprimir)
                {
                    // Ruta del ejecutable SumatraPDF 
                    string sumatraExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SumatraPDF.exe");

                    // Controla si esta disponible el programa para evitar excepciones
                    if(!File.Exists(sumatraExe))
                    {
                        throw new InvalidOperationException("No se pudo lanzar la impresion del PDF.");
                    }

                    // Configura el proceso para lanzar la impresion silenciosa
                    var psi = new ProcessStartInfo
                    {
                        FileName = sumatraExe,
                        Arguments = $"-print-to-default -silent \"{rutaPdfSalida}\"",
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false
                    };

                    // Inicia el proceso de impresion
                    using(var proceso = Process.Start(psi))
                    {
                        // Espera a que SumatraPDF termine
                        proceso.WaitForExit();

                        // Comprueba el código de salida
                        if(proceso.ExitCode != 0)
                        {
                            throw new InvalidOperationException($"La impresión del PDF falló. Código de salida: {proceso.ExitCode}");
                        }
                    }
                }
            }

            // Captura de error si no esta diponible el programa de impresion
            catch (InvalidOperationException ex)
            {
                resultado = ex.Message;
            }

            // Captura el error generico al insertar el QR
            catch(Exception ex)
            {
                resultado = "Error al insertar el QR: " + ex.Message;
            }

            return resultado;
        }


        private static PdfPage InsertaMarcaAgua(PdfPage pagina, XGraphics gfx)
        {
            // Texto de la marca de agua
            string marcaAgua = Configuracion.MarcaAgua;

            // Fuente y pincel para dibujar el texto
            XFont fuenteMarca = new XFont("Arial", 20, XFontStyle.BoldItalic);
            XBrush pincelMarca = new XSolidBrush(XColor.FromArgb(0, 225, 225, 225)); // Gris muy claro (el primer cero es la transparencia pero no se puede aplicar a un PDF)

            // Ajuste en varias lineas si es necesario
            List<string> lineas = new List<string>();
            string[] bloques = marcaAgua.Split(new string[] { "\n" }, StringSplitOptions.None);
            string linea = "";

            // Se define un cuadrado seguro de 210x210 mm para insertar la marca
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
            double anchoMaximo = ladoCuadrado ;

            // Se divide el texto en lineas que no sobrepasen el ancho maximo
            foreach(var bloque in bloques)
            {
                string[] palabras = bloque.Split(' ');

                foreach(var palabra in palabras)
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
    }
}

