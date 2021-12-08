using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using QRCoder;
using System.Web;
using System.Drawing;
using System.Web.Http;
using System.Web.Http.Cors;
namespace Java.API.Controllers
{
    public class TestQRController : ApiController
    {
        [HttpGet]
        public void TestQR()
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode("Pranav", QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            qrCodeImage.Save("C:\\Websites\\DevFrontEnd\\QRCodes\\" + "qrTest" + ".png");

        }
    }
}
