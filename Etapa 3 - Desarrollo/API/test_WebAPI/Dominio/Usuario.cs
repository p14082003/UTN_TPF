using System.ComponentModel.DataAnnotations;

namespace Dominio
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100, ErrorMessage = "El apellido no puede superar los 100 caracteres.")]
        public string  Surname { get; set; }

        [Range(0, 200, ErrorMessage = "La edad debe estar entre 0 y 200.")]
        public int Age { get; set; }
    }
}
