using System;
using Microsoft.Xrm.Sdk;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Account
{
    /// <summary>
    /// Crie um plug-in assíncrono, registrado na tabela Criar Mensagem da conta (ou seja, entidade aqui)
    /// este plug-in criará uma atividade de tarefa que lembrará o criador(proprietário)
    /// da conta para acompanhar uma semana depois.
    /// </summary>
    public class FallowupAccount : IPlugin
    {
        //public FallowupAccount(string unsecure, string secure)
        //{

        //}
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtenha o serviço de rastreamento.
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtenha o contexto de execução do provedor de serviços.  
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // A coleção InputParameters contém todos os dados passados ​​na solicitação de mensagem.  
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity account)
            {
                // Obtenha a referência de serviço da organização que você precisará para
                // chamadas de serviço da web.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    tracingService.Trace(account.Id.ToString());

                    Entity followup = new Entity("task");
                    followup["subject"] = "Send e-mail to the new customer.";
                    followup["description"] =
                        "Follow up with the customer. Check if there are any new issues that need resolution.";
                    followup["scheduledstart"] = DateTime.Now.AddDays(7);
                    followup["scheduledend"] = DateTime.Now.AddDays(7);
                    followup["category"] = context.PrimaryEntityName;

                    // Consulte a conta na atividade de tarefa.
                    if (context.OutputParameters.Contains("id"))
                    {
                        Guid regardingobjectid = new Guid(context.OutputParameters["id"].ToString());
                        string regardingobjectidType = "account";

                        followup["regardingobjectid"] =
                        new EntityReference(regardingobjectidType, regardingobjectid);

                        // Crie a tarefa no Microsoft Dynamics CRM.
                        tracingService.Trace($"{this.GetType().Name}: Creating the task activity.");
                        service.Create(followup);
                    }

                }
                catch (Exception ex)
                {
                    tracingService.Trace($"{this.GetType().Name} Error: {0}", ex.ToString());
                    
                }

            }
        }

        //private int CoutContact( Guid accountId, ITracingService tracing) {
        //    tracing.Trace("logged");
        //    var myInt = 0;
        //    return myInt;
        //}
    }
}
