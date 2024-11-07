using ApiPeliculas.Data;
using ApiPeliculas.PeliculasMappers;
using ApiPeliculas.Repositorio;
using ApiPeliculas.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using ApiPeliculas.Modelos;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
                  opciones.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSQL")));


//Agregamos el auto mapper
builder.Services.AddAutoMapper(typeof(PeliculasMapper));


//Aqui se configura  la autentificacion
var key = builder.Configuration.GetValue<string>("ApiSettings:Secreta");


//soporte para autentificacion con .net Identity
builder.Services.AddIdentity<AppUsuario, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();


//Soporte para versionamiento
var apiVersionInbuilder = builder.Services.AddApiVersioning(opcion =>
{
    opcion.AssumeDefaultVersionWhenUnspecified = true;
    opcion.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    opcion.ReportApiVersions = true;
//opcion.ApiVersionReader = ApiVersionReader.Combine(
//    new QueryStringApiVersionReader("Api-version") //?api-version= 1.0
//    //new HeaderApiVersionReader("X-Version"),
//    //new MediaTypeApiVersionReader("Ver"));
//    );
});

apiVersionInbuilder.AddApiExplorer(
    opciones =>
    {
        opciones.GroupNameFormat = "'v'VVV";
        opciones.SubstituteApiVersionInUrl = true;  
    }
    );

builder.Services.AddAuthentication
    (
        x => 
        { 
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; 
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }
    ).AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateIssuer = false,
            ValidateAudience = false
        };

    });


builder.Services.AddControllers(opcion =>
{
    //Cache profile. un cache global y así no tener que ponerlo en todas partes.
    opcion.CacheProfiles.Add("PorDefecto", new CacheProfile() { Duration = 30});
});


builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description =
                    $"Autentificacion JWT usando el esquema Bearer." + "\n" + "\n" +
                    $"Ingresa la palabra 'Bearer' seguido de un [espacio] y despues su token en el campo de abajo." + "\n" + "\n" +
                    $"Ejemplo: Bearer dfdfnjdfj ",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Scheme = "Bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In= ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
                options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Version = "v1.0",
                        Title = "Peliculas Api v1",
                        Description = "Api de Peliculas version 1",
                        TermsOfService = new Uri("https://google.com.mx"),
                        Contact = new OpenApiContact
                        {
                            Name = "Abraham Jimenez",
                            Email = "abrahamijg@gmai.com",
                        
                        },
                        License = new OpenApiLicense
                        {
                            Name = "Licencia Personal",
                            Url = new Uri("https://google.com.mx")
                        }
                    });
                options.SwaggerDoc("v2", new OpenApiInfo
                {
                    Version = "v2.0",
                    Title = "Peliculas Api v2",
                    Description = "Api de Peliculas version 2",
                    TermsOfService = new Uri("https://google.com.mx"),
                    Contact = new OpenApiContact
                    {
                        Name = "Abraham Jimenez",
                        Email = "abrahamijg@gmai.com",

                    },
                    License = new OpenApiLicense
                    {
                        Name = "Licencia Personal",
                        Url = new Uri("https://google.com.mx")
                    }
                });
            });


//Aqui se configuran los CORS
builder.Services.AddCors(p => p.AddPolicy("politicaCors",build =>
{
    //*
    build.WithOrigins("http://localhost:3223").AllowAnyMethod().AllowAnyHeader().AllowCredentials();
}));


//Aqui se configura el cache
builder.Services.AddResponseCaching();


//Agregamos lo repositorios
builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
builder.Services.AddScoped<IPeliculaRepositorio, PeliculaRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opciones =>
    {
        opciones.SwaggerEndpoint("/swagger/v1/swagger.json","ApiPeliculasV1");
        opciones.SwaggerEndpoint("/swagger/v2/swagger.json", "ApiPeliculasV2");
    });
}

//Soporte para archivos estaticos como imagenes
app.UseStaticFiles();

app.UseHttpsRedirection();


//soporte para CORS
app.UseCors("politicaCors");


//Soporte para autentificacion
app.UseAuthentication();


app.UseAuthorization();


app.MapControllers();


app.Run();
