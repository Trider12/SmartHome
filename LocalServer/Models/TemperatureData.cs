using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalServer.Models
{
    [Table("Temperature")]
    public class TemperatureData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string MacAddress { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        public float Temperature { get; set; }
        public DateTime Time { get; set; }
    }
}