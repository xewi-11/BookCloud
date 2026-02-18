using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookCloud.Models
{
    [Table("Pagos")]
    public class Pago
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("PedidoId")]
        public int PedidoId { get; set; }

        [Column("FechaPago")]
        public DateTime FechaPago { get; set; }

        [Required]
        [Column("Monto")]
        public decimal Monto { get; set; }

        [Required]
        [Column("Metodo")]
        public string Metodo { get; set; }

        [Required]
        [Column("Estado")]
        public string Estado { get; set; }

        [Required]
        [Column("Activo")]
        public bool Activo { get; set; }

        [ForeignKey("PedidoId")]
        public Pedido Pedido { get; set; }
    }
}
