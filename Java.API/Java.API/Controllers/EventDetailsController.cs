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
    public class EventDetailsResponse
    {
        public int EventID { get; set; }
        public string EventName { get; set; }
        public string Banner { get; set; }
        public string EventType { get; set; }
        public string EventLanguage { get; set; }
        public string EventDate { get; set; }
        public string EventPlaytime { get; set; }
        public string ShowTime { get; set; }
        public string AgeGroup { get; set; }
        public string Description { get; set; }
        public string VenueName { get; set; }
        public decimal Price { get; set; }
        public bool isPastEvent { get; set; }
        public List<Artist> Artists { get; set; }

        public string SeatingChart { get; set; }
    }

    public class Artist
    {
        public int ArtistID { get; set; }
        public string Image { get; set; }
        public string ArtistName { get; set; }
    }

    public class EventDetailModel
    {
        public int eventID { get; set; }
    }

    public class EventDetailsController : ApiController
    {
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public EventDetailsResponse GetEventDetails(EventDetailModel e)
        {
            JAVADBEntities db = new JAVADBEntities();

            var evt = db.tblEvents.Where(ev => ev.EventID == e.eventID).SingleOrDefault();

            var view_image_path = Common.GetViewImagePath();

            EventDetailsResponse response = new EventDetailsResponse();

            List<Artist> iArtists = new List<Artist>();

            decimal min_price = Convert.ToDecimal(db.tblEventLayoutBlocks.Where(ev => ev.EventID == evt.EventID).Min(s => s.Price));

            var isPastEvent = db.tblEvents.SqlQuery("SELECT * FROM TBLEVENTS WHERE CONVERT(DATE,EVENTDATE,103) < CONVERT(DATE,GETDATE(),103) AND EVENTID = " + evt.EventID + "").Any();

            response.EventID = evt.EventID;
            response.EventName = evt.EventName;
            response.EventLanguage = evt.EventLanguage;
            response.Banner = view_image_path + "/MainBanners/" + evt.EventMainBanner;
            response.Description = evt.EventDescription;
            response.AgeGroup = evt.AgeGroup;
            response.EventDate = (evt.EventDate != null ? Convert.ToDateTime(evt.EventDate).ToString("dddd, dd MMMM") : "");
            response.EventPlaytime = evt.EventPlaytime;
            response.VenueName = (evt.tblVenue != null ? evt.tblVenue.VenueName : "");
            response.ShowTime = evt.ShowTime;
            response.Price = min_price;
            response.isPastEvent = isPastEvent;
            response.SeatingChart = evt.tblVenue.AppSeatingChart;

            var artists = db.tblEventArtists.Where(a => a.EventID == e.eventID).ToList();

            foreach(var artist in artists)
            {
                iArtists.Add(new Artist() {
                    ArtistID = artist.ArtistID,
                    ArtistName = artist.Name,
                    Image = view_image_path + "/ArtistImages/" + artist.Image
                });
            }

            response.Artists = iArtists;


            return response;
        }
    }
}
