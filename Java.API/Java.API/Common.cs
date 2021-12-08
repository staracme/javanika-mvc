using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Java.API.Models;
namespace Java.API
{
    public class Common
    {
        public static string SelectSeat(int eventID,string uuid, int seatID)
        {
            try
            {
                JAVADBEntities db = new JAVADBEntities();
                
                var isSeatBooked = db.tblSeatSelections.Where(t => t.SeatID == seatID && t.EventID == eventID && t.SessionID == "" && t.OrderID != null).Any();

                var curr_seat = db.tblSeats.Where(s => s.SeatID == seatID).SingleOrDefault();

                var getBlock = db.tblBlocks.Where(t => t.BlockID == curr_seat.tblSeatRow.BlockID).SingleOrDefault();

                var getPrice = db.tblEventLayoutBlocks.Where(e => e.EventID == eventID && e.BlockID == getBlock.BlockID).SingleOrDefault();

                //seat can be selected only if it is not purchased
                if (isSeatBooked == false)
                {
                    tblSeatSelection seat = new tblSeatSelection();
                    seat.SessionID = uuid;
                    seat.EventID = eventID;
                    seat.SeatID = seatID;
                    seat.Price = getPrice.Price;
                    seat.CreatedDate = DateTime.Now;
                    db.tblSeatSelections.Add(seat);
                    db.SaveChanges();
                    return "OK";
                }
                else
                {
                    //remove session associated seat data if this ticket is already booked
                    var getSeat = db.tblSeatSelections.Where(s => s.SeatID == seatID && s.SessionID == uuid && s.OrderID == null && s.EventID == eventID).SingleOrDefault();
                    db.tblSeatSelections.Remove(getSeat);
                    db.SaveChanges();
                    return "BOOKED";
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    return ex.InnerException.Message;
                else
                    return ex.Message;
            }
        }

        public static string RemoveSeat(int eventID,string uuid, int seatID)
        {
            JAVADBEntities db = new JAVADBEntities();

            //remove session associated seat data if this ticket is already booked
            var getSeat = db.tblSeatSelections.Where(s => s.SeatID == seatID && s.SessionID == uuid && s.OrderID == null && s.EventID == eventID).ToList();

            foreach (var s in getSeat)
            {
                db.tblSeatSelections.Remove(s);
                db.SaveChanges();
            }
            return "OK";
        }

        public static string GetViewImagePath()
        {
            return System.Configuration.ConfigurationManager.AppSettings["ViewImagePath"].ToString();
        }

        public static string GetContent(string content, int length)
        {
            if (!string.IsNullOrEmpty(content) && length > 0)
            {
                int config_length = length;

                if (content.Length > config_length)
                    return content.Substring(0, config_length) + "... ";
                else
                    return content;
            }
            else
                return "N/A";

        }
    }
}