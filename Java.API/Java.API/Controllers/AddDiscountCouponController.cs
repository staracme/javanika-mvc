using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Java.API.Models;
using System.Web.Http.Cors;
using System.Data.SqlClient;
using Dapper;

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
        public string coupon_code { get; set; }
        public decimal discount_amount { get; set; }
        public decimal Percentage { get; set; }
        public decimal TotalPrice { get; set; }
        public int NoOfSeats { get; set; }
        public decimal TotalPriceAfterDiscount { get; set; }
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
            CouponResponse response = new CouponResponse();

            string sessionID = cm.sessionID;

            int eventID = cm.eventID;

            bool IsValidCouponCode = false;

            tblCoupon coupon = new tblCoupon();
            var couponVariant = db.tblCoupons.Where(c => c.EventID == eventID).Take(1).SingleOrDefault();
            if (couponVariant.CouponVariant == "ONETOMANY")
            {
                IsValidCouponCode = CheckIsValidCouponCode(eventID, cm.coupon_code);
            }

            if (!IsValidCouponCode && couponVariant.CouponVariant == "ONETOMANY")
            {
                response.status = "USED";
                response.errorMessage = "Sorry, All the coupons are availed.";
                return response;
            }
            else
            {
                if (couponVariant.CouponVariant == "ONETOMANY")
                {
                    coupon = db.tblCoupons.Where(c => c.CouponCode == cm.coupon_code && c.EventID == eventID && c.IsUsed == null).Take(1).SingleOrDefault();
                }
                else
                {
                    coupon = db.tblCoupons.Where(c => c.CouponCode == cm.coupon_code && c.EventID == eventID).SingleOrDefault();
                }
            }

            //var coupon = db.tblCoupons.Where(c => c.CouponCode == cm.coupon_code && c.EventID == eventID).SingleOrDefault();



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

                            var seats = db.Database.SqlQuery<SelectedSeatsV5>("select SessionID, count(*) as 'NoOfSeats', sum(Price) as 'TotalPrice' from tblSeatSelections where EventID = " + eventID + " and SessionID = '" + sessionID + "' group by SessionID").ToList();

                            decimal amount = seats.Sum(i => i.TotalPrice);

                            decimal priceAfterDiscount = amount;
                            decimal coupon_discount = Convert.ToDecimal(coupon.Discount);
                            decimal discount_amount = ((amount * coupon_discount / 100));

                            priceAfterDiscount = priceAfterDiscount - ((priceAfterDiscount * coupon_discount / 100));

                            response.coupon_code = coupon.CouponCode;
                            response.discount_amount = discount_amount;
                            response.Percentage = coupon_discount;

                            response.status = "OK";

                            response.TotalPrice = amount;
                            response.NoOfSeats = Convert.ToInt32(seats.Sum(s => s.NoOfSeats));
                            response.TotalPriceAfterDiscount = priceAfterDiscount;

                            return response;
                        }
                        else
                        {
                            response.status = "USED";
                            response.errorMessage = "You have already used this coupon in your shopping cart.";
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
                response.errorMessage = "Sorry! This Coupon does not exist.";
                return response;
            }
        }

        #region Private Functions
        private bool CheckIsValidCouponCode(int eventId, string couponCode)
        {
            bool IsValidCouponCode = false;
            string constr = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            using (SqlConnection con = new SqlConnection(constr))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@EventId", eventId);
                parameters.Add("@couponCode", couponCode);

                using (SqlMapper.GridReader multi = con.QueryMultiple("CheckIsValidCouponCode"
                    , parameters
                    , commandType: System.Data.CommandType.StoredProcedure))
                {
                    var records = multi.Read<string>().SingleOrDefault();

                    if (records != null && records == "VALID")
                        IsValidCouponCode = true;
                }
            }
            return IsValidCouponCode;
        } 
        #endregion
    }
}
