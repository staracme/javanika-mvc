using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Admin.Models;
namespace Admin.Controllers
{
    public class BlockResponse
    {
        public int EventBlockID { get; set; }
        public int BlockID { get; set; }
        public string BlockName { get; set; }
        public string TierName { get; set; }
        public decimal Price { get; set; }
    }


    public class SeatResponse
    {
        public int seatID { get; set; }
        public string seatNumber { get; set; }
        public bool isBlockedByAdmin { get; set; }
    }

    public class BlockSeatResponse
    {
        public decimal price { get; set; }
        public List<SeatResponse> seats { get; set; }
    }


    public class SelectedSeats
    {
        public string SessionID { get; set; }
        public int NoOfSeats { get; set; }
        public decimal TotalPrice { get; set; }
    }



    public class BlockSeatController : Controller
    {
        // GET: BlockSeat

        JAVADBEntities db = new JAVADBEntities();

        public ActionResult Index()
        {
            JAVADBEntities db = new JAVADBEntities();

            ViewData["Events"] = db.tblEvents.OrderBy(e=>e.EventID).ToList();
            return View();
        }


        public JsonResult GetBlocks(int eventID)
        {

            var blocks = db.tblEventLayoutBlocks.Where(s => s.EventID == eventID).ToList();

            List<BlockResponse> response = new List<BlockResponse>();

            foreach(var block in blocks)
            {
                response.Add(new BlockResponse()
                {
                    EventBlockID = block.EventLayoutBlockID,
                    BlockID = Convert.ToInt32(block.BlockID),
                    BlockName = block.tblBlock.BlockNumber,
                    TierName = block.tblTier.TierName,
                    Price = Convert.ToDecimal(block.Price)
                });
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBlockPrice(int eventBlockID)
        {
            var block = db.tblEventLayoutBlocks.Where(s => s.EventLayoutBlockID == eventBlockID).SingleOrDefault();

            BlockSeatResponse response = new BlockSeatResponse();

            List<SeatResponse> seats = new List<SeatResponse>();

            if (block != null && block.Price != null)
            {
                int blockID = Convert.ToInt32(block.BlockID);

                var rows_seats = db.tblSeats.SqlQuery("SELECT * FROM TBLSEAT WHERE SEATROWID IN (SELECT SEATROWID FROM TBLSEATROWS WHERE BLOCKID = " + blockID + ") AND STATUS = 'PRESENT' AND SEATID NOT IN (SELECT SEATID FROM TBLSEATSELECTIONS WHERE EVENTID = " + block.EventID  + " AND SESSIONID='' AND ORDERID IS NOT NULL AND ORDERID IN (SELECT ORDERID FROM TBLTICKETORDERS WHERE EVENTID = " + block.EventID + " AND STATUS ='SUCCESS' AND PAYMENTSTATUS = 'COMPLETED' AND ORDERNO > 0))").ToList();//db.tblSeats.Where(t => t.tblSeatRow.BlockID == blockID && t.Status == "PRESENT").ToList();

                foreach(var seat in rows_seats)
                {
                    var isBlockedByAdmin = db.tblSeatSelections.SqlQuery("SELECT * FROM TBLSEATSELECTIONS WHERE SEATID = " + seat.SeatID + " AND EVENTID =" + block.EventID + " AND ORDERID IS NOT NULL  AND SESSIONID = '' AND ORDERID IN (SELECT ORDERID FROM TBLTICKETORDERS WHERE EVENTID = " + block.EventID + " AND STATUS = 'SUCCESS' AND ORDERNO = 0) ").Any();

                    seats.Add(new SeatResponse()
                    {
                        seatID = seat.SeatID,
                        seatNumber = seat.SeatNumber,
                        isBlockedByAdmin = isBlockedByAdmin
                    });
                }

                response.price = Convert.ToDecimal(block.Price);
                response.seats = seats;
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(response, JsonRequestBehavior.AllowGet);
        }

        public string Block()
        {
            int seatID = Convert.ToInt32(Request["seatID"]);
            int eventID = Convert.ToInt32(Request["eventID"]);
            
            var seat = db.tblSeatSelections.SqlQuery("SELECT * FROM TBLSEATSELECTIONS WHERE SEATID = " + seatID + " AND EVENTID =" + eventID + " AND ORDERID IS NOT NULL  AND SESSIONID = '' AND ORDERID IN (SELECT ORDERID FROM TBLTICKETORDERS WHERE EVENTID = " + eventID + " AND STATUS = 'SUCCESS' AND ORDERNO = 0) ").SingleOrDefault();

            if(eventID == 2026)
                seat = db.tblSeatSelections.SqlQuery("SELECT * FROM TBLSEATSELECTIONS WHERE SEATID = " + seatID + " AND EVENTID =" + eventID + " AND ORDERID IS NOT NULL  AND SESSIONID = '' AND ORDERID IN (SELECT ORDERID FROM TBLTICKETORDERS WHERE EVENTID = " + eventID + " AND STATUS = 'SUCCESS' AND ORDERNO = 22) ").SingleOrDefault();

            var order = db.tblTicketOrders.Where(o => o.EventID == eventID && o.OrderNo == 0).SingleOrDefault();


            if (eventID == 2026)
                order = db.tblTicketOrders.Where(o => o.EventID == eventID && o.OrderNo == 22).SingleOrDefault();


            var curr_seat = db.tblSeats.Where(s => s.SeatID == seatID).SingleOrDefault();

            var getBlock = db.tblBlocks.Where(t => t.BlockID == curr_seat.tblSeatRow.BlockID).SingleOrDefault();

            var getPrice = db.tblEventLayoutBlocks.Where(e => e.EventID == eventID && e.BlockID == getBlock.BlockID).SingleOrDefault();

            if (seat == null)
            {
                tblSeatSelection selection = new tblSeatSelection();
                selection.SeatID = seatID;
                selection.SessionID = "";
                selection.EventID = eventID;
                selection.OrderID = order.OrderID;
                selection.Price = getPrice.Price;
                selection.CreatedDate = DateTime.Now;
                db.tblSeatSelections.Add(selection);
                db.SaveChanges();


                var seats = db.Database.SqlQuery<SelectedSeats>("select SessionID, count(*) as 'NoOfSeats', sum(Price) as 'TotalPrice' from tblSeatSelections where EventID = " + eventID + " and SessionID = '' AND ORDERID IS NOT NULL AND ORDERID IN (SELECT ORDERID FROM TBLTICKETORDERS WHERE ORDERNO = 0 AND EVENTID = " + eventID + " AND STATUS = 'SUCCESS') group by SessionID").ToList();

                if(eventID == 2026)
                    seats = db.Database.SqlQuery<SelectedSeats>("select SessionID, count(*) as 'NoOfSeats', sum(Price) as 'TotalPrice' from tblSeatSelections where EventID = " + eventID + " and SessionID = '' AND ORDERID IS NOT NULL AND ORDERID IN (SELECT ORDERID FROM TBLTICKETORDERS WHERE ORDERNO = 22 AND EVENTID = " + eventID + " AND STATUS = 'SUCCESS') group by SessionID").ToList();


                order.NoOfTickets = Convert.ToInt32(seats.Sum(s => s.NoOfSeats));
                order.Amount = Convert.ToDecimal(seats.Sum(s => s.TotalPrice));
                db.SaveChanges();
            }

            return "OK";
        }


        public string Unblock()
        {
            int seatID = Convert.ToInt32(Request["seatID"]);
            int eventID = Convert.ToInt32(Request["eventID"]);
            
            var seat = db.tblSeatSelections.SqlQuery("SELECT * FROM TBLSEATSELECTIONS WHERE SEATID = " + seatID + " AND EVENTID =" + eventID + " AND ORDERID IS NOT NULL  AND SESSIONID = '' AND ORDERID IN (SELECT ORDERID FROM TBLTICKETORDERS WHERE EVENTID = " + eventID + " AND STATUS = 'SUCCESS' AND ORDERNO = 0) ").SingleOrDefault();

            if(eventID == 2026)
                seat = db.tblSeatSelections.SqlQuery("SELECT * FROM TBLSEATSELECTIONS WHERE SEATID = " + seatID + " AND EVENTID =" + eventID + " AND ORDERID IS NOT NULL  AND SESSIONID = '' AND ORDERID IN (SELECT ORDERID FROM TBLTICKETORDERS WHERE EVENTID = " + eventID + " AND STATUS = 'SUCCESS' AND ORDERNO = 22) ").SingleOrDefault();
            
            var order = db.tblTicketOrders.Where(o => o.EventID == eventID && o.OrderNo == 0).SingleOrDefault();

            if (eventID == 2026)
                order = db.tblTicketOrders.Where(o => o.EventID == eventID && o.OrderNo == 22).SingleOrDefault();


            if (seat != null)
            {
                decimal price = Convert.ToDecimal(seat.Price);

                var sel = db.tblSeatSelections.Where(s => s.SeatSelectionID == seat.SeatSelectionID).SingleOrDefault();
                db.tblSeatSelections.Remove(sel);
                db.SaveChanges();

                var seats = db.Database.SqlQuery<SelectedSeats>("select SessionID, count(*) as 'NoOfSeats', sum(Price) as 'TotalPrice' from tblSeatSelections where EventID = " + eventID + " and SessionID = '' AND ORDERID IS NOT NULL AND ORDERID IN (SELECT ORDERID FROM TBLTICKETORDERS WHERE ORDERNO = 0 AND EVENTID = " + eventID + " AND STATUS = 'SUCCESS') group by SessionID").ToList();

                if (eventID == 2026)
                    seats = db.Database.SqlQuery<SelectedSeats>("select SessionID, count(*) as 'NoOfSeats', sum(Price) as 'TotalPrice' from tblSeatSelections where EventID = " + eventID + " and SessionID = '' AND ORDERID IS NOT NULL AND ORDERID IN (SELECT ORDERID FROM TBLTICKETORDERS WHERE ORDERNO = 22 AND EVENTID = " + eventID + " AND STATUS = 'SUCCESS') group by SessionID").ToList();


                order.NoOfTickets = Convert.ToInt32(seats.Sum(s => s.NoOfSeats));
                order.Amount = Convert.ToDecimal(seats.Sum(s => s.TotalPrice));
                db.SaveChanges();
            }

            return "OK";
        }
    }
}