using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

namespace SistemaConferenciaPedidos.Services
{
    public class LeituraCodigoService
    {
        public Bitmap GerarBitmapViaLabelary(string zpl)
        {
            try
            {
                using var client = new HttpClient();

                var content = new StringContent(
                    zpl,
                    Encoding.UTF8,
                    "application/x-www-form-urlencoded");

                var response = client
                    .PostAsync("http://api.labelary.com/v1/printers/8dpmm/labels/4x6/0/", content)
                    .Result;

                if (!response.IsSuccessStatusCode)
                    return null;

                var bytes = response.Content.ReadAsByteArrayAsync().Result;

                using var ms = new MemoryStream(bytes);
                return new Bitmap(ms);
            }
            catch
            {
                return null;
            }
        }

        public string LerCodigoDaImagem(Bitmap bitmap)
        {
            if (bitmap == null)
                return "";

            var reader = new BarcodeReader
            {
                AutoRotate = true,
                Options = new DecodingOptions
                {
                    TryHarder = true,
                    PossibleFormats = new List<BarcodeFormat>
                    {
                        BarcodeFormat.CODE_128,
                        BarcodeFormat.CODE_39,
                        BarcodeFormat.EAN_13,
                        BarcodeFormat.EAN_8,
                        BarcodeFormat.QR_CODE,
                        BarcodeFormat.DATA_MATRIX,
                        BarcodeFormat.PDF_417,
                        BarcodeFormat.CODABAR,
                        BarcodeFormat.ITF
                    }
                }
            };

            var result = reader.Decode(bitmap);

            if (result != null && !string.IsNullOrWhiteSpace(result.Text))
                return result.Text.Trim();

            return "";
        }
    }
}