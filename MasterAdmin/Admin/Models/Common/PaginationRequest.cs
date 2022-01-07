using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Admin.Models
{
    public interface IPaginationRequest
    {
        int rowsPerPage { get; set; }
        int pageNum { get; set; }
    }

    public interface IOrderRequest
    {
        string orderBy { get; set; }
    }

    public class ReportParam
    {
        public string reportType { get; set; }
        public int EventId { get; set; }
    }
}