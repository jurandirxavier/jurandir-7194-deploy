using Microsoft.OpenApi.Models;
using Loja.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.ResponseCompression;
using System.Linq;

namespace Loja
{
    public class Startup
    {
        private const string V = "/swagger/v1/swagger.json";

        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }
        
        public IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services) {

            services.AddCors(); // Pág. 30
            services.AddResponseCompression(options => {
                options.Providers.Add<GzipCompressionProvider>(); // Ele vai comprimir o JSON antes de mandar para a tela, ele vai pegar nossa informação vai zipar ela antes de mandar para tela e depois o HTML tem a habilidade de descompactar
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json" }); // Estou dizendo que quero comprimir tudo que for "application/json"
            });
            // services.AddResponseCaching();
            services.AddControllers();
            
            var key = Encoding.ASCII.GetBytes(Settings.Secret);
            services.AddAuthentication(x => {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x => {
                x.RequireHttpsMetadata = false; // Desabilitada
                x.SaveToken = true; // Vai salvar esse "Token" aqui
                x.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuerSigningKey = true, // Vai validar se tem uma chave aqui
                    // A chave que ele vai validar do "Front" com o "Backend" aqui que será enviada 'SymmetricSecurityKey(key)' da chave 
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });


            // Informar à aplicação que tem um DataContext e que esse DataContext é o "Datacontext"
            // Dentro do DataContext tem a opção de informá lo que tipo de banco está usando (PostgreSQL, SqlServer, InMemoryDatabase)
            // Como vai usar o InMemoryDatabase antes de conectar ao banco, precisa dar um nome a ele, que nesse será "Database", mas pode ser qualquer nome
            // # services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("Database")), antes de conectar ao SqlSever estava usado em memória;
            services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("Database")); // Pág. 32
            // services.AddDbContext<DataContext>(opt => opt.UseSqlServer(Configuration.GetConnectionString("connectionstring")));
        //services.AddScoped<DataContext, DataContext>(); // Resolvendo a dependência criada, essa linha pode ser removida pág.33

            // Pág. 30 e 31 Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c => 
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Loja", Version = "v1"});
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Pág. 31
                app.UseSwagger(); // Especificação da API no formato json 
                // Ferramenta visual para testar a aplicação
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Loja v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            // Pág. 30
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
            // Informar à aplicação que tem um DataContext e que esse DataContext é o "Datacontext"
            // # services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("Database"));
            // Dentro do DataContext tem a opção de informá lo que tipo de banco está usando (PostgreSQL, SqlServer, InMemoryDatabase)
            // Como está usando o InMemoryDatabase, precisa dar um nome a ele, que nesse será "Database", mas pode ser qualquer nome
            // # services.AddScoped<DataContext, DataContext>(); // Resolvendo a dependência criada