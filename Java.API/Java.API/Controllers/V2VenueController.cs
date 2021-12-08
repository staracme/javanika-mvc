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

    public class SeatingV2
    {
        public string event_name { get; set; }
        public string event_date { get; set; }
        public BlockV2 block1 { get; set; }
        public BlockV2 block2 { get; set; }
        public BlockV2 block3 { get; set; }
        public BlockV2 block4 { get; set; }
        public BlockV2 block5 { get; set; }
    }


    public class BlockV2
    {
        public int blockID { get; set; }
        public string name { get; set; }
        public List<SeatRowV2> SeatRows { get; set; }
    }


    public class SeatRowV2
    {
        public int rowID { get; set; }
        public int blockID { get; set; }
        public string RowNumber { get; set; }
        public List<SeatV2> Seats { get; set; }
    }

    public class SeatV2
    {
        public int SeatID { get; set; }
        public int SeatRowID { get; set; }
        public string SeatNumber { get; set; }
        public string Status { get; set; }
        public string tooltipText { get; set; }
        public bool isBooked { get; set; }
    }

    public class V2VenueModel
    {
        public int eventID { get; set; }
    }

    public class V2VenueController : ApiController
    {
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public SeatingV2 GetBlock(V2VenueModel v2)
        {
            JAVADBEntities db = new JAVADBEntities();

            SeatingV2 seating = new SeatingV2();

            BlockV2 bl1 = new BlockV2();
            BlockV2 bl2 = new BlockV2();
            BlockV2 bl3 = new BlockV2();
            BlockV2 bl4 = new BlockV2();
            BlockV2 bl5 = new BlockV2();


            List<SeatRowV2> b1_seat_rows = new List<SeatRowV2>();
            List<SeatRowV2> b2_seat_rows = new List<SeatRowV2>();
            List<SeatRowV2> b3_seat_rows = new List<SeatRowV2>();
            List<SeatRowV2> b4_seat_rows = new List<SeatRowV2>();
            List<SeatRowV2> b5_seat_rows = new List<SeatRowV2>();

            var block1 = db.tblBlocks.Where(b => b.BlockNumber == "V2B1").SingleOrDefault();
            var block2 = db.tblBlocks.Where(b => b.BlockNumber == "V2B2").SingleOrDefault();
            var block3 = db.tblBlocks.Where(b => b.BlockNumber == "V2B3").SingleOrDefault();
            var block4 = db.tblBlocks.Where(b => b.BlockNumber == "V2B4").SingleOrDefault();
            var block5 = db.tblBlocks.Where(b => b.BlockNumber == "V2B5").SingleOrDefault();

            var evt = db.tblEvents.Where(e => e.EventID == v2.eventID).SingleOrDefault();

            if (block1 != null)
            {
                var block_pricing = db.tblEventLayoutBlocks.Where(t => t.BlockID == block1.BlockID && t.EventID == v2.eventID).SingleOrDefault();

                var seat_rows = db.tblSeatRows.Where(r => r.BlockID == block1.BlockID).ToList();

                foreach (var row in seat_rows)
                {
                    List<SeatV2> b1_row_seats = new List<SeatV2>();

                    var seats = db.tblSeats.Where(r => r.SeatRowID == row.SeatRowID).ToList();

                    foreach (var seat in seats)
                    {
                        var isBooked = db.tblSeatSelections.SqlQuery("SELECT * FROM TBLSEATSELECTIONS WHERE SEATID = " + seat.SeatID + " AND EVENTID =" + v2.eventID + " AND ORDERID IS NOT NULL AND ORDERID IN (SELECT ORDERID FROM TBLTICKETORDERS WHERE STATUS = 'SUCCESS')").Any();

                        b1_row_seats.Add(new SeatV2()
                        {
                            SeatID = seat.SeatID,
                            SeatRowID = row.SeatRowID,
                            SeatNumber = seat.SeatNumber,
                            Status = seat.Status,
                            tooltipText = (block_pricing != null && block_pricing.tblTier != null ? (block_pricing.tblTier.TierName) + "-" + "$" + (block_pricing != null ? Decimal.Round(Convert.ToDecimal(block_pricing.Price)) : 0) : ""),
                            isBooked = (isBooked || seat.SeatRowID == 1014 ? true : false)
                        });
                    }

                    b1_seat_rows.Add(new SeatRowV2()
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

            if (block2 != null)
            {
                var block_pricing = db.tblEventLayoutBlocks.Where(t => t.BlockID == block2.BlockID && t.EventID == v2.eventID).SingleOrDefault();

                var seat_rows = db.tblSeatRows.Where(r => r.BlockID == block2.BlockID).ToList();

                foreach (var row in seat_rows)
                {
                    List<SeatV2> b2_row_seats = new List<SeatV2>();

                    var seats = db.tblSeats.Where(r => r.SeatRowID == row.SeatRowID).ToList();

                    foreach (var seat in seats)
                    {
                        var isBooked = db.tblSeatSelections.SqlQuery("SELECT * FROM TBLSEATSELECTIONS WHERE SEATID = " + seat.SeatID + " AND EVENTID =" + v2.eventID + " AND ORDERID IS NOT NULL AND ORDERID IN (SELECT ORDERID FROM TBLTICKETORDERS WHERE STATUS = 'SUCCESS')").Any();

                        b2_row_seats.Add(new SeatV2()
                        {
                            SeatID = seat.SeatID,
                            SeatRowID = row.SeatRowID,
                            SeatNumber = seat.SeatNumber,
                            Status = seat.Status,
                            tooltipText = (block_pricing != null && block_pricing.tblTier != null ? (block_pricing.tblTier.TierName) + "-" + "$" + (block_pricing != null ? Decimal.Round(Convert.ToDecimal(block_pricing.Price)) : 0) : ""),
                            isBooked = (isBooked || seat.SeatRowID == 1014 ? true : false)
                        });
                    }

                    b2_seat_rows.Add(new SeatRowV2()
                    {
                        rowID = row.SeatRowID,
                        blockID = block2.BlockID,
                        RowNumber = row.RowNumber,
                        Seats = b2_row_seats
                    });
                }

                bl2.blockID = block2.BlockID;
                bl2.SeatRows = b2_seat_rows;
            }

            if (block3 != null)
            {
                var block_pricing = db.tblEventLayoutBlocks.Where(t => t.BlockID == block3.BlockID && t.EventID == v2.eventID).SingleOrDefault();

                var seat_rows = db.tblSeatRows.Where(r => r.BlockID == block3.BlockID).ToList();

                foreach (var row in seat_rows)
                {
                    List<SeatV2> b3_row_seats = new List<SeatV2>();

                    var seats = db.tblSeats.Where(r => r.SeatRowID == row.SeatRowID).ToList();

                    foreach (var seat in seats)
                    {
                        var isBooked = db.tblSeatSelections.SqlQuery("SELECT * FROM TBLSEATSELECTIONS WHERE SEATID = " + seat.SeatID + " AND EVENTID =" + v2.eventID + " AND ORDERID IS NOT NULL AND ORDERID IN (SELECT ORDERID FROM TBLTICKETORDERS WHERE STATUS = 'SUCCESS')").Any();

                        b3_row_seats.Add(new SeatV2()
                        {
                            SeatID = seat.SeatID,
                            SeatRowID = row.SeatRowID,
                            SeatNumber = seat.SeatNumber,
                            Status = seat.Status,
                            tooltipText = (block_pricing != null && block_pricing.tblTier != null ? (block_pricing.tblTier.TierName) + "-" + "$" + (block_pricing != null ? Decimal.Round(Convert.ToDecimal(block_pricing.Price)) : 0) : ""),
                            isBooked = (isBooked || seat.SeatRowID == 1014 ? true : false)
                        });
                    }

                    b3_seat_rows.Add(new SeatRowV2()
                    {
                        rowID = row.SeatRowID,
                        blockID = block3.BlockID,
                        RowNumber = row.RowNumber,
                        Seats = b3_row_seats
                    });
                }

                bl3.blockID = block3.BlockID;
                bl3.SeatRows = b3_seat_rows;
            }


            if (block4 != null)
            {
                var block_pricing = db.tblEventLayoutBlocks.Where(t => t.BlockID == block4.BlockID && t.EventID == v2.eventID).SingleOrDefault();

                var seat_rows = db.tblSeatRows.Where(r => r.BlockID == block4.BlockID).ToList();

                foreach (var row in seat_rows)
                {
                    List<SeatV2> b4_row_seats = new List<SeatV2>();

                    var seats = db.tblSeats.Where(r => r.SeatRowID == row.SeatRowID).ToList();

                    foreach (var seat in seats)
                    {
                        var isBooked = db.tblSeatSelections.SqlQuery("SELECT * FROM TBLSEATSELECTIONS WHERE SEATID = " + seat.SeatID + " AND EVENTID =" + v2.eventID + " AND ORDERID IS NOT NULL AND ORDERID IN (SELECT ORDERID FROM TBLTICKETORDERS WHERE STATUS = 'SUCCESS')").Any();

                        b4_row_seats.Add(new SeatV2()
                        {
                            SeatID = seat.SeatID,
                            SeatRowID = row.SeatRowID,
                            SeatNumber = seat.SeatNumber,
                            Status = seat.Status,
                            tooltipText = (block_pricing != null && block_pricing.tblTier != null ? (block_pricing.tblTier.TierName) + "-" + "$" + (block_pricing != null ? Decimal.Round(Convert.ToDecimal(block_pricing.Price)) : 0) : ""),
                            isBooked = (isBooked || seat.SeatRowID == 1014 ? true : false)
                        });
                    }

                    b4_seat_rows.Add(new SeatRowV2()
                    {
                        rowID = row.SeatRowID,
                        blockID = block4.BlockID,
                        RowNumber = row.RowNumber,
                        Seats = b4_row_seats
                    });
                }

                bl4.blockID = block4.BlockID;
                bl4.SeatRows = b4_seat_rows;
            }

            if (block5 != null)
            {
                var block_pricing = db.tblEventLayoutBlocks.Where(t => t.BlockID == block5.BlockID && t.EventID == v2.eventID).SingleOrDefault();

                var seat_rows = db.tblSeatRows.Where(r => r.BlockID == block5.BlockID).ToList();

                foreach (var row in seat_rows)
                {
                    List<SeatV2> b5_row_seats = new List<SeatV2>();

                    var seats = db.tblSeats.Where(r => r.SeatRowID == row.SeatRowID).ToList();

                    foreach (var seat in seats)
                    {
                        var isBooked = db.tblSeatSelections.SqlQuery("SELECT * FROM TBLSEATSELECTIONS WHERE SEATID = " + seat.SeatID + " AND EVENTID =" + v2.eventID + " AND ORDERID IS NOT NULL AND ORDERID IN (SELECT ORDERID FROM TBLTICKETORDERS WHERE STATUS = 'SUCCESS')").Any();

                        b5_row_seats.Add(new SeatV2()
                        {
                            SeatID = seat.SeatID,
                            SeatRowID = row.SeatRowID,
                            SeatNumber = seat.SeatNumber,
                            Status = seat.Status,
                            tooltipText = (block_pricing != null && block_pricing.tblTier != null ? (block_pricing.tblTier.TierName) + "-" + "$" + (block_pricing != null ? Decimal.Round(Convert.ToDecimal(block_pricing.Price)) : 0) : ""),
                            isBooked = (isBooked || seat.SeatRowID == 1014 ? true : false)
                        });
                    }

                    b5_seat_rows.Add(new SeatRowV2()
                    {
                        rowID = row.SeatRowID,
                        blockID = block5.BlockID,
                        RowNumber = row.RowNumber,
                        Seats = b5_row_seats
                    });
                }

                bl5.blockID = block5.BlockID;
                bl5.SeatRows = b5_seat_rows;
            }

            seating.event_name = evt.EventName;
            seating.block1 = bl1;
            seating.block2 = bl2;
            seating.block3 = bl3;
            seating.block4 = bl4;
            seating.block5 = bl5;
            seating.event_date = (evt.EventDate != null ? Convert.ToDateTime(evt.EventDate).ToString("dddd, dd MMMM") + evt.ShowTime : "");
            return seating;
        }
    }
}
