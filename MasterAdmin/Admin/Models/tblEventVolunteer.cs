//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Admin.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tblEventVolunteer
    {
        public int EventVolunteerID { get; set; }
        public Nullable<int> EventID { get; set; }
        public Nullable<int> VolunteerID { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
    
        public virtual tblEvent tblEvent { get; set; }
        public virtual tblVolunteer tblVolunteer { get; set; }
    }
}
