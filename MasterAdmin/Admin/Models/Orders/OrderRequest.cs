using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Admin.Models
{
    public class OrderRequest : PaginationRequest   
    {
        public int? EventId { get; set; }
    }
}