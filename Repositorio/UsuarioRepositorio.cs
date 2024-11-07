using ApiPeliculas.Data;
using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using XSystem.Security.Cryptography;

namespace ApiPeliculas.Repositorio
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {


        private readonly ApplicationDbContext _bd;
        private string claveSecreta;
        private readonly UserManager<AppUsuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;

        public UsuarioRepositorio(ApplicationDbContext bd,IConfiguration config, UserManager<AppUsuario> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper)
        {
            _bd = bd;
            claveSecreta = config.GetValue<string>("ApiSettings:Secreta");
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }


        public AppUsuario GetUsuario(string UsuarioId)
        {
            return _bd.AppUsuario.FirstOrDefault(c => c.Id==UsuarioId);
        }


        public ICollection<AppUsuario> GetUsuarios()
        {
            return _bd.AppUsuario.OrderBy(c => c.UserName).ToList();
        }


        //public bool IsUniqueUser(string Usuario)
        //{
        //    var usuarioBd = _bd.Usuario.FirstOrDefault(u => u.NombreUsuario == Usuario);

        //    if(usuarioBd == null)
        //    {
        //        return true;
        //    }
        //    return false;
        //}
        public bool IsUniqueUser(string Usuario)
        {
            var usuarioBd = _bd.AppUsuario.FirstOrDefault(u => u.UserName == Usuario);

            if (usuarioBd == null)
            {
                return true;
            }
            return false;
        }


        public async Task<UsuarioLoginRespuestaDto> Login(UsuarioLoginDto usuarioLoginDto)
        {
            //var passwordEncriptado = obtenerMd5(usuarioLoginDto.Passsword);

            var usuario = _bd.AppUsuario.FirstOrDefault(
                u => u.UserName.ToLower() == usuarioLoginDto.NombreUsuario.ToLower());

            bool isValid = await _userManager.CheckPasswordAsync(usuario,usuarioLoginDto.Passsword);
            
            // validamos si el usuario no existe, con la combinacion de usuario y contraseña
            if (usuario == null || isValid == false)
            {
                return new UsuarioLoginRespuestaDto()
                {
                    Token = "",
                    Usuario = null
                };
            }

            //Aqui existe usuario
            var roles = await _userManager.GetRolesAsync(usuario);

            var manejadorToken = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(claveSecreta);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, usuario.UserName.ToString()),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new (new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256Signature)
            };

            var token = manejadorToken.CreateToken(tokenDescriptor);

            UsuarioLoginRespuestaDto usuarioLoginRespuesta = new UsuarioLoginRespuestaDto()
            {
                Token = manejadorToken.WriteToken(token),
                Usuario = _mapper.Map<AppUsuario>(usuario),
                //Role = roles.FirstOrDefault()
            };

            return usuarioLoginRespuesta;
        }


        public async Task<UsuarioDatosDto> Registro(UsuarioRegistroDto UsuarioRegistroDto)
        {
            //var passwordEncriptado = obtenerMd5(UsuarioRegistroDto.Passsword);

            AppUsuario usuario = new AppUsuario()
            {
                UserName = UsuarioRegistroDto.NombreUsuario,
                Email = UsuarioRegistroDto.NombreUsuario,
                NormalizedEmail = UsuarioRegistroDto.NombreUsuario.ToUpper(),
                Nombre = UsuarioRegistroDto.Nombre
            };

            //Validaciones
            var result = await _userManager.CreateAsync(usuario, UsuarioRegistroDto.Passsword);
            if (result.Succeeded) 
            {
                if (!_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult()) 
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    await _roleManager.CreateAsync(new IdentityRole("Registrado"));
                    await _userManager.AddToRoleAsync(usuario, "Admin");
                }
                else
                {
                    await _userManager.AddToRoleAsync(usuario, "Admin");
                    var usuarioRetornado = _bd.AppUsuario.FirstOrDefault(u => u.UserName == UsuarioRegistroDto.NombreUsuario);  //O = usuario.username
                    return _mapper.Map<UsuarioDatosDto>(usuarioRetornado);
                }
            }

            //_bd.AppUsuario.Add(usuario);
            //await _bd.SaveChangesAsync();
            //usuario.Passsword = passwordEncriptado;

            return new UsuarioDatosDto();
        }


        //Metodo para encriptar contraseña con md5 se usa tanto en ek acceso como en el registro
        //public static string obtenerMd5(string valor)
        //{
        //    MD5CryptoServiceProvider x = new MD5CryptoServiceProvider();
        //    byte[] data = System.Text.Encoding.UTF8.GetBytes(valor);
        //    data = x.ComputeHash(data);
        //    string resp = "";
        //    for (int i = 0; i < data.Length; i++)
        //        resp += data[i].ToString("X2").ToLower();
        //    return resp;
        //}


    }
}
