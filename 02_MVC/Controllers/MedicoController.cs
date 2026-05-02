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
    public class MedicoController : Controller
    {
        private UnificadaBDEntities db = new UnificadaBDEntities();

        // GET: Medico
        [PermissionAuthorize("Medicos", "Read")]
        public ActionResult Index()
        {
            var medico = db.Medico.Include(m => m.Clinica).Include(m => m.Especialidad);
            return View(medico.ToList());
        }

        // GET: Medico/Details/5
        [PermissionAuthorize("Medicos", "Read")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Medico medico = db.Medico.Find(id);
            if (medico == null)
            {
                return HttpNotFound();
            }
            return View(medico);
        }

        // GET: Medico/Create
        [PermissionAuthorize("Medicos", "Create")]
        public ActionResult Create()
        {
            ViewBag.codClinica = new SelectList(db.Clinica, "codClinica", "nombre");
            ViewBag.codEspecialidad = new SelectList(db.Especialidad, "codEspecialidad", "nombre");
            return View();
        }

        // POST: Medico/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("Medicos", "Create")]
        public ActionResult Create([Bind(Include = "codMedico,nombre,horaInicio,horaFin,codEspecialidad,codClinica")] Medico medico)
        {
            if (ModelState.IsValid)
            {
                db.Medico.Add(medico);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.codClinica = new SelectList(db.Clinica, "codClinica", "nombre", medico.codClinica);
            ViewBag.codEspecialidad = new SelectList(db.Especialidad, "codEspecialidad", "nombre", medico.codEspecialidad);
            return View(medico);
        }

        // GET: Medico/Edit/5
        [PermissionAuthorize("Medicos", "Update")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Medico medico = db.Medico.Find(id);
            if (medico == null)
            {
                return HttpNotFound();
            }
            ViewBag.codClinica = new SelectList(db.Clinica, "codClinica", "nombre", medico.codClinica);
            ViewBag.codEspecialidad = new SelectList(db.Especialidad, "codEspecialidad", "nombre", medico.codEspecialidad);
            return View(medico);
        }

        // POST: Medico/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("Medicos", "Update")]
        public ActionResult Edit([Bind(Include = "codMedico,nombre,horaInicio,horaFin,codEspecialidad,codClinica")] Medico medico)
        {
            if (ModelState.IsValid)
            {
                db.Entry(medico).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.codClinica = new SelectList(db.Clinica, "codClinica", "nombre", medico.codClinica);
            ViewBag.codEspecialidad = new SelectList(db.Especialidad, "codEspecialidad", "nombre", medico.codEspecialidad);
            return View(medico);
        }

        // GET: Medico/Delete/5
        [PermissionAuthorize("Medicos", "Delete")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Medico medico = db.Medico.Find(id);
            if (medico == null)
            {
                return HttpNotFound();
            }
            return View(medico);
        }

        // POST: Medico/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize("Medicos", "Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Medico medico = db.Medico.Find(id);
            db.Medico.Remove(medico);
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
