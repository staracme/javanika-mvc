using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using Java.API.Models;
namespace Java.API.Controllers
{
    public class SeatSelectResponse
    {
        public string status { get; set; }

        public int NoOfSeats { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class SeatSelectModel
    {
        public int eventID { get; set; }
        public string uuid { get; set; }
        public int seatID { get; set; }
    }

    public class SelectedSeatsV5
    {
        public string SessionID { get; set; }
        public int NoOfSeats { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class SelectSeatController : ApiController
    {
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public SeatSelectResponse SelectResponse(SeatSelectModel st)
        {
            JAVADBEntities db = new JAVADBEntities();

            int eventID = st.eventID;
            int seatID = st.seatID;
            string sessionID = st.uuid;

            string selectSeat = Common.SelectSeat(eventID, sessionID, seatID);

            SeatSelectResponse response = new SeatSelectResponse();

            if (selectSeat == "OK")
            {
                var seats = db.Database.SqlQuery<SelectedSeatsV5>("select SessionID, count(*) as 'NoOfSeats', sum(Price) as 'TotalPrice' from tblSeatSelections where EventID = " + eventID + " and SessionID = '" + st.uuid + "'  and ORDERID IS NULL  group by SessionID").ToList();
                response.status = "OK";
                response.NoOfSeats = Convert.ToInt32(seats.Sum(s => s.NoOfSeats));
                response.TotalPrice = Convert.ToDecimal(seats.Sum(s => s.TotalPrice));
            }
            return response;
        }
    }
}
