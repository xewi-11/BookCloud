using System.ComponentModel.DataAnnotations;

namespace BookCloud.Models.ViewModels
{
    public class DatosTarjetaViewModel
    {
        [Required(ErrorMessage = "El número de tarjeta es obligatorio")]
        [RegularExpression(@"^[\d\s]{13,19}$", ErrorMessage = "El número de tarjeta debe tener entre 13 y 19 dígitos")]
        [Display(Name = "Número de tarjeta")]
        public string NumeroTarjeta { get; set; }

        [Required(ErrorMessage = "El nombre del titular es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        [Display(Name = "Nombre del titular")]
        public string NombreTitular { get; set; }

        [Required(ErrorMessage = "La fecha de vencimiento es obligatoria")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Formato inválido. Use MM/AA")]
        [Display(Name = "Fecha de vencimiento")]
        public string FechaVencimiento { get; set; }

        [Required(ErrorMessage = "El CVV es obligatorio")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "El CVV debe tener 3 o 4 dígitos")]
        [Display(Name = "CVV")]
        public string CVV { get; set; }

        public decimal Total { get; set; }
    }
}
