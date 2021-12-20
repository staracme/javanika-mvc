using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Admin.Models;
using AutoMapper;
using Dapper;

namespace Admin.Controllers
{
    public class AddPastEventsController : Controller
    {
        JAVADBEntities db = new JAVADBEntities();
        public ActionResult Index()
        {
            List<EventsViewModel> eventsVM = null;
            EventsViewModel pastEventsVM = null;
            int eventID = (string.IsNullOrEmpty(Request["eventID"]))
                ? 0 : Convert.ToInt32(Request["eventID"]);
            if (eventID > 0)
            {
                TempData["eventID"] = eventID;
                pastEventsVM = new EventsViewModel();
                var res = db.tblEvents.Where(p => p.EventID == eventID).ToList().SingleOrDefault();
                Mapper.Map(res, pastEventsVM);
                TempData["ListEvents"] = getEvents();
                TempData["EventRequest"] = getEventRequest(eventID);
            }
            else {
                eventsVM = TempData["ListEvents"] as List<EventsViewModel>;
            }
            return View(pastEventsVM);
        }

        public string UploadFiles()
        {
            try
            {
                int eventID = (string.IsNullOrEmpty(Request["eventID"]))
                ? 0 : Convert.ToInt32(Request["eventID"]);
                string drpMediaType = Convert.ToString(Request["drpMediaType"]);

                foreach (string file in Request.Files)
                {
                    HttpPostedFileBase image = Request.Files[file];

                    if (image != null && image.ContentLength > 0)
                    {
                        Guid uniqueName = new Guid(); //String.Format("{0:d9}", (DateTime.Now.Ticks / 10) % 1000000000);
                        //string path = Server.MapPath("~/PastEventImages/"); //Server.MapPath("~/HomeIcons/");
                        string sFileName = uniqueName.ToString() + "_" + image.FileName;
                        bool isFileSaved = false;
                        if (image.ContentType == "image/jpeg" || image.ContentType == "video/mp4")
                        {
                            isFileSaved = SaveMedia(image, sFileName, eventID.ToString());
                        }
                        if (isFileSaved)
                        {
                            tblPastEvent pastEvent = new tblPastEvent();
                            pastEvent.EventID = Convert.ToInt32(Request["eventID"]);
                            pastEvent.ImagePath = sFileName;
                            pastEvent.ImageType = image.ContentType;
                            pastEvent.CreatedDate = DateTime.Now;
                            db.tblPastEvents.Add(pastEvent);
                            db.SaveChanges();
                        }
                    }
                }
                return "OK";
            }
            catch (Exception ex)
            {
                return (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }
        }

        public bool SaveMedia(HttpPostedFileBase image, string imageName, string folder)
        {
            try
            {
                // Path
                string path = Server.MapPath("~/PastEventImages/" + folder + "/");

                // Check if directory exist
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path); // Create directory if it doesn't exist
                }
                // set the image path
                string imgPath = Path.Combine(path, imageName);

                image.SaveAs(imgPath);
                bool isFileThumbNailSaved = false;
                if (image.ContentType == "image/jpeg")
                {
                    isFileThumbNailSaved = SaveImgThumbnail(image, imageName, folder);
                } else
                {
                    isFileThumbNailSaved = true;
                }

                return isFileThumbNailSaved;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool SaveImgThumbnail(HttpPostedFileBase image, string imageName, string folder)
        {
            try
            {
                // Thumbnail Path
                string tbpath = Server.MapPath("~/PastEventImages/" + folder + "/thumb/");

                if (!System.IO.Directory.Exists(tbpath))
                {
                    System.IO.Directory.CreateDirectory(tbpath); //Create Thumb directory if it doesn't exist
                }

                //set thumbnail path
                string thumbnailPath = Path.Combine(tbpath, "tb-" + imageName);

                byte[] imageBytes = null;
                using (var binaryReader = new BinaryReader(image.InputStream))
                {
                    imageBytes = binaryReader.ReadBytes(image.ContentLength);
                }

                Stream imgStream = new MemoryStream(imageBytes);
                bool cropResTb = Crop(
                    Convert.ToInt32(ConfigurationManager.AppSettings["profileTbWidth"]),
                    Convert.ToInt32(ConfigurationManager.AppSettings["profileTbHeight"]),
                    imgStream,
                    thumbnailPath);

                //Rectangle cropRect = new Rectangle();
                //cropRect.Height = Convert.ToInt32(ConfigurationManager.AppSettings["profileTbWidth"]);
                //cropRect.Width = Convert.ToInt32(ConfigurationManager.AppSettings["profileTbHeight"]);

                //Bitmap src = Image.FromFile(imageName) as Bitmap;
                //Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

                //using (Graphics g = Graphics.FromImage(target))
                //{
                //    g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                //                     cropRect,
                //                     GraphicsUnit.Pixel);
                //}
                return true;
                

                //if (cropResTb)
                //{
                //    return true;
                //}
                //else
                //{
                //    return false;
                //}
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public bool Crop(int width, int height, Stream streamImg, string saveFilePath)
        {
            Bitmap sourceImage = new Bitmap(streamImg);
            try
            {
                using (Bitmap objBitmap = new Bitmap(width, height))
                {
                    objBitmap.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
                    using (Graphics objGraphics = Graphics.FromImage(objBitmap))
                    {
                        objGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                        objGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        objGraphics.DrawImage(sourceImage, 0, 0, width, height);

                        // save the file path in png format
                        objBitmap.Save(saveFilePath);

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        

        public string DeleteImage(int id)
        {
            try
            {
                JAVADBEntities db = new JAVADBEntities();

                var pastEvent = db.tblPastEvents.Where(t => t.PastEventID == id).SingleOrDefault();
                db.tblPastEvents.Remove(pastEvent);
                db.SaveChanges();

                return "OK";
            }
            catch (Exception ex)
            {
                return (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }
        }

        public ActionResult SearchEvents(EventRequest eventRequest)
        {
            List<tblEvent> eventsVM = new List<tblEvent>();
            List<EventsViewModel> eventsResponse = new List<EventsViewModel>();
            TempData["EventRequest"] = eventRequest;
            TempData["ListEvents"] = getEvents();
            eventsResponse = getEvents(eventRequest);
            TempData["SearchEvents"] = eventsResponse[0];
            TempData["eventImageList"] = getEventImageList(eventsResponse[0].EventID); ;
            TempData["isSearch"] = true;
            return View("~/Views/AddPastEvents/Index.cshtml");
        }

        private List<EventsImageListViewModel> getEventImageList(decimal eventId)
        {
            List<EventsImageListViewModel> eventImageListVM = null;
            eventImageListVM = new List<EventsImageListViewModel>();
            var res = db.tblPastEvents.Where(e => e.EventID == eventId).ToList();
            Mapper.Map(res, eventImageListVM);
            return eventImageListVM;
        }

        private List<EventsViewModel> getEvents()
        {
            List<EventsViewModel> eventsVM = null;
            eventsVM = new List<EventsViewModel>();
            var res = db.tblEvents.OrderByDescending(p => p.EventDate).ToList();
            Mapper.Map(res, eventsVM);
            return eventsVM;
        }
        private List<EventsViewModel> getEvents(EventRequest eventRequest)
        {
            List<EventsViewModel> orderModels = null;
            string constr = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            using (SqlConnection con = new SqlConnection(constr))
            {
                orderModels = new List<EventsViewModel>();
                var parameters = new DynamicParameters();
                parameters.Add("@EventId", eventRequest.EventId);
                parameters.Add("@rowsPerPage", eventRequest.rowsPerPage);
                parameters.Add("@pageNum", eventRequest.pageNum);

                using (SqlMapper.GridReader multi = con.QueryMultiple("GetEvents"
                    , parameters
                    , commandType: System.Data.CommandType.StoredProcedure))
                {
                    orderModels = multi.Read<EventsViewModel>().ToList();
                }
            }
            return orderModels;
        }

        private EventRequest getEventRequest(int eventID)
        {
            EventRequest eventRequest = new EventRequest();
            eventRequest.EventId = eventID;
            eventRequest.pageNum = 1;
            eventRequest.rowsPerPage = (string.IsNullOrEmpty(Request["rowsPerPage"]))
                ? 5 : Convert.ToInt32(Request["rowsPerPage"]);
            return eventRequest;
        }

    }
}