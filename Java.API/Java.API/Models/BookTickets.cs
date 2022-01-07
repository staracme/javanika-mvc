using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Java.API.Models
{
    public class BookTickets
    {
    }

    public class BookResponse
    {
        public string status { get; set; }
        public int orderID { get; set; }
        public int no_of_tickets { get; set; }
        public int ticket_stock { get; set; }

    }

    public class OrderInput
    {
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public int NoOfTickets { get; set; }
        public decimal Price { get; set; }
        public decimal TotalAmount { get; set; }
        public string TicketType { get; set; }
    }
}