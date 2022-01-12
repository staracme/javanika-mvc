using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Java.API.Models;
using Java.API.Models;
using System.Net;
using System.Net.Mail;
using QRCoder;
using System.Web;
using System.Drawing;
using System.Web.Http;
using System.Web.Http.Cors;
namespace Java.API.Controllers
{
    public class PlaceOrderResponse
    {
        public string event_name { get; set; }
        public int NoOfSeats { get; set; }
        public decimal TotalPrice { get; set; }
    }


    public class PlaceOrderModel
    {
        public string id { get; set; }
        public string state { get; set; }
        public string intent { get; set; }
        public string amount { get; set; }
        public string sessionID { get; set; }
        public int eventID { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string mobile { get; set; }
        public string ticketType { get; set; }
        public string actualPrice { get; set; }
        public string discountedAmount { get; set; }
        public string priceAfterDiscount { get; set; }
        public string processingFee { get; set; }
    }

    public class FinalResponse
    {
        public string status { get; set; }
        public int orderID { get; set; }
    }

    public class FinalPlaceOrderResponse
    {
        public string status { get; set; }
        public string orderID { get; set; }
    }


    public class PlaceOrderController : ApiController
    {
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public FinalResponse PlaceOrder(PlaceOrderModel orderModel)
        {
            FinalResponse resp = new FinalResponse();

            try
            {
                JAVADBEntities db = new JAVADBEntities();
                string sessionID = orderModel.sessionID;
                int eventID = orderModel.eventID;

                decimal discount_amount = 0;
                string coupon_code = "";
                decimal coupon_value = 0;

                var seats = db.tblSeatSelections.Where(t => t.OrderID == null && t.SessionID == sessionID && t.EventID == eventID).ToList();

                var evt = db.tblEvents.Where(e => e.EventID == eventID).SingleOrDefault();

                var orders = db.tblTicketOrders.Where(os => os.Status == "SUCCESS" && os.PaymentStatus == "COMPLETED" && os.EventID == eventID).ToList();

                var coupon_used = db.tblOrderCoupons.Where(t => t.OrderID == null && t.SessionID == sessionID && t.tblCoupon.EventID == eventID).SingleOrDefault();

                int serialNo = (orders.Count() > 0 ? Convert.ToInt32((orders.OrderByDescending(os => os.OrderID).Take(1).SingleOrDefault().OrderNo) + 1) : 1);

                tblTicketOrder order = new tblTicketOrder();
                order.EventID = eventID;
                order.NoOfTickets = seats.Count();
                order.Name = orderModel.name;
                order.Email = orderModel.email;
                order.Mobile = orderModel.mobile;
                order.Amount = Convert.ToDecimal(orderModel.amount);
                order.TicketType = (Convert.ToInt32(orderModel.ticketType) == 1) ? "EARLYBIRD" : "GENERAL";
                order.ActualPrice = Convert.ToDecimal(orderModel.actualPrice);
                order.DiscountAmount = Convert.ToDecimal(orderModel.discountedAmount);
                order.PriceAfterDiscount = Convert.ToDecimal(orderModel.priceAfterDiscount);
                order.ProcessingFee = Convert.ToDecimal(orderModel.processingFee);
                order.CreatedDate = DateTime.Now;
                order.OrderNo = serialNo;
                order.Status = "SUCCESS";
                order.PaypalOrderID = orderModel.id;
                order.PaymentStatus = "COMPLETED";
                order.Intent = orderModel.intent;
                db.tblTicketOrders.Add(order);
                db.SaveChanges();

                foreach (var seat in seats)
                {
                    seat.OrderID = order.OrderID;
                    seat.SessionID = "";
                }
                db.SaveChanges();

                string DataContents = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath("~/Templates/ticketseats.html"));

                var items = db.Database.SqlQuery<SelectedSeatsV5>("select SessionID, count(*) as 'NoOfSeats', sum(Price) as 'TotalPrice' from tblSeatSelections where EventID = " + eventID + " and SessionID = '' and ORDERID = " + order.OrderID + " group by SessionID").ToList();

                // check again
                string Receipt = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath("~/Templates/receiptseats.html"));
                string type = "";

                if (coupon_used != null)
                {
                    coupon_used.SessionID = "";
                    coupon_used.OrderID = order.OrderID;

                    //decimal amount = items.Sum(i => i.TotalPrice);

                    coupon_code = coupon_used.tblCoupon.CouponCode;

                    if (coupon_used.tblCoupon.DiscountIn == "Percentage")
                    {
                        //string tiers = coupon_used.tblCoupon.tblCouponGroup.Tiers;
                        //var seat_items = db.Database.SqlQuery<SelectedSeatsV5>("select SessionID, count(*) as 'NoOfSeats', sum(Price) as 'TotalPrice' from tblSeatSelections where EventID = " + eventID + " and SessionID = '' and orderid = " + order.OrderID + " AND SEATID IN(SELECT SEATID FROM tblSeat WHERE SeatRowID IN(SELECT SeatRowID FROM tblSeatRows WHERE BlockID IN(SELECT BLOCKID FROM tblEventLayoutBlocks WHERE TierID IN(" + tiers + ")))) group by SessionID").ToList();
                        //coupon_value = Convert.ToDecimal(coupon_used.tblCoupon.Discount);
                        //decimal seat_amount = seat_items.Sum(i => i.TotalPrice);
                        //discount_amount = ((seat_amount * coupon_value / 100));

                        coupon_value = Convert.ToDecimal(coupon_used.tblCoupon.Discount);

                        if (evt.ProcessingFee != null && evt.ProcessingFee > 0)
                        {
                            //decimal process_fees_perc = Convert.ToDecimal(evt.ProcessingFee);
                            //decimal process_amount = ((final_amount * process_fees_perc / 100));

                            //order.ProcessingFee = process_amount;
                            //db.SaveChanges();

                            decimal process_fees_perc = Convert.ToDecimal(evt.ProcessingFee);
                            decimal process_amount = Convert.ToDecimal(orderModel.processingFee);

                            Receipt = Receipt.Replace("[perc]", Decimal.Round(process_fees_perc, 0).ToString() + "%");
                            Receipt = Receipt.Replace("[service_charge]", Decimal.Round(process_amount, 2).ToString());
                        }
                        else
                        {
                            Receipt = Receipt.Replace("[service_charge]", "0");
                            Receipt = Receipt.Replace("[prec]", "");
                        }
                    }
                    else if (coupon_used.tblCoupon.DiscountIn == "Value")
                    {

                        //string tiers = coupon_used.tblCoupon.tblCouponGroup.Tiers;
                        //coupon_value = Convert.ToDecimal(coupon_used.tblCoupon.Discount);
                        //discount_amount = (Convert.ToDecimal(coupon_used.tblCoupon.Discount));

                        if (evt.ProcessingFee != null && evt.ProcessingFee > 0)
                        {
                            //decimal process_fees_perc = Convert.ToDecimal(evt.ProcessingFee);
                            //decimal process_amount = ((final_amount * process_fees_perc / 100));
                            //order.ProcessingFee = process_amount;

                            decimal process_fees_perc = Convert.ToDecimal(evt.ProcessingFee);
                            decimal process_amount = Convert.ToDecimal(orderModel.processingFee);

                            Receipt = Receipt.Replace("[perc]", Decimal.Round(process_fees_perc, 0).ToString() + "%");
                            Receipt = Receipt.Replace("[service_charge]", Decimal.Round(process_amount, 2).ToString());
                        }
                        else
                        {
                            Receipt = Receipt.Replace("[service_charge]", "0");
                            Receipt = Receipt.Replace("[perc]", "");
                        }


                    }


                    coupon_used.SessionID = "";
                    coupon_used.OrderID = order.OrderID;
                    coupon_used.tblCoupon.UsedDate = DateTime.Now;
                    coupon_used.tblCoupon.IsUsed = true;
                    db.SaveChanges();
                }
                else
                {
                    if (evt.ProcessingFee != null && evt.ProcessingFee > 0)
                    {
                        //decimal amount = items.Sum(i => i.TotalPrice);
                        decimal process_fees_perc = Convert.ToDecimal(evt.ProcessingFee);
                        //decimal process_amount = ((amount * process_fees_perc / 100));
                        decimal process_amount = Convert.ToDecimal(orderModel.processingFee);
                        //order.ProcessingFee = process_amount;
                        //db.SaveChanges();
                        Receipt = Receipt.Replace("[perc]", Decimal.Round(process_fees_perc, 0).ToString() + "%");
                        Receipt = Receipt.Replace("[service_charge]", Decimal.Round(process_amount, 2).ToString());
                    }
                    else
                    {
                        Receipt = Receipt.Replace("[service_charge]", "0");
                        Receipt = Receipt.Replace("[perc]", "");
                    }
                }

                int orderID = order.OrderID;

                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(order.OrderID.ToString(), QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);
                qrCodeImage.Save("C:\\Websites\\DevFrontEnd\\QRCodes\\" + orderID.ToString() + ".png");

                order.QRCode = orderID.ToString() + ".png";
                db.SaveChanges();

                string username = System.Configuration.ConfigurationManager.AppSettings["Username"].ToString();
                string password = System.Configuration.ConfigurationManager.AppSettings["Password"].ToString();

                try
                {
                    if (order.NoOfTickets > 0)
                    {
                        var booked_seats = db.tblSeatSelections.Where(s => s.OrderID == order.OrderID).ToList();
                        var o = db.tblTicketOrders.Where(os => os.OrderID == orderID).SingleOrDefault();
                        DataContents = DataContents.Replace("[srno]", o.OrderNo.ToString());
                        DataContents = DataContents.Replace("[event_name]", evt.EventName.ToString());
                        DataContents = DataContents.Replace("[image_name]", evt.EventBanner.ToString());
                        DataContents = DataContents.Replace("[tickets]", string.Join(",", booked_seats.Select(s => s.tblSeat.SeatNumber)));
                        DataContents = DataContents.Replace("[address]", evt.tblVenue.VenueName);
                        DataContents = DataContents.Replace("[date]", Convert.ToDateTime(evt.EventDate).ToString("dddd, dd MMMM yyyy") + " " + evt.ShowTime);
                        DataContents = DataContents.Replace("[image]", order.QRCode);

                        SmtpClient smtp = new SmtpClient
                        {
                            Host = "smtp.gmail.com", // smtp server address here…
                            Port = 587,
                            EnableSsl = true,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            Credentials = new System.Net.NetworkCredential(username, password),
                            Timeout = 30000,
                        };


                        MailMessage message = new MailMessage(username, order.Email, "Your Tickets", DataContents);
                        message.IsBodyHtml = true;
                        smtp.Send(message);

                        tblMailLog log = new tblMailLog();
                        log.OrderID = order.OrderID;
                        log.Email = order.Email;
                        log.SentDate = DateTime.Now;
                        log.MailContent = DataContents;
                        db.tblMailLogs.Add(log);
                        db.SaveChanges();

                        //string Receipt = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath("~/Templates/receiptseats.html"));
                        //string type = "";


                        decimal amountBeforeDiscount = Decimal.Round(Convert.ToDecimal(items.Sum(s => s.TotalPrice)), 2);

                        Receipt = Receipt.Replace("[name]", order.Name.ToString());
                        Receipt = Receipt.Replace("[order_no]", order.OrderID.ToString());

                        Receipt = Receipt.Replace("[sr_no]", order.OrderNo.ToString());
                        Receipt = Receipt.Replace("[type]", type);
                        Receipt = Receipt.Replace("[individual_price]", Convert.ToDecimal(amountBeforeDiscount / order.NoOfTickets).ToString());
                        Receipt = Receipt.Replace("[amount]", Decimal.Round(amountBeforeDiscount, 2).ToString());
                        Receipt = Receipt.Replace("[total_amount]", Decimal.Round((decimal)order.Amount, 2).ToString());
                        Receipt = Receipt.Replace("[event_name]", evt.EventName.ToString());
                        Receipt = Receipt.Replace("[no_of_tickets]", booked_seats.Count().ToString());
                        Receipt = Receipt.Replace("[address]", evt.tblVenue.VenueName);
                        Receipt = Receipt.Replace("[date_time]", Convert.ToDateTime(order.CreatedDate).ToString("dddd, dd MMMM yyyy hh:mm tt"));
                        Receipt = Receipt.Replace("[event_date_time]", Convert.ToDateTime(evt.EventDate).ToString("dddd, dd MMMM yyyy") + " " + evt.ShowTime);
                        Receipt = Receipt.Replace("[image]", order.QRCode);
                        Receipt = Receipt.Replace("[seat_nos]", string.Join(",", booked_seats.Select(s => s.tblSeat.SeatNumber)));

                        if (order.DiscountAmount > 0 && coupon_code != "")
                        {
                            if (coupon_used.tblCoupon.DiscountIn == "Percentage")
                            {
                                Receipt = Receipt.Replace("[coupon_code]", coupon_code + "-" + coupon_value + "%");
                                //Receipt = Receipt.Replace("[discount]", Decimal.Round(discount_amount, 2).ToString());
                            }
                            else if (coupon_used.tblCoupon.DiscountIn == "Value")
                            {
                                Receipt = Receipt.Replace("[coupon_code]", coupon_code);
                                //Receipt = Receipt.Replace("[discount]", Decimal.Round(discount_amount, 2).ToString());
                            }
                            Receipt = Receipt.Replace("[discount]", Decimal.Round((decimal)order.DiscountAmount, 2).ToString());
                        }
                        else
                            Receipt = Receipt.Replace("[class_name]", "hide_discount");

                        if (order.TicketType == "EARLYBIRD")
                            Receipt = Receipt.Replace("[eb_discount]", Decimal.Round((decimal)order.DiscountAmount, 2).ToString());
                        else
                            Receipt = Receipt.Replace("[eb_class_name]", "hide_discount");

                        MailMessage message1 = new MailMessage(username, order.Email, "Your Receipt", Receipt);
                        message1.IsBodyHtml = true;
                        smtp.Send(message1);

                        MailMessage message2 = new MailMessage(username, "javanikabooking@gmail.com", "New Order", Receipt);
                        message2.IsBodyHtml = true;
                        smtp.Send(message2);

                        //FinalResponse response = new FinalResponse();
                        resp.orderID = orderID;
                        resp.status = "OK";
                    }
                    else
                    {
                        string emailDistList = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["EmailDistList"]);
                        string[] emails = emailDistList.Split(',').ToArray();

                        SmtpClient smtp = new SmtpClient
                        {
                            Host = "smtp.gmail.com", // smtp server address here…
                            Port = 587,
                            EnableSsl = true,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            Credentials = new System.Net.NetworkCredential(username, password),
                            Timeout = 30000,
                        };

                        foreach (var e in emails)
                        {
                            MailMessage message1 = new MailMessage(username, e, "Zero Seats Order Received", "You have received a zero seats order. Order number is " + order.OrderID);
                            message1.IsBodyHtml = true;
                            smtp.Send(message1);
                        }

                        resp.orderID = 0;
                        resp.status = "ZERO";
                    }
                }
                catch (Exception ex)
                {
                    resp.orderID = 0;
                    resp.status = "FAILED";
                }

            }
            catch(Exception ex)
            {
                resp.orderID = 0;
                resp.status = "FATAL";
            }

            return resp;
        }
    }
}
