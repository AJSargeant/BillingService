﻿using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BillingTests
{
    public static class TokenGen
    {
        public static JwtSecurityToken UserToken(string role)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MY TOP SECRET TEST KEY"));
            var claims = new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, "Test-UserID-String-1"),
                new Claim(ClaimTypes.Name, "John Smith"),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: "issuer",
                audience: "audience",
                claims: claims,
                notBefore: DateTime.Now.AddSeconds(-1),
                expires: DateTime.Now.AddDays(7),
                signingCredentials: new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256)
            );
            return token;
        }
    }
}
