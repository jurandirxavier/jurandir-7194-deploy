using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Loja.Models;
using Microsoft.IdentityModel.Tokens;

namespace Loja.Services {
    public static class TokenService { // Esse "TokenService" é uma classe estática, ou seja, não precisa instanciar, precisa dar um "new" nele
        public static string GenerateToken(User user){ // Ele tem um método estático, que retorna uma string que é o Token chamado de "GenerateToken" aqui vai receber um "User", precisa de um "user" válido aqui para ele
            var tokenHandler = new JwtSecurityTokenHandler(); // tokenHandler vai manipular o Token, vai responsável por gerar o Token de fato
            var key = Encoding.ASCII.GetBytes(Settings.Secret); // Precisa novamente da chave, por isso criamos o "Settings.Secret" lá em Services
            var tokenDescriptor = new SecurityTokenDescriptor { // Descrição do que vai ter dentro do Token
                
                Subject = new ClaimsIdentity(new Claim[] { 

                    new Claim(ClaimTypes.Name, user.Username.ToString()), // Pág. 32
                    new Claim(ClaimTypes.Role, user.Role.ToString())    
                }),
                Expires = DateTime.UtcNow.AddHours(2), // Não recomendável que o Token dure por muito tempo, o usuário pode perder
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)

            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token); 
        }
    }
}