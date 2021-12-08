using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Admin.Models;
namespace Admin.Controllers
{
    public class AuthCode
    {
        public string CouponCode { get; set; }
    }

    public class Tier
    {
        public int tierID { get; set; }
        public string TierName { get; set; }
        public decimal Price { get; set; }
    }


    public class CouponModel
    {
        public string CouponCode { get; set; }
        public string DiscountIn { get; set; }
        public string EventName { get; set; }
        public decimal Discount { get; set; }
    }




    public class CouponGenerationController : Controller
    {
        // GET: CouponGeneration
        public ActionResult Index()
        {
            JAVADBEntities db = new JAVADBEntities();

            ViewData["Events"] = db.tblEvents.SqlQuery("SELECT * FROM TBLEVENTS WHERE CONVERT(DATE,EVENTDATE,103) >= CONVERT(DATE,GETDATE(),103) and EventName != 'Test Event' ORDER BY EVENTID DESC").ToList();

            ViewData["CouponData"] = db.tblCouponGroups.ToList();

            return View();
        }

        public string Generate()
        {
            int eventID = Convert.ToInt32(Request["drpEventID"]);
            string prefix = Request["txtPrefix"];
            int noOfCoupons = Convert.ToInt32(Request["txtNoOfCoupons"]);
            decimal percentage = Convert.ToDecimal(Request["txtPercentage"]);

            JAVADBEntities db = new JAVADBEntities();

            tblCouponGroup group = new tblCouponGroup();
            group.NoOfCoupons = noOfCoupons;
            group.Prefix = prefix;
            group.EventID = eventID;
            group.Discount = percentage;
            group.DiscountIn = Request["drpDiscountIn"];
            group.Tiers = Request["tiers"];
            group.CreatedDate = DateTime.Now;
            db.tblCouponGroups.Add(group);
            db.SaveChanges();

            GenerateRandomCodes(prefix, group.CouponGroupID, group.DiscountIn, noOfCoupons, percentage, eventID);


            return "OK";
        }

        public void GenerateRandomCodes(string prefix,int groupID, string discountIn, int noOfcoupons, decimal percentage, int eventID)
        {
            JAVADBEntities db = new JAVADBEntities();

            int duplicates = 0;


            Random random = new Random((int)DateTime.Now.Ticks);
            for (int o = 0; o < noOfcoupons; o++)
            {
                int code = random.Next(100000000, 999999999);
                string strCode = prefix + code.ToString();
                bool exists = db.Database.SqlQuery<AuthCode>("SELECT 1 FROM tblCoupons WHERE COUPONCODE = '" + strCode + "'").Any();
                
                if (!exists)
                    db.Database.ExecuteSqlCommand("INSERT INTO tblCoupons (COUPONCODE,CouponGroupID,DiscountIn,Discount,EventID,CREATEDDATE) VALUES('" + strCode + "'," + groupID + ",'" + discountIn +"', "+  percentage + ", " + eventID +  ",GETDATE())");
                else
                    duplicates++;
            }

            if(duplicates > 0)
                GenerateRandomCodes(prefix, groupID, discountIn, duplicates, percentage, eventID);
        }

        public JsonResult GetTiers(int eventID)
        {
            JAVADBEntities db = new JAVADBEntities();

            var tiers = db.Database.SqlQuery<Tier>("SELECT DISTINCT tr.tierID, tr.TierName, te.Price FROM tblEventLayoutBlocks te inner join tblTier tr on te.TierID = tr.TierID where eventID = " + eventID + "").ToList();

            List<Tier> lstTiers = new List<Tier>();

            foreach(var t in tiers)
            {
                lstTiers.Add(new Tier()
                {
                    tierID = t.tierID,
                    TierName = t.TierName,
                    Price = Convert.ToDecimal(t.Price)
                });
            }

            return Json(lstTiers, JsonRequestBehavior.AllowGet);
        }


        public void ExportCoupons(int groupID)
        {
            JAVADBEntities db = new JAVADBEntities();

            Response.AppendHeader("Content-Disposition", "attachment;filename=Coupons.xls");
            Response.ContentType = "application/vnd.ms-excel";

            string constr = "data source=ESSELBKUP;initial catalog=JAVADB;User Id=essel208;Password=essel@2008;MultipleActiveResultSets=True";

            string sql = "SELECT TC.COUPONCODE, TC.DISCOUNTIN, TC.DISCOUNT, TE.EVENTNAME FROM TBLCOUPONS TC INNER JOIN TBLCOUPONGROUPS TG ON TC.COUPONGROUPID = TG.COUPONGROUPID INNER JOIN TBLEVENTS TE ON TG.EVENTID = TE.EVENTID WHERE TC.COUPONGROUPID = " + groupID + "";
            
            string data = "";

            var coupons = db.Database.SqlQuery<CouponModel>(sql).ToList();

            data += "<table border='1'>";
            data += "<tr>";
            data += "<td><center><b>Coupon Code</b></center></td>";
            data += "<td><center><b>Discount In</b></center></td>";
            data += "<td><center><b>Discount</b></center></td>";
            data += "<td><center><b>Event Name</b></center></td>";
            data += "</tr>";

            foreach (var coupon in coupons)
            {   
                data += "<tr>";
                data += "<td><center>" + coupon.CouponCode + "</center></td>";
                data += "<td><center>" + coupon.DiscountIn + "</b></center></td>";
                data += "<td><center>" + coupon.Discount + "</center></td>";
                data += "<td><center>" + coupon.EventName + "</center></td>";
                data += "</tr>";  
            }

            data += "</table>";
            Response.Write(data);
        }
    }
}