using PlacetoPay.Integrations.Library.CSharp.Contracts;
using PlacetoPay.Integrations.Library.CSharp.Entities;
using PlacetoPay.Integrations.Library.CSharp.Message;
using redireccionp2p.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using WebApplication2.Models;
using P2P = PlacetoPay.Integrations.Library.CSharp.PlacetoPay;

namespace redireccionp2p.Controllers
{
    public class TransaccionesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Transacciones
        public ActionResult Index()
        {
            return View(db.Transaccions.ToList());
        }

        // GET: Transacciones/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaccion transaccion = db.Transaccions.Find(id);
            if (transaccion == null)
            {
                return HttpNotFound();
            }
            return View(transaccion);
        }

        // GET: Transacciones/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Transacciones/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,URL,requestId,transaccionOk,estadotransaccion,motivotransaccion,Autorizacion,referencia,fecha")] Transaccion transaccion)
        {
            if (ModelState.IsValid)
            {
                db.Transaccions.Add(transaccion);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(transaccion);
        }

        // GET: Transacciones/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaccion transaccion = db.Transaccions.Find(id);
            if (transaccion == null)
            {
                return HttpNotFound();
            }
            return View(transaccion);
        }

        // POST: Transacciones/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,URL,requestId,transaccionOk,estadotransaccion,motivotransaccion,Autorizacion,referencia,fecha")] Transaccion transaccion)
        {
            if (ModelState.IsValid)
            {
                db.Entry(transaccion).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(transaccion);
        }

        // GET: Transacciones/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaccion transaccion = db.Transaccions.Find(id);
            if (transaccion == null)
            {
                return HttpNotFound();
            }
            return View(transaccion);
        }

        // POST: Transacciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Transaccion transaccion = db.Transaccions.Find(id);
            db.Transaccions.Remove(transaccion);
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
        public Gateway CrearAutenticacion()
        {
            Gateway gateway = new P2P("6dd490faf9cb87a9862245da41170ff2",
            "024h1IlD",
            new Uri("https://test.placetopay.com/redirection/"),
            Gateway.TP_REST);
            return gateway;
        }
        public RedirectRequest CrearTransaccion()
        {
            Gateway gateway = CrearAutenticacion();
            Amount amount = new Amount(20000);
            string referencia = "pruebasd_" + db.Transaccions.ToList().Count();
            Payment payment = new Payment(referencia, "No hay descripcion", amount);
            Transaccion transaccion = new Transaccion();
            transaccion.Id = db.Transaccions.ToList().Count() + 1;
            Person buyer = new Person("10000004", "CC", "Daniel", "Betancur", "pruebasp2p@hotmail.com");

            RedirectRequest request = new RedirectRequest(payment,
                "http://localhost:63562/Transacciones/returnURL" + "?" + "id=" + transaccion.Id,
                "192.168.0.2",
                "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:47.0) Gecko/20100101 Firefox/47.0",
                (DateTime.Now).AddMinutes(40).ToString("yyyy-MM-ddTHH\\:mm\\:sszzz"), null, null, false, false, false, null, buyer);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            RedirectResponse response = gateway.Request(request);
            transaccion.requestId = response.RequestId;
            transaccion.URL = response.ProcessUrl;
            transaccion.referencia = referencia;
            db.Transaccions.Add(transaccion);
            Response.Redirect(response.ProcessUrl);
            db.SaveChanges();
            return request;

        }
        [HttpGet]
        public ActionResult returnURL(int? id)
        {
            Transaccion transaccion = db.Transaccions.Find(id);
            Gateway gateway = CrearAutenticacion();
            RedirectInformation response = gateway.Query(transaccion.requestId);
            if (response.Payment != null)
            {
                transaccion.estadotransaccion = response.Payment[0].Status.status;
                if (transaccion.estadotransaccion == "APPROVED")
                {
                    transaccion.estadotransaccion = "Aprobada";
                }
                else if (transaccion.estadotransaccion == "REJECTED")
                {
                    transaccion.estadotransaccion = "Rechazada";
                }
                else
                {
                    transaccion.estadotransaccion = "Pendiente";
                }
                transaccion.motivotransaccion = response.Payment[0].Status.Message;
                transaccion.transaccionOk = true;
                transaccion.fecha = response.Status.Date;
                transaccion.Autorizacion = response.Payment[0].Authorization;
                db.SaveChanges();
            }
            return RedirectToAction("Mensajetrasaccion", new { id = transaccion.Id });

        }

        public ActionResult Mensajetrasaccion(int? id)
        {
            Transaccion transaccion = db.Transaccions.Find(id);
            ViewData["Nombre"] = "Eduard Tomàs";
            ViewData["Twitter"] = "eiximenis";
            return View("Mensajetrasaccion", transaccion);
        }
        [HttpPost]
        public void Notificacion(data datos)
        {
            db.Transaccions.Where(t => t.requestId == datos.requestId).First().motivotransaccion = datos.status.message;
            db.Transaccions.Where(t => t.requestId == datos.requestId).First().estadotransaccion = datos.status.status;
            db.SaveChanges();
        }
    }
}
