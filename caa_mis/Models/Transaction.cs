using System.ComponentModel.DataAnnotations;
namespace caa_mis.Models
{
    public class Transaction : IValidatableObject
    {
        public int ID { get; set; }

        [Required]
        [Display(Name = "Prepared By")]
        public int EmployeeID { get; set; }
        public Employee Employee { get; set; }

        
        [Required]
        [Display(Name = "Status")]
        public int TransactionStatusID { get; set; }
       
        public TransactionStatus TransactionStatus { get; set; }

       
        [Required]
        [Display(Name = "Transaction Type")]
        public int TransactionTypeID { get; set; }
        
        public TransactionType TransactionType { get; set; }

        [Required]
        [Display(Name = "Origin")]        
        public int? OriginID { get; set; }
        public Branch Origin { get; set; }
        [Required]
        [Display(Name = "Destination")]        
        public int? DestinationID { get; set; }
        public Branch Destination { get; set; }

        [Display(Name = "Transaction Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime TransactionDate { get; set; }

        [Display(Name = "Date Received")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ReceivedDate { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        public string Shipment { get; set; }

        public ICollection<TransactionItem> TransactionItems { get; set; } = new HashSet<TransactionItem>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (OriginID == DestinationID)
            {
                yield return new ValidationResult("Origin Branch should not be the same with Destination Branch", new[] { "DestinationID" });
            }
           
        }
    }
}
