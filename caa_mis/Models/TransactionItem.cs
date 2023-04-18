using System.ComponentModel.DataAnnotations;
namespace caa_mis.Models
{
    public class TransactionItem : IValidatableObject
    {
        public int ID { get; set; }

        [Required]
        public int ItemID { get; set; }
        public Item Item { get; set; }

        [Required]
        public int TransactionID { get; set; }
        public Transaction Transaction { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, int.MaxValue)]
        public int? ReceivedQuantity { get; set; }

        public bool? IsEdited { get; set; } = false;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Quantity <= 0)
            {
                yield return new ValidationResult("Cannot Enter a quantity less than or equal to 0.", new[] { "Quantity" });
            }

        }
    }
}
