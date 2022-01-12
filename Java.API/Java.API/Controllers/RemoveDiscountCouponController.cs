using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Java.API.Models;
namespace Java.API.Controllers
{
    public class RemoveDiscountCouponController : ApiController
    {
        public CouponResponse RemoveCoupon(CouponModel cm)
        {
            CouponResponse response = new CouponResponse();

            try
            {
                JAVADBEntities db = new JAVADBEntities();

                string sessionID = cm.sessionID;

                int eventID = cm.eventID;
                
                var checkCoupon = db.tblOrderCoupons.Where(c => c.CouponID == cm.couponID && c.SessionID == sessionID && c.OrderID == null).SingleOrDefault();

                if (checkCoupon != null)
                {
                    db.tblOrderCoupons.Remove(checkCoupon);
                    db.SaveChanges();
                    response.status = "OK";
                    return response;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.status = "OK";
                response.errorMessage = (ex.InnerException != null ? ex.InnerException.Message : "");
                return response;
            }
        }
    }
}
