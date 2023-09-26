using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sistema_Fallas_IMSS.ViewModels
{
    public class VM_Area
    {
        public List<VM_Areas> Areas { get; set; } = new List<VM_Areas>();
    }

    public class VM_Areas
    {
        public int Id { get; set; }
        public string Nombre_area { get; set; }
        public int Id_hospital { get; set; }
        public string Hospital { get; set; }
        public List<SelectListItem> Hospitales { get; set; } = new List<SelectListItem>();
    }

}