using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TrabajoFinalWeb.Models;
using System.Windows.Forms;
using Hotel_UPC.Authorization;
using Restaurante.Constantes;

namespace TrabajoFinalWeb.Controllers
{
    [UserLoggedOn]
    [AdminsOnly]
    public class DetalleBoletasController : Controller
    {
        private RestauranteEntitiesContext db = new RestauranteEntitiesContext();
        // GET: DetalleBoletas
        public ActionResult Index()
        {
            var detalleBoletas = db.DetalleBoletas.Include(d => d.ModoDePago).Include(d => d.Pedido);
            return View(detalleBoletas.ToList());
        }

        [AllowAnonymous]
        // GET: DetalleBoletas
        public ActionResult Index_Especifico()
        {
            int id_Pedido = (from c in db.Pedidoes select c).Count();
            var detalleBoletas = from c in db.DetalleBoletas where c.IdPedido == id_Pedido select c;
            return View(detalleBoletas.ToList());
        }
        
        public ActionResult generarCodigoQR(int? id)
        {
            //DetalleBoleta detalle = db.DetalleBoletas.Find(id);
            //var detalleBoletas = db.DetalleBoletas.Include(d => d.ModoDePago).Include(d => d.Pedido);
            //String monto = Convert.ToString(detalle.MontoTotal);
            //QRCodeEncoder encoder = new QRCodeEncoder();
            //Bitmap img = encoder.Encode(monto);
            //Image QR = (Image)img;
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    QR.Save(ms, ImageFormat.Jpeg);
            //    byte[] imageBytes = ms.ToArray();
            //    ViewBag.Texto = "data:image/gif;base64" + Convert.ToBase64String(imageBytes);
            //}
            return RedirectToAction("Index_Especifico");
        }
        // GET: DetalleBoletas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Incluir productos relacionados al pedido de la boleta
            var detalleBoleta = db.DetalleBoletas
                .Include(d => d.Pedido)
                .Include(d => d.Pedido.Productos_Pedidos.Select(p => p.Producto)) // Incluir productos
                .FirstOrDefault(d => d.ID == id);

            if (detalleBoleta == null)
            {
                return HttpNotFound();
            }

            return View(detalleBoleta);
        }

        // GET: DetalleBoletas/Create
        public ActionResult Create()
        {
            ViewBag.IdModoDePago = new SelectList(db.ModoDePagoes, "ID", "Descripcion");
            ViewBag.IdPedido = new SelectList(db.Pedidoes, "ID", "Detalle");
            return View();
        }

        // POST: DetalleBoletas/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
        [Bind(Include = "ID,IdPedido,MontoTotal,IdModoDePago")] DetalleBoleta detalleBoleta,
        string NumeroTarjeta,
        string NombreTitular,
        string FechaExpiracion,
        string CVV)
        {
            if (ModelState.IsValid)
            {
                if (detalleBoleta.IdModoDePago == 2) // Suponiendo que 2 es el ID para "tarjeta"
                {
                    if (string.IsNullOrWhiteSpace(NumeroTarjeta) || string.IsNullOrWhiteSpace(NombreTitular) ||
                        string.IsNullOrWhiteSpace(FechaExpiracion) || string.IsNullOrWhiteSpace(CVV))
                    {
                        ModelState.AddModelError("", "Debe completar todos los campos de la tarjeta.");
                        ViewBag.ModosDePago = db.ModoDePagoes.ToList();
                        return View(detalleBoleta);
                    }

                    // Datos ficticios para fines académicos (solo imprime en consola)
                    System.Diagnostics.Debug.WriteLine("Número de Tarjeta: " + NumeroTarjeta);
                    System.Diagnostics.Debug.WriteLine("Nombre del Titular: " + NombreTitular);
                    System.Diagnostics.Debug.WriteLine("Fecha de Expiración: " + FechaExpiracion);
                    System.Diagnostics.Debug.WriteLine("CVV: " + CVV);
                }

                db.DetalleBoletas.Add(detalleBoleta);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ModosDePago = db.ModoDePagoes.ToList();
            return View(detalleBoleta);
        }



        // GET: DetalleBoletas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DetalleBoleta detalleBoleta = db.DetalleBoletas.Find(id);
            if (detalleBoleta == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdModoDePago = new SelectList(db.ModoDePagoes, "ID", "Descripcion", detalleBoleta.IdModoDePago);
            ViewBag.IdPedido = new SelectList(db.Pedidoes, "ID", "Detalle", detalleBoleta.IdPedido);
            return View(detalleBoleta);
        }

        // POST: DetalleBoletas/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,IdPedido,MontoTotal,IdModoDePago")] DetalleBoleta detalleBoleta)
        {
            if (ModelState.IsValid)
            {
                db.Entry(detalleBoleta).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdModoDePago = new SelectList(db.ModoDePagoes, "ID", "Descripcion", detalleBoleta.IdModoDePago);
            ViewBag.IdPedido = new SelectList(db.Pedidoes, "ID", "Detalle", detalleBoleta.IdPedido);
            return View(detalleBoleta);
        }

        // GET: DetalleBoletas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DetalleBoleta detalleBoleta = db.DetalleBoletas.Find(id);
            if (detalleBoleta == null)
            {
                return HttpNotFound();
            }
            return View(detalleBoleta);
        }

        // POST: DetalleBoletas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DetalleBoleta detalleBoleta = db.DetalleBoletas.Find(id);
            db.DetalleBoletas.Remove(detalleBoleta);
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
        //Pedido_Boleta

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Pedido_Boleta()
        {
            int indice = (from c in db.PS select c).Count();
            ViewBag.MontoTotal = (from a in db.PPS.
                            Include("Producto").Include("P").AsEnumerable()
                                  where a.IdPedido == indice
                                  select a.Producto.Precio).Sum();
            ViewBag.IdModoDePago = new SelectList(db.ModoDePagoes, "ID", "Descripcion");
            return View();
        }
        //RegistrarPedido

        [AllowAnonymous]
        public IEnumerable<TrabajoFinalWeb.Models.P> Getpedidos()
        {
            return db.PS.AsEnumerable<TrabajoFinalWeb.Models.P>();
        }

        [AllowAnonymous]
        public IEnumerable<TrabajoFinalWeb.Models.PP> Getproducto_pedidos()
        {
            return db.PPS.AsEnumerable<TrabajoFinalWeb.Models.PP>();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegistrarPedido(int IdModoDePago)
        {
            IEnumerable<TrabajoFinalWeb.Models.P> lista = Getpedidos();
            IEnumerable<TrabajoFinalWeb.Models.PP> listaPP = Getproducto_pedidos();

            Pedido pedido = new Pedido();
            Empleado objUser = (Empleado)Session[SessionName.User];

            pedido.Atendido = (bool)lista.ElementAt(0).Atendido;
            pedido.Detalle = objUser.Nombre;
            pedido.IdEmpleado = "ADMINMax";

            db.Pedidoes.Add(pedido);
            db.SaveChanges();

            int id_Pedido = pedido.ID;

            for (int i = 0; i < listaPP.Count(); i++)
            {
                Productos_Pedidos producto_pedido = new Productos_Pedidos
                {
                    IdPedido = id_Pedido,
                    IdProducto = (int)listaPP.ElementAt(i).IdProducto,
                    Cantidad = 1 // Ajusta según sea necesario
                };

                // Actualizar el stock del producto
                var producto = db.Productoes.Find(producto_pedido.IdProducto);
                if (producto != null)
                {
                    if (producto.Stock >= producto_pedido.Cantidad)
                    {
                        producto.Stock -= producto_pedido.Cantidad;
                        db.Entry(producto).State = EntityState.Modified;
                    }
                    else
                    {
                        // Mostrar mensaje al usuario y cancelar toda la operación si no hay suficiente stock
                        ModelState.AddModelError("", $"El producto {producto.Nombre} no tiene suficiente stock. Disponibles: {producto.Stock}");

                        // Restaurar transacciones si se necesita
                        db.Pedidoes.Remove(pedido); // Eliminar el pedido recién creado
                        db.SaveChanges();
                        return View("Index");
                    }
                }

                db.Productos_Pedidos.Add(producto_pedido);
            }

            db.SaveChanges();

            DetalleBoleta detalle = new DetalleBoleta
            {
                IdModoDePago = IdModoDePago,
                IdPedido = id_Pedido,
                MontoTotal = (decimal)(from a in db.Productos_Pedidos
                                       where a.IdPedido == id_Pedido
                                       select (a.Producto.Precio * a.Cantidad)).Sum()
            };

            db.DetalleBoletas.Add(detalle);
            db.SaveChanges();

            db.eliminarTodo();
            db.SaveChanges();

            var detalleBoleta = db.DetalleBoletas
                .Include(d => d.ModoDePago)
                .Include(d => d.Pedido.Productos_Pedidos.Select(p => p.Producto)) // Incluir productos
                .FirstOrDefault(d => d.IdPedido == id_Pedido);

            return View("Index_Especifico", new List<DetalleBoleta> { detalleBoleta });
        }





    }
}
