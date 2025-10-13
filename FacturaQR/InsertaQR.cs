using System;
using System.Collections.Generic;
using System.Drawing;
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

                Color colorQR = ColorTranslator.FromHtml(Configuracion.ColorQR);

                using(QRCodeGenerator qrGenerator = new QRCodeGenerator())
                using(QRCodeData qrCodeData = qrGenerator.CreateQrCode(textoQr, QRCodeGenerator.ECCLevel.Q))
                using(QRCode qrCode = new QRCode(qrCodeData))
                using(Bitmap qrBitmap = qrCode.GetGraphic(20, colorQR, Color.White, true))
                {
                    XImage qrImage;
                    using(var ms = new System.IO.MemoryStream())
                    {
                        qrBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        ms.Position = 0;
                        qrImage = XImage.FromStream(ms);
                    }

                    // Se añade el QR a la primera página del PDF
                    PdfPage pagina = documento.Pages[0];
                    XGraphics gfx = XGraphics.FromPdfPage(pagina);

                    // Insertar marca de agua (solo si tiene contenido)
                    if(!string.IsNullOrEmpty(Configuracion.MarcaAgua))
                    {
                        string marcaAgua = Configuracion.MarcaAgua;

                        // Fuente y pincel para dibujar el texto
                        XFont fuenteMarca = new XFont("Arial", 40, XFontStyle.BoldItalic);
                        XBrush pincelMarca = new XSolidBrush(XColor.FromArgb(0, 245, 245, 245)); // Gris muy claro (el primer cero es la transparencia pero no se puede aplicar a un PDF)

                        // Ajuste en varias lineas si es necesario
                        List<string> lineas = new List<string>();
                        string[] bloques = marcaAgua.Split(new string[] {"\n"}, StringSplitOptions.None);
                        string linea = "";

                        // Calculo del ancho maximo de la marca de agua
                        double margenMm = 20;
                        double anchoPagina = pagina.Width.Point;
                        double altoPagina = pagina.Height.Point;
                        double anchoDiagonal = Math.Sqrt(Math.Pow(anchoPagina, 2) + Math.Pow(altoPagina, 2));
                        double margen = XUnit.FromMillimeter(margenMm).Point;
                        double factorAjuste = 0.85; // Se aplica un ajuste ya que al rotar el texto se desplazan los margenes del recuadro donde se dibuja.
                        double anchoMaximo = (anchoDiagonal - 2 * margen) * factorAjuste;

                        foreach(var bloque in bloques)
                        {
                            string[] palabras = bloque.Split(' ');

                            foreach(var palabra in palabras)
                            {
                                // Primera parte, añadir a la linea actual
                                string textoLinea = string.IsNullOrEmpty(linea) ? palabra : linea + " " + palabra;
                                XSize size = gfx.MeasureString(textoLinea, fuenteMarca);

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

                        // Calcula el centro de la pagina para rotar
                        double centroX = pagina.Width / 2;
                        double centroY = pagina.Height / 2;

                        // Se añade un desplazamiento para ajustar la posicion del texto dentro de la pagina (al rotar se desplaza)
                        double offsetX = 0;
                        double offsetY = 0;

                        // Se guarda la configuracion para aplicarla solo a la marca de agua
                        gfx.Save();

                        // Rotacion 45 grados a la izquierda para poner la marca de agua
                        gfx.RotateAtTransform(-45, new XPoint(centroX, centroY));

                        // Se aplica el desplazamiento a la posicion del texto.
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
                    }

                    // Insertar el codigo QR
                    double altoFuente = 9; // Altura aproximada del texto en puntos

                    // Fuente para los textos
                    XFont font = new XFont("Arial", altoFuente, XFontStyle.Bold);

                    // Texto encima del QR (se deja un margen de 10 puntos)
                    gfx.DrawString(textoArriba, font, XBrushes.Black, new XRect(posX, posY - altoFuente, ancho, altoFuente), XStringFormats.Center);

                    // QR
                    gfx.DrawImage(qrImage, posX, posY, ancho, alto);

                    // Texto debajo del QR (se deja un margen de 2 puntos ademas del alto de de la fuente)
                    gfx.DrawString(textoAbajo, font, XBrushes.Black, new XRect(posX, posY + alto, ancho, altoFuente), XStringFormats.Center);
                }
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
