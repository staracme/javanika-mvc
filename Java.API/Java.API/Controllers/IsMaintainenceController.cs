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
    public class IsMaintainenceController : ApiController
    {
        [HttpPost]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public bool IsMaintainence()
        {
            return Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["IsMaintainence"]);
        }
    }
}
