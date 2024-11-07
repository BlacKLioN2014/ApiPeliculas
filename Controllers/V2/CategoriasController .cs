using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiPeliculas.Controllers.V2
{
    //[Authorize(Roles = "Admin")]
    //[ResponseCache(Duration =20)]
    [Authorize]
    [Route("api/v{version:apiVersion}/categorias")]
    [ApiController]
    //[EnableCors("PoliticaCors")] //aplica la politica CORS a este método
    [ApiVersion("2.0")]
    public class CategoriasController : ControllerBase
    {


        private readonly ICategoriaRepositorio _ctRepo;
        private readonly IMapper _mapper;


        public CategoriasController(ICategoriaRepositorio ctRepo, IMapper mapper)
        {
            _ctRepo = ctRepo;
            _mapper = mapper;
        }


        [HttpGet("GetString")]
        [AllowAnonymous]
        [ResponseCache(Duration = 20)]
        //[MapToApiVersion("2.0")]
        public IEnumerable<string> Get()
        {
            return new string[] { "Abraham", "Michelle" };
        }


    }
}
