using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Java.API.Models;
using System.Web.Http.Cors;
namespace Java.API.Controllers
{
    public class CouponModel
    {
        public string sessionID { get; set; }
        public string coupon_code { get; set; }
        public int eventID { get; set; }
        public Nullable<int> couponID { get; set; }
    }

    public class CouponResponse
    {
        public string status { get; set; }
        public string errorMessage { get; set; }
    }

    public class Tier
    {
        public int tierID { get; set; }
    }
    

    public class AddDiscountCouponController : ApiController
    {
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public CouponResponse AddCoupon(CouponModel cm)
        {
            JAVADBEntities db = new JAVADBEntities();

            string sessionID = cm.sessionID;

            int eventID = cm.eventID;

            var coupon = db.tblCoupons.Where(c => c.CouponCode == cm.coupon_code && c.EventID == eventID).SingleOrDefault();

            CouponResponse response = new CouponResponse();

            if (coupon != null)
            {
                var coupon_group = db.tblCouponGroups.Where(c => c.CouponGroupID == coupon.CouponGroupID).SingleOrDefault();

                string tiers = coupon_group.Tiers;

                var getTiers = db.Database.SqlQuery<Tier>("select distinct TierID from tblSeatSelections ts inner join tblSeat s on ts.SeatID = s.SeatID inner join tblSeatRows r on s.SeatRowID = r.SeatRowID   inner join tblEventLayoutBlocks eb on eb.BlockID = r.BlockID where ts.SessionID = '" + sessionID + "' and ts.OrderID is null and ts.EventID  = " + eventID + "  and TierID is not null").ToList();

                bool isEligibleForCoupon = false;

                foreach (var tier in getTiers)
                {
                    int tierID = tier.tierID;

                    if (isEligibleForCoupon == false)
                    {
                        foreach (var t in tiers.Split(','))
                        {
                            if (Convert.ToInt32(t) == tierID)
                            {
                                isEligibleForCoupon = true;
                                break;
                            }
                        }
                    }
                }

                if (isEligibleForCoupon == true)
                {
                    if (coupon.IsUsed == null && coupon.UsedDate == null)
                    {
                        var checkCoupon = db.tblOrderCoupons.Where(c => c.CouponID == coupon.CouponID && c.SessionID == sessionID && c.OrderID == null).SingleOrDefault();

                        if (checkCoupon == null)
                        {
                            tblOrderCoupon orderCoupon = new tblOrderCoupon();
                            orderCoupon.CouponID = coupon.CouponID;
                            orderCoupon.SessionID = sessionID;
                            db.tblOrderCoupons.Add(orderCoupon);
                            db.SaveChanges();
                            response.status = "OK";
                            return response;
                        }
                        else
                        {
                            response.status = "USED";
                            response.errorMessage = "This coupon has been already used.";
                            return response;
                        }
                    }
                    else
                    {
                        response.status = "USED";
                        response.errorMessage = "This coupon has been already used.";
                        return response;
                    }
                }
                else
                {
                    response.status = "NOT APPLICABLE";
                    response.errorMessage = "Sorry, This coupon is not applicable on these seats.";
                    return response;
                }
            }
            else
            {
                response.status = "INVALID";
                response.errorMessage = "You have entered an invalid coupon code. Try using different coupon code.";
                return response;
            }
        }
    }
}
