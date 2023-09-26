using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sistema_Fallas_IMSS.ViewModels
{
    public class VM_Hospital
    {
        public string Direccion_ip {  get; set; }
        public List<VM_Hospitales> Hospitales { get; set;}
    }
    public class VM_Hospitales
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Director { get; set; }
        public string Direccion { get; set; }
        public string Municipio { get; set; }
        public string Estado { get; set; }
    }
}