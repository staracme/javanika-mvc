using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using Java.API.Models;

namespace Java.API.Controllers
{
    public class SeatingV5
    {
        public string event_name { get; set; }
        public string event_date { get; set; }
        public BlockV5 block1 { get; set; }
    }


    public class BlockV5
    {
        public int blockID { get; set; }
        public string name { get; set; }
        public List<SeatRowV5> SeatRows { get; set; }
    }


    public class SeatRowV5
    {
        public int rowID { get; set; }
        public int blockID { get; set; }
        public string RowNumber { get; set; }
        public List<SeatV5> Seats { get; set; }
    }

    public class SeatV5
    {
        public int SeatID { get; set; }
        public int SeatRowID { get; set; }
        public string SeatNumber { get; set; }
        public string Status { get; set; }
        public string tooltipText { get; set; }
        public bool isBooked { get; set; }
    }

    public class V5VenueModel
    {
        public int eventID { get; set; }
    }

    public class V5VenueController : ApiController
    {
        JAVADBEntities db = new JAVADBEntities();
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public SeatingV5 GetBlock(V5VenueModel v5)
        {
            SeatingV5 seating = new SeatingV5();
            BlockV5 bl1 = new BlockV5();
            List<SeatRowV5> b1_seat_rows = new List<SeatRowV5>();

            var block1 = db.tblBlocks.Where(b => b.BlockNumber == "V5B1").SingleOrDefault();

            var evt = db.tblEvents.Where(e => e.EventID == v5.eventID).SingleOrDefault();

            if (block1 != null)
            {
                var block_pricing = db.tblEventLayoutBlocks.Where(t => t.BlockID == block1.BlockID && t.EventID == v5.eventID).SingleOrDefault();

                var seat_rows = db.tblSeatRows.Where(r => r.BlockID == block1.BlockID).OrderByDescending(s => s.SeatRowID).ToList();
                
                foreach (var row in seat_rows)
                {
                    List<SeatV5> b1_row_seats = new List<SeatV5>();

                    var seats = db.tblSeats.Where(r => r.SeatRowID == row.SeatRowID).ToList();

                    foreach (var seat in seats)
                    {
                        var isBooked = db.tblSeatSelections.SqlQuery("SELECT * FROM TBLSEATSELECTIONS WHERE SEATID = " + seat.SeatID + " AND EVENTID =" + v5.eventID + " AND ORDERID IS NOT NULL AND ORDERID IN (SELECT ORDERID FROM TBLTICKETORDERS WHERE STATUS = 'SUCCESS')").Any();

                        b1_row_seats.Add(new SeatV5()
                        {
                            SeatID = seat.SeatID,
                            SeatRowID = row.SeatRowID,
                            SeatNumber = seat.SeatNumber,
                            Status = seat.Status,
                            tooltipText = (block_pricing != null && block_pricing.tblTier != null ? (block_pricing.tblTier.TierName) + "-" + "$" + (block_pricing != null ? Decimal.Round(Convert.ToDecimal(block_pricing.Price)) : 0) : ""),
                            isBooked = (isBooked || seat.SeatRowID == 1014 ? true : false)
                        });
                    }

                    b1_seat_rows.Add(new SeatRowV5()
                    {
                        rowID = row.SeatRowID,
                        blockID = block1.BlockID,
                        RowNumber = row.RowNumber,
                        Seats = b1_row_seats
                    });
                }


                bl1.blockID = block1.BlockID;
                bl1.SeatRows = b1_seat_rows;
            }

            seating.event_name = evt.EventName;
            seating.block1 = bl1;
            seating.event_date = (evt.EventDate != null ? Convert.ToDateTime(evt.EventDate).ToString("dddd, dd MMMM") + evt.ShowTime : "");
            return seating;
        }
    }
}
