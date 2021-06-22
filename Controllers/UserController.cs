using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Loja.Data;
using Loja.Models;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Loja.Services;

namespace Loja.Controllers {

    [Route("users")]
    public class UserController : Controller {

        [HttpGet]
        [Route("")]
        [Authorize(Roles = "manager")] // Só o gerente tem acesso a lista de usuários
        public async Task<ActionResult<List<User>>> Get([FromServices] DataContext context) {
            var users = await context
            .Users
            .AsNoTracking()
            .ToListAsync();
        return users;    
        }

        [HttpPost]
        [Route("")]
        [AllowAnonymous] //  [Authorize(Roles = "manager")] só o gerente vai conseguir criar usuário
        public async Task<ActionResult<User>> Post([FromServices] DataContext context, [FromBody]User model) {

            // Verifica se os dados são válidos
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try {
                // Força o usuário ser sempre funcionário
                model.Role = "employee"; // Pág. 32

                context.Users.Add(model);
                await context.SaveChangesAsync();
                // Esconde a senha 
                model.Password = ""; // Pág. 32
                return model;
            }    
            catch (Exception) {
                return BadRequest(new { message = "Não foi possível criar o usuário" });
            }
        }
        
        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = "manager")] // Só o gerente pode atualizar/alterar um usuário
        public async Task<ActionResult<User>> Put ([FromServices] DataContext context, int id, [FromBody] User model) {
            // Verifica se os dados são válidos
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // verifica se o ID informado é o mesmo do modelo
            if (id != model.Id)
                return NotFound(new { message = "Usuário não encontrado"});

            try {
                context.Entry(model).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return model;
            }
            catch (Exception) {
                return BadRequest(new { message = "Não foi possível criar o usuário"});
            }        
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<dynamic>> Authenticate([FromServices] DataContext context, [FromBody]User model){

            var user = await context.Users
                .AsNoTracking()
                .Where(x => x.Username == model.Username && x.Password == model.Password)
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "Usuário ou senha inválido" });

            var token = TokenService.GenerateToken(user);
            // Esconde a senha
            user.Password = ""; // Pág. 32
            return new {

                user = user,
                token = token 
            };        
        }
    }
}

//        EXEMPLO DE AUTORIZAÇÃO (Authorize) método "Get"

//        [HttpGet]
//        [Route("anonimo")] // Qualquer pessoa pode acessar
//        [AllowAnonymous] // Apesar de não ter nenhuma restrição, permite o acesso de qualquer anônimo a esse método, mas por padrão já permitido o acesso
//        public string Anonimo() => "Anonimo";

//        [HttpGet]
//        [Route("autenticado")] // Somente usuário autenticado, mas independente do Roles pode acessar
//        [Authorize] // Só pode acessar esse método se você tiver autenticado, como não passou nenhum role, qualquer um serve
//        public string Autenticado() => "Autenticado";

//        [HttpGet]
//        [Route("funcionario")] // Onde só o funcionário pode acessar
//        [Authorize(Roles="employee")] // Aqui tem Roles "employee"
//        public string Funcionario() => "Funcionario";

//        [HttpGet]
//        [Route("gerente")] // Só o gerente pode acessar
//        [Authorize(Roles="manager")] // Aqui tem Roles "manager"
//        public string Gerente() => "Gerente";