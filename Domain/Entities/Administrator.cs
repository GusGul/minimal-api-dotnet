using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinimalApi.Domain.Entities
{
    [Table("Administrators")]
    public class Administrator
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(255)]
        public required string Email { get; set; }
        [StringLength(50)]
        public required string Password { get; set; }
        [StringLength(10)]
        public required string Role { get; set; }
    }
}
