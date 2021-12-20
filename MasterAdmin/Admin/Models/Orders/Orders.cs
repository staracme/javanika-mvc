using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Admin.Models
{
    public class Orders
    {
    }

    public class EventOrderModel
    {
        public int EventID { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public int TicketsCount { get; set; }
        public string EventStatus { get; set; }
    }

    public class EventOrderDetails
    {
        public int OrderNo { get; set; }
        public int OrderID { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public int Quantity { get; set; }
        public decimal TicketPrice { get; set; }
        public decimal AmountPerTicket { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ProcessingFee { get; set; }
        public decimal ActualPrice { get; set; }
        public decimal PriceAfterDiscount { get; set; }
        public string TicketType { get; set; }
        public string TicketOrderStatus { get; set; }
        public string TicketPaymentStatus { get; set; }
        public string PaypalOrderID { get; set; }
        public string Intent { get; set; }
        public string VenueName { get; set; }
        public string EventStatus { get; set; }
    }

    public class OrderSummary
    {
        public int OrderCount { get; set; }
        public int TicketCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class OrderDetails
    {
        public List<EventOrderDetails> eventOrderDetails { get; set; }
        public OrderSummary orderSummary { get; set; }
    }
}