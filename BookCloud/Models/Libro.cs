using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookCloud.Models
{
    [Table("Libros")]
    public class Libro
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("Titulo")]
        public string Titulo { get; set; }

        [Required]
        [Column("Autor")]
        public string Autor { get; set; }

        [Column("Descripcion")]
        public string? Descripcion { get; set; }

        [Required]
        [Column("Precio")]
        public decimal Precio { get; set; }

        [Required]
        [Column("Stock")]
        public int Stock { get; set; }

        [Required]
        [Column("UsuarioId")]
        public int UsuarioId { get; set; }

        [Column("FechaPublicacion")]
        public DateTime FechaPublicacion { get; set; }

        [Required]
        [Column("Activo")]
        public bool Activo { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }
    }
}
