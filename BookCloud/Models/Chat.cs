using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookCloud.Models
{
    [Table("Chats")]
    public class Chat
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("Usuario1Id")]
        public int Usuario1Id { get; set; }

        [Required]
        [Column("Usuario2Id")]
        public int Usuario2Id { get; set; }

        [Column("FechaCreacion")]
        public DateTime FechaCreacion { get; set; }

        [Required]
        [Column("Activo")]
        public bool Activo { get; set; }

        [ForeignKey("Usuario1Id")]
        public Usuario Usuario1 { get; set; }

        [ForeignKey("Usuario2Id")]
        public Usuario Usuario2 { get; set; }

        public ICollection<Mensaje> Mensajes { get; set; }
    }
}
