using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Java.API.Models;

using System.Web.Cors;
using System.Web.Http.Cors;
namespace Java.API.Controllers
{
    public class Order
    {
        public int orderID { get; set; }
        public string event_name { get; set; }
        public string venue_name { get; set; }
        public string event_date { get; set; }
        public string event_time { get; set; }
    }

    public class MyOrders
    {
        public string status { get; set; }
        public List<Order> my_orders { get; set; }
    }

    public class MyOrderModel
    {
        public string email { get; set; }
        public string otp { get; set; }
    }

    public class MyOrdersController : ApiController
    {

        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public MyOrders GetOrders(MyOrderModel m)
        {
            JAVADBEntities db = new JAVADBEntities();

            var validateOTP = db.tblOTPS.Where(w => w.Email == m.email).OrderByDescending(w => w.OTPID).Take(1).SingleOrDefault();

            List<Order> my_orders = new List<Order>();
            MyOrders o = new MyOrders();

            if (validateOTP != null)
            {
                if (validateOTP.OTP == m.otp)
                {
                    var orders = db.tblTicketOrders.Where(e => e.Email == m.email).OrderByDescending(or => or.OrderID).ToList();

                    foreach(var order in orders)
                    {
                        var evt = db.tblEvents.Where(e => e.EventID == order.EventID).SingleOrDefault();

                        my_orders.Add(new Order() {
                            orderID = order.OrderID,
                            venue_name = evt.tblVenue.VenueName,
                            event_name = evt.EventName,
                            event_date = Convert.ToDateTime(evt.EventDate).ToString("dddd, dd MMMM yyyy"),
                            event_time = evt.ShowTime
                        });
                    }

                    o.status = "OK";
                    o.my_orders = my_orders;
                    return o;
                }
                else
                {
                    o.status = "OTP MISMATCH";
                    return o;
                }
            }
            else
            {
                o.status = "OTP EXPIRED";
                return o;
            }
        }
    }
}
