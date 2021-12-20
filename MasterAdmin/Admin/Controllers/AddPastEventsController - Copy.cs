using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Admin.Models;
namespace Admin.Controllers
{
    public class AddPastEventsControllerCopy : Controller
    {
        // GET: AddPastEvents
        public ActionResult Index()
        {
            JAVADBEntities db = new JAVADBEntities();
            
            return View(db.tblPastEvents.OrderByDescending(p => p.PastEventID).ToList());
        }

        public string UploadFiles()
        {
            try
            {
                JAVADBEntities db = new JAVADBEntities();

                foreach (string file in Request.Files)
                {
                    HttpPostedFileBase image = Request.Files[file];

                    if (image != null && image.ContentLength > 0)
                    {
                        string uniqueName = String.Format("{0:d9}", (DateTime.Now.Ticks / 10) % 1000000000);
                        string path = Server.MapPath("~/PastEventImages/"); //Server.MapPath("~/HomeIcons/");

                        string sFileName = uniqueName + "_" + image.FileName;

                        image.SaveAs(path + sFileName);

                        tblPastEvent pastEvent = new tblPastEvent();
                        pastEvent.EventName = Request["txtEventName"];
                        pastEvent.Year = Request["drpYear"];
                        pastEvent.Month = Request["drpMonth"];
                        pastEvent.ImagePath = sFileName;
                        pastEvent.CreatedDate = DateTime.Now;
                        db.tblPastEvents.Add(pastEvent);
                        db.SaveChanges();
                    }
                }
                return "OK";
            }
            catch(Exception ex)
            {
                return (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
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

     

    }
}