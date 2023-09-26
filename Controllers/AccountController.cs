using Sistema_Fallas_IMSS.Models;
using Sistema_Fallas_IMSS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Sistema_Fallas_IMSS.Controllers
{
    public class AccountController : Controller
    {
        // GET: Login
        public ActionResult Login(VM_Login _usuario)
        {
            if (_usuario.Usuario == null) { return View("Login"); }
            int validar = ValidarUsuario(_usuario);
            if (validar == 1)
            {
                FormsAuthentication.SetAuthCookie(_usuario.Usuario, false);
                string redirectURL = FormsAuthentication.GetRedirectUrl(_usuario.Usuario, false);
                redirectURL = redirectURL.Substring(redirectURL.LastIndexOf("/") + 1);
                if (redirectURL != "~/Account/Logout")
                    return Redirect($"~/Home/Index?usuario={_usuario.Usuario}");
                else
                    return View(_usuario);
            }
            else
            {
                _usuario.Mensaje = "error";
                return View(_usuario);
            }
            
        }

        public void Logout()
        {
            FormsAuthentication.SignOut();
            FormsAuthentication.RedirectToLoginPage();
        }
       
        public int ValidarUsuario(VM_Login _usuario)
        {
            using(var context = new IMSSEntities())
            {
                var usuario = context.usuarios.Where(u => u.cuenta == _usuario.Usuario && u.pass == _usuario.Password).FirstOrDefault();
                if (usuario != null)
                    return 1;
                else return 0;
            }
        }

    }
}