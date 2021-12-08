using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Admin.Models;
using AutoMapper;

namespace Admin.Controllers
{
    public class LoginResponse
    {
        public string status { get; set; }
        public string type { get; set; }
    }

    public class LoginController : Controller
    {
        JAVADBEntities db = new JAVADBEntities();
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult Login()
        {
            try
            {
                string username = Request["txtUsername"];
                string password = Request["txtPassword"];

                //var admin = db.tblAdminUsers; //.SingleOrDefault(); //.Where(e => e.Username == username && e.Password == password).SingleOrDefault();
                var admin = db.tblAdminUsers.SingleOrDefault();
                LoginResponse response = new LoginResponse();

                if (admin != null)
                {
                    Response.Cookies["Admin"]["AdminID"] = admin.UserID.ToString();
                    Response.Cookies["Admin"]["FirstName"] = admin.FirstName;
                    Response.Cookies["Admin"]["LastName"] = admin.LastName;
                    response.status = "OK";
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    response.status = "N";
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LoginResponse response = new LoginResponse();
                response.status = "ERROR";
                return Json(response, JsonRequestBehavior.AllowGet);
            }
        }

    }
}