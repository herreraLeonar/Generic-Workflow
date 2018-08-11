using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using TaskNet.Models;
using System.Reflection;
using System.CodeDom.Compiler;
using System.ServiceModel.Description;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace TaskNet
{
    public partial class TaskNet : ServiceBase
    {
        static workflowServiceEntities db;
        private int eventId;
        private bool enEjecucion;
        //private System.ComponentModel.IContainer components;
        private System.Diagnostics.EventLog eventLog1;
        private string mensajeIntervalo;
        private string mensajeDetenido;
        private string mensajeInicio;
        public TaskNet()
        {
            InitializeComponent();
            db = new workflowServiceEntities();
            eventLog1 = new System.Diagnostics.EventLog();
            service_config sc = db.service_config.FirstOrDefault();
            if ( !System.Diagnostics.EventLog.SourceExists(sc.service_log) )
            {
                System.Diagnostics.EventLog.CreateEventSource( sc.nombre_source, sc.service_log );
            }
            eventLog1.Source = sc.nombre_source;
            eventLog1.Log = sc.service_log;
            eventId = 1;
        }

        protected override void OnStart(string[] args)
        {
            workflowServiceEntities dbe = new workflowServiceEntities();
            enEjecucion = false;
            service_config sc = dbe.service_config.FirstOrDefault();
            mensajeIntervalo = sc.mensaje_intervalo;
            mensajeDetenido = sc.mensaje_detenido;
            mensajeInicio = sc.mensaje_inicio;
            Tracking(mensajeInicio);
            // configurado para dispararse cada intervalo_ms en milisegundos  
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = sc.intervalo_ms;
            
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            
            eventLog1.WriteEntry(sc.mensaje_inicio);

            timer.Start();
        }

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            if (!enEjecucion) {
                enEjecucion = true;
                db = new workflowServiceEntities();
                List<tarea> listaTareas = db.tareas.ToList();
                foreach (var tarea in listaTareas)
                {
                    TimeSpan tiempoEjecucion = tarea.tiempo_ejecucion;

                    DateTime tiempoActual = DateTime.Now;
                    DateTime ultimaEjecucion = tarea.ultima_ejecucion;
                    DateTime proximaEjecucion = ultimaEjecucion + tiempoEjecucion;

                    if (proximaEjecucion <= tiempoActual)
                    {
                        try
                        {
                            string resp = EjecutarServicio(tarea.url_servicio, tarea.metodo, tarea.parametros, tarea.nombre_contrato);

                            this.TareasEjecutadas(tarea.id, resp);

                            Tracking(mensajeIntervalo + eventId);

                            eventLog1.WriteEntry(mensajeIntervalo + " Nro# " + eventId, EventLogEntryType.Information, eventId++);
                        }
                        catch (Exception e)
                        {
                            Tracking("OnTime Error: " + e.Message + " Nro# " + eventId);

                            eventLog1.WriteEntry("OnTime Error: " + e.Message + " Nro# " + eventId, EventLogEntryType.Error, eventId++);
                        }
                    }
                }
                enEjecucion = false;
            }
        }

        private string EjecutarServicio(string url_servicio, string metodo, string parametros, string nombre_contrato)
        {
            try
            {
                //llamar servicio wcf sin agregar referencia y usando reflection
                CompilerResults compilerResults = null;

                object proxyInstance = GetProxyInstance(ref compilerResults, url_servicio, nombre_contrato);

                var methodInfo = proxyInstance.GetType().GetMethod(metodo);
                
                // crear una instancia para que podamos enviar la instancia al método de servicio al completar sus parámetros
                ParameterInfo[] paramInfos = methodInfo.GetParameters();

                // El parámetro de tipo Object, si es requerido por el método se agrega en parámetros
                object[] operationParameters = new object[] { parametros.Split(';') };

                //Invokes service method and get the result.
                var result = methodInfo.Invoke(proxyInstance, BindingFlags.InvokeMethod, null, operationParameters, null);

                // Obtener los valores de las propiedades de resultado y usarlos
                return result.ToString();
            }
            catch (Exception ex)
            {
                Tracking("EjecutarServicio Error: " + ex.Message + " Nro# " + eventId);

                eventLog1.WriteEntry("EjecutarServicio Error: " + ex.Message + " Nro# " + eventId, EventLogEntryType.Error, eventId);

                throw ex;
            }
        }

        private object GetProxyInstance(ref CompilerResults compilerResults, string url_servicio, string contractName)
        {
            try
            {
                // Definir el WSDL Obtener la dirección, el nombre del contrato y los parámetros, con esto podemos extraer los detalles del WSDL en cualquier momento
                object proxyInstance = null;

                // Para los puntos finales HttpGet use una dirección WSDL de servicio un mexMode de .HttpGet y para los puntos finales MEX use una dirección MEX y un mexMode de .MetadataExchange
                Uri address = new Uri(url_servicio);

                MetadataExchangeClientMode mexMode = MetadataExchangeClientMode.HttpGet;

                // Obtenga el archivo de metadatos del servicio.
                MetadataExchangeClient metadataExchangeClient = new MetadataExchangeClient(address, mexMode);

                metadataExchangeClient.ResolveMetadataReferences = true;

                // También se pueden proporcionar credenciales si el servicio lo necesita con ayuda de dos líneas.   ICredentials networkCredential = new NetworkCredential("", "", ""); metadataExchangeClient.HttpCredentials = networkCredential; 
                // Obtiene la información de metadatos del servicio.
                MetadataSet metadataSet = metadataExchangeClient.GetMetadata();

                // Importar todos los contratos y endpoints.
                WsdlImporter wsdlImporter = new WsdlImporter(metadataSet);

                // Importar todos los contratos.
                Collection<ContractDescription> contracts = wsdlImporter.ImportAllContracts();

                //importa todos los endpoints.
                ServiceEndpointCollection allEndpoints = wsdlImporter.ImportAllEndpoints();

                // Generar información de tipo para cada contrato.
                ServiceContractGenerator serviceContractGenerator = new ServiceContractGenerator();

                //Dictionary se ha definido para mantener todos los puntos finales del contrato presentes el nombre del contrato es la clave del elemento del diccionario.
                var endpointsForContracts = new Dictionary<string, IEnumerable<ServiceEndpoint>>();

                foreach (ContractDescription contract in contracts)
                {
                    serviceContractGenerator.GenerateServiceContractType(contract);

                    // lista de los EndPoints de cada contrato.
                    endpointsForContracts[contract.Name] = allEndpoints.Where(ep => ep.Contract.Name == contract.Name).ToList();
                }

                // Genera un archivo de código para los contratos.
                CodeGeneratorOptions codeGeneratorOptions = new CodeGeneratorOptions();
                codeGeneratorOptions.BracingStyle = "C";

                // Crea una instancia de compilación de un idioma específico.
                CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider("C#");

                // Agrega WCF-related assemblies references as copiler parameters, para hacer la compilación de un contrato de servicio particular.
                CompilerParameters compilerParameters = new CompilerParameters(new string[] { "System.dll", "System.ServiceModel.dll", "System.Runtime.Serialization.dll" });

                compilerParameters.GenerateInMemory = true;

                // Obtiene el ensamblado compilado.
                compilerResults = codeDomProvider.CompileAssemblyFromDom(compilerParameters, serviceContractGenerator.TargetCompileUnit);

                if (compilerResults.Errors.Count <= 0)
                {
                    // Encuentra el tipo de proxy que se generó para el contrato especificado.
                    Type proxyType = compilerResults.CompiledAssembly.GetTypes().First(t => t.IsClass && t.GetInterface(contractName) != null &&
                    t.GetInterface(typeof(ICommunicationObject).Name) != null);

                    // Ahora obtenemos el primer punto final de servicio para el contrato particular.
                    ServiceEndpoint serviceEndpoint = endpointsForContracts[contractName].First();

                    // Crea una instancia del proxy pasando el enlace y la dirección del endpoint como parámetros.
                    proxyInstance = compilerResults.CompiledAssembly.CreateInstance(proxyType.Name, false, System.Reflection.BindingFlags.CreateInstance, null,
                        new object[] { serviceEndpoint.Binding, serviceEndpoint.Address }, CultureInfo.CurrentCulture, null);
                }
                return proxyInstance;
            }
            catch(Exception ex)
            {
                Tracking("GetProxyInstance Error: " + ex.Message + " Nro# " + eventId);

                eventLog1.WriteEntry("GetProxyInstance Error: " + ex.Message + " Nro# " + eventId, EventLogEntryType.Error, eventId);

                throw ex;
            }
        }

        protected override void OnStop()
        {
            Tracking(mensajeDetenido);
            eventLog1.WriteEntry(mensajeDetenido);
        }

        private void InitializeComponent()
        {
            this.ServiceName = "ServiciosW3.TaskNet";
        }
        
        internal void TestStartupAndStop(string[] args)
        {
            this.OnStart(args);
            Console.ReadLine();
            this.OnStop();
        }

        private void Tracking(string descripcion)
        {
            try
            {
                bool debugService = bool.Parse(System.Configuration.ConfigurationManager.AppSettings["debugService"].ToString());

                if (debugService)
                {
                    workflowServiceEntities dbe = new workflowServiceEntities();
                    track tr = dbe.tracks.Add(new track());
                    tr.descripcion = descripcion;
                    tr.fecha = DateTime.Now;
                    dbe.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void TareasEjecutadas(int idTarea, string resp)
        {
            try
            {
                workflowServiceEntities dbe = new workflowServiceEntities();
                tareas_ejecutadas nuevaEjecucion = dbe.tareas_ejecutadas.Add(new tareas_ejecutadas());
                DateTime momento = DateTime.Now;
                nuevaEjecucion.momento_ejecucion = momento;
                nuevaEjecucion.id_tarea = idTarea;
                nuevaEjecucion.respuesta = resp;
                dbe.SaveChanges();

                dbe = new workflowServiceEntities();
                tarea updateTarea = dbe.tareas.Find(idTarea);
                updateTarea.ultima_ejecucion = momento;
                dbe.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
