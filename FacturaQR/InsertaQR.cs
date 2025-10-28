using System;
using System.Drawing;
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
            string resultado = string.Empty;
            string rutaPdfOriginal = Configuracion.PdfEntrada;
            string rutaPdfSalida = Configuracion.PdfSalida;
            string textoQr = Configuracion.UrlEnvio;
            string textoArriba = Configuracion.TextoArriba;
            string textoAbajo = Configuracion.TextoAbajo;

            // Convierte las posiciones X e Y, y el tamaño del QR a unidades de punto (1/72 pulgadas)
            double posX = XUnit.FromMillimeter(Configuracion.PosX).Point;
            double posY = XUnit.FromMillimeter(Configuracion.PosY).Point;
            double ancho = XUnit.FromMillimeter(Configuracion.Ancho).Point;
            double alto = XUnit.FromMillimeter(Configuracion.Alto).Point;

            try
            {
                PdfDocument documento = PdfReader.Open(rutaPdfOriginal, PdfDocumentOpenMode.Modify);

                XImage qrImage = null;

                // Carga o genera el código QR
                if(Configuracion.UsarQrExterno == true)
                {
                    // Si se pasa un fichero externo, se carga directamente
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

                // Se añade el QR a la primera página del PDF
                PdfPage pagina = documento.Pages[0];
                XGraphics gfx = XGraphics.FromPdfPage(pagina);

                // No se inserta texto si el QR es externo
                double altoFuente = 9; // Altura aproximada del texto en puntos

                // Fuente para los textos
                XFont font = new XFont("Arial", altoFuente, XFontStyle.Bold);

                // Inserta el texto arrib del QR
                if(Configuracion.UsarQrExterno != true) // Solo se pone el texto cuando no se use el QR externo
                {
                    // Texto encima del QR (se deja un margen de 10 puntos)
                    gfx.DrawString(textoArriba, font, XBrushes.Black, new XRect(posX, posY - altoFuente, ancho, altoFuente), XStringFormats.Center);
                }

                // Inserta el QR
                gfx.DrawImage(qrImage, posX, posY, ancho, alto);

                // Inserta el texto debajo del QR
                if(Configuracion.UsarQrExterno != true) // Solo se pone el texto cuando no se use el QR externo
                {
                    // Texto debajo del QR (se deja un margen de 2 puntos ademas del alto de de la fuente)
                    gfx.DrawString(textoAbajo, font, XBrushes.Black, new XRect(posX, posY + alto, ancho, altoFuente), XStringFormats.Center);
                }

                qrImage.Dispose();

                documento.Save(rutaPdfSalida);

            }
            catch(Exception ex)
            {
                resultado = "Error al insertar el QR: " + ex.Message;
            }

            return resultado;
        }
    }
}
