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
    public class SeatSelectResponse
    {
        public string status { get; set; }

        public int NoOfSeats { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class SeatSelectModel
    {
        public int eventID { get; set; }
        public string uuid { get; set; }
        public int seatID { get; set; }
    }

    public class SelectedSeatsV5
    {
        public string SessionID { get; set; }
        public int NoOfSeats { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal ActualPrice { get; set; }
        public decimal DiscountedAmount { get; set; }
        public decimal Price { get; set; }
        public decimal EBPrice { get; set; }
    }

    public class SelectedSeatsV5Response
    {
        public string status { get; set; }
        public int NoOfSeats { get; set; }
        public string coupon_code { get; set; }
        public decimal discount_amount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal Percentage { get; set; }
        public decimal TotalPriceAfterDiscount { get; set; }
    }

    public class SelectSeatController : ApiController
    {
        JAVADBEntities db = new JAVADBEntities();
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public SelectedSeatsV5Response SelectResponse(SeatSelectModel st)
        {
            SelectedSeatsV5Response response = null;
            string json = string.Empty;
            int eventID = st.eventID;
            int seatID = st.seatID;
            string sessionID = st.uuid;
            response = new SelectedSeatsV5Response();

            #region Check if seats are vacant or already taken by another user
            var isSelected = db.tblSeatSelections.Where(ts => (!string.IsNullOrEmpty(ts.SessionID)
                        && ts.SessionID != sessionID) && ts.SeatID == seatID && ts.OrderID == null
                        && ts.EventID == eventID).ToList();
            if (isSelected.Any())
            {
                response.status = "OCCUPIED";
                //string result = JsonConvert.SerializeObject(response);
                return response;
            }

            #endregion

            string selectSeat = Common.SelectSeat(eventID, sessionID, seatID);


            if (selectSeat == "OK")
            {
                var seats = db.Database.SqlQuery<SelectedSeatsV5>("select SessionID, count(*) as 'NoOfSeats', sum(Price) as 'TotalPrice' from tblSeatSelections where EventID = " + eventID + " and SessionID = '" + st.uuid + "'  and ORDERID IS NULL  group by SessionID").ToList();

                decimal amount = seats.Sum(i => i.TotalPrice);

                decimal priceAfterDiscount = amount;

                var couponsUsed = (db
                            .tblOrderCoupons
                            .Where(c => c.SessionID == sessionID && c.OrderID == null)
                            .SingleOrDefault());

                if (couponsUsed != null)
                {
                    decimal coupon_discount = Convert.ToDecimal(couponsUsed.tblCoupon.Discount);

                    decimal discount_amount = ((amount * coupon_discount / 100));

                    priceAfterDiscount = priceAfterDiscount - ((priceAfterDiscount * coupon_discount / 100));

                    response.coupon_code = couponsUsed.tblCoupon.CouponCode;
                    response.discount_amount = discount_amount;
                    response.Percentage = coupon_discount;

                }

                response.status = "OK";
                response.TotalPrice = amount;
                response.NoOfSeats = Convert.ToInt32(seats.Sum(s => s.NoOfSeats));
                //response.TotalPrice = Convert.ToDecimal(seats.Sum(s => s.TotalPrice));
                response.TotalPriceAfterDiscount = priceAfterDiscount;

            }
            return response;
        }
    }
}
