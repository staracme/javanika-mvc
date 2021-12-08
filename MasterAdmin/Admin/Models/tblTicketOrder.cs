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
    
    public partial class tblTicketOrder
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblTicketOrder()
        {
            this.tblOrderCoupons = new HashSet<tblOrderCoupon>();
        }
    
        public int OrderID { get; set; }
        public Nullable<int> OrderNo { get; set; }
        public Nullable<int> EventID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public Nullable<int> NoOfTickets { get; set; }
        public Nullable<decimal> AmountPerTicket { get; set; }
        public Nullable<decimal> DiscountAmount { get; set; }
        public Nullable<decimal> ProcessingFee { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public string PaypalOrderID { get; set; }
        public string Intent { get; set; }
        public string QRCode { get; set; }
        public Nullable<bool> IsPickedUp { get; set; }
        public Nullable<System.DateTime> DatePickedUp { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string TicketType { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblOrderCoupon> tblOrderCoupons { get; set; }
    }
}
