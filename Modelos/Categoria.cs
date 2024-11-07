using System.ComponentModel.DataAnnotations;

namespace ApiPeliculas.Modelos
{  


    public class Categoria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        //[Display(Name = "Fecha de creación")]
        public DateTime FechaCreacion { get; set; }
    }


}
