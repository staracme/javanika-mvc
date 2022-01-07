using Admin.Models;
using ClosedXML.Excel;
using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
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
                if(eventOrderModels.Count == 0)
                {
                    TempData["Message"] = "No records found.";
                    return RedirectToAction("Index", "NewEvents");
                }
                TempData["EventsOrders"] = eventOrderModels;
            }
            return View();
        } 

        public ActionResult Orders(OrderRequest orderRequest)
        {
            TempData["rowsPerPage"] = setRowsPerPage();
            TempData["orderBy"] = setOrderBy();
            TempData["ReportTypes"] = setReportTypes();
            ViewData["OrderRequest"] = getOrderRequest(orderRequest);
            OrderDetails od =  getOrderDetails((OrderRequest)ViewData["OrderRequest"]);
            if (od.eventOrderDetails.Count == 0)
            {
                TempData["Message"] = "<script>alert('No Orders found.');</script>";
                return RedirectToAction("Index", "NewEvents");
            }
            ViewData["OrderDetails"] = od;


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
            OrderDetails od = getOrderDetails(orderRequest);
            if (od.eventOrderDetails.Count == 0)
            {
                TempData["Message"] = "<script>alert('No Records found.');</script>";
                return RedirectToAction("Orders", "Report", new { EventID = orderRequest.EventId });
            }
            ViewData["OrderDetails"] = od;
            return View("~/Views/Report/Orders.cshtml");
        }

        public void GenerateReport(ReportParam reportParam)
        {
            string constr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            //string sql = "select ParentName as 'Parent Name', ParentEmail as 'Parent Email', ParentMobile as 'Parent Mobile', ChildName as 'Child Name', ChildDOB as 'Date Of Birth',25 as 'Amount (In $)',PayPalOrderID as 'Paypal Order ID', PaymentStatus as 'Payment Status', 'SUCCESS' as 'Registration Status', CreatedDate as 'Date Of Registration' from VOBARegistration where PaymentStatus = 'COMPLETED'";

            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "RptTicketSalesReport";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@EventId", reportParam.EventId));
                    using (SqlDataAdapter sda = new SqlDataAdapter())
                    {
                        cmd.Connection = con;
                        sda.SelectCommand = cmd;
                        using (DataTable dt = new DataTable())
                        {
                            sda.Fill(dt);
                            using (XLWorkbook wb = new XLWorkbook())
                            {
                                //wb.Worksheets.Add(dt, "Heritage & Allegra Business");
                                var ws = wb.Worksheets.Add(dt, reportParam.reportType + " Report");

                                ws.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;


                                Response.Clear();
                                Response.Buffer = true;
                                Response.Charset = "";
                                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                                Response.AddHeader("content-disposition", "attachment;filename="+ reportParam.reportType.Replace(" ", "-") +"-Report.xlsx");
                                using (MemoryStream MyMemoryStream = new MemoryStream())
                                {
                                    wb.SaveAs(MyMemoryStream);
                                    MyMemoryStream.WriteTo(Response.OutputStream);
                                }
                            }
                        }
                    }
                }
            }
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
                parameters.Add("@OrderBy", orderRequest.orderBy);
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
            //orderBy.Add("Order Date");
            return orderBy;
        }

        private List<string> setReportTypes()
        {
            List<string> reportTypes = new List<string>();
            reportTypes.Add("Ticket Sales Wise");
            return reportTypes;
        }

        private OrderRequest getOrderRequest(OrderRequest request)
        {
            request.EventId = request.EventId;
            request.pageNum = (request.pageNum == 0) ? 1 : Convert.ToInt32(request.pageNum);
            request.rowsPerPage = (string.IsNullOrEmpty(Request["rowsPerPage"]))  ? 50 : Convert.ToInt32(Request["rowsPerPage"]);
            request.orderBy = (string.IsNullOrEmpty(Request["orderBy"])) ? "Orders" : Request["orderBy"];
            return request;
        }
        #endregion
    }
}