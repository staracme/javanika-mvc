using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Admin.Models
{
    public class Event
    {
        public int LayoutID { get; set; }
        public string EventName { get; set; }
        public string EventType { get; set; }
        public string EventLanguage { get; set; }
        public string EventPlaytime { get; set; }
        public string EventDurationType { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ShowTime { get; set; }
        public string EventBanner { get; set; }
        public string EventMainBanner { get; set; }
        public string EventHomeBanner { get; set; }
        public string LocationTracker { get; set; }
        public string EventDescription { get; set; }
        public string EventTNC { get; set; }
        public string AgeGroup { get; set; }
        public DateTime CreatedDate { get; set; }
        public string BookingType { get; set; }
        public Decimal ProcessingFee { get; set; }
        public int TicketStock { get; set; }
        public string Status { get; set; }
        public int PercentEBSeats { get; set; }
        public string EarlyBirdTicketSelection { get; set; }
        public int TicketsAvailable { get; set; }
        public Decimal TicketPrice { get; set; }
        public Decimal EBTicketPrice { get; set; }
        public int EBMinTicketOrder { get; set; }
        public int EBMaxTicketOrder { get; set; }

    }
}