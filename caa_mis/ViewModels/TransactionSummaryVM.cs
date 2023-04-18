using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace caa_mis.ViewModels
{
    public class TransactionSummaryVM
    {
        public int ID { get; set; }
        public int EmployeeID { get; set; }
        [Display(Name = "Employee Name")]
        public string EmployeeName { get; set; }
        public int OriginID { get; set; }
        [Display(Name = "Origin Branch")]
        public string OriginName { get; set; }
        public int DestinationID { get; set; }
        [Display(Name = "Destination Branch")]
        public string DestinationName { get; set; }
        public int TransactionStatusID { get; set; }
        [Display(Name = "TransactionStatus")]
        public string TransactionStatusName { get; set; }
        public int TransactionTypeID { get; set; }
        [Display(Name = "TransactionType")]
        public string TransactionTypeName { get; set; }

        [Display(Name = "Transaction Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime TransactionDate { get; set; }

        [Display(Name = "Date Received")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ReceivedDate { get; set; }
        public string Description { get; set; }
        public string Shipment { get; set; }
    }
}
