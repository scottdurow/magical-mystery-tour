using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Contoso.API.Payments;

public static class ClaimsPrincipalExtensions
{
    // Check if the user is in a specific role or if we are running locally always return true
    public static bool AssertUserInRoles(this ClaimsPrincipal claimsPrincipal, params string[] roleDemands)
    {
        if (EnvironmentExtensions.IsRunningLocally())
        {
            return true;
        }
        var userRoles = claimsPrincipal.Claims.Where(e => e.Type == "roles").Select(e => e.Value);

        // Check that userRoles.Contains all the parameter roles
        bool allRoleDemandsMet = roleDemands.All(role => userRoles.Contains(role));

        // Check the user is in the roles, and if not raise a unauthorized exception
        if (allRoleDemandsMet)
        {
            return true;
        }
        else
        {
            throw new UnauthorizedAccessException($"The following roles are required to run this function: {string.Join(",", roleDemands)}. Current roles for identity [{claimsPrincipal.Identity?.Name}] are {string.Join(",",userRoles)}");
        }
    }
}

