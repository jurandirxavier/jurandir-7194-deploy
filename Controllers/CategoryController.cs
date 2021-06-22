using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loja.Data;
using Loja.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
// Endpoint é o mesmo que URL
// https://localhost:5001/categories Para chegar nesse "controller" usa se um atirbuto chamado de "Route"
[Route("v1/categories")] // Defini a rota "categories"
public class CategoryController : ControllerBase {
    // https://localhost:5001/categories Para chegar no "Metodo" aqui em baixo se não define a "rota", vai funcionar normalmente
    
    // GET
    [HttpGet] // GET
    [Route("")]
    [AllowAnonymous] // Qualquer um pode ver a categoria
    [ResponseCache(VaryByHeader = "User-Agent", Location = ResponseCacheLocation.Any, Duration = 30)] // Informações Pág. 29
    // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)] // Desabilita o cache
    public async Task<ActionResult<List<Category>>> Get([FromServices]DataContext context) {

        var categories = await context.Categories.AsNoTracking().ToListAsync(); 
        return Ok(categories); // No 'Get' Vai retornar uma lista de categoria
    }    
    // GETBYID
    [HttpGet] // GETBYID
    [Route("{id:int}")] // :int estou colocando restrição na rota, ou seja, só aceita números inteiros, por exemplo
    [AllowAnonymous] // Qualquer um pode ver o Id
    // Caso eu tente passar um conjunto de caracteres, vai dar um 404 (not found), ou seja, não posso passar uma cadeia de caracteres por uma rota que espera um inteiro
    public async Task<ActionResult<Category>> GetById(int id, [FromServices]DataContext context) {

        var category = await context.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return category; // No 'GetById' vai retornar só uma categoria, baseada no id da mesma
    }
    // POST
    [HttpPost]
    [Route("")]
    [Authorize(Roles = "employee")] // Só os funcionários podem criar categoria
    // Agora vamos capturar o JSON enviado para cá, tem q defenir um formato para ele
    // Cujas informações estão nos "Models (pasta) Category.cs"
    // CTRL + . (Category) e 'using Loja.Models;'
    public async Task<ActionResult<List<Category>>> Post([FromBody]Category models, [FromServices]DataContext context) {
        if (!ModelState.IsValid) // Se o ModelState não estiver válido "! (not)"
            return BadRequest(ModelState); // Validação do modelo, o ModelState não for válido retorna BadRequest
        try {
            context.Categories.Add(models);
            await context.SaveChangesAsync();            
            return Ok(models);
        }
        catch { // 'catch(Exception)' vai dar detalhes do erro, caso não queira, pode deixar 'catch'
            return BadRequest(new { message = "Não foi possível criar a categoria"});
        }
        // Retorna "models" para tela de novo, 
        // como ñ posso converter esse models p/ string, coloque 'public Category Post()' antes 'public string Post()'
        // Precisa dizer onde ele vai buscar esse parâmetro Category, p/ isso ultiliza '[FromBody]', ou seja, vai vir uma categoria no corpo da riquisição
    }
    // PUT
    // No PUT vamos mesclar o envio pela rota e pelo body da categoria '{id:int}' e '[FromBody]Category models'
    [HttpPut]
    [Route("{id:int}")]
    [Authorize(Roles = "employee")] // Funcionário também pode atualizar a categoria
    public async Task<ActionResult<List<Category>>> Put(int id, [FromBody]Category models, [FromServices]DataContext context) {
        // Verifica se o id informado é o mesmo do modelo
        if (id != models.Id) // Aqui vamos colcar um 'if'. Se o models.Id for igual ao id  que recebemos, return models, senão return null
            return NotFound(new { message = "Categoria não encontrada" });

        // Verifica se os dados são válidos
        if (!ModelState.IsValid)
            return BadRequest(ModelState); // Também pode 'BadRequest(new { message = "Categoria não encontrada" });'
        try {
            context.Entry<Category>(models).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return Ok(models);
        }
        catch (DbUpdateConcurrencyException) {
            return BadRequest (new { message = "Este registro já foi atualizado"});
        }
        catch (Exception) {
            return BadRequest (new { message = "Não foi possível atualizar a categoria"});
        }
    }
    // DELETE
    [HttpDelete]
    [Route("{id:int}")]
    [Authorize(Roles = "employee")] // Funcionário também pode deletar a categoria
    public async Task<ActionResult<List<Category>>> Delete(int id, [FromServices] DataContext context) {
        
        var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (category == null)
                return NotFound(new { message = "Categoria não encontrada"});

            try {
                context.Categories.Remove(category);
                await context.SaveChangesAsync();
                return Ok(new { message = "Categoria removida com sucesso" }); // Ou simplismente 'return Ok(category)'
            }
            catch (Exception){
                return BadRequest(new { message = "Não foi possível remover a categoria"});
            }        
    }
}

// Para testar essa aplicação
// Ctrl + F5, vai abrir o browser, feche e use o POSTMAN
// Não esqueça do RESTART (Ctrl + Shift + F5)
// Por padrão o REST tem sempre o mesma "Rote" para as URL, só muda o verbo 
// GET, POST, PUT e DELETE