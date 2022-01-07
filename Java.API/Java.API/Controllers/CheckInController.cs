using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using Java.API.Models;

namespace Java.API.Controllers
{
    public class CheckInController : ApiController
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
                var ticket = db.tblTicketOrders.Where(f => f.OrderID == ticketID && f.EventID == e.eventID).SingleOrDefault();

                if (ticket != null)
                {
                    ticket.IsPickedUp = true;
                    ticket.DatePickedUp = DateTime.Now;
                    db.SaveChanges();

                    response.status = "OK";
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
