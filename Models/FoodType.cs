using System.ComponentModel.DataAnnotations;

namespace Billbyte_BE.Models
{
    public class FoodType
    {
        [Key]
        public int FoodTypeId { get; set; }

        [Required]
        public string FoodTypeName { get; set; }

        public int DisplayOrder { get; set; }
    }
}
