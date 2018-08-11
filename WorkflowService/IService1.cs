using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace WorkflowService
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de interfaz "IService1" en el código y en el archivo de configuración a la vez.
    [ServiceContract(Name ="Service1")]
    public interface IService1
    {
        [OperationContract]
        string IniciarWorkflow1(string[] args);
        [OperationContract]
        string IniciarWorkflow2(string[] args);
    }
}
