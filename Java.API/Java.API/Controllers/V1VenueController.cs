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
    public class Seating
    {
        public Block block1 { get; set; }
        public Block block2 { get; set; }
    }
    
    public class Block
    {
        public int blockID { get; set; }
        public string name { get; set; }
        public List<SeatRow> SeatRows { get; set; }
    }

    public class SeatRow
    {
        public int rowID { get; set; }
        public int blockID { get; set; }
        public string RowNumber { get; set; }  
        public List<Seat> Seats { get; set; }
    }

    public class Seat
    {
        public int SeatID { get; set; }
        public int SeatRowID { get; set; }
        public string SeatNumber { get; set; }
        public string Status { get; set; }
        public string tooltipText { get; set; }
    }

    public class V1VenueModel
    {
        public int eventID { get; set; }
    }


    public class V1VenueController : ApiController
    {
        // GET api/values
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public Seating GetBlock(V1VenueModel v1)
        {
            JAVADBEntities db = new JAVADBEntities();

            Seating seating = new Seating();
            Block bl1 = new Block();
            Block bl2 = new Block();
            List<SeatRow> b1_seat_rows = new List<SeatRow>();
            List<SeatRow> b2_seat_rows = new List<SeatRow>();


            var block1 = db.tblBlocks.Where(b => b.BlockNumber == "V1L1B1").SingleOrDefault();

            var block2 = db.tblBlocks.Where(b => b.BlockNumber == "V1L1B2").SingleOrDefault();


            if (block1 != null)
            {
                var block_pricing = db.tblEventLayoutBlocks.Where(t => t.BlockID == block2.BlockID && t.EventID == v1.eventID).SingleOrDefault();

                var seat_rows = db.tblSeatRows.Where(r => r.BlockID == block1.BlockID).ToList();

                foreach (var row in seat_rows)
                {
                    List<Seat> b1_row_seats = new List<Seat>();

                    var seats = db.tblSeats.Where(r => r.SeatRowID == row.SeatRowID).ToList();

                    foreach (var seat in seats)
                    {
                        b1_row_seats.Add(new Seat()
                        {
                            SeatID = seat.SeatID,
                            SeatRowID = row.SeatRowID,
                            SeatNumber = seat.SeatNumber,
                            Status = seat.Status,
                            tooltipText = (block_pricing != null && block_pricing.tblTier != null ? (block_pricing.tblTier.TierName) + "-" + "$" + (block_pricing != null ? Decimal.Round(Convert.ToDecimal(block_pricing.Price)) : 0) : ""),
                        });
                    }

                    b1_seat_rows.Add(new SeatRow()
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
                var seat_rows = db.tblSeatRows.Where(r => r.BlockID == block2.BlockID).ToList();

                var block_pricing = db.tblEventLayoutBlocks.Where(t => t.BlockID == block2.BlockID && t.EventID == v1.eventID).SingleOrDefault();

                foreach (var row in seat_rows)
                {
                    List<Seat> b2_row_seats = new List<Seat>();

                    var seats = db.tblSeats.Where(r => r.SeatRowID == row.SeatRowID).ToList();

                    foreach (var seat in seats)
                    {
                        b2_row_seats.Add(new Seat()
                        {
                            SeatID = seat.SeatID,
                            SeatRowID = row.SeatRowID,
                            SeatNumber = seat.SeatNumber,
                            Status = seat.Status,
                            tooltipText = (block_pricing != null && block_pricing.tblTier != null ? (block_pricing.tblTier.TierName) + "-" + "$" + (block_pricing != null ? Decimal.Round(Convert.ToDecimal(block_pricing.Price)) : 0) : ""),
                        });
                    }

                    b2_seat_rows.Add(new SeatRow()
                    {
                        rowID = row.SeatRowID,
                        blockID = block2.BlockID,
                        RowNumber = row.RowNumber,
                        Seats = b2_row_seats
                    });

                    bl2.blockID = block2.BlockID;
                    bl2.SeatRows = b2_seat_rows;
                }
            }

            seating.block1 = bl1;
            seating.block2 = bl2;

            return seating;
        }
    }
}
