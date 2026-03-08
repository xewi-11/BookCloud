using System.ComponentModel.DataAnnotations;

namespace BookCloud.Models.ViewModels
{
    public class RecargaTarjetaViewModel
    {
        [Required(ErrorMessage = "El monto es requerido")]
        [Range(1, 10000, ErrorMessage = "El monto debe estar entre $1 y $10,000")]
        [Display(Name = "Monto a recargar")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "El número de tarjeta es requerido")]
        [StringLength(19, MinimumLength = 13, ErrorMessage = "El número de tarjeta debe tener entre 13 y 19 dígitos")]
        [Display(Name = "Número de tarjeta")]
        [RegularExpression(@"^[\d\s]+$", ErrorMessage = "El número de tarjeta solo puede contener dígitos y espacios")]
        public string NumeroTarjeta { get; set; }

        [Required(ErrorMessage = "El nombre del titular es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre del titular")]
        public string NombreTitular { get; set; }

        [Required(ErrorMessage = "La fecha de vencimiento es requerida")]
        [Display(Name = "Fecha de vencimiento (MM/AA)")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "El formato debe ser MM/AA (ej: 12/25)")]
        public string FechaVencimiento { get; set; }

        [Required(ErrorMessage = "El CVV es requerido")]
        [StringLength(4, MinimumLength = 3, ErrorMessage = "El CVV debe tener 3 o 4 dígitos")]
        [Display(Name = "CVV")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "El CVV solo puede contener números")]
        public string CVV { get; set; }
    }
}
