using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace caa_mis.ViewModels
{
    public class StockSummaryByBranchVM
    {
        public int ID { get; set; }
        public int BranchID { get; set; }
        [Display(Name = "Branch Name")]
        public string BranchName { get; set; }
        [Display(Name = "Product")]
        public string ItemName { get; set; }

        [Display(Name = "Product Cost")]
        public double ItemCost { get; set; }
        public int Quantity { get; set; }
        public int MinLevel { get; set; }
    }
}
