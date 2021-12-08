using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Admin.Models;
namespace Admin.Controllers
{

    public class EventsResponse
    {
        public int EventID { get; set; }
        public bool isBlockedByAdmin { get; set; }
    }


    public class AddVolunteersController : Controller
    {
        // GET: AddVolunteers
        JAVADBEntities db = new JAVADBEntities();
        public ActionResult Index()
        {
            var events = db.tblEvents.Where(t => t.Status == "a").OrderByDescending(e => e.EventID).ToList(); 
            ViewData["Events"] = events;
            return View(db.tblVolunteers.OrderByDescending(p => p.VolunteerID).ToList());
        }


        [ValidateInput(false)]
        public string ManageVolunteers()
        {
            try
            {
                string status = "i";

                tblVolunteer vol = null;

                if (Request["txtVolunteerID"] != "" && Request["txtVolunteerID"] != "0")
                {
                    int volunteerID = Convert.ToInt32(Request["txtVolunteerID"]);
                    vol = db.tblVolunteers.Where(e => e.VolunteerID == volunteerID).SingleOrDefault();
                    status = "u";
                }
                else
                    vol = new tblVolunteer();

                vol.Name = Request["txtVolunteerName"];
                vol.Username = Request["txtUserName"];
                vol.Password = Request["txtPassword"];

                if (status == "i")
                {
                    vol.CreatedDate = DateTime.Now;
                    db.tblVolunteers.Add(vol);
                }

                db.SaveChanges();


                if (Request["chkEvents"] != null)
                {
                    var evts = db.tblEventVolunteers.Where(ca => ca.VolunteerID == vol.VolunteerID).ToList();
                    foreach (var evt in evts)
                        db.tblEventVolunteers.Remove(evt);
                    db.SaveChanges();


                    string[] events = Request["chkEvents"].Split(',');
                    foreach (string evt in events)
                    {
                        int eventID = Convert.ToInt32(evt);

                        tblEventVolunteer mapevt = new tblEventVolunteer();
                        mapevt.EventID = eventID;
                        mapevt.VolunteerID = vol.VolunteerID;
                        mapevt.CreatedDate = DateTime.Now.Date;
                        db.tblEventVolunteers.Add(mapevt);
                        db.SaveChanges();
                    }
                }
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

        public string CheckUsername(String username)
        {
            username = username.ToLower().Trim();

            var user = (db
                          .tblVolunteers
                          .Where(p => p.Username.ToLower() == username)
                          .Any());

            if (user)
                return "1";
            else
                return "0";
        }

        public JsonResult GetVolunteer(int volunteerID)
        {
            JAVADBEntities db = new JAVADBEntities();

            Dictionary<string, string> data = new Dictionary<string, string>();

            var volunteer = db.tblVolunteers.Where(v => v.VolunteerID == volunteerID).SingleOrDefault();

            data.Add("VolunteerID", volunteerID.ToString());
            data.Add("Name", volunteer.Name);
            data.Add("Username", volunteer.Username);
            data.Add("Password", volunteer.Password);
            data.Add("Events", string.Join(",", db.tblEventVolunteers.Where(v=>v.VolunteerID== volunteerID).Select(e=>e.EventID)));

            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}