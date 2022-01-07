using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Admin.Models;

using System.Data.Sql;
using System.Data.SqlClient;
using System.IO;
using ClosedXML.Excel;
using System.Configuration;
using System.Data;
namespace Admin.Controllers
{
    public class VOBAEventController : Controller
    {
        // GET: VOBAEvent
        JAVADBEntities db = new JAVADBEntities();
        public ActionResult Index()
        {
            return View(db.VOBARegistrations.Where(v=>v.PaymentStatus == "COMPLETED").OrderByDescending(d=>d.RegistrationID).ToList());
        }

        public void ExportMIS()
        {
            JAVADBEntities db = new JAVADBEntities();
            //string constr = "data source=S104-238-124-19;initial catalog=JAVADB;User Id=sa;password=mahadev21!;MultipleActiveResultSets=True";
            string constr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            string sql = "select ParentName as 'Parent Name', ParentEmail as 'Parent Email', ParentMobile as 'Parent Mobile', ChildName as 'Child Name', ChildDOB as 'Date Of Birth',25 as 'Amount (In $)',PayPalOrderID as 'Paypal Order ID', PaymentStatus as 'Payment Status', 'SUCCESS' as 'Registration Status', CreatedDate as 'Date Of Registration' from VOBARegistration where PaymentStatus = 'COMPLETED'";

            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand(sql))
                {
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
                                var ws = wb.Worksheets.Add(dt, "Brand Wise Report");

                                ws.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;


                                Response.Clear();
                                Response.Buffer = true;
                                Response.Charset = "";
                                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                                Response.AddHeader("content-disposition", "attachment;filename=VOBA.xlsx");
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
    }
}