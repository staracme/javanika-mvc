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
    public class UnbookedModel
    {
        public int eventID { get; set; }
        public string uuid { get; set; }
    }

    public class UnbookedResponse
    {
        public string status { get; set; }
        public string message { get; set; }
    }

    public class RemoveUnbookedSeatsController : ApiController
    {
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public UnbookedResponse SelectResponse(UnbookedModel st)
        {
            UnbookedResponse response = new UnbookedResponse();

            try
            {
                JAVADBEntities db = new JAVADBEntities();
                
                string sessionID = st.uuid;
                int eventID = st.eventID;

                //remove all the orphan(unconfirmed) seats associated with the current session
                var unconfirmed_seats = db.tblSeatSelections.Where(t => t.EventID == eventID && t.SessionID == sessionID && t.OrderID == null).ToList();

                foreach (var seat in unconfirmed_seats)
                    db.tblSeatSelections.Remove(seat);
                db.SaveChanges();

                //remove all the orphan(unconfirmed) discount coupons associated with the current session
                var unconfirmed_coupons = db.tblOrderCoupons.Where(t => t.SessionID == sessionID && t.OrderID == null).ToList();

                foreach (var coupon in unconfirmed_coupons)
                    db.tblOrderCoupons.Remove(coupon);
                db.SaveChanges();

                response.status = "OK";
            }
            catch(Exception ex)
            {
                response.status = "ERROR";

                if (ex.InnerException != null)
                   response.message =  ex.InnerException.Message;
                else
                   response.message = ex.Message;
            }

            return response;
        }
    }
}
