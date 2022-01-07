using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Admin.Models;
using Admin.Utility;

namespace Admin.Controllers
{
    public class EventsController : Controller
    {
        // GET: Events
        JAVADBEntities db = new JAVADBEntities();
        public ActionResult Index()
        {
            ViewData["Venues"] = db.tblVenues.ToList();
            return View(db.tblEvents.OrderByDescending(e => e.EventID).OrderByDescending(e=>e.EventID).ToList());
        }

        [ValidateInput(false)]
        public string ManageEvents()
        {
            try
            {
                string status = "i";

                tblEvent evt = null;

                if (Request["txtEventID"] != "0")
                {
                    int eventID = Convert.ToInt32(Request["txtEventID"]);
                    evt = db.tblEvents.Where(e => e.EventID == eventID).SingleOrDefault();
                    status = "u";
                }
                else
                    evt = new tblEvent();

                evt.EventName = Request["txtEventName"];
                evt.EventType = Request["txtEventType"];
                evt.EventDurationType = Request["drpEventDurationType"];
                evt.EventPlaytime = Request["txtEventPlaytime"];
                evt.EventLanguage = Request["txtEventLanguage"];
                evt.EventDate = Convert.ToDateTime(Request["txtEventDate"]);
                evt.ShowTime = Request["txtShowTime"];
                evt.BookingType = Request["drpBookingType"];

                if(!string.IsNullOrEmpty(Request["txtProcessingFee"]))
                    evt.ProcessingFee = Convert.ToDecimal(Request["txtProcessingFee"]);


                HttpPostedFileBase homeBannerImage = Request.Files["txtHomeBannerImage"];

                if (homeBannerImage != null && homeBannerImage.ContentLength > 0)
                {
                    string uniqueName = String.Format("{0:d9}", (DateTime.Now.Ticks / 10) % 1000000000);
                    string path = Server.MapPath("~/HomeBanners/");
                    string sFileName = sFileName = uniqueName + "_" + homeBannerImage.FileName;
                    homeBannerImage.SaveAs(path + sFileName);
                    evt.EventHomeBanner = sFileName;
                }

                HttpPostedFileBase mainBannerImage = Request.Files["txtMainBannerImage"];

                if (mainBannerImage != null && mainBannerImage.ContentLength > 0)
                {
                    string uniqueName = String.Format("{0:d9}", (DateTime.Now.Ticks / 10) % 1000000000);
                    string path = Server.MapPath("~/MainBanners/");
                    string sFileName = sFileName = uniqueName + "_" + mainBannerImage.FileName;
                    mainBannerImage.SaveAs(path + sFileName);
                    evt.EventMainBanner = sFileName;
                }

                HttpPostedFileBase eventBannerImage = Request.Files["txtEventBannerImage"];

                if (eventBannerImage != null && eventBannerImage.ContentLength > 0)
                {
                    string uniqueName = String.Format("{0:d9}", (DateTime.Now.Ticks / 10) % 1000000000);
                    string path = Server.MapPath("~/EventBanners/");
                    string sFileName = sFileName = uniqueName + "_" + eventBannerImage.FileName;
                    eventBannerImage.SaveAs(path + sFileName);
                    evt.EventBanner = sFileName;
                }

                if (!string.IsNullOrEmpty(Request["txtAgeGroup"]))
                    evt.AgeGroup = Request["txtAgeGroup"];

                if (!string.IsNullOrEmpty(Request["txtEventDesc"]))
                    evt.EventDescription = Request["txtEventDesc"];

                if (!string.IsNullOrEmpty(Request["txtEventTNC"]))
                    evt.EventTNC = Request["txtEventTNC"];

                evt.Status = "a";
                evt.EventDurationType = "S";
                if (status == "i")
                {
                    evt.CreatedDate = DateTime.Now;
                    db.tblEvents.Add(evt);
                }

                db.SaveChanges();


                if (status == "i")
                {
                    //dummy order for offline seat booking
                    tblTicketOrder order = new tblTicketOrder();
                    order.OrderNo = 0;
                    order.EventID = evt.EventID;
                    order.Name = "Nilaysh Shah";
                    order.Email = "niljag@yahoo.com";
                    order.Mobile = "5105795206";
                    order.NoOfTickets = 0;
                    order.AmountPerTicket = 0;
                    order.Amount = 0;
                    order.Status = "SUCCESS";
                    order.PaymentStatus = "COMPLETED";
                    order.PaypalOrderID = "OFFLINE-ORDER-ID";
                    order.Intent = "CAPTURE";
                    order.CreatedDate = DateTime.Now;
                    db.tblTicketOrders.Add(order);
                    db.SaveChanges();
                }

                return status;
            }
            catch(Exception ex)
            {
                if (ex.InnerException != null)
                    return ex.InnerException.Message;
                else
                    return ex.Message;
            }
        }


        public JsonResult GetLayout(int venueID)
        {
            var layouts = (from l in db.tblLayouts
                           where l.VenueID == venueID && l.Status == "a"
                           select new
                           {
                               layoutID = l.LayoutID,
                               layout_name = l.LayoutName,
                               layout_number = l.LayoutNumber
                           });

            return Json(layouts, JsonRequestBehavior.AllowGet);
        }

        public string GetBlocks(int layoutID)
        {
            var blocks = (from l in db.tblBlocks
                          where l.LayoutID == layoutID
                          select new
                          {
                              layoutID = l.LayoutID,
                              blockID = l.BlockID,
                              block_number = l.BlockNumber
                          }).ToList();

            var tiers = db.tblTiers.ToList();

            string str = "";

            foreach(var b in blocks)
            {
                str += "<tr>";
                str += "<td>"  + b.block_number + " </td>";
                str += "<td>";
                str += "<select class='FormTxtStyle' id='block_tier_" + b.blockID + "' name='block_tier_" + b.blockID + "'>";
                str += "<option value=''>--Select Tier--</option>";
                foreach (var t in tiers)
                {
                    str += "<option value='" + t.TierID + "'>" + t.TierName + "</option>";
                }
                str += "</select>";
                str += "</td>";
                str += "<td><input type='text' class='FormTxtStyle' placeholder='Enter Price'  id='block_price_" + b.blockID + "' name='block_price_" + b.blockID + "' /></td>";
                str += "</tr>";
            }
            return str;
        }

        public ActionResult Venue(int evtID)
        {
            ViewData["Venues"] = db.tblVenues.ToList();

            var evt = db.tblEvents.Where(e => e.EventID == evtID).SingleOrDefault();
            ViewData["Event"] = evt;

            int venueID = Convert.ToInt32(evt.VenueID);
            var venue = db.tblVenues.Where(t => t.VenueID == venueID).SingleOrDefault();

            ViewData["Layouts"] = db.tblLayouts.Where(e => e.VenueID == evt.VenueID).ToList();
            ViewData["Blocks"] = db.tblBlocks.Where(e => e.LayoutID == evt.LayoutID).ToList();
            return View(venue);
        }


        public string ManageVenue()
        {
            try
            {
                int evtID = Convert.ToInt32(Request["txtEventID"]);
                int venueID = Convert.ToInt32(Request["drpVenues"]);
                int layoutID = Convert.ToInt32(Request["drpLayout"]);

                //remove and re-insert the data
                var layout_data = db.tblEventLayoutBlocks.Where(e => e.EventID == evtID).ToList();

                foreach (var layout in layout_data)
                    db.tblEventLayoutBlocks.Remove(layout);
                db.SaveChanges();

                foreach (string req in Request.Form)
                {
                    if (req.Contains("block_tier"))
                    {
                        int blockID = Convert.ToInt32(req.Replace("block_tier_", ""));
                        decimal priceVal = 0;

                        if (decimal.TryParse(Request["block_price_" + blockID], out priceVal))
                        {
                            int tierID = Convert.ToInt32(Request["block_tier_" + blockID]);
                            decimal price = Convert.ToDecimal(Request["block_price_" + blockID]);

                            tblEventLayoutBlock layout = new tblEventLayoutBlock();
                            layout.EventID = evtID;
                            layout.BlockID = blockID;
                            layout.TierID = tierID;
                            layout.Price = price;
                            layout.CreatedDate = DateTime.Now;
                            db.tblEventLayoutBlocks.Add(layout);
                            db.SaveChanges();
                        }
                    }
                }

                var evt = db.tblEvents.Where(e => e.EventID == evtID).SingleOrDefault();
                evt.VenueID = venueID;
                evt.LayoutID = layoutID;
                db.SaveChanges();

                return "OK";
            }
            catch(Exception ex)
            {
                if (ex.InnerException != null)
                    return ex.InnerException.Message;
                else
                    return ex.Message;
            }
        }

        public JsonResult GetEvent(int eventID)
        {
            var evt = db.tblEvents.Where(e => e.EventID == eventID).SingleOrDefault();
            Log.Info("Fetching events started...");
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("EventID", evt.EventID.ToString());
            data.Add("EventName", evt.EventName);
            data.Add("EventType", evt.EventType);
            data.Add("EventLanguage", evt.EventLanguage);
            data.Add("EventPlayTime", evt.EventPlaytime);
            data.Add("BookingType", evt.BookingType);
            data.Add("EventDate", Convert.ToDateTime(evt.EventDate).ToString("yyyy-MM-dd"));
            data.Add("ShowTime", evt.ShowTime);
            data.Add("LocationTracker", evt.LocationTracker);
            data.Add("EventDesc", evt.EventDescription);
            data.Add("ProcessingFee", (evt.ProcessingFee != null ? evt.ProcessingFee.ToString() : "0"));
            data.Add("EventTNC", evt.EventTNC);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Artists()
        {
            int eventID = Convert.ToInt32(Request["eventID"]);

            return View(db.tblEventArtists.Where(e => e.EventID == eventID).ToList());
        }

        public JsonResult GetArtist(int artistID)
        {
            var artist = db.tblEventArtists.Where(e => e.ArtistID == artistID).SingleOrDefault();

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("ArtistID", artist.ArtistID.ToString());
            data.Add("EventID", artist.EventID.ToString());
            data.Add("ArtistName", artist.Name);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(false)]
        public string ManageArtists()
        {
            try
            {
                string status = "i";

                tblEventArtist evt = null;

                int eventID = Convert.ToInt32(Request["txtEventID"]);

                if (Request["txtArtistID"] != "0")
                {
                    int artistID = Convert.ToInt32(Request["txtArtistID"]);
                    evt = db.tblEventArtists.Where(e => e.ArtistID == artistID).SingleOrDefault();
                    status = "u";
                }
                else
                    evt = new tblEventArtist();

                evt.Name = Request["txtArtistName"];
                evt.EventID = eventID;
            
                HttpPostedFileBase artistImage = Request.Files["txtArtistImage"];

                if (artistImage != null && artistImage.ContentLength > 0)
                {
                    string uniqueName = String.Format("{0:d9}", (DateTime.Now.Ticks / 10) % 1000000000);
                    string path = Server.MapPath("~/ArtistImages/");
                    string sFileName = sFileName = uniqueName + "_" + artistImage.FileName;
                    artistImage.SaveAs(path + sFileName);
                    evt.Image = sFileName;
                }

                if (status == "i")
                    db.tblEventArtists.Add(evt);

                db.SaveChanges();
                return status;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    return ex.InnerException.Message;
                else
                    return ex.Message;
            }
        }

        public string DeleteArtist(int id)
        {
            try
            {

                var artist = db.tblEventArtists.Where(e => e.ArtistID == id).SingleOrDefault();
                db.tblEventArtists.Remove(artist);
                db.SaveChanges();

                return "OK";
            }
            catch(Exception ex)
            {
                if (ex.InnerException != null)
                    return ex.InnerException.Message;
                else
                    return ex.Message;
            }
      }


        public string DeleteUnorderedSeats(int id)
        {
            try
            {

                var seats = db.tblSeatSelections.SqlQuery("SELECT * FROM tblSeatSelections WHERE CREATEDDATE < DATEADD(minute, -15, GETDATE()) AND ORDERID IS NULL AND EVENTID = " + id + "").ToList();

                foreach (var seat in seats)
                {
                    tblReleaseLog log = new tblReleaseLog();
                    log.EventID = seat.EventID;
                    log.SeatID = seat.SeatID;
                    log.CreatedDate = DateTime.Now;
                    db.tblReleaseLogs.Add(log);
                    db.SaveChanges();

                    db.tblSeatSelections.Remove(seat);
                    db.SaveChanges();
                }
                return "OK";
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    return ex.InnerException.Message;
                else
                    return ex.Message;
            }
        }
    }
}