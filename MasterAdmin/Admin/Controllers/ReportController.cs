using Admin.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Admin.Controllers
{
    public class ReportController : Controller
    {
        JAVADBEntities db = new JAVADBEntities();

        #region Public Functions
        public ActionResult Index()
        {
            int eventID = (string.IsNullOrEmpty(Request["eventID"]))
                ? 0 : Convert.ToInt32(Request["eventID"]);
            //if (TempData["Events"] == null)
            //{
            List<int> rowsPerPage = new List<int>();
            rowsPerPage.Add(10);
            rowsPerPage.Add(20);
            rowsPerPage.Add(30);
            rowsPerPage.Add(50);
            rowsPerPage.Add(100);
            TempData["rowsPerPage"] = rowsPerPage;

            //    List<EventsViewModel> eventsVM = getEvents();
            //    TempData["Events"] = eventsVM;
            //}
            if (TempData["EventsOrders"] == null)
            {
                List<EventOrderModel> eventOrderModels = new List<EventOrderModel>();
                OrderRequest orderRequest = new OrderRequest();
                orderRequest.EventId = eventID;
                orderRequest.pageNum = 1;
                orderRequest.rowsPerPage = (string.IsNullOrEmpty(Request["rowsPerPage"]))
                    ? 50 : Convert.ToInt32(Request["rowsPerPage"]); ;

                eventOrderModels = getOrders(orderRequest);
                TempData["EventsOrders"] = eventOrderModels;
            }
            return View();
        } 

        public ActionResult Orders(OrderRequest orderRequest)
        {
            TempData["rowsPerPage"] = setRowsPerPage();
            TempData["orderBy"] = setOrderBy();
            ViewData["OrderRequest"] = getOrderRequest(orderRequest);
            ViewData["OrderDetails"] = getOrderDetails((OrderRequest)ViewData["OrderRequest"]);
            return View("~/Views/report/Orders.cshtml");
        }

        public ActionResult SearchEvents(OrderRequest eventRequest)
        {
            List<tblEvent> eventsVM = new List<tblEvent>();
            List<EventsViewModel> eventsResponse = new List<EventsViewModel>();
            OrderRequest orderRequest = new OrderRequest();
            orderRequest.EventId = (int)eventRequest.EventId;
            orderRequest.rowsPerPage = (int)eventRequest.rowsPerPage;
            orderRequest.pageNum = (int)eventRequest.pageNum;
            orderRequest.orderBy = (string)eventRequest.orderBy;
            ViewData["OrderRequest"] = eventRequest;
            ViewData["OrderDetails"] = getOrderDetails(orderRequest); ;
            return View("~/Views/Report/Orders.cshtml");
        }
        #endregion

        #region Private Functions
        private List<EventOrderModel> getOrders(OrderRequest orderRequest)
        {
            List<EventOrderModel> eventOrderModels = null;
            string constr = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            using (SqlConnection con = new SqlConnection(constr))
            {
                eventOrderModels = new List<EventOrderModel>();
                var parameters = new DynamicParameters();
                parameters.Add("@EventId", orderRequest.EventId);
                parameters.Add("@rowsPerPage", orderRequest.rowsPerPage);
                parameters.Add("@pageNum", orderRequest.pageNum);

                using (SqlMapper.GridReader multi = con.QueryMultiple("GetOrders"
                    , parameters
                    , commandType: System.Data.CommandType.StoredProcedure))
                {
                    eventOrderModels = multi.Read<EventOrderModel>().ToList();
                }
            }
            return eventOrderModels;
        }

        private OrderDetails getOrderDetails(OrderRequest orderRequest)
        {
            OrderDetails orderDetails = null;
            List<EventOrderDetails> eventOrderModels = null;
            OrderSummary orderSummary = null;
            string constr = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            using (SqlConnection con = new SqlConnection(constr))
            {
                eventOrderModels = new List<EventOrderDetails>();
                orderDetails = new OrderDetails();
                orderSummary = new OrderSummary();
                var parameters = new DynamicParameters();
                parameters.Add("@EventId", orderRequest.EventId);
                parameters.Add("@OrderId", orderRequest.OrderId);
                parameters.Add("@rowsPerPage", orderRequest.rowsPerPage);
                parameters.Add("@pageNum", orderRequest.pageNum);

                using (SqlMapper.GridReader multi = con.QueryMultiple("GetOrdersDetails"
                    , parameters
                    , commandType: System.Data.CommandType.StoredProcedure))
                {
                    eventOrderModels = multi.Read<EventOrderDetails>().ToList();
                    orderSummary = multi.Read<OrderSummary>().SingleOrDefault();

                    orderDetails.eventOrderDetails = eventOrderModels;
                    orderDetails.orderSummary = orderSummary;
                }
            }
            return orderDetails;
        }

        private List<int> setRowsPerPage()
        {
            List<int> rowsPerPage = new List<int>();
            rowsPerPage.Add(5);
            rowsPerPage.Add(10);
            rowsPerPage.Add(25);
            rowsPerPage.Add(50);
            rowsPerPage.Add(100);
            return rowsPerPage;
        }

        private List<string> setOrderBy()
        {
            List<string> orderBy = new List<string>();
            orderBy.Add("Orders");
            orderBy.Add("Customer Name");
            orderBy.Add("Phone No");
            orderBy.Add("Ticket Price");
            orderBy.Add("Ticket Type");
            orderBy.Add("Order Date");
            return orderBy;
        }

        private OrderRequest getOrderRequest(OrderRequest request)
        {
            request.EventId = request.EventId;
            request.pageNum = (request.pageNum == 0) ? 1 : Convert.ToInt32(request.pageNum);
            request.rowsPerPage = (string.IsNullOrEmpty(Request["rowsPerPage"]))  ? 50 : Convert.ToInt32(Request["rowsPerPage"]);
            return request;
        }
        #endregion
    }
}