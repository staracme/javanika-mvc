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
    public class OrderSummaryResponse
    {
        public string event_name { get; set; }
        public string event_details { get; set; }
        public string event_date { get; set; }
        public string event_time { get; set; }
        public int NoOfSeats { get; set; }
        public decimal TotalPrice { get; set; }
        public int couponID { get; set; }
        public string coupon_code { get; set; }
        public bool is_processing_fee { get; set; }
        public decimal processing_fee { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
    }


    public class OrderSummaryModel
    {
        public string sessionID { get; set; }
        public int eventID { get; set; }
    }


    public class OrderSummaryController : ApiController
    {

        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public OrderSummaryResponse GetSummary(OrderSummaryModel model)
        {
            JAVADBEntities db = new JAVADBEntities();

           
            var items = db.Database.SqlQuery<SelectedSeatsV5>("select SessionID, count(*) as 'NoOfSeats', sum(Price) as 'TotalPrice' from tblSeatSelections where EventID = " + model.eventID + " and SessionID = '" + model.sessionID + "' group by SessionID").ToList();

            OrderSummaryResponse response = new OrderSummaryResponse();

            var evt = db.tblEvents.Where(e => e.EventID == model.eventID).SingleOrDefault();

            decimal subTotal = items.Sum(s => s.TotalPrice);
            decimal amount = subTotal;
            decimal discount_amount = 0;
            decimal coupon_value = 0;

            var couponsUsed = (db
                   .tblOrderCoupons
                   .Where(c => c.SessionID == model.sessionID && c.OrderID == null)
                   .SingleOrDefault());


            if (couponsUsed != null)
            {
                if (couponsUsed.tblCoupon.DiscountIn == "Percentage")
                {
                    string tiers = couponsUsed.tblCoupon.tblCouponGroup.Tiers;
                    var seat_items = db.Database.SqlQuery<SelectedSeatsV5>("select SessionID, count(*) as 'NoOfSeats', sum(Price) as 'TotalPrice' from tblSeatSelections where EventID = " + model.eventID + " and SessionID = '" + model.sessionID + "' and orderid is null AND SEATID IN(SELECT SEATID FROM tblSeat WHERE SeatRowID IN(SELECT SeatRowID FROM tblSeatRows WHERE BlockID IN(SELECT BLOCKID FROM tblEventLayoutBlocks WHERE TierID IN(" + tiers + ")))) group by SessionID").ToList();
                    coupon_value = Convert.ToDecimal(couponsUsed.tblCoupon.Discount);

                    decimal seat_amount = seat_items.Sum(i => i.TotalPrice);

                    discount_amount = ((seat_amount * coupon_value / 100));

                    amount = amount - discount_amount;

                    if (evt.ProcessingFee != null && evt.ProcessingFee > 0)
                    {
                        decimal process_fees_perc = Convert.ToDecimal(evt.ProcessingFee);
                        decimal process_amount = ((amount * process_fees_perc / 100));

                        amount = (amount + process_amount);

                        response.processing_fee = process_amount;
                    }

                }
                else if (couponsUsed.tblCoupon.DiscountIn == "Value")
                {
                    string tiers = couponsUsed.tblCoupon.tblCouponGroup.Tiers;
                    coupon_value = Convert.ToDecimal(couponsUsed.tblCoupon.Discount);
                    discount_amount = (Convert.ToDecimal(couponsUsed.tblCoupon.Discount));

                    amount = amount - discount_amount;

                    if (evt.ProcessingFee != null && evt.ProcessingFee > 0)
                    {
                        decimal process_fees_perc = Convert.ToDecimal(evt.ProcessingFee);
                        decimal process_amount = ((amount * process_fees_perc / 100));

                        amount = (amount + process_amount);

                        response.processing_fee = process_amount;
                    }
                }
                else
                {
                    if (evt.ProcessingFee != null && evt.ProcessingFee > 0)
                    {
                        decimal process_fees_perc = Convert.ToDecimal(evt.ProcessingFee);
                        decimal process_amount = ((amount * process_fees_perc / 100));

                        amount = (amount + process_amount);

                        response.processing_fee = process_amount;
                    }
                }


                response.coupon_code = couponsUsed.tblCoupon.CouponCode;
                response.Discount = discount_amount;
                response.couponID = Convert.ToInt32(couponsUsed.CouponID);
            }
            else
            {
                if (evt.ProcessingFee != null && evt.ProcessingFee > 0)
                {
                    decimal process_fees_perc = Convert.ToDecimal(evt.ProcessingFee);
                    decimal process_amount = ((amount * process_fees_perc / 100));

                    amount = (amount + process_amount);

                    response.processing_fee = process_amount;
                }
            }

            response.event_name = evt.EventName;
            response.event_date = Convert.ToDateTime(evt.EventDate).ToString("dddd, dd MMMM yyyy");
            response.event_time = evt.ShowTime;
            response.event_details = (evt.tblVenue != null ? evt.tblVenue.VenueName : "");
            response.NoOfSeats = items.Sum(s => s.NoOfSeats);
            response.TotalPrice = Decimal.Round(Convert.ToDecimal(amount),2);
            response.SubTotal = subTotal;
            response.is_processing_fee = (evt.ProcessingFee != null);
            response.Discount = Decimal.Round(discount_amount,2)
                ;

            return response;
        }
    }
}
