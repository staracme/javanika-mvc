using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Admin.Models
{
    public class OrderResponse
    {
        public int OrderID { get; set; }
        public int OrderNo { get; set; }
        public string EventName { get; set; }
        public string Mobile { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public Nullable<decimal> TicketPrice { get; set; }
        public int Quantity { get; set; }
        public Nullable<decimal> TotalAmount { get; set; }
        public string ServiceFees { get; set; }
        public Nullable<decimal> NetAmount { get; set; }
        public string BOOKINGTYPE { get; set; }

        public string EventDate { get; set; }
    }
}