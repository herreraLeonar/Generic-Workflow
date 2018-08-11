using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowNet.Models;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;

namespace WorkflowNet
{
    public class WorkflowControl
    {
        private WorkflowNetEntities db;
        private List<Task> trackAsync = new List<Task>();
        public WorkflowControl()
        {
            db = new WorkflowNetEntities();
        }
        public int IniciarWorkflow(int idWorkflow)
        {
            try
            {
                workflow workflowCreate = db.workflow.Where(x => x.id == idWorkflow).FirstOrDefault();

                if (workflowCreate == null)
                    throw new IndexOutOfRangeException("El workflow " + idWorkflow + " no existe en el sistema");

                estados estado = db.estados.FirstOrDefault(x => x.id_workflow == idWorkflow && x.estado_inicial == true);

                if (estado == null)
                    throw new IndexOutOfRangeException("El estado inicial no se encuentra configurado para el workflow " + workflowCreate.nombre);

                var change = db.proceso_workflow.Add(new proceso_workflow());
                change.id_workflow = workflowCreate.id;
                change.id_estado_actual = estado.id;
                db.SaveChanges();
                trackAsync.Add(Task.Run(() => TrackAsync("resultado", "IniciarWorkflow", change.id.ToString(), "id:" + change.id + ",estadoInicial:" + estado.id + ",nombreWorkflow:" + workflowCreate.nombre)));
                return change.id;
            }
            catch (Exception ex)
            {
                trackAsync.Add(Task.Run(() => TrackAsync("excepcion", "IniciarWorkflow", ex.ToString(), "")));
                throw ex;
            }
        }
        public List<proceso_workflow> GetProcesosWorkflow()
        {
            try
            {
                List<proceso_workflow> lista = db.proceso_workflow.ToList();
                trackAsync.Add(Task.Run(() => TrackAsync("resultado", "GetProcesosWorkflow", lista.ToString(), "")));
                if (lista.Count() == 0)
                    throw new ArgumentOutOfRangeException("no existen workflows");
                trackAsync.Add(Task.Run(() => TrackAsync("resultado", "GetProcesosWorkflow", lista.ToString(), "")));
                Task.WaitAll(trackAsync.ToArray());
                return lista;
            }
            catch (Exception ex)
            {
                trackAsync.Add(Task.Run(() => TrackAsync("excepcion", "GetProcesosWorkflow", ex.ToString(), "")));
                Task.WaitAll(trackAsync.ToArray());
                return null;
            }
        }
        public List<workflow> GetWorkflows()
        {
            try
            {
                List<workflow> wfs = db.workflow.Where(x => x.estado == true).ToList();
                if (wfs.Count() == 0)
                    throw new ArgumentOutOfRangeException("no existen workflows");
                trackAsync.Add(Task.Run(() => TrackAsync("resultado", "GetWorkflows", wfs.ToString(), "")));
                return wfs;
            }
            catch (Exception ex)
            {
                trackAsync.Add(Task.Run(() => TrackAsync("excepcion", "GetWorkflows", ex.ToString(), "")));
                return null;
            }
        }
        public string SiguienteEstado(int idProcesoWorkflow, string parametro)
        {
            // Resumen:
            // la funcion devuelve un string con el estado actual despues del cambio del proceso seleccionado
            // la funcion puede devolver una excepcion
            try
            {
                int idSiguienteEstado = 0;
                var idWorkflow = db.proceso_workflow.Find(idProcesoWorkflow).id_workflow;
                var idEstadoAnterior = db.proceso_workflow.Find(idProcesoWorkflow).id_estado_actual;

                var isParametrizable = db.estado_dependencia.FirstOrDefault(x => x.id_estado_desde == idEstadoAnterior && x.id_workflow == idWorkflow).parametrizable;
                if (isParametrizable == true)
                {
                    if (string.IsNullOrEmpty(parametro))
                        throw new FormatException("no se ha seleccionado un parametro para el siguiente estado");
                    else
                        idSiguienteEstado = this.getParametroSecuenciaEstado(idWorkflow, idEstadoAnterior, parametro).id_estado_siguiente;
                }
                else
                {
                    idSiguienteEstado = db.estado_dependencia.FirstOrDefault(x => x.id_workflow == idWorkflow && x.id_estado_desde == idEstadoAnterior).id_estado_hasta.Value;
                }

                var procesoWorkflow = db.proceso_workflow.Find(idProcesoWorkflow);

                procesoWorkflow.id_estado_actual = idSiguienteEstado;

                var result = db.SaveChanges();
                trackAsync.Add(Task.Run(() => TrackAsync("resultado", "SiguienteEstado",result.ToString(), "id_proceso_workflow:" + idProcesoWorkflow + ",id_estado_anterior:" + idEstadoAnterior + ",id_siguiente_estado:" + idSiguienteEstado + "parametro_recibido:" + parametro)));

                EjecutarTareas(procesoWorkflow.id);
                
                return db.estados.Find(procesoWorkflow.id_estado_actual).nombre;
            }
            catch (Exception ex)
            {
                trackAsync.Add(Task.Run(() => TrackAsync("excepcion","SiguienteEstado",ex.ToString(),"idProcesoWorkflow:" + idProcesoWorkflow + ",parametro:" + parametro )));
                throw ex;
            }
        }
        public List<parametro_secuencia_estado> getParametrosToSiguienteEstado(int idProcesoWorkflow)
        {
            try
            {
                var idWorkflow = db.proceso_workflow.Find(idProcesoWorkflow).id_workflow;
                var idEstadoActual = db.proceso_workflow.Find(idProcesoWorkflow).id_estado_actual;
                var idEstadoDependencia = db.estado_dependencia.FirstOrDefault(x => x.id_workflow == idWorkflow && x.id_estado_desde == idEstadoActual).id;
                var idParametro = db.parametros_estado_dependencia.FirstOrDefault(x => x.id_estado_dependencia == idEstadoDependencia).id_parametro;
                List<parametro_secuencia_estado> lista = db.parametro_secuencia_estado.Where(x => x.id_parametro == idParametro).ToList();
                trackAsync.Add(Task.Run(() => TrackAsync("resultado", "getParametrosToSiguienteEstado",lista.ToString(), "idProcesoWorkflow:" + idProcesoWorkflow )));
                return lista;
            }
            catch (Exception ex)
            {
                trackAsync.Add(Task.Run(() => TrackAsync("excepcion", "getParametrosToSiguienteEstado", ex.ToString(), "idProcesoWorkflow:" + idProcesoWorkflow )));
                return null;
            }
        }
        public string getDescripcionParametrosSiguienteEstado(int idProcesoWorkflow)
        {
            var idWorkflow = db.proceso_workflow.Find(idProcesoWorkflow).id_workflow;
            var idEstadoActual = db.proceso_workflow.Find(idProcesoWorkflow).id_estado_actual;
            try
            {
                var idEstadoDependencia = db.estado_dependencia.FirstOrDefault(x => x.id_workflow == idWorkflow && x.id_estado_desde == idEstadoActual).id;
                var idParametro = db.parametros_estado_dependencia.FirstOrDefault(x => x.id_estado_dependencia == idEstadoDependencia).id_parametro;
                string descripcion = db.parametros.Find(int.Parse(idParametro.ToString())).descripcion;
                trackAsync.Add(Task.Run(() => TrackAsync("resultado", "getDescripcionParametrosSiguienteEstado",descripcion ,"idProcesoWorkflow:" + idProcesoWorkflow)) );
                return descripcion;
            }
            catch (Exception ex)
            {
                trackAsync.Add(Task.Run(() => TrackAsync("excepcion", "getDescripcionParametrosSiguienteEstado", ex.ToString(), "idProcesoWorkflow:" + idProcesoWorkflow )));
                return "";
            }
        }
        public string getNombreEstadoToParametro(int idProcesoWorkflow, string parametro)
        {
            try
            {
                var idWorkflow = db.proceso_workflow.Find(idProcesoWorkflow).id_workflow;
                var idEstadoActual = db.proceso_workflow.Find(idProcesoWorkflow).id_estado_actual;
                string nombre = "";
                if (!string.IsNullOrEmpty(parametro))
                {
                    var idSiguienteEstado = this.getParametroSecuenciaEstado(idWorkflow, idEstadoActual, parametro).id_estado_siguiente;
                    nombre = db.estados.Find(idSiguienteEstado).nombre;
                }
                else
                {
                    var idSiguienteEstado = db.estado_dependencia.FirstOrDefault(x => x.id_workflow == idWorkflow && x.id_estado_desde == idEstadoActual).id_estado_hasta;
                    estados estadoSiguiente = db.estados.Find(idSiguienteEstado);

                    if (estadoSiguiente == null)
                        throw new Exception("El estado siguiente no fue encontrado: Id " + idSiguienteEstado);

                    nombre = estadoSiguiente.nombre;
                }
                trackAsync.Add(Task.Run(() => TrackAsync("resultado", "getNombreEstadoToParametro", nombre,"idProcesoWorkflow:" + idProcesoWorkflow + ",parametro:" + parametro)));
                return nombre;
            }
            catch (Exception ex)
            {
                trackAsync.Add(Task.Run(() => TrackAsync("excepcion", "getNombreEstadoToParametro", ex.ToString(), "idProcesoWorkflow:" + idProcesoWorkflow + ",parametro:" + parametro)));
                return "";
            }
        }
        public string getNombreEstadoActual(int idProcesoWorkflow)
        {
            try
            {
                var procesoWorkflow = db.proceso_workflow.Find(idProcesoWorkflow);
                string nombre = db.estados.Find(procesoWorkflow.id_estado_actual).nombre;
                trackAsync.Add(Task.Run(() => TrackAsync("resultado", "getNombreEstadoActual", nombre, "idProcesoWorkflow:" + idProcesoWorkflow)) );
                return nombre;
            }
            catch (Exception ex)
            {
                trackAsync.Add(Task.Run(() => TrackAsync("excepcion", "getNombreEstadoActual",ex.ToString(), "idProcesoWorkflow:" + idProcesoWorkflow )));
                return "";
            }
        }
        private parametro_secuencia_estado getParametroSecuenciaEstado(int? idWorkflow, int? idEstadoActual, string parametro)
        {
            var idEstadoDependencia = db.estado_dependencia.FirstOrDefault(x => x.id_workflow == idWorkflow && x.id_estado_desde == idEstadoActual).id;
            var idParametroEstadoDependencia = db.parametros_estado_dependencia.FirstOrDefault(x => x.id_estado_dependencia == idEstadoDependencia).id;
            var idParametro = db.parametros_estado_dependencia.FirstOrDefault(x => x.id_estado_dependencia == idEstadoDependencia).id_parametro;
            parametro_secuencia_estado lista = db.parametro_secuencia_estado.SingleOrDefault(x => x.id_parametro == idParametro && x.valor_parametro == parametro);
            trackAsync.Add(Task.Run(() => TrackAsync("resultado", "getParametroSecuenciaEstado",lista.ToString(),"idWorkflow: " + idWorkflow + ",idEstadoActual:" + idEstadoActual + ",parametro:" + parametro )));
            return lista;
        }
        private void EjecutarTareas(int idProcesoWorkflow)
        {
            //List<Object> tareasSegundoPlano = null;
            proceso_workflow procesoWorkflow = db.proceso_workflow.Where(x => x.id == idProcesoWorkflow).FirstOrDefault();
            var tareas = db.tareas_estado.Where(x => x.id_estado == procesoWorkflow.id_estado_actual).ToList();
            trackAsync.Add(Task.Run(() => TrackAsync("resultado", "EjecutarTareas", tareas.ToString(), "idProcesoWorkflow:" + idProcesoWorkflow)));
            //lista de tareas asincronas
            List<Task> tasksAsync = new List<Task>();
            foreach (var item in tareas)
            {
                if (item.es_asincrona == true)
                {
                    //Task.Run tareas en cola
                    var ResultTaskAsync = Task.Run( () => EjecutarTareaAsync(item.id, idProcesoWorkflow) );
                    tasksAsync.Add(ResultTaskAsync);
                }
                else
                {
                    Object resultTask = EjecutarTarea(item.id, idProcesoWorkflow);
                    // TODO: guardar resultado de tarea sincrona tipo object
                }
            }
        }
        public async Task EjecutarTareaAsync(int idTarea, int idProcesoWorkflow)
        {
            var dbe = new WorkflowNetEntities();
            var item = dbe.tareas_estado.Find(idTarea);
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    string metodo = item.metodo;
                    string rutaDll = item.ruta_dll;
                    string parametros = item.parametros;
                    string stringType = item.nombre_libreria + "." + item.nombre_clase;

                    Assembly AssemblyClase;
                    object result = null;
                    if (!File.Exists(rutaDll))
                        throw new FileNotFoundException("el archivo dll no existe");

                    AssemblyClase = Assembly.LoadFile(rutaDll);

                    Type type = Type.GetType(stringType);
                    if (string.IsNullOrEmpty(type.ToString()))
                        throw new FileLoadException("la variable " + item.nombre_libreria + " de la clase " + item.nombre_clase + " es nula");

                    ConstructorInfo magicConstructor = type.GetConstructor(Type.EmptyTypes);
                    object magicClassObject = magicConstructor.Invoke(new object[] { });

                    MethodInfo magicMethod = type.GetMethod(metodo);
                    if (magicMethod == null)
                        throw new InvalidOperationException("el metodo no existe");

                    ParameterInfo[] parameters = magicMethod.GetParameters();
                    /////////////////validacion para metodos con parametros
                    if (parameters.Length == 0)
                    {
                        result = magicMethod.Invoke(magicClassObject, null);
                    }
                    else
                    {
                        var parametersArray = new object[] { parametros.Split(';') };
                        result = magicMethod.Invoke(magicClassObject, parametersArray);
                    }
                    trackAsync.Add(Task.Run(() => TrackAsync("resultado", "EjecutarTareaAsync",result.ToString(),"idProcesoWorkflow:" + idProcesoWorkflow + ",estado:" + item.id_estado + ",metodo:" + metodo + ",stringType:" + stringType + ",rutaDll:" + rutaDll + ",parametros:" + parametros)));
                    return result;
                }
                catch (Exception ex)
                {
                    trackAsync.Add(Task.Run(() => TrackAsync("excepcion", "EjecutarTareaAsync", ex.ToString(), "idProcesoWorkflow:" + idProcesoWorkflow + ",idTarea:" + idTarea )));
                    throw ex;
                }
            });
        }
        public Object EjecutarTarea(int idTarea, int idProcesoWorkflow)
        {
            var item = db.tareas_estado.Find(idTarea);
            try
            {
                string metodo = item.metodo;
                string rutaDll = item.ruta_dll;
                string parametros = item.parametros;
                string stringType = item.nombre_libreria + "." + item.nombre_clase;

                Assembly AssemblyClase;
                object result = null;
                if (!File.Exists(rutaDll))
                    throw new FileNotFoundException("el archivo dll no existe");

                AssemblyClase = Assembly.LoadFile(rutaDll);

                Type type = Type.GetType(stringType);
                if (string.IsNullOrEmpty(type.ToString()))
                    throw new FileLoadException("la variable " + item.nombre_libreria + " de la clase " + item.nombre_clase + " es nula");

                ConstructorInfo magicConstructor = type.GetConstructor(Type.EmptyTypes);
                object magicClassObject = magicConstructor.Invoke(new object[] { });

                MethodInfo magicMethod = type.GetMethod(metodo);
                if (magicMethod == null)
                    throw new InvalidOperationException("el metodo no existe");

                ParameterInfo[] parameters = magicMethod.GetParameters();
                /////////////////validacion para metodos con parametros
                if (parameters.Length == 0)
                {
                    result = magicMethod.Invoke(magicClassObject, null);
                }
                else
                {
                    var parametersArray = new object[] { parametros.Split(';') };
                    result = magicMethod.Invoke(magicClassObject, parametersArray);
                }
                trackAsync.Add(Task.Run(() => TrackAsync("resultado", "EjecutarTarea",result.ToString(), "idProcesoWorkflow:" + idProcesoWorkflow + ",estado:" + item.id_estado + ",metodo:" + metodo + ",stringType:" + stringType + ",rutaDll:" + rutaDll + ",parametros:" + parametros)));
                return result;
            }
            catch (Exception ex)
            {
                trackAsync.Add(Task.Run(() => TrackAsync("excepcion", "EjecutarTarea", ex.ToString(), "idProcesoWorkflow:" + idProcesoWorkflow + ",idTarea:" + idTarea)));
                throw ex;
            }
        }
        private async Task TrackAsync(string tipo, string metodo, string resultado, string descripcion)
        {
            await Task.Factory.StartNew(() =>
            {
                var dbe = new WorkflowNetEntities();
                tracks track = dbe.tracks.Add(new tracks());
                track.tipo_respuesta = tipo;
                track.metodo = metodo;
                track.respuesta = resultado;
                track.descripcion = descripcion;
                track.fecha = DateTime.Now;
                dbe.SaveChanges();
            });
        }
        public int PruebaTareas(string[] args)
        {
            //Resumen: Inicia un proceso workflow
            return this.IniciarWorkflow(int.Parse(args[0]));
        }
    }
}
