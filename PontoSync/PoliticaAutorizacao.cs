using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PontoSync
{
    public class PoliticaAutorizacao
    {
    }

    public class RequisitoGrupos : IAuthorizationRequirement
    {
        public IEnumerable<String> Grupos { get; }

        public RequisitoGrupos(IEnumerable<String> grupos)
        {
            Grupos = grupos;
        }
    }

    public class RequisitoGruposHandler : AuthorizationHandler<RequisitoGrupos>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       RequisitoGrupos requirement)
        {
            var grupos = context.User.FindAll("Grupos");
            foreach (var g in grupos)
            {               
                if (requirement.Grupos.Any(gr => gr.CompareTo(g.Value) == 0))
                {
                    context.Succeed(requirement);
                    break;
                }
            }
            return Task.CompletedTask;
        }
    }
}
