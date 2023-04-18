using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace caa_mis.ViewModels
{
    public class ProductListVM
    {

        public string ProductName
        {
            get
            {
                return Name + " ( "+ Quantity+" )";
            }
        }

        public int ID { get; set; }

        public int BranchID { get; set; }

        public int Quantity { get; set; }

        public string Name { get; set; }

        public string SKUNumber { get; set; }
    }
}
