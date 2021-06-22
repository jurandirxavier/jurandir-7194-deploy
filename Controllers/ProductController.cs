using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loja.Models;
using Loja.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Loja.Controllers {
    [Route("products")]
    public class ProductController : ControllerBase {
        [HttpGet] // GET não há necessidade de colocar esse '[HttpGet]' aqui no módulo, colocamos apenas para ficar mais visível na tela
        [Route("")] // '[Route("")]' vai ser vazio, ou seja, ele vai pegar só a raiz do 'products'
        [AllowAnonymous]
        public async Task<ActionResult<List<Product>>> Get ([FromServices] DataContext context) {
            var products = await context.Products.Include (x => x.Category).AsNoTracking().ToListAsync();
            return products;
        }

        [HttpGet] // GETBYID
        [Route("{id:int}")] // No 'GetById' vai retornar só uma categoria, baseada no id da mesma
        [AllowAnonymous]
        public async Task<ActionResult<Product>> GetById ([FromServices] DataContext context, int id){
            var product = await context.Products.Include(x => x.Category).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return product;
        }
        // Esse método pode ser dentro de categorias ou produtos, nesse caso usamos em produtos pela praticidade
        [HttpGet] // GETBYCATEGORY products/categories/1, ou seja, listar todos os produtos da categoria 1
        [Route("categories/{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Product>>> GetByCategory([FromServices] DataContext context, int id){
            var products = await context.Products.Include(x => x.Category).AsNoTracking().Where(x => x.CategoryId == id).ToListAsync();
            return products;
        }

        [HttpPost] // POST criar o prodduto
        [Route("")]
        [Authorize(Roles = "employee")] // O funcionário pode criar produto
        public async Task<ActionResult<Product>> Post([FromServices] DataContext context, [FromBody]Product models) {
            if (ModelState.IsValid) {
                context.Products.Add(models);
                await context.SaveChangesAsync();
                return models; // Ou Ok(model)
            }
            else {
                return BadRequest(ModelState);
            }
        }
    }
}

// GetByCategory, [Route("")] products/categories/1 (Essa é a rota para esse caso)