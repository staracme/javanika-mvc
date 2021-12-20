using AutoMapper;
using Java.API.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Java.API.Controllers
{
    public class GalleryModel
    {
        public string sessionID { get; set; }
        public string coupon_code { get; set; }
        public int eventID { get; set; }
    }
    public class GalleryController : ApiController
    {
        JAVADBEntities db = new JAVADBEntities();

        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public List<EventsImageListViewModel> getPastEventImages(GalleryModel input)
        {
            List<EventsImageListViewModel> eventImageListVM = null;
            eventImageListVM = new List<EventsImageListViewModel>();
            var res = db.tblPastEvents.Where(e => e.EventID == input.eventID).ToList();
            Mapper.Map(res, eventImageListVM);
            return eventImageListVM;
        }

        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public List<EventsViewModel> getPastEvents()
        {
            List<EventsViewModel> eventsVM = null;
            eventsVM = new List<EventsViewModel>();
            var res = db.tblEvents.OrderByDescending(p => p.EventDate).ToList();
            Mapper.Map(res, eventsVM);
            return eventsVM;
        }
    }
}
