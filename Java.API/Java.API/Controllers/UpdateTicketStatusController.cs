using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Java.API.Models;
using System.Web.Http.Cors;
namespace Java.API.Controllers
{
    public class UpdateTicketStatusController : ApiController
    {
        JAVADBEntities db = new JAVADBEntities();

        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public CheckTicketResponse updateTicketStatus(string ticketID)
        {
            CheckTicketResponse response = new CheckTicketResponse();

            try
            {
                int tID = Convert.ToInt32(ticketID);
                var ticket = db.tblTicketOrders.Where(t => t.OrderID == tID).SingleOrDefault();
                ticket.IsPickedUp = true;
                ticket.DatePickedUp = DateTime.Now;
                db.SaveChanges();

                response.isPickedUp = Convert.ToBoolean(ticket.IsPickedUp);
                response.ticketID = Convert.ToInt32(ticketID);
                response.seats = Convert.ToInt32(ticket.NoOfTickets);
                response.status = "OK";
                return response;
            }
            catch (Exception ex)
            {
                response.status = "ERROR";
            }
            return response;
        }

    }
}
