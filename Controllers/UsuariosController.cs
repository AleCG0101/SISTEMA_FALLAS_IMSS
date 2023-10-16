using Sistema_Fallas_IMSS.Models;
using Sistema_Fallas_IMSS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Sistema_Fallas_IMSS.Controllers
{
    public class UsuariosController : Controller
    {
        // GET: Usuarios
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UsuariosGrid()
        {
            using (var context = new IMSSEntities())
            {
                List<VM_ListUsuarios> data = (from usuario in context.usuarios
                                         join roles in context.roles on usuario.id_rol equals roles.Id_rol
                                         select new VM_ListUsuarios
                                         {
                                             Id = usuario.Id,
                                             Nombre = usuario.nombre,
                                             Cuenta = usuario.cuenta,
                                             Rol = roles.nombre
                                         }).ToList();

                return PartialView("_IndexGrid", data);
            }
        }
        [HttpPost]
        public PartialViewResult AbrirModalUsuarios(int _id_usuario)
        {
            using(var context = new IMSSEntities())
            {
                VM_ListUsuarios data = new VM_ListUsuarios();
                data.Roles = (from roles in context.roles
                              select new SelectListItem
                              {
                                  Value = roles.Id_rol.ToString(),
                                  Text = roles.nombre,
                              }).ToList();
                if (_id_usuario > 0)
                {
                    var usuario = (from usuarios in context.usuarios
                                   join roles in context.roles on usuarios.id_rol equals roles.Id_rol
                                   select new VM_ListUsuarios
                                   {
                                       Id = usuarios.Id,
                                       Nombre = usuarios.nombre,
                                       Cuenta = usuarios.cuenta,
                                       Rol = roles.Id_rol.ToString(),
                                   }).FirstOrDefault();
                    data.Id = usuario.Id;
                    data.Nombre = usuario.Nombre;
                    data.Cuenta = usuario.Cuenta;
                    data.Rol = usuario.Rol;

                    return PartialView("_ModalUsuario", data);
                }
                data.Rol = "";
                return PartialView("_ModalUsuario", data);
            }
        }
    }
}