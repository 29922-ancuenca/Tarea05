using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _02_MVC.Models;
using Microsoft.AspNet.Identity;

namespace _02_MVC
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}

namespace _02_MVC.Infrastructure
{
    public class PermissionAuthorizeAttribute : AuthorizeAttribute
    {
        public string Module { get; }
        public string ActionName { get; }

        public PermissionAuthorizeAttribute(string module, string actionName)
        {
            Module = module;
            ActionName = actionName;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext))
            {
                return false;
            }

            var user = httpContext?.User;
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                return false;
            }

            // Bypass para el SuperAdmin
            if (string.Equals(user.Identity.Name, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            using (var db = new UnificadaBDEntities())
            {
                var superAdminId = db.AspNetUsers
                    .Where(u => u.UserName == "SuperAdmin" || u.Email == "superadmin@hotmail.com")
                    .Select(u => u.Id)
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(superAdminId))
                {
                    return false;
                }

                // Roles objetivo: Médico y Paciente
                var roleNames = new[] { "Medico", "Paciente" }
                    .Where(r => user.IsInRole(r))
                    .ToList();

                // Si no es Médico/Paciente, se mantiene compatibilidad con los roles legacy (create/edit/delete/view)
                if (!roleNames.Any())
                {
                    var legacyRole = ActionName == "Read" ? "view"
                        : ActionName == "Create" ? "create"
                        : ActionName == "Update" ? "edit"
                        : ActionName == "Delete" ? "delete"
                        : null;

                    return legacyRole != null && user.IsInRole(legacyRole);
                }

                foreach (var roleName in roleNames)
                {
                 var rolePrefix = "Permission." + roleName + ".";
                    var isConfiguredForRole = db.AspNetUserClaims.Any(c => c.UserId == superAdminId
                        && c.ClaimType.StartsWith(rolePrefix));

                    // Si aún no se ha configurado ningún permiso para este rol, por defecto se permite.
                    if (!isConfiguredForRole)
                    {
                        return true;
                    }

                    var requiredClaimType = rolePrefix + Module + "." + ActionName;
                    var allowed = db.AspNetUserClaims.Any(c => c.UserId == superAdminId
                        && c.ClaimType == requiredClaimType
                        && (c.ClaimValue == "true" || c.ClaimValue == "1"));

                    if (allowed)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User?.Identity?.IsAuthenticated == true)
            {
             filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                filterContext.Controller.ViewBag.ForbiddenModule = Module;
                filterContext.Controller.ViewBag.ForbiddenAction = ActionName;
                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/Forbidden.cshtml"
                };
                return;
            }

            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}
