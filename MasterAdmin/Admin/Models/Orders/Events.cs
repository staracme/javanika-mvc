using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Admin.Models
{
    public class TicketRequest
    {
        public int eventId { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public int ticketPercentile { get; set; }
    }

    public class EventsDTO
    {
        public int EventID { get; set; }
        public int VenueID { get; set; }
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
        public decimal ProcessingFee { get; set; }
        public int TicketStock { get; set; }
        public string Status { get; set; }

    }

    public class EventsViewModel
    {
        public int EventID { get; set; }
        public int VenueID { get; set; }
        public string VenueName { get; set; }
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
        public decimal ProcessingFee { get; set; }
        public int TicketStock { get; set; }
        public string Status { get; set; }
        public string EventStatus { get; set; }
        public decimal EBTickets { get; set; }

    }
    public class EventsImageListViewModel
    {
        public int PastEventID { get; set; }
        public string EventName { get; set; }
        public string ImagePath { get; set; }
        public string Year { get; set; }
        public string Month { get; set; }
        public DateTime? CreatedDate { get; set; }
        public decimal? EventID { get; set; }
        public string ImageType { get; set; }

    }
    public class EventArtistViewModel
    {
        public int ArtistID { get; set; }
        public int EventID { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string EventName { get; set; }
        public DateTime? EventDate { get; set; }
        public string Status { get; set; }

    }

    public class EventRequest : IPaginationRequest
    {
        public int? EventId { get; set; }
        public int rowsPerPage { get; set; }
        public int pageNum { get; set; }
    }
}