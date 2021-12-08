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
    public class HomePageResponse
    {
        public List<Event> past_events { get; set; }
    
        public List<Event> current_events { get; set; }

        public string appVersion { get; set; }
    }

    public class Event
    {
        public int EventID { get; set; }
        public string EventName { get; set; }
        public string image { get; set; }
        public string SliderImage { get; set; }
        public string EventDate { get; set; }
    }



    public class EventModel
    {
        public int EventID { get; set; }
        public string EventName { get; set; }
        public string EventType { get; set; }
        public string EventLanguage { get; set; }
        public string EventPlayTime { get; set; }
        public string EventBanner { get; set; }
    }


    public class HomePageController : ApiController
    {
        //GET api/values
        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HomePageResponse GetHomePage()
        {
            JAVADBEntities db = new JAVADBEntities();

            HomePageResponse response = new HomePageResponse();

            var events = db.tblEvents.Where(t => t.Status == "a").ToList();
            string version = "";
            List<Event> past_events = new List<Event>();
            List<Event> current_events = new List<Event>();

            string current_events_sql = "SELECT TOP 2 * FROM TBLEVENTS WHERE CONVERT(DATE,EVENTDATE,103) >= CONVERT(DATE,GETDATE(),103) and status = 'a' ORDER BY EVENTID DESC";
            string past_events_sql = "SELECT TOP 2 * FROM TBLEVENTS WHERE CONVERT(DATE, EVENTDATE, 103) < CONVERT(DATE, GETDATE(), 103) and status = 'a' ORDER BY EVENTID DESC";


            var past = db.tblEvents.SqlQuery(past_events_sql).ToList();
            var current = db.tblEvents.SqlQuery(current_events_sql).ToList();


            List<tblAppVersion> versions = db.tblAppVersions.ToList();

            if (versions.Count() > 0)
            {
                tblAppVersion latestVersion = versions.OrderByDescending(v => v.AppVersionID).Take(1).SingleOrDefault();
                version = latestVersion.Version;
            }

            foreach (var evt in past)
            {
                past_events.Add(new Event()
                {
                    EventID = evt.EventID,
                    EventName = Common.GetContent(evt.EventName, 17),
                    image = "http://admin.javanika.com/EventBanners/" + evt.EventBanner,
                    SliderImage = "http://admin.javanika.com/HomeBanners/" + evt.EventHomeBanner,
                    EventDate = Convert.ToDateTime(evt.EventDate).ToString("dd MMM yyyy")
                });
            }

            foreach (var evt in current)
            {
                current_events.Add(new Event()
                {
                    EventID = evt.EventID,
                    EventName = Common.GetContent(evt.EventName, 17),
                    SliderImage = "http://admin.javanika.com/HomeBanners/" + evt.EventHomeBanner,
                    image = "http://admin.javanika.com/EventBanners/" + evt.EventBanner,
                    EventDate = Convert.ToDateTime(evt.EventDate).ToString("dd MMM yyyy")
                });
            }

            response.current_events = current_events;
            response.past_events = past_events;
            response.appVersion = version;

            return response;
        }
    }
}
