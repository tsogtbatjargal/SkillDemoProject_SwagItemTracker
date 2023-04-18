using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace caa_mis.ViewModels
{
    public class EventSummaryVM
    {
        public int ID { get; set; }

        public int EventID { get; set; }
        [Display(Name = "Event Name")]
        public string EventName { get; set; }
        public int EmployeeID { get; set; }
        [Display(Name = "Employee Name")]
        public string EmployeeName { get; set; }
        public int BranchID { get; set; }
        [Display(Name = "Branch Name")]
        public string BranchName { get; set; }
        public int TransactionStatusID { get; set; }
        [Display(Name = "Transaction Status")]
        public string TransactionStatusName { get; set; }        

        [Display(Name = "Event Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EventDate { get; set; }

        public int ItemID { get; set; }
        [Display(Name = "Product Name")]
        public string ItemName { get; set; }
        public int EventQuantity { get; set; }

    }
}
