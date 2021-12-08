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
    public class RemoveSeatController : ApiController
    {
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public SeatSelectResponse RemoveSeatResponse(SeatSelectModel st)
        {
            JAVADBEntities db = new JAVADBEntities();

            int eventID = st.eventID;
            int seatID = st.seatID;
            string sessionID = st.uuid;

            string removeSeat = Common.RemoveSeat(eventID, sessionID, seatID);

            SeatSelectResponse response = new SeatSelectResponse();

            if (removeSeat == "OK")
            {
                var seats = db.Database.SqlQuery<SelectedSeatsV5>("select SessionID, count(*) as 'NoOfSeats', sum(Price) as 'TotalPrice' from tblSeatSelections where EventID = " + eventID + " and SessionID = '" + st.uuid + "' group by SessionID").ToList();

                response.status = "OK";
                response.NoOfSeats = Convert.ToInt32(seats.Sum(s => s.NoOfSeats));
                response.TotalPrice = Convert.ToDecimal(seats.Sum(s => s.TotalPrice));
            }
            return response;
        }
    }
}
