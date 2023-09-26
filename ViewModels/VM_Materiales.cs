using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sistema_Fallas_IMSS.ViewModels
{
    public class VM_Materiales
    {
        public List<VM_MaterialesCatalogo> Materiales { get; set; } = new List<VM_MaterialesCatalogo>();

    }

    public class VM_MaterialesCatalogo
    {
        public int Id_material { get; set; }
        public string Nombre { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public List<SelectListItem> Tipos { get; set; }
        public string Tipo_hardware { get; set; }
        public string Centro_costos { get; set; }
        public string Proyecto { get; set; }
        public List<SelectListItem> Estados = new List<SelectListItem>();
        public string Estado { get; set; }
        public string Comentarios { get; set; }
    }

    public class VM_Existencias
    {
        public int Id_existencia { get; set; }
        public string Area { get; set; }
        public List<SelectListItem> Materiales { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Areas { get; set; } = new List<SelectListItem>();
        public string Material { get; set; }
        public string Pc { get; set; }
        public string Cuenta { get; set; }
        public string Nombre_persona { get; set; }
        public string Direccion_ip { get; set; }
        public string Serial { get; set; }
        public string Nsm { get; set; }
        public string Nnn { get; set; }
        public int Tipo_existencia { get; set; }
        public bool Tipo { get; set; }
    }

    public class VM_TipoHardware
    {
        public int Id_tipo_hardware { get; set; }
        public string Tipo_producto { get; set; }
    }
}