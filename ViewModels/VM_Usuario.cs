using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sistema_Fallas_IMSS.ViewModels
{
    public class VM_Usuario
    {
       

    }

    public class VM_ListUsuarios
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Matricula { get; set; }
        public string Cuenta { get; set; }
        public string Password { get; set; }
        public string Rol { get; set; }
        public List<SelectListItem> Roles { get; set; } = new List<SelectListItem>();
    }
}