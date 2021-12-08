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
    public class Events
    {
        public string status { get; set; }
        public string error_message { get; set; }
        public List<Event> past_events { get; set; }
        public List<Event> upcoming_events { get; set; }
    }

    public class EventsController : ApiController
    {
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public Events GetEvents()
        {
            JAVADBEntities db = new JAVADBEntities();

            var upcoming = db.tblEvents.SqlQuery("SELECT * FROM TBLEVENTS WHERE CONVERT(DATE,EVENTDATE,103) >= CONVERT(DATE,GETDATE(),103) and EVENTID IN (3039,3040) ORDER BY EVENTID DESC").ToList();
            var past = db.tblEvents.SqlQuery("SELECT * FROM TBLEVENTS WHERE CONVERT(DATE,EVENTDATE,103) < CONVERT(DATE,GETDATE(),103)  and status = 'a' ORDER BY EVENTID DESC").ToList();

            List<Event> upcoming_events = new List<Event>();
            List<Event> past_events = new List<Event>();
            Events events = new Events();

            try
            {
                foreach (var evt in upcoming)
                {
                    upcoming_events.Add(new Event()
                    {
                        EventID = evt.EventID,
                        EventName = evt.EventName,
                        EventDate = Convert.ToDateTime(evt.EventDate).ToString("dd MMM yyyy"),
                        image = "http://admin.javanika.com/EventBanners/" + evt.EventBanner,
                    });
                }

                foreach (var evt in past)
                {
                    past_events.Add(new Event()
                    {
                        EventID = evt.EventID,
                        EventName = evt.EventName,
                        EventDate = Convert.ToDateTime(evt.EventDate).ToString("dd MMM yyyy"),
                        image = "http://admin.javanika.com/EventBanners/" + evt.EventBanner,
                    });
                }

                events.status = "OK";
                events.upcoming_events = upcoming_events;
                events.past_events = past_events;
                return events;
            }
            catch(Exception ex)
            {
                events.status = "ERROR";
                events.error_message = (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                return events;
            }
        }
    }
}
