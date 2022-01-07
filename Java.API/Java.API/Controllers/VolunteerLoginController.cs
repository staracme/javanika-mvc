using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using Java.API.Models;

namespace Java.API.Controllers
{
    public class LoginModel
    {
        public string username { get; set; }
    
        public string password { get; set; }
    }

    public class LoginResponse
    { 
        public string status { get; set; }
        public string error_message { get; set; }
        public int id { get; set; }
        public string name { get; set; }
    }

    public class TBLVOLUNTEERS
    { 
        public int VolunteerID { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime CreatedDate { get; set; }
    }



    public class VolunteerLoginController : ApiController
    {
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public LoginResponse GetLoginResponse(LoginModel model)
        {
            JAVADBEntities db = new JAVADBEntities();
            LoginResponse response = new LoginResponse();

            try
            {
                var volunteer = db.Database.SqlQuery<TBLVOLUNTEERS>("SELECT * FROM TBLVOLUNTEERS WHERE USERNAME = '" + model.username + "' AND PASSWORD = '" + model.password + "'").SingleOrDefault();

                if (volunteer != null)
                {
                    response.status = "OK";
                    response.id = volunteer.VolunteerID;
                    response.name = volunteer.Name;
                }
                else
                    response.status = "INVALID";

                return response;
            }
            catch(Exception ex)
            {
                response.error_message = (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                response.status = "ERROR";
                return response;
            }
        }
    }
}
