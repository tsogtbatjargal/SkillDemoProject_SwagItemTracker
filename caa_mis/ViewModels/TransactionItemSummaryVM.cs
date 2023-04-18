using caa_mis.Models;
using System.ComponentModel.DataAnnotations;

namespace caa_mis.ViewModels
{
    public class TransactionItemSummaryVM
    {
        public int ID { get; set; }
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public int TransactionID { get; set; }
        public string EmployeeName { get; set; }
        public int OriginID { get; set; }
        [Display(Name = "Origin Branch")]
        public string OriginName { get; set; }
        public int DestinationID { get; set; }
        [Display(Name = "Destination Branch")]
        public string DestinationName { get; set; }

        [Display(Name = "TransactionStatus")]
        public string TransactionStatusName { get; set; }
        public int Quantity { get; set; }
    }
}
