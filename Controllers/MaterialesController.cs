using Sistema_Fallas_IMSS.Models;
using Sistema_Fallas_IMSS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Sistema_Fallas_IMSS.Controllers
{
    public class MaterialesController : Controller
    {
        // GET: Materiales
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MaterialesGrid()
        {
            using(var context = new IMSSEntities())
            {
                List<VM_MaterialesCatalogo> materiales = (from material in context.materiales
                                                    join tipo in context.tipo_hardware on material.id_tipo_hardware equals tipo.Id_tipo_hardware
                                                    join estados in context.material_estados on material.id_estado equals estados.Id_estado
                                                    select new VM_MaterialesCatalogo
                                                    {
                                                        Id_material = material.Id_material,
                                                        Nombre = material.nombre,
                                                        Marca = material.marca,
                                                        Modelo= material.modelo,
                                                        Tipo_hardware = tipo.tipo_producto,
                                                        Centro_costos = material.centro_costos,
                                                        Proyecto = material.nombre_proyecto,
                                                        Estado = estados.nombre,
                                                        Comentarios = material.comentarios,
                                                    }).ToList();

                return PartialView("_MaterialesGrid",materiales);
            }
        }

        public ActionResult ExistenciaGrid()
        {
            using(var context = new IMSSEntities())
            {
                List<VM_Existencias> existencias = (from existencia in context.existencias
                                                    join material in context.materiales
                                                    on existencia.id_material equals material.Id_material
                                                    join areas in context.areas_imss
                                                    on existencia.id_area equals areas.Id_area
                                                    select new VM_Existencias
                                                    {
                                                        Id_existencia = existencia.Id_existencia,
                                                        Material = material.nombre + " " + material.marca + " " + material.modelo,
                                                        Pc = existencia.pc,
                                                        Cuenta = existencia.cuenta,
                                                        Nombre_persona = existencia.nombre_persona,
                                                        Nsm = existencia.nsm,
                                                        Nnn = existencia.nnn,
                                                        Direccion_ip = existencia.direccion_ip,
                                                        Serial = existencia.serial,
                                                        Area = areas.nombre_area,
                                                        Tipo_existencia = existencia.tipo_existencia,
                                                    }).ToList();

                return PartialView("_ExistenciasGrid", existencias);
            }    
        }

        public ActionResult TiposHardwareGrid()
        {
            using(var context = new IMSSEntities())
            {
                List<VM_TipoHardware> tipos = (from tipo in context.tipo_hardware
                                               select new VM_TipoHardware
                                               {
                                                   Id_tipo_hardware = tipo.Id_tipo_hardware,
                                                   Tipo_producto = tipo.tipo_producto,
                                               }).ToList();

                return PartialView("_TiposHardwareGrid", tipos);
            }
        }

        [HttpPost]
        public ActionResult AbrirModalMaterial(int _id_material)
        {
            using (var context = new IMSSEntities())
            {
                VM_MaterialesCatalogo data = new VM_MaterialesCatalogo();
                data.Tipos = (from tipos in context.tipo_hardware
                              select new SelectListItem
                              {
                                  Value = tipos.Id_tipo_hardware.ToString(),
                                  Text = tipos.tipo_producto,
                              }).ToList();
                data.Estados = (from estados in context.material_estados
                                select new SelectListItem
                                {
                                    Value = estados.Id_estado.ToString(),
                                    Text = estados.nombre,
                                }).ToList();

                if (_id_material > 0)
                {
                    var material = context.materiales.Find(_id_material);

                    data.Id_material = material.Id_material;
                    data.Nombre = material.nombre;
                    data.Marca = material.marca;
                    data.Modelo = material.modelo;
                    data.Centro_costos = material.centro_costos;
                    data.Proyecto = material.nombre_proyecto;
                    data.Tipo_hardware = material.id_tipo_hardware.ToString();
                    data.Estado = material.id_estado.ToString();
                    data.Comentarios = material.comentarios;


                    return PartialView("_ModalMaterial", data);
                }
                data.Estado = "1";
                return PartialView("_ModalMaterial", data);
            }
        }

        [HttpPost]
        public ActionResult AbrirModalExistencias(int _id_existencia)
        {
            using (var context = new IMSSEntities())
            {
                VM_Existencias data = new VM_Existencias();

                if (_id_existencia > 0)
                {
                    data = (from existencia in context.existencias
                            join material in context.materiales
                            on existencia.id_material equals material.Id_material
                            join areas in context.areas_imss
                            on existencia.id_area equals areas.Id_area
                            where existencia.Id_existencia == _id_existencia
                            select new VM_Existencias
                            {
                                Id_existencia = existencia.Id_existencia,
                                Material = material.Id_material.ToString(),
                                Pc = existencia.pc,
                                Area = areas.Id_area.ToString(),
                                Cuenta = existencia.cuenta,
                                Nombre_persona = existencia.nombre_persona,
                                Nsm = existencia.nsm,
                                Nnn = existencia.nnn,
                                Direccion_ip = existencia.direccion_ip,
                                Serial = existencia.serial,
                                Tipo = existencia.tipo_existencia == 1,
                                Tipo_existencia = existencia.tipo_existencia,
                            }).FirstOrDefault();
                }



                data.Materiales = context.materiales.Select(model => new SelectListItem
                {
                    Value = model.Id_material.ToString(),
                    Text = model.nombre + " " + model.marca + " " + model.modelo,
                }).ToList();
                data.Areas = context.areas_imss.Select(model => new SelectListItem
                    {
                        Value = model.Id_area.ToString(),
                        Text = model.nombre_area,
                    }).ToList();
       
              return PartialView("_ModalExistencia", data);
            }
        }

        [HttpPost]
        public ActionResult AbrirModalTipo(int _id_tipo)
        {
            using(var context = new IMSSEntities())
            {
                VM_TipoHardware data = new VM_TipoHardware();
                if (_id_tipo > 0)
                {
                    var tipo = context.tipo_hardware.Find(_id_tipo);
                    data.Id_tipo_hardware = _id_tipo;
                    data.Tipo_producto = tipo.tipo_producto;

                    return PartialView("_ModalTipo", data);
                }
                return PartialView("_ModalTipo", data);
            }
        }
        [HttpPost]
        public int RegistrarEditarExistencia(VM_Existencias _existencia)
        {
            using (var context = new IMSSEntities())
            {
                using(var transaction = context.Database.BeginTransaction())
                {
                    try
                    {

                        if (_existencia.Id_existencia > 0)
                        {
                            var existencia = context.existencias.Find(_existencia.Id_existencia);
                            existencia.id_material = Convert.ToInt32(_existencia.Material);
                            existencia.pc = _existencia.Pc;
                            existencia.cuenta = _existencia.Cuenta;
                            existencia.direccion_ip = _existencia.Direccion_ip;
                            existencia.id_area = Convert.ToInt32(_existencia.Area);
                            existencia.nombre_persona = _existencia.Nombre_persona;
                            existencia.serial = _existencia.Serial;
                            existencia.tipo_existencia = _existencia.Tipo_existencia;
                            existencia.nsm = _existencia.Nsm;
                            existencia.nnn = _existencia.Nnn;

                        }
                        else
                        {
                            existencias nueva_existencia = new existencias
                            {
                                id_material = Convert.ToInt32(_existencia.Material),
                                pc = _existencia.Pc,
                                cuenta = _existencia.Cuenta,
                                direccion_ip = _existencia.Direccion_ip,
                                id_area = Convert.ToInt32(_existencia.Area),
                                nombre_persona = _existencia.Nombre_persona,
                                serial = _existencia.Serial,
                                tipo_existencia = _existencia.Tipo_existencia,
                                nsm = _existencia.Nsm,
                                nnn = _existencia.Nnn,
                        };
                            context.existencias.Add(nueva_existencia);
                        }

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

        [HttpPost]
        public int RegistrarEditarMaterial(VM_MaterialesCatalogo _material)
        {
            using(var context = new IMSSEntities())
            {
                try
                {
                    if (_material.Id_material > 0)
                    {
                        var material = context.materiales.Find(_material.Id_material);
                        material.nombre = _material.Nombre;
                        material.marca = _material.Marca;
                        material.modelo = _material.Modelo;
                        material.centro_costos = _material.Centro_costos;
                        material.nombre_proyecto = _material.Proyecto;
                        material.id_tipo_hardware = Convert.ToInt32(_material.Tipo_hardware);
                        material.id_estado = Convert.ToInt32(_material.Estado);
                        material.comentarios = _material.Comentarios;

                    }
                    else
                    {
                        materiales nuevo_material = new materiales
                        {
                            nombre = _material.Nombre,
                            marca = _material.Marca,
                            modelo = _material.Modelo,
                            centro_costos = _material.Centro_costos,
                            nombre_proyecto = _material.Proyecto,
                            id_tipo_hardware = Convert.ToInt32(_material.Tipo_hardware),
                            id_estado = Convert.ToInt32(_material.Estado),
                            comentarios = _material.Comentarios,
                        };
                        context.materiales.Add(nuevo_material);
                    }
                    context.SaveChanges();
                    return 1;
                }
                catch (Exception)
                {
                    return 0;
                    throw;
                }
            }
        }

        [HttpPost]
        public int RegistrarEditarTipo(VM_TipoHardware _tipo)
        {
            using (var context = new IMSSEntities())
            {
                try
                {
                    if (_tipo.Id_tipo_hardware > 0)
                    {
                        var tipo = context.tipo_hardware.Find(_tipo.Id_tipo_hardware);
                        tipo.tipo_producto = _tipo.Tipo_producto;
                    }
                    else
                    {
                        tipo_hardware nievo_tipo = new tipo_hardware
                        {
                            tipo_producto = _tipo.Tipo_producto,
                        };
                        context.tipo_hardware.Add(nievo_tipo);
                    }
                    context.SaveChanges();
                    return 1;
                }
                catch (Exception)
                {
                    return 0;
                    throw;
                }
            }
        }

    }
}