using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Java.API.Models;
using System.Web.Http.Cors;
using Java.API.Enums;

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
        public decimal ActualPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal processingPercentage { get; set; }
        public string ticketType { get; set; }
        public int isEarlyBird { get; set; }
        public decimal price { get; set; }
        public decimal discountAmount { get; set; }
        public decimal discountedAmount { get; set; }
        public decimal amount { get; set; }
        public string couponUsed { get; set; }
    }


    public class OrderSummaryModel
    {
        public string sessionID { get; set; }
        public int eventID { get; set; }
    }


    public class OrderSummaryController : ApiController
    {
        JAVADBEntities db = new JAVADBEntities();

        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public OrderSummaryResponse GetSummary(OrderSummaryModel model)
        {
            OrderSummaryResponse response = new OrderSummaryResponse();

            var evt = db.tblEvents.Where(e => e.EventID == model.eventID).SingleOrDefault();

            var couponsUsed = (db
                   .tblOrderCoupons
                   .Where(c => c.SessionID == model.sessionID && c.OrderID == null)
                   .SingleOrDefault());

            var items = db.Database.SqlQuery<SelectedSeatsV5>("select SessionID, count(*) as 'NoOfSeats', sum(Price) as 'TotalPrice' from tblSeatSelections where EventID = " + model.eventID + " and SessionID = '" + model.sessionID + "' group by SessionID").ToList();

            if (items == null || items.Count == 0)
            {
                return response; // set response
            }

            var selectedSeatsTicketDetails = db.Database.SqlQuery<SelectedSeatsV5>("select SessionID, Price, EBPrice  from tblSeatSelections where EventID = " + model.eventID + " and SessionID = '" + model.sessionID + "' ").ToList();

            decimal amount = items.Sum(i => i.TotalPrice);
            amount = selectedSeatsTicketDetails.Sum(x => x.Price);
            items[0].ActualPrice = amount;

            decimal subTotal = items.Sum(s => s.TotalPrice);
            //decimal amount = selectedSeatsTicketDetails.Sum(x => x.Price); //subTotal;
            decimal discount_amount = 0;
            decimal coupon_value = 0;
            //items[0].ActualPrice = amount;

            var seats = db.tblSeatSelections.Where(t => t.OrderID == null && t.SessionID == model.sessionID && t.EventID == model.eventID).ToList();

            if (couponsUsed != null)
            {
                if (couponsUsed.tblCoupon.DiscountIn == "Percentage")
                {
                    string tiers = couponsUsed.tblCoupon.tblCouponGroup.Tiers;

                    var seat_items = db.Database.SqlQuery<SelectedSeatsV5>("select SessionID, count(*) as 'NoOfSeats', sum(Price) as 'TotalPrice' from tblSeatSelections where EventID = " + model.eventID + " and SessionID = '" + model.sessionID + "' and orderid is null AND SEATID IN(SELECT SEATID FROM tblSeat WHERE SeatRowID IN(SELECT SeatRowID FROM tblSeatRows WHERE BlockID IN(SELECT BLOCKID FROM tblEventLayoutBlocks WHERE TierID IN(" + tiers + ")))) group by SessionID").ToList();
                    coupon_value = Convert.ToDecimal(couponsUsed.tblCoupon.Discount);

                    decimal seat_amount = seat_items.Sum(i => i.TotalPrice);

                    discount_amount = ((seat_amount * coupon_value / 100));

                    decimal final_amount = amount - discount_amount;

                    if (evt.ProcessingFee != null && evt.ProcessingFee > 0)
                    {
                        decimal process_fees_perc = Convert.ToDecimal(evt.ProcessingFee);
                        decimal process_amount = ((final_amount * process_fees_perc / 100));

                        final_amount = (final_amount + process_amount);

                        response.processingPercentage = process_fees_perc;
                        response.processing_fee = process_amount;
                    }

                    response.discountAmount = discount_amount;
                    response.amount = final_amount;
                    amount = final_amount;
                    //response.couponUsed = couponsUsed;
                    items[0].Price = final_amount;
                    response.ticketType = "GENERAL";
                    response.isEarlyBird = 0;
                    response.discountedAmount = discount_amount;
                }
                else if (couponsUsed.tblCoupon.DiscountIn == "Value")
                {
                    string tiers = couponsUsed.tblCoupon.tblCouponGroup.Tiers;
                    coupon_value = Convert.ToDecimal(couponsUsed.tblCoupon.Discount);
                    discount_amount = (Convert.ToDecimal(couponsUsed.tblCoupon.Discount));

                    decimal final_amount = amount - discount_amount;

                    if (evt.ProcessingFee != null && evt.ProcessingFee > 0)
                    {
                        decimal process_fees_perc = Convert.ToDecimal(evt.ProcessingFee);
                        decimal process_amount = ((amount * process_fees_perc / 100));

                        final_amount = (final_amount + process_amount);

                        response.processingPercentage = process_fees_perc;
                        response.processing_fee = process_amount;
                    }

                    response.discountAmount = discount_amount;
                    response.amount = final_amount;
                    amount = final_amount;
                    //response.couponUsed = couponsUsed;
                    items[0].Price = final_amount;
                    response.ticketType = "GENERAL";
                    response.isEarlyBird = 0;
                    response.discountedAmount = discount_amount;
                }
                //else
                //{
                //    if (evt.ProcessingFee != null && evt.ProcessingFee > 0)
                //    {
                //        decimal process_fees_perc = Convert.ToDecimal(evt.ProcessingFee);
                //        decimal process_amount = ((amount * process_fees_perc / 100));

                //        amount = (amount + process_amount);

                //        response.processing_fee = process_amount;
                //    }
                //}


                response.coupon_code = couponsUsed.tblCoupon.CouponCode;
                response.Discount = discount_amount;
                response.couponID = Convert.ToInt32(couponsUsed.CouponID);
            }
            else
            {
                OrderInput input = CheckIsEarlyBirdApplicable(model.eventID, selectedSeatsTicketDetails);
                if (input.TicketType == "EARLYBIRD")
                {
                    if (evt.BookingType == "SEATS")
                    {
                        input.TotalAmount = selectedSeatsTicketDetails.Sum(x => x.EBPrice);
                    }
                    else
                    {
                        input.TotalAmount = (int)evt.EBTicketPrice;
                    }
                    //input.TotalAmount = selectedSeatsTicketDetails.Sum(x => x.EBPrice);
                    response.isEarlyBird = 1;
                }
                else
                {
                    input.TotalAmount = selectedSeatsTicketDetails.Sum(x => x.Price);
                    response.isEarlyBird = 0;
                }

                amount = input.TotalAmount;
                //to check current no of tickets does not exceed thre remaining stock of tickets
                bool isTicketCountValid = CheckSeatsAvailability(model.eventID, selectedSeatsTicketDetails.Count);

                //if (isTicketCountValid && evt.ProcessingFee != null && evt.ProcessingFee > 0)
                if (evt.ProcessingFee != null && evt.ProcessingFee > 0)
                {
                    decimal process_fees_perc = Convert.ToDecimal(evt.ProcessingFee);
                    decimal process_amount = ((amount * process_fees_perc / 100));

                    amount = (amount + process_amount);
                    response.processingPercentage = process_fees_perc;
                    response.processing_fee = process_amount;
                    response.ticketType = input.TicketType;
                    response.isEarlyBird = (input.TicketType == "EARLYBIRD") ? 1 : 0;
                    response.price = input.TotalAmount;
                    items[0].Price = input.TotalAmount;
                    items[0].DiscountedAmount = items[0].ActualPrice - items[0].Price;
                    response.discountedAmount = items[0].DiscountedAmount;
                }
                response.amount = amount;
            }

            response.event_name = evt.EventName;
            response.event_date = Convert.ToDateTime(evt.EventDate).ToString("dddd, dd MMMM yyyy");
            response.event_time = evt.ShowTime;
            response.event_details = (evt.tblVenue != null ? evt.tblVenue.VenueName : "");
            response.NoOfSeats = items.Sum(s => s.NoOfSeats);
            response.TotalPrice = Decimal.Round(Convert.ToDecimal(amount),2);
            response.SubTotal = subTotal;
            response.ActualPrice = subTotal;
            response.is_processing_fee = (evt.ProcessingFee != null);
            response.Discount = Decimal.Round(discount_amount,2);

            return response;
        }

        public bool CheckSeatsAvailability(int eventID, int no_of_tickets)
        {
            //= Convert.ToInt32(Request["eventID"]);
            //= Convert.ToInt32(Request["no_of_tickets"]);

            var evt = db.tblEvents.Where(e => e.EventID == eventID).SingleOrDefault();

            BookResponse response = new BookResponse();
            //check if stock is available
            var soldTicketsCount = db.tblTicketOrders.Where(e => e.EventID == eventID
                                                && e.Status == "SUCCESS"
                                                && e.PaymentStatus == "COMPLETED").ToList();

            int totalSeatsAvailable = (int)(evt.TicketStock - soldTicketsCount.Count);
            //to check current no of tickets does not exceed thre remaining stock of tickets
            bool isTicketCountValid = totalSeatsAvailable > no_of_tickets;
            return isTicketCountValid;
        }

        private OrderInput CheckIsEarlyBirdApplicable(int eventID, List<SelectedSeatsV5> selectedSeatsTicketDetails)
        {
            var evt = db.tblEvents.Where(e => e.EventID == eventID).SingleOrDefault();
            //decimal price = (decimal)evt.TicketPrice;
            //decimal price = selectedSeatsTicketDetails.Sum(e => e.Price);
            //decimal EBPrice = selectedSeatsTicketDetails.Sum(e => e.EBPrice);

            int noOfTickets = selectedSeatsTicketDetails.Count;

            var soldTicketsCount = db.tblTicketOrders.Where(e => e.EventID == eventID
                                        && e.Status == "SUCCESS"
                                        && e.PaymentStatus == "COMPLETED").ToList();

            //decimal totalAmount = (noOfTickets * price);

            var input = new OrderInput
            {
                CustomerName = "",
                CustomerEmail = "",
                NoOfTickets = noOfTickets,
                //Price = price,
                //TotalAmount = totalAmount
            };
            //Logic to check Early Bird ticket allocation base on configurtion 
            //datewise ot seatwise
            if (evt.EarlyBirdTicketSelection == EarlyBirdTicketType.DATEWISE.ToString())
            {
                DateTime currentDate = System.DateTime.Today;
                if (currentDate >= evt.StartDate && currentDate <= evt.EndDate)
                {
                    input.TicketType = TicketType.EARLYBIRD.ToString();
                    //input.Price = (decimal)EBPrice;
                }
                else
                {
                    input.TicketType = TicketType.GENERAL.ToString();
                    //input.Price = (decimal)price;
                }
            }
            else if (evt.EarlyBirdTicketSelection == EarlyBirdTicketType.SEATWISE.ToString())
            {
                int percentEBSeats = (int)evt.PercentEBSeats;
                int totalEBSeatsAllowed = (int)((evt.TicketStock * percentEBSeats) / 100);

                if (totalEBSeatsAllowed >= noOfTickets && evt.TicketsAvailable >= noOfTickets)
                {
                    input.TicketType = TicketType.EARLYBIRD.ToString();
                    //input.Price = (decimal)EBPrice;
                }
                else
                {
                    input.TicketType = TicketType.GENERAL.ToString();
                    //input.Price = (decimal)price;
                }
            }
            //input.TotalAmount = (input.NoOfTickets * input.Price);
            return input;
        }
    }
}
