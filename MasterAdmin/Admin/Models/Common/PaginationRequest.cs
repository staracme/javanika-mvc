using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Admin.Models
{
    public class PaginationRequest
    {
        public int rowsPerPage { get; set; }
        public int pageNum { get; set; }
    }
}