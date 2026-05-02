using _02_MVC;
using _02_MVC.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using _02_MVC;
using _02_MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;


namespace _02_MVC
{

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


            // CONDIGO INSERTADO
            ApplicationDbContext db = new ApplicationDbContext();

            CrearRoles(db);
            CrearSuperUsuario(db);
            CrearAdministrador(db);
            CrearGerente(db);
            AsignarPermisos(db);

            db.Dispose();
        }

        private void CrearRoles(ApplicationDbContext db)
        {
            var rolemanager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));

            if (!rolemanager.RoleExists("view"))
            {
                rolemanager.Create(new IdentityRole("view"));
            }
            if (!rolemanager.RoleExists("edit"))
            {
                rolemanager.Create(new IdentityRole("edit"));
            }
            if (!rolemanager.RoleExists("delete"))
            {
                rolemanager.Create(new IdentityRole("delete"));
            }
            if (!rolemanager.RoleExists("create"))
            {
                rolemanager.Create(new IdentityRole("create"));
            }

        }

        private void CrearSuperUsuario(ApplicationDbContext db)
        {
            var usermanager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            var user = usermanager.FindByName("SuperAdmin@hotmail.com");
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = "SuperAdmin",
                    Email = "superadmin@hotmail.com"
                };
                usermanager.Create(user, "superadmin");
            }
        }



        private void CrearAdministrador(ApplicationDbContext db)
        {
            var usermanager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            var user = usermanager.FindByName("administrador@hotmail.com");
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = "Administrador",
                    Email = "administrador@hotmail.com"
                };
                usermanager.Create(user, "administrador");
            }
        }



        private void CrearGerente(ApplicationDbContext db)
        {
            var usermanager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            var user = usermanager.FindByName("gerente@hotmail.com");
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = "Gerente",
                    Email = "gerente@hotmail.com"
                };
                usermanager.Create(user, "gerente");
            }
        }



        private void AsignarPermisos(ApplicationDbContext db)
        {
            var usermanager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            // Gerente: solo view
            var gerente = usermanager.FindByName("Gerente");
            if (gerente != null)
            {
                if (!usermanager.IsInRole(gerente.Id, "view"))
                    usermanager.AddToRole(gerente.Id, "view");
            }

            // SuperAdmin: todo el CRUD
            var superAdmin = usermanager.FindByName("SuperAdmin");
            if (superAdmin != null)
            {
                if (!usermanager.IsInRole(superAdmin.Id, "view"))
                    usermanager.AddToRole(superAdmin.Id, "view");

                if (!usermanager.IsInRole(superAdmin.Id, "create"))
                    usermanager.AddToRole(superAdmin.Id, "create");

                if (!usermanager.IsInRole(superAdmin.Id, "edit"))
                    usermanager.AddToRole(superAdmin.Id, "edit");

                if (!usermanager.IsInRole(superAdmin.Id, "delete"))
                    usermanager.AddToRole(superAdmin.Id, "delete");
            }

            // Administrador: edit y delete
            var administrador = usermanager.FindByName("Administrador");
            if (administrador != null)
            {
                if (!usermanager.IsInRole(administrador.Id, "edit"))
                    usermanager.AddToRole(administrador.Id, "edit");

                if (!usermanager.IsInRole(administrador.Id, "delete"))
                    usermanager.AddToRole(administrador.Id, "delete");
            }
        }

    }
    }
