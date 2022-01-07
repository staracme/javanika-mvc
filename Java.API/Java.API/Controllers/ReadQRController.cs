using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using Java.API.Models;

namespace Java.API.Controllers
{
    public class QRTicketModel
    { 
        public string ticketID { get; set; }
        public int eventID { get; set; }
    }


    public class QRTicketResponse
    { 
        public string ticketID { get; set; }
        public int srNo { get; set; }
        public string no_of_seats { get; set; }
        public string status { get; set; }
        public string error_message { get; set; }
        public string is_scanned { get; set; }
        public string scanned_date { get; set; }
    }


    public class ReadQRController : ApiController
    {
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public QRTicketResponse GetEventDetails(QRTicketModel e)
        {
            JAVADBEntities db = new JAVADBEntities();
            QRTicketResponse response = new QRTicketResponse();

            try
            {
                int ticketID = Convert.ToInt32(e.ticketID);
                var ticket = db.tblTicketOrders.Where(f => f.OrderID == ticketID && f.EventID == e.eventID && f.Status == "SUCCESS" && f.PaymentStatus == "COMPLETED").SingleOrDefault();

                if (ticket != null)
                {
                    response.status = "OK";
                    response.srNo = Convert.ToInt32(ticket.OrderNo);
                    response.ticketID = Convert.ToInt32(ticket.OrderID).ToString();
                    response.no_of_seats = Convert.ToInt32(ticket.NoOfTickets).ToString();
                    response.is_scanned = (ticket.IsPickedUp == true ? "YES" : "NO");
                    response.scanned_date = (ticket.IsPickedUp == true && ticket.DatePickedUp != null ? Convert.ToDateTime(ticket.DatePickedUp).ToString("yyyy-MM-dd") : "-");
                    return response;
                }
                else
                    response.status = "INVALID_TICKET";
                return response;
            }
            catch (Exception ex)
            {
                response.status = "ERROR";
                response.error_message = (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                return response;
            }
        }
    }
}
