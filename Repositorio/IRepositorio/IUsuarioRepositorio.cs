using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;

namespace ApiPeliculas.Repositorio.IRepositorio
{
    public interface IUsuarioRepositorio
    {


        ICollection<AppUsuario> GetUsuarios();


        AppUsuario GetUsuario(string UsuarioId);


        bool IsUniqueUser(string Usuario);

        Task<UsuarioLoginRespuestaDto> Login(UsuarioLoginDto usuarioLoginDto);


        Task<UsuarioDatosDto> Registro(UsuarioRegistroDto usuario);


    }
}
