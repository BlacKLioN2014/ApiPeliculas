using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiPeliculas.Modelos
{
    public class Pelicula
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Duracion { get; set; }
        public string? RutaImagen { get; set; }
        public string? RutaLocalImagen { get; set; }
        public enum TipoClasificacion { Siete, Trece, Dieciseis, Dieciocho}
        public TipoClasificacion clasificacion {  get; set; }
        public DateTime? FechaCreacion { get; set; }
        //Relacion con categoria
        public int categoriaId { get; set; }
        [ForeignKey("categoriaId")]
        public Categoria categoria { get; set; }
    }
}
