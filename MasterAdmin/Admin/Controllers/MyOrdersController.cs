using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;
using DapperParameters;
using System.Data.SqlClient;
using Admin.Models;
using AutoMapper;

namespace Admin.Controllers
{
    

    public class MyOrdersController : Controller
    {
        JAVADBEntities db = new JAVADBEntities();

        // GET: MyOrders
        public ActionResult Index()
        {
            int eventID = (string.IsNullOrEmpty(Request["eventID"]))
                ? 0 : Convert.ToInt32(Request["eventID"]);
            if (TempData["Events"] == null)
            {
                List<int> rowsPerPage = new List<int>();
                rowsPerPage.Add(10);
                rowsPerPage.Add(20);
                rowsPerPage.Add(30);
                rowsPerPage.Add(50);
                rowsPerPage.Add(100);
                TempData["rowsPerPage"] = rowsPerPage;

                List<EventsViewModel> eventsVM = getEvents();
                TempData["Events"] = eventsVM;
            }
            List<OrderModel> orderModels = new List<OrderModel>();
            OrderRequest orderRequest = new OrderRequest();
            orderRequest.EventId = eventID;
            orderRequest.pageNum = 1;
            orderRequest.rowsPerPage = (string.IsNullOrEmpty(Request["rowsPerPage"]))
                ? 10 : Convert.ToInt32(Request["rowsPerPage"]); ;

            orderModels = getOrders(orderRequest);

            return View(orderModels);
            
        }

        private List<OrderModel> getOrders(OrderRequest orderRequest)
        {
            List<OrderModel> orderModels = null;
            string constr = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            //string sql = "SELECT * FROM ORDERS ORDER BY 1 DESC";

            using (SqlConnection con = new SqlConnection(constr))
            {
                orderModels = new List<OrderModel>();
                var parameters = new DynamicParameters();
                parameters.Add("@EventId", orderRequest.EventId);
                parameters.Add("@rowsPerPage", orderRequest.rowsPerPage);
                parameters.Add("@pageNum", orderRequest.pageNum);

                using (SqlMapper.GridReader multi = con.QueryMultiple("GetOrders"
                    , parameters
                    , commandType: System.Data.CommandType.StoredProcedure))
                {
                    orderModels = multi.Read<OrderModel>().ToList();
                    //var b = multi.Read<OrderModel>().ToList();
                }
            }
            return orderModels;
        }

        private List<EventsViewModel> getEvents()
        {
            List<EventsViewModel> eventsVM = null;
            using (var context = new JAVADBEntities())
            {
                eventsVM = new List<EventsViewModel>();
                var res = context.tblEvents.ToList();
                Mapper.Map(res, eventsVM);
            }
            return eventsVM;
        }

        public JsonResult GetOrder()
        {
            //JAVADBEntities db = new JAVADBEntities();

            int orderID = Convert.ToInt32(Request["orderID"]);
            var order = db.tblTicketOrders.Where(o => o.OrderID == orderID).SingleOrDefault();
            var evt = db.tblEvents.Where(o => o.EventID == order.EventID).SingleOrDefault();

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("OrderID", order.OrderID.ToString());
            data.Add("SrNo", order.OrderNo.ToString());
            data.Add("NoOfTickets", order.NoOfTickets.ToString());
            data.Add("Amount", order.Amount.ToString());
            data.Add("PaypalOrderID", order.PaypalOrderID);
            data.Add("PaymentStatus", order.PaymentStatus);
            data.Add("OrderStatus", order.Status);
            data.Add("EventName", evt.EventName);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public void ExportMIS()
        {
            //JAVADBEntities db = new JAVADBEntities();

            Response.AppendHeader("Content-Disposition", "attachment;filename=Orders.xls");
            Response.ContentType = "application/vnd.ms-excel";


            string sql = "SELECT O.OrderID, O.OrderNo as 'OrderNo',EVT.EventName as 'EventName', O.CreatedDate as 'PurchaseDate',o.Name as 'CustomerName',o.Mobile as 'Mobile', Email, (case when evt.EventID = 25 then o.AmountPerTicket else (Amount / NoOfTickets)  end) as 'TicketPrice',(case when evt.EventID = 25 then 'NA' else 'SEATS' end) as 'BOOKINGTYPE',NoOfTickets as 'Quantity', Amount as 'TotalAmount', 'NA' as 'ServiceFees', Amount as 'NetAmount'  FROM tblTicketOrders O  INNER JOIN tblEvents EVT ON O.EventID = EVT.EventID WHERE O.STATUS = 'SUCCESS' and NoOfTickets > 0  ORDER BY OrderNo ASC";

            if (!string.IsNullOrEmpty(Request["drpEventID"]))
                sql = "SELECT O.OrderID, O.OrderNo as 'OrderNo',EVT.EventName as 'EventName', O.CreatedDate as 'PurchaseDate',o.Name as 'CustomerName',o.Mobile as 'Mobile', Email, (case when evt.EventID = 25 then o.AmountPerTicket else (Amount / NoOfTickets) end) as 'TicketPrice',(case when evt.EventID = 25 then 'NA' else 'SEATS' end) as 'BOOKINGTYPE',NoOfTickets as 'Quantity', Amount as 'TotalAmount', 'NA' as 'ServiceFees', Amount as 'NetAmount'  FROM tblTicketOrders O  INNER JOIN tblEvents EVT ON O.EventID = EVT.EventID WHERE O.STATUS = 'SUCCESS'  and NoOfTickets > 0 AND EVT.EVENTID = " + Convert.ToInt32(Request["drpEventID"]) + " ORDER BY OrderNo ASC";

            string data = "";

            var orders = db.Database.SqlQuery<OrderModel>(sql).ToList();

            data += "<table border='1'>";
            data += "<tr>";
            data += "<td><center><b>Event Name</b></center></td>";
            data += "<td><center><b>Date</b></center></td>";
            data += "<td><center><b>Serial</b></center></td>";
            data += "<td><center><b>Name</b></center></td>";
            data += "<td><center><b>E-mail</b></center></td>";
            data += "<td><center><b>Book</b></center></td>";
            data += "<td><center><b>Price</b></center></td>";
            data += "<td><center><b>Total</b></center></td>";
            //data += "<td><center><b>Net Amount</b></center></td>";
            //data += "<td><center><b>Discount Coupon</b></center></td>";
            data += "<td><center><b>Discount</b></center></td>";
            data += "<td><center><b>Fees</b></center></td>";
            data += "<td><center><b>Net Total</b></center></td>";
            //data += "<td><center><b>PayPal Fee</b></center></td>";
            //data += "<td><center><b>Phone#</b></center></td>";
            data += "</tr>";

            foreach (var order in orders)
            {
                if (order.OrderNo > 5)
                {
                    var coupon_used = db.tblOrderCoupons.Where(t => t.OrderID == order.OrderID && t.SessionID == "").SingleOrDefault();

                    var o = db.tblTicketOrders.Where(r => r.OrderID == order.OrderID).SingleOrDefault();

                    decimal discount_amount = 0;
                    string coupon_code = "";
                    decimal coupon_value = 0;



                    var items = db.Database.SqlQuery<SelectedSeatsOrder>("select SessionID, count(*) as 'NoOfSeats', sum(Price) as 'TotalPrice' from tblSeatSelections where SessionID = '' and ORDERID = " + order.OrderID + " group by SessionID").ToList();

                    data += "<tr>";
                    data += "<td><center>" + order.EventName + "</center></td>";
                    data += "<td><center>" + order.PurchaseDate + "</b></center></td>";
                    data += "<td><center>" + order.OrderNo + "</center></td>";
                    data += "<td><center>" + order.CustomerName + "</center></td>";
                    data += "<td><center>" + order.Email + "</center></td>";
                    data += "<td><center>" + order.Quantity + "</center></td>";

                    if (o.DiscountAmount != null)
                    {
                        data += "<td><center>" + o.AmountPerTicket + "</center></td>";
                    }
                    else
                        data += "<td><center>" + order.TicketPrice + "</center></td>";

                    if (coupon_used != null)
                    {
                        if (coupon_used.tblCoupon.DiscountIn == "Percentage")
                        {
                            string tiers = coupon_used.tblCoupon.tblCouponGroup.Tiers;

                            var seat_items = db.Database.SqlQuery<SelectedSeatsOrder>("select SessionID, count(*) as 'NoOfSeats', sum(Price) as 'TotalPrice' from tblSeatSelections where  SessionID = '' and orderid = " + order.OrderID + " AND SEATID IN(SELECT SEATID FROM tblSeat WHERE SeatRowID IN(SELECT SeatRowID FROM tblSeatRows WHERE BlockID IN(SELECT BLOCKID FROM tblEventLayoutBlocks WHERE TierID IN(" + tiers + ")))) group by SessionID").ToList();

                            coupon_value = Convert.ToDecimal(coupon_used.tblCoupon.Discount);

                            decimal seat_amount = seat_items.Sum(i => i.TotalPrice);

                            discount_amount = ((seat_amount * coupon_value / 100));

                            coupon_code = coupon_used.tblCoupon.CouponCode;
                        }
                        else if (coupon_used.tblCoupon.DiscountIn == "Value")
                        {

                            string tiers = coupon_used.tblCoupon.tblCouponGroup.Tiers;

                            coupon_value = Convert.ToDecimal(coupon_used.tblCoupon.Discount);

                            discount_amount = (Convert.ToDecimal(coupon_used.tblCoupon.Discount));

                            coupon_code = coupon_used.tblCoupon.CouponCode;
                        }
                    }


                    data += "<td><center>" + Decimal.Round(Convert.ToDecimal(order.TicketPrice * order.Quantity), 2) + "</center></td>";

                    //if (order.BOOKINGTYPE == "SEATS")
                    //{
                    //    var seats = db.tblSeatSelections.Where(s => s.OrderID == order.OrderID).ToList();

                    //    data += "<td><center>" + string.Join(",", seats.Select(s => s.tblSeat.SeatNumber)) + "</center></td>";
                    //}
                    //else
                    //    data += "<td><center>N/A</center></td>";



                    //data += "<td><center>" + (!string.IsNullOrEmpty(coupon_code) ? coupon_code : "-") + "</center></td>";
                    data += "<td><center>" + discount_amount + "</center></td>";



                    decimal final_amount = Convert.ToDecimal(order.TotalAmount);
                    decimal process_fees_perc = 0;
                    decimal process_amount = 0;

                    var evt = db.tblEvents.Where(e => e.EventID == o.EventID).SingleOrDefault();

                    if (evt.ProcessingFee != null && evt.ProcessingFee > 0)
                    {
                        process_fees_perc = Convert.ToDecimal(evt.ProcessingFee);
                        process_amount = ((final_amount * process_fees_perc / 100));
                        final_amount = (final_amount + process_amount);

                        data += "<td><center>" + process_amount + "</center></td>";
                    }
                    else
                        data += "<td><center>0</center></td>";

                    //if (order.BOOKINGTYPE == "SEATS")
                    //    data += "<td><center>" + Decimal.Round(Convert.ToDecimal((items.Sum(i => i.TotalPrice) - discount_amount) + process_amount),2) + "</center></td>";
                    //else
                    data += "<td><center>" + Decimal.Round(final_amount, 2) + "</center></td>";
                    //data += "<td><center></center></td>";
                    data += "</tr>";
                }
            }
            Response.Write(data);
        }
    }
}