using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookCloud.Models
{
    [Table("Pedidos")]
    public class Pedido
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("UsuarioId")]
        public int UsuarioId { get; set; }

        [Column("FechaPedido")]
        public DateTime FechaPedido { get; set; }

        [Required]
        [Column("Total")]
        public decimal Total { get; set; }

        [Required]
        [Column("Estado")]
        public string Estado { get; set; }

        [Required]
        [Column("Activo")]
        public bool Activo { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        public ICollection<Pago> Pagos { get; set; }
    }
}
