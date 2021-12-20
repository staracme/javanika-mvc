using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Admin.Models
{
    public class OrderRequest : IPaginationRequest, IOrderRequest
    {
        public int EventId { get; set; }
        public int? OrderId { get; set; }
        public string orderBy { get; set; }
        public int rowsPerPage { get; set; }
        public int pageNum { get; set; }
    }
}