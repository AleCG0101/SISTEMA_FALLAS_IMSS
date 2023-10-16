using Sistema_Fallas_IMSS.Models;
using Sistema_Fallas_IMSS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.IO;
using System.Web.Mvc;
using Rotativa;

namespace Sistema_Fallas_IMSS.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index(string usuario)
        {
            using(var context = new IMSSEntities())
            {
                if (usuario == null)
                {
                    return Redirect("~/Account/Login");
                }
                //var contexto = System.Web.HttpContext.Current;
                //string localIP = contexto.Request.UserHostAddress;
                string localIP = "";
                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());// objeto para guardar la ip
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily.ToString() == "InterNetwork")
                    {
                        localIP = ip.ToString().Trim();// esta es nuestra ip
                    }
                }

                var persona = context.existencias.Where(model => model.direccion_ip == localIP).Select(model => model.nombre_persona).FirstOrDefault();
                VM_Index data = new VM_Index
                {
                    Direccion_ip = localIP,
                    Usuario = usuario,
                    Persona = persona != null ? persona.ToString() : "Sin existencia actual",
                    Reporte = ObtnerDatos(),
                };
                if (ObtenerRol(usuario) > 3)
                {
                    return View("IndexUsuario", data);
                }
                else
                    return View("IndexAdmin", data);
            }
            
        }
        public static int ObtenerRol(string _usuario)
        {
            using (var context = new IMSSEntities())
            {
                var usuario = context.usuarios.Where(us => us.cuenta == _usuario).FirstOrDefault();

                return (int)usuario.id_rol;
            }
        }

        public VM_Reportes ObtnerDatos()
        {
            using(var context = new IMSSEntities())
            {
                VM_Reportes reportes = new VM_Reportes
                {
                    Tipos = context.tipos_falla.Select(model => new SelectListItem
                    {
                        Value = model.Id_tipo_falla.ToString(),
                        Text = model.descripcion,
                    }).ToList(),
                };
                reportes.Tipos.Insert(reportes.Tipos.Count, new SelectListItem
                {
                    Value = "0",
                    Text = "Otro"
                });
                return reportes;
            }
        }
        [HttpPost]
        public ActionResult ModalReporte()
        {
            VM_Reportes data = ObtnerDatos();

            return PartialView("_ModalReporte",data);
        }
     
        [HttpGet]
        public JsonResult ObtenerFallas(int _id_tipo)
        {
            using (var context = new IMSSEntities())
            {
                List<SelectListItem> items = context.fallas.Where(x => x.Id_tipo_falla == _id_tipo).Select(x => new SelectListItem
                {
                    Value = x.Id_falla.ToString(),
                    Text = x.descripcion,
                }).ToList();

                return Json(items, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public int GenerarReporte(VM_Reportes _reporte)
        {
            using (var context = new IMSSEntities())
            {
                using(var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        reporte reporte = new reporte
                        {
                            ip_usuario = _reporte.Usuario,
                            descripcion = _reporte.Descripcion,
                            contacto = _reporte.Contacto,
                            estatus = 1,
                            fecha_registro = DateTime.Now,
                        };
                        context.reporte.Add(reporte);
                        context.SaveChanges();

                        reporte_fallas fallas = new reporte_fallas
                        {
                            id_reporte = reporte.Id_reporte,
                            falla = _reporte.Otra_falla,
                            id_falla = _reporte.Falla != "null" ? Convert.ToInt32(_reporte.Falla) : 0,
                        };
                        context.reporte_fallas.Add(fallas);
                        context.SaveChanges();
                        transaction.Commit();
                        return 1;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return 0;
                    }
                }
            }
        }
        public ActionResult Reporte(int id_reporte)
        {
            VM_Reportes reporte = ObtenerReporte(id_reporte);
            reporte.Fecha = reporte.Fecha_registro.ToLongDateString();
            reporte.Contacto = String.IsNullOrEmpty(reporte.Contacto) ? "Sin contacto" : reporte.Contacto;
            return View("Documento",reporte);
        }

        public ActionResult Imprimir(int _id_reporte)
        {           
            return new ActionAsPdf("Reporte", new { id_reporte = _id_reporte });
        }
        private VM_Reportes ObtenerReporte(int _id_reporte)
        {
            using (var context = new IMSSEntities())
            {

                var sqlString = $@"SELECT * FROM (
                                    SELECT
                                        reporte.Id_reporte,
                                        existencias.nombre_persona usuario,
                                        reporte.descripcion,
                                        reporte.estatus,
                                        reporte.fecha_registro,
                                        reporte.fecha_concluido,
                                        reporte.contacto,
                                        fallas.descripcion falla,
                                        tipo.descripcion tipo,
                                        area.nombre_area
                                    FROM
                                        reporte
                                    INNER JOIN existencias ON reporte.ip_usuario = existencias.direccion_ip
                                    INNER JOIN areas_imss area ON existencias.id_area = area.Id_area
                                    LEFT JOIN reporte_fallas rfallas ON reporte.Id_reporte = rfallas.id_reporte
                                    INNER JOIN fallas ON rfallas.id_falla = fallas.Id_falla
                                    INNER JOIN tipos_falla tipo ON fallas.Id_tipo_falla = tipo.Id_tipo_falla
                                    UNION
                                    SELECT DISTINCT
                                        reporte.Id_reporte,
                                        COALESCE(existencias.nombre_persona, reporte.ip_usuario) AS usuario,
                                        reporte.descripcion,
                                        reporte.estatus,
                                        reporte.fecha_registro,
                                        reporte.fecha_concluido,
                                        reporte.contacto,
                                        COALESCE(fallas.descripcion, rfallas.falla) AS falla,
										COALESCE(tipo.descripcion, 'Otro') AS tipo,
                                        COALESCE(area.nombre_area, 'Sin registrar') AS usuario
                                    FROM
                                        reporte
                                    LEFT JOIN existencias ON reporte.ip_usuario = existencias.direccion_ip
									LEFT JOIN areas_imss area ON existencias.id_area = area.Id_area
									LEFT JOIN reporte_fallas rfallas ON reporte.Id_reporte = rfallas.id_reporte
									LEFT JOIN fallas ON rfallas.id_falla = fallas.Id_falla
									LEFT JOIN tipos_falla tipo ON fallas.Id_tipo_falla = tipo.Id_tipo_falla
                                  ) AS consulta
                                    WHERE consulta.Id_reporte = {_id_reporte};";

                return context.Database.SqlQuery<VM_Reportes>(sqlString).FirstOrDefault();
            }
        }

        public ActionResult IndexGrid(string _usuario)
        {
            VM_Index data = new VM_Index
            {
                Reportes = ObtenerReportes(_usuario),
            };
            return PartialView("_IndexGrid",data);
        }

        private List<VM_Reportes> ObtenerReportes(string _usuario)
        {
            using (var context = new IMSSEntities())
            {
                var usuario = context.usuarios.Where(us => us.cuenta == _usuario).FirstOrDefault();

                var sqlString = $@"SELECT * FROM (
                                    SELECT
                                        reporte.Id_reporte,
                                        existencias.nombre_persona usuario,
                                        reporte.descripcion,
                                        reporte.estatus,
                                        reporte.fecha_registro,
                                        reporte.fecha_concluido,
                                        reporte.contacto,
                                        fallas.descripcion falla,
                                        tipo.descripcion tipo,
                                        area.nombre_area
                                    FROM
                                        reporte
                                    INNER JOIN existencias ON reporte.ip_usuario = existencias.direccion_ip
                                    INNER JOIN areas_imss area ON existencias.id_area = area.Id_area
                                    LEFT JOIN reporte_fallas rfallas ON reporte.Id_reporte = rfallas.id_reporte
                                    INNER JOIN fallas ON rfallas.id_falla = fallas.Id_falla
                                    INNER JOIN tipos_falla tipo ON fallas.Id_tipo_falla = tipo.Id_tipo_falla
                                    UNION
                                    SELECT DISTINCT
                                        reporte.Id_reporte,
                                        COALESCE(existencias.nombre_persona, reporte.ip_usuario) AS usuario,
                                        reporte.descripcion,
                                        reporte.estatus,
                                        reporte.fecha_registro,
                                        reporte.fecha_concluido,
                                        reporte.contacto,
                                        COALESCE(fallas.descripcion, rfallas.falla) AS falla,
										COALESCE(tipo.descripcion, 'Otro') AS tipo,
                                        COALESCE(area.nombre_area, 'Sin registrar') AS usuario
                                    FROM
                                        reporte
                                    LEFT JOIN existencias ON reporte.ip_usuario = existencias.direccion_ip
									LEFT JOIN areas_imss area ON existencias.id_area = area.Id_area
									LEFT JOIN reporte_fallas rfallas ON reporte.Id_reporte = rfallas.id_reporte
									LEFT JOIN fallas ON rfallas.id_falla = fallas.Id_falla
									LEFT JOIN tipos_falla tipo ON fallas.Id_tipo_falla = tipo.Id_tipo_falla
                                  ) AS consulta
                                  ORDER BY consulta.Id_reporte DESC";

                return context.Database.SqlQuery<VM_Reportes>(sqlString).ToList();
            }
        }
    }
}