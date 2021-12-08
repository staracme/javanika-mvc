using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using Java.API.Models;
using System.Net;
using System.Net.Mail;
namespace Java.API.Controllers
{
    public class OTPModel
    {
        public string email { get; set; }
    }

    public class OTPResponse
    {
        public string email { get; set; }

        public string otp { get; set; }
        public string status { get; set; }
    }

    public class OTPController : ApiController
    {
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public OTPResponse OTP(OTPModel model)
        {
            JAVADBEntities db = new JAVADBEntities();

            OTPResponse response = new OTPResponse();

            var exists = db.tblTicketOrders.Where(w => w.Email == model.email).Any();

            if (exists)
            {
                string username = System.Configuration.ConfigurationManager.AppSettings["Username"].ToString();
                string password = System.Configuration.ConfigurationManager.AppSettings["Password"].ToString();


                Random generator = new Random();
                String otp = generator.Next(0, 100000).ToString("D5");

                SmtpClient smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com", // smtp server address here…
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new System.Net.NetworkCredential(username, password),
                    Timeout = 30000,
                };

                tblOTP tblotp = new tblOTP();
                tblotp.OTP = otp;
                tblotp.Email = model.email;
                db.tblOTPS.Add(tblotp);
                db.SaveChanges();


                MailMessage message = new MailMessage(username, model.email, "Your OTP", "Your OTP is " + otp);
                message.IsBodyHtml = true;
                smtp.Send(message);

                response.email = model.email;
                response.otp = otp;
                response.status = "OK";
                return response; 
            }
            else
            {
                response.email = model.email;
                response.otp = "";
                response.status = "NOT FOUND";

                return response;
            }
        }
    }
}
