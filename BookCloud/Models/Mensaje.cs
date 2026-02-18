using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookCloud.Models
{
    [Table("Mensajes")]
    public class Mensaje
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("ChatId")]
        public int ChatId { get; set; }

        [Required]
        [Column("RemitenteId")]
        public int RemitenteId { get; set; }

        [Required]
        [Column("Contenido")]
        public string Contenido { get; set; }

        [Column("FechaEnvio")]
        public DateTime FechaEnvio { get; set; }

        [Required]
        [Column("Activo")]
        public bool Activo { get; set; }

        [ForeignKey("ChatId")]
        public Chat Chat { get; set; }

        [ForeignKey("RemitenteId")]
        public Usuario Remitente { get; set; }
    }

}
