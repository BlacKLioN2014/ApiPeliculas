using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiPeliculas.Controllers.V1
{
    [Route("api/v{version:apiVersion}/peliculas")]
    [ApiController]
    public class PeliculasController : ControllerBase
    {


        private readonly IPeliculaRepositorio _pelRepo;
        private readonly IMapper _mapper;


        public PeliculasController(IPeliculaRepositorio pelRepo, IMapper mapper)
        {
            _pelRepo = pelRepo;
            _mapper = mapper;
        }

        #region V1 GetPeliculas
        //V1
        //[AllowAnonymous]
        //[HttpGet]
        //[ProducesResponseType(StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //public IActionResult GetPeliculas()
        //{
        //    var listaPeliculas = _pelRepo.GetPeliculas();

        //    var listaPeliculasDto = new List<PeliculaDto>();

        //    foreach (var lista in listaPeliculas)
        //    {
        //        listaPeliculasDto.Add(_mapper.Map<PeliculaDto>(lista));
        //    }

        //    // Ordena la lista en orden ascendente según la propiedad que desees, por ejemplo, por "Titulo".
        //    //var listaOrdenada = listaPeliculasDto.OrderBy(p => p.Id).ToList();

        //    if (listaPeliculasDto.Count > 1)
        //    {
        //        return Ok(listaPeliculasDto.OrderBy(p => p.Id).ToList());
        //    }
        //    else
        //    {
        //        return Ok(listaPeliculasDto);
        //    }
        //}
        #endregion


        //V2
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetPeliculas([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var totalPeliculas = _pelRepo.GetTotalPeliculas();
                var peliculas = _pelRepo.GetPeliculas(pageNumber, pageSize);

                if  (peliculas == null || !peliculas.Any()) 
                { 
                    return NotFound("No se encontraro peliculas.");
                }

                var peliculasDto = peliculas.Select(p => _mapper.Map<PeliculaDto>(p)).ToList();

                var response = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalPeliculas / (double)pageSize),
                    TotalItems = totalPeliculas,
                    Items = peliculasDto
                };

                return Ok(response);

            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error recuperando datos de la aplicacion");
            }
        }


        [AllowAnonymous]
        [HttpGet("{PeliculaId:int}", Name = "GetPelicula")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetPelicula(int PeliculaId)
        {
            var itemPelicula = _pelRepo.GetPelicula(PeliculaId);

            if (itemPelicula == null)
            {
                return NotFound();
            }

            var itemPeliculaDto = _mapper.Map<PeliculaDto>(itemPelicula);

            return Ok(itemPeliculaDto);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(PeliculaDto))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CrearPelicula([FromForm] CrearPeliculaDto crearPeliculaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (crearPeliculaDto == null)
            {
                return BadRequest(crearPeliculaDto);
            }

            if (_pelRepo.ExistePelicula(crearPeliculaDto.Nombre))
            {
                ModelState.AddModelError("", $"La Pelicula, ya existe");
                return StatusCode(404, ModelState);
            }

            var pelicula = _mapper.Map<Pelicula>(crearPeliculaDto);

            //if (!_pelRepo.CrearPelicula(pelicula))
            //{
            //    ModelState.AddModelError("", $"Algo salio mal guardando el registro {pelicula.Nombre}");
            //    return StatusCode(404, ModelState);
            //}

            //Subida de archivo
            if (crearPeliculaDto.Imagen != null)
            {
                string nombreArchivo = pelicula.Id + System.Guid.NewGuid().ToString() + Path.GetExtension(crearPeliculaDto.Imagen.FileName);
                string rutaArchivo = @"wwwroot\ImagenesPeliculas\" + nombreArchivo;

                var UbicacionDirectorio = Path.Combine(Directory.GetCurrentDirectory(), rutaArchivo);

                FileInfo file = new FileInfo(UbicacionDirectorio);

                if (file.Exists)
                {
                    file.Delete();
                }
                using (var fileStream = new FileStream(UbicacionDirectorio, FileMode.Create))
                {
                    crearPeliculaDto.Imagen.CopyTo(fileStream);
                }

                var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                pelicula.RutaImagen = baseUrl + "/ImagenesPeliculas/" + nombreArchivo;
                pelicula.RutaLocalImagen = rutaArchivo;

            }
            else
            {
                pelicula.RutaImagen = "https://placehold.co/600x400";
            }

            _pelRepo.CrearPelicula(pelicula);
            return CreatedAtRoute("GetPelicula", new { PeliculaId = pelicula.Id }, pelicula);
        }


        [Authorize(Roles = "Admin")]
        [HttpPatch("{peliculaId:int}", Name = "ActualizarPatchPelicula")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ActualizarPatchPelicula(int peliculaId, [FromForm] ActualizarPeliculaDto actualizarPeliculaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (actualizarPeliculaDto == null || peliculaId != actualizarPeliculaDto.Id)
            {
                return BadRequest(ModelState);
            }

            var peliculaExistente = _pelRepo.GetPelicula(peliculaId);
            if (peliculaExistente == null)
            {
                return NotFound($"No se encontro la pelicula, con Id {peliculaId}");
            }

            var pelicula = _mapper.Map<Pelicula>(actualizarPeliculaDto);

            //if (!_pelRepo.ActualizarPelicula(pelicula))
            //{
            //    ModelState.AddModelError("", $"Algo salio mal actualizando el registro {pelicula.Nombre}");
            //    return StatusCode(500, ModelState);
            //}

            //Subida de archivo
            if (actualizarPeliculaDto.Imagen != null)
            {
                string nombreArchivo = pelicula.Id + System.Guid.NewGuid().ToString() + Path.GetExtension(actualizarPeliculaDto.Imagen.FileName);
                string rutaArchivo = @"wwwroot\ImagenesPeliculas\" + nombreArchivo;

                var UbicacionDirectorio = Path.Combine(Directory.GetCurrentDirectory(), rutaArchivo);

                FileInfo file = new FileInfo(UbicacionDirectorio);

                if (file.Exists)
                {
                    file.Delete();
                }
                using (var fileStream = new FileStream(UbicacionDirectorio, FileMode.Create))
                {
                    actualizarPeliculaDto.Imagen.CopyTo(fileStream);
                }

                var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                pelicula.RutaImagen = baseUrl + "/ImagenesPeliculas/" + nombreArchivo;
                pelicula.RutaLocalImagen = rutaArchivo;

            }
            else
            {
                pelicula.RutaImagen = "https://placehold.co/600x400";
            }

            _pelRepo.ActualizarPelicula(pelicula);
            return NoContent();
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{peliculaId:int}", Name = "BorrarPelicula")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult BorrarPelicula(int peliculaId)
        {

            if (!_pelRepo.ExistePelicula(peliculaId))
            {
                return NotFound();
            }

            var pelicula = _pelRepo.GetPelicula(peliculaId);

            if (!_pelRepo.BorrarPelicula(pelicula))
            {
                ModelState.AddModelError("", $"Algo salio mal borrando el registro {pelicula.Nombre}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }


        [AllowAnonymous]
        [HttpGet("GetPeliculasEnCategoria/{categoriaId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetPeliculasEnCategoria(int categoriaId)
        {
            try
            {
                var listaPeliculas = _pelRepo.GetPeliculasEnCategoria(categoriaId);

                if (listaPeliculas == null || !listaPeliculas.Any())
                {
                    return NotFound($"No se encotraron peliculas");
                }

                var itemPelicula = listaPeliculas.Select(pelicula => _mapper.Map<PeliculaDto>(pelicula)).ToList();

                //foreach (var item in listaPeliculas)
                //{
                //    itemPelicula.Add(_mapper.Map<PeliculaDto>(item));
                //}

                return Ok(itemPelicula);
            }
            catch (Exception X)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, "Hubo un error" + X.ToString());
            }
            
        }


        [AllowAnonymous]
        [HttpGet("Buscar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Buscar(string nombre)
        {
            try
            {
                var resultado = _pelRepo.BuscarPelicula(nombre);

                if (!resultado.Any())
                {
                    return NotFound($"No se encotraron peliculas");
                }
                
                var peliculasDto = _mapper.Map<IEnumerable<PeliculaDto>>(resultado);
                return Ok(peliculasDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, " Error recuperando datos de la aplicacion");
            }
        }
    }
}
