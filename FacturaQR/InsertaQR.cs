using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;
using QRCoder;
using System.Drawing;

namespace FacturaQR
{
    public class InsertaQR
    {
        public static void InsertarQrConTexto(
            string rutaPdfOriginal,
            string rutaPdfSalida,
            string textoQr,
            string textoArriba,
            string textoAbajo,
            double posX,
            double posY,
            double ancho,
            double alto)
        {
            PdfDocument documento = PdfReader.Open(rutaPdfOriginal, PdfDocumentOpenMode.Modify);

            using(QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using(QRCodeData qrCodeData = qrGenerator.CreateQrCode(textoQr, QRCodeGenerator.ECCLevel.Q))
            using(QRCode qrCode = new QRCode(qrCodeData))
            using(Bitmap qrBitmap = qrCode.GetGraphic(20))
            {
                XImage qrImage;
                using(var ms = new System.IO.MemoryStream())
                {
                    qrBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    qrImage = XImage.FromStream(ms);
                }

                foreach(PdfPage pagina in documento.Pages)
                {
                    XGraphics gfx = XGraphics.FromPdfPage(pagina);

                    // Fuente para los textos
                    XFont font = new XFont("Arial", 8, XFontStyle.Bold);

                    // Texto encima del QR
                    gfx.DrawString(textoArriba, font, XBrushes.Black, new XRect(posX, posY - 10, ancho, 10), XStringFormats.Center);

                    // QR
                    gfx.DrawImage(qrImage, posX, posY, ancho, alto);

                    // Texto debajo del QR
                    gfx.DrawString(textoAbajo, font, XBrushes.Black, new XRect(posX, posY + alto, ancho, 10), XStringFormats.Center);
                }
            }

            documento.Save(rutaPdfSalida);
        }

    }
}
