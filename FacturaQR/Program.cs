using System;

namespace FacturaQR
{
    public class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 8)
            {
                Console.WriteLine("Uso: FacturaQR <pdfOriginal> <pdfSalida> <textoQr> <textoArriba> <textoAbajo> <posX> <posY> <ancho> <alto>");
                return;
            }

            try
            {
                string pdfOriginal = args[0];
                string pdfSalida = args[1];
                string textoQr = args[2];
                string textoArriba = args[3];
                string textoAbajo = args[4];
                double posX = double.Parse(args[5]);
                double posY = double.Parse(args[6]);
                double ancho = double.Parse(args[7]);
                double alto = double.Parse(args[8]);

                InsertaQR.InsertarQrConTexto(pdfOriginal, pdfSalida, textoQr, textoArriba, textoAbajo, posX, posY, ancho, alto);

                Console.WriteLine($"QR insertado correctamente en '{pdfSalida}'");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error al insertar el QR: " + ex.Message);
            }
        }


    }
}
