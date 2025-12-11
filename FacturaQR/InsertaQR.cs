using System;
using System.Drawing;
using System.Text;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using QRCoder;

namespace FacturaQR
{
    public static class InsertaQR
    {
        public static StringBuilder InsertarQR(PdfPage pagina, XGraphics gfx, StringBuilder resultado)
        {
            
            // Configuracion de las propiedades del QR
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
                // Genera el codigo QR segun si es una imagen o se han pasado los datos
                XImage qrImage = GenerarQR(textoQr);

                // Ajuste de la posicion del QR por si hay desbordamiento a la derecha
                double desbordaDerecha = posX + ancho - pagina.Width;
                if(desbordaDerecha > 0)
                {
                    posX -= desbordaDerecha + 10;
                }


                // Primero se inserta la marca de agua (si tiene contenido) para que quede debajo del todo
                if(!string.IsNullOrEmpty(Configuracion.MarcaAgua))
                {
                    pagina = Utilidades.InsertaMarcaAgua(pagina, gfx, Configuracion.MarcaAgua);
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

                // Libera la imagen del QR
                qrImage.Dispose();

            }

            // Captura de error si no esta diponible el programa de impresion
            catch(InvalidOperationException ex)
            {
                resultado.AppendLine(ex.Message);
            }

            // Captura el error generico al insertar el QR
            catch(Exception ex)
            {
                resultado.AppendLine($"Error al insertar el QR: {ex.Message}");
            }

            return resultado;
        }

        private static XImage GenerarQR(string textoQr)
        {
            // Objeto para almacenar el código QR generado
            XImage qrGenerado;

            // Carga o genera el código QR
            if(Configuracion.UsarQrExterno == true)
            {
                // Si se pasa un fichero externo, se carga la imagen en el objeto QR
                qrGenerado = XImage.FromFile(Configuracion.NombreFicheroQR);
            }
            else
            {
                // En otro caso se genera el código QR a partir del texto proporcionado
                using(QRCodeGenerator qrGenerator = new QRCodeGenerator())
                using(QRCodeData qrCodeData = qrGenerator.CreateQrCode(textoQr, QRCodeGenerator.ECCLevel.Q))
                using(QRCode qrCode = new QRCode(qrCodeData))
                using(Bitmap qrBitmap = qrCode.GetGraphic(20))
                using(var ms = new System.IO.MemoryStream())
                {
                    qrBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    qrGenerado = XImage.FromStream(ms);
                }

            }

            return qrGenerado;
        }

        
    }
}

