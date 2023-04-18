using caa_mis.Models;
using System.ComponentModel.DataAnnotations;

namespace caa_mis.ViewModels
{
    public class TransactionItemVM
    {
        public int ID { get; set; }

        public int ProductID { get; set; }

        public int  TransactionID{ get; set; }

        public int Quantity { get; set; }
    }
}
