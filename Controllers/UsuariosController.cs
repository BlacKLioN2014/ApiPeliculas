using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiPeliculas.Controllers
{
    [Route("api/v{version:apiVersion}/Usuarios")]
    [ApiController]
    [ApiVersionNeutral]
    public class UsuariosController : ControllerBase
    {


        private readonly IUsuarioRepositorio _usRepo;
        private readonly IMapper _mapper;
        protected RespuestaApi _RespuestaApi { get; set; }


        public UsuariosController(IUsuarioRepositorio usRepo, IMapper mapper)
        {
            _usRepo = usRepo;
            _mapper = mapper;
            _RespuestaApi = new();
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetUsuarios()
        {
            var listaUsuarios = _usRepo.GetUsuarios();

            var listaUsuariosDto = new List<UsuarioDatosDto>();

            foreach (var lista in listaUsuarios)
            {
                listaUsuariosDto.Add(_mapper.Map<UsuarioDatosDto>(lista));
            }
            return Ok(listaUsuariosDto);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("{usuarioId}", Name = "GetUsuario")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetUsuario(string usuarioId)
        {
            var itemUsuario = _usRepo.GetUsuario(usuarioId);

            if (itemUsuario == null)
            {
                return NotFound();
            }

            var itemUsuarioDto = _mapper.Map<UsuarioDatosDto>(itemUsuario);

            return Ok(itemUsuarioDto);
        }


        [AllowAnonymous]
        [HttpPost("registro")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> registro([FromBody] UsuarioRegistroDto usuarioRegistroDto)
        {
            bool validarNombreUsuarioUnico = _usRepo.IsUniqueUser(usuarioRegistroDto.NombreUsuario);
            if (!validarNombreUsuarioUnico)
            {
                _RespuestaApi.statusCode = System.Net.HttpStatusCode.BadRequest;
                _RespuestaApi.IsSuccess = false;
                _RespuestaApi.ErrorMessages.Add("El nombre de usuario ya existe");
                return BadRequest(_RespuestaApi);
            }

            var usuario = await _usRepo.Registro(usuarioRegistroDto);
            if (usuario == null)
            {
                _RespuestaApi.statusCode = System.Net.HttpStatusCode.BadRequest;
                _RespuestaApi.IsSuccess = false;
                _RespuestaApi.ErrorMessages.Add("Error en el registro");
                return BadRequest(_RespuestaApi);
            }

            _RespuestaApi.statusCode = System.Net.HttpStatusCode.OK;
            _RespuestaApi.IsSuccess = true;
            return Ok(_RespuestaApi);
        }


        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> login([FromBody] UsuarioLoginDto usuarioLoginDto)
        {
            var respuestaLogin = await _usRepo.Login(usuarioLoginDto);

            if (respuestaLogin.Usuario == null || string.IsNullOrEmpty(respuestaLogin.Token))
            {
                _RespuestaApi.statusCode = System.Net.HttpStatusCode.BadRequest;
                _RespuestaApi.IsSuccess = false;
                _RespuestaApi.ErrorMessages.Add("El nombre de usuario o password, son incorrectos");
                return BadRequest(_RespuestaApi);
            }

            _RespuestaApi.statusCode = System.Net.HttpStatusCode.OK;
            _RespuestaApi.IsSuccess = true;
            _RespuestaApi.Result = respuestaLogin;
            return Ok(_RespuestaApi);

        }


    }
}
