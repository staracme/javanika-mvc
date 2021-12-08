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
    public class TicketModel
    {
        public int orderID { get; set; }
    }

    public class TicketResponse
    {
        public int orderID { get; set; }
        public string seats { get; set; }
        public string qrCode { get; set; }
        public int eventID { get; set; }
        public string event_name { get; set; }
        public string venue_name { get; set; }
        public string event_date { get; set; }
        public string event_time { get; set; }
        public string event_banner { get; set; }
        public string srNo { get; set; }
    }

    public class TicketController : ApiController
    {
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public TicketResponse GetTikcet(TicketModel ti)
        {
            int ticketID = ti.orderID;

            JAVADBEntities db = new JAVADBEntities();

            TicketResponse response = new TicketResponse();

            var ticket = db.tblTicketOrders.Where(t => t.OrderID == ticketID).SingleOrDefault();
            var evt = db.tblEvents.Where(t => t.EventID == ticket.EventID).SingleOrDefault();
            var seats = db.tblSeatSelections.Where(s => s.OrderID == ticket.OrderID).ToList();

            response.orderID = ticket.OrderID;
            response.qrCode = "http://javanika.com/QrCodes/" + ticket.QRCode;
            response.eventID = evt.EventID;
            response.event_name = evt.EventName;
            response.event_date = Convert.ToDateTime(evt.EventDate).ToString("dddd, dd MMMM yyyy");
            response.event_time = evt.ShowTime;
            response.srNo = ticket.OrderNo.ToString();
            response.seats = string.Join(",", seats.Select(s => s.tblSeat.SeatNumber));
            response.venue_name = evt.tblLayout.LayoutName;
            response.event_banner = "http://admin.javanika.com/EventBanners/" + evt.EventBanner;

            return response;
        }
    }
}
