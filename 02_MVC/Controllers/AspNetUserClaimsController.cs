using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _02_MVC.Models;

namespace _02_MVC.Controllers
{
    [Authorize(Users = "SuperAdmin")]
    public class AspNetUserClaimsController : Controller
    {
        private UnificadaBDEntities db = new UnificadaBDEntities();

        private string GetSuperAdminId()
        {
            return db.AspNetUsers
                .Where(u => u.UserName == "SuperAdmin" || u.Email == "superadmin@hotmail.com")
                .Select(u => u.Id)
                .FirstOrDefault();
        }

        // GET: AspNetUserClaims
        public ActionResult Index()
        {
            // Lista solo los roles objetivo (Médico y Paciente)
            var roles = db.AspNetRoles
                .Where(r => r.Name == "Medico" || r.Name == "Paciente")
                .OrderBy(r => r.Name)
                .ToList();

            return View(roles);
        }

        // GET: AspNetUserClaims/GestionarPermisos?rolId=...
        public ActionResult GestionarPermisos(string rolId)
        {
            if (string.IsNullOrWhiteSpace(rolId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var rol = db.AspNetRoles.Find(rolId);
            if (rol == null || (rol.Name != "Medico" && rol.Name != "Paciente"))
            {
                return HttpNotFound();
            }

            var superAdminId = GetSuperAdminId();
            if (string.IsNullOrWhiteSpace(superAdminId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "No existe el usuario SuperAdmin en AspNetUsers.");
            }

            EnsureRolePermissionsInitialized(superAdminId, rol.Name);

            var rolePrefix = "Permission." + rol.Name + ".";

            var claims = db.AspNetUserClaims
              .Where(c => c.UserId == superAdminId && c.ClaimType.StartsWith(rolePrefix))
                .ToList();

            var model = new RolePermissionsViewModel
            {
                RolId = rol.Id,
                RolNombre = rol.Name,
             Pacientes_Create = HasClaim(claims, $"Permission.{rol.Name}.Pacientes.Create"),
                Pacientes_Read = HasClaim(claims, $"Permission.{rol.Name}.Pacientes.Read"),
                Pacientes_Update = HasClaim(claims, $"Permission.{rol.Name}.Pacientes.Update"),
                Pacientes_Delete = HasClaim(claims, $"Permission.{rol.Name}.Pacientes.Delete"),
                Medicos_Create = HasClaim(claims, $"Permission.{rol.Name}.Medicos.Create"),
                Medicos_Read = HasClaim(claims, $"Permission.{rol.Name}.Medicos.Read"),
                Medicos_Update = HasClaim(claims, $"Permission.{rol.Name}.Medicos.Update"),
                Medicos_Delete = HasClaim(claims, $"Permission.{rol.Name}.Medicos.Delete"),
            };

            return View("Edit", model);
        }

        // POST: AspNetUserClaims/GuardarPermisos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuardarPermisos(RolePermissionsViewModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.RolId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var rol = db.AspNetRoles.Find(model.RolId);
            if (rol == null || (rol.Name != "Medico" && rol.Name != "Paciente"))
            {
                return HttpNotFound();
            }

            var superAdminId = GetSuperAdminId();
            if (string.IsNullOrWhiteSpace(superAdminId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "No existe el usuario SuperAdmin en AspNetUsers.");
            }

            var rolePrefix = "Permission." + rol.Name + ".";

            var desired = new[]
            {
             new { Type = $"Permission.{rol.Name}.Pacientes.Create", Enabled = model.Pacientes_Create },
                new { Type = $"Permission.{rol.Name}.Pacientes.Read", Enabled = model.Pacientes_Read },
                new { Type = $"Permission.{rol.Name}.Pacientes.Update", Enabled = model.Pacientes_Update },
                new { Type = $"Permission.{rol.Name}.Pacientes.Delete", Enabled = model.Pacientes_Delete },
                new { Type = $"Permission.{rol.Name}.Medicos.Create", Enabled = model.Medicos_Create },
                new { Type = $"Permission.{rol.Name}.Medicos.Read", Enabled = model.Medicos_Read },
                new { Type = $"Permission.{rol.Name}.Medicos.Update", Enabled = model.Medicos_Update },
                new { Type = $"Permission.{rol.Name}.Medicos.Delete", Enabled = model.Medicos_Delete },
            };

            var existing = db.AspNetUserClaims
              .Where(c => c.UserId == superAdminId && c.ClaimType.StartsWith(rolePrefix))
                .ToList();

          foreach (var p in desired)
            {
                var match = existing.FirstOrDefault(c => c.ClaimType == p.Type);
                if (match == null)
                {
                    match = new AspNetUserClaims
                    {
                        UserId = superAdminId,
                        ClaimType = p.Type,
                        ClaimValue = p.Enabled ? "true" : "false"
                    };
                    db.AspNetUserClaims.Add(match);
                }
                else
                {
                    match.ClaimValue = p.Enabled ? "true" : "false";
                }
            }

            db.SaveChanges();
            TempData["PermisosGuardados"] = "Permisos actualizados correctamente.";
            return RedirectToAction("GestionarPermisos", new { rolId = model.RolId });
        }

        private static bool HasClaim(System.Collections.Generic.IEnumerable<AspNetUserClaims> claims, string claimType)
        {
            return claims.Any(c => c.ClaimType == claimType && (c.ClaimValue == "true" || c.ClaimValue == "1"));
        }

        private void EnsureRolePermissionsInitialized(string superAdminId, string roleName)
        {
         var rolePrefix = "Permission." + roleName + ".";
            var all = new[]
            {
              $"Permission.{roleName}.Pacientes.Create",
                $"Permission.{roleName}.Pacientes.Read",
                $"Permission.{roleName}.Pacientes.Update",
                $"Permission.{roleName}.Pacientes.Delete",
                $"Permission.{roleName}.Medicos.Create",
                $"Permission.{roleName}.Medicos.Read",
                $"Permission.{roleName}.Medicos.Update",
                $"Permission.{roleName}.Medicos.Delete",
            };

            var existing = db.AspNetUserClaims
                .Where(c => c.UserId == superAdminId && c.ClaimType.StartsWith(rolePrefix))
                .ToList();

            var changed = false;
            foreach (var t in all)
            {
                var match = existing.FirstOrDefault(c => c.ClaimType == t);
                if (match == null)
                {
                    db.AspNetUserClaims.Add(new AspNetUserClaims
                    {
                        UserId = superAdminId,
                        ClaimType = t,
                        ClaimValue = "true"
                    });
                    changed = true;
                }
            }

            if (changed)
            {
                db.SaveChanges();
            }
        }

        // GET: AspNetUserClaims/Edit/5
        public ActionResult Edit(int? id)
        {
            // Vista `Edit` se usa para la pantalla de Gestión de Permisos.
            // Se deshabilita el CRUD directo de claims para evitar conflictos de vistas.
            return HttpNotFound();
        }

        // POST: AspNetUserClaims/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserId,ClaimType,ClaimValue")] AspNetUserClaims aspNetUserClaims)
        {
            // Vista `Edit` se usa para la pantalla de Gestión de Permisos.
            // Se deshabilita el CRUD directo de claims para evitar conflictos de vistas.
            return HttpNotFound();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
