using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookCloud.Models
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }
        [Column("Nombre")]
        public string Nombre { get; set; }
        [Column("Email")]
        public string Correo { get; set; }

        [Column("PassWordHash")]
        public byte[] PassWordHash { get; set; }
        [Column("Salt")]
        public string Salt { get; set; }

        [Column("FechaRegistro")]
        public DateTime FechaRegistro { get; set; }
        [Column("Activo")]
        public bool Activo { get; set; }
        [Column("FotoUrl")]

        public string? Foto { get; set; }
    }
}
