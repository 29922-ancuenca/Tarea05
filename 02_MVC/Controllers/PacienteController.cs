using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _02_MVC.Models;
using _02_MVC.Infrastructure;

namespace _02_MVC.Controllers
{
    public class PacienteController : Controller
    {
        private UnificadaBDEntities db = new UnificadaBDEntities();

        // GET: Paciente
        [PermissionAuthorize("Pacientes", "Read")]
        public ActionResult Index()
        {
            return View(db.Paciente.ToList());
        }

        // GET: Paciente/Details/5
        [PermissionAuthorize("Pacientes", "Read")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Paciente paciente = db.Paciente.Find(id);
            if (paciente == null)
            {
                return HttpNotFound();
            }
            return View(paciente);
        }

        // GET: Paciente/Create
        [PermissionAuthorize("Pacientes", "Create")]
        public ActionResult Create()
        {
            return View(new Paciente());
        }

        // POST: Paciente/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Paciente/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("Pacientes", "Create")]
        public ActionResult Create([Bind(Include = "codPaciente,cedula,nombre,fechaNacimiento,foto")] Paciente paciente, string PesoTexto, string AlturaTexto)
        {
            ModelState.Remove("peso");
            ModelState.Remove("altura");

            if (!string.IsNullOrEmpty(PesoTexto))
            {
                paciente.peso = Convert.ToDouble(PesoTexto.Replace(".", ","));
            }

            if (!string.IsNullOrEmpty(AlturaTexto))
            {
                paciente.altura = Convert.ToDouble(AlturaTexto.Replace(".", ","));
            }

            if (ModelState.IsValid)
            {
                db.Paciente.Add(paciente);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(paciente);
        }

        // GET: Paciente/Edit/5
        [PermissionAuthorize("Pacientes", "Update")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Paciente paciente = db.Paciente.Find(id);
            if (paciente == null)
            {
                return HttpNotFound();
            }
            return View(paciente);
        }

        // POST: Paciente/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Paciente/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("Pacientes", "Update")]
        public ActionResult Edit([Bind(Include = "codPaciente,cedula,nombre,fechaNacimiento,foto")] Paciente paciente, string PesoTexto, string AlturaTexto)
        {
            // Quitar validación automática de esos campos
            ModelState.Remove("peso");
            ModelState.Remove("altura");

            // Convertir valores a double
            if (!string.IsNullOrEmpty(PesoTexto))
            {
                paciente.peso = Convert.ToDouble(PesoTexto.Replace(".", ","));
            }

            if (!string.IsNullOrEmpty(AlturaTexto))
            {
                paciente.altura = Convert.ToDouble(AlturaTexto.Replace(".", ","));
            }

            if (ModelState.IsValid)
            {
                db.Entry(paciente).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(paciente);
        }

        // GET: Paciente/Delete/5
        [PermissionAuthorize("Pacientes", "Delete")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Paciente paciente = db.Paciente.Find(id);
            if (paciente == null)
            {
                return HttpNotFound();
            }
            return View(paciente);
        }

        // POST: Paciente/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("Pacientes", "Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Paciente paciente = db.Paciente.Find(id);
            db.Paciente.Remove(paciente);
            db.SaveChanges();
            return RedirectToAction("Index");
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
