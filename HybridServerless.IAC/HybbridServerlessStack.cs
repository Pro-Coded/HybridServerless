using System.Reflection;
using Pulumi;
using Pulumi.Azure.AppService.Inputs;
using Pulumi.Azure.Core;
using AppInsights = Pulumi.Azure.AppInsights;
using AppService = Pulumi.Azure.AppService;
using OperationalInsights = Pulumi.Azure.OperationalInsights;
using ServiceBus = Pulumi.Azure.ServiceBus;
using Storage = Pulumi.Azure.Storage;

public class HybbridServerlessStack : Stack
{

    private const string ROOT_NAME = "pro-hybrid";
    private const string LOCATION = "WestEurope";

    public HybbridServerlessStack()
    {
        var resourceGroup = new ResourceGroup($"{ROOT_NAME}-rg", new ResourceGroupArgs { Location = LOCATION });

        var plan = new AppService.Plan($"{ROOT_NAME}-plan", new AppService.PlanArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Kind = "FunctionApp",
            Sku = new PlanSkuArgs
            {
                Tier = "Dynamic",
                Size = "Y1"
            }
        });

        var storageAccount = new Storage.Account("propulumifuncst", new Storage.AccountArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AccountReplicationType = "LRS",
            AccountTier = "Standard",
            AccountKind = "StorageV2"
        });

        var container = new Storage.Container("propulumifunccn", new Storage.ContainerArgs
        {
            StorageAccountName = storageAccount.Name,
            ContainerAccessType = "private"
        });

        var analyticsWorkspace = new OperationalInsights.AnalyticsWorkspace($"{ROOT_NAME}-aw", new OperationalInsights.AnalyticsWorkspaceArgs
        {
            Location = resourceGroup.Location,
            ResourceGroupName = resourceGroup.Name,
            Sku = "PerGB2018",
            RetentionInDays = 30,
        });

        var appInsights = new AppInsights.Insights($"{ROOT_NAME}-insights", new AppInsights.InsightsArgs
        {
            Location = resourceGroup.Location,
            ResourceGroupName = resourceGroup.Name,
            WorkspaceId = analyticsWorkspace.Id,
            ApplicationType = "web",
        });

        var serviceBusNamespace = new ServiceBus.Namespace($"{ROOT_NAME}-namespace", new ServiceBus.NamespaceArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Sku = "Basic"
        });
        var queue = new ServiceBus.Queue($"{ROOT_NAME}-queue", new ServiceBus.QueueArgs
        {
            NamespaceId = serviceBusNamespace.Id,
            MaxSizeInMegabytes = 1024,
            EnablePartitioning = false,
            DefaultMessageTtl = System.Xml.XmlConvert.ToString(TimeSpan.FromSeconds(30))
        });

        _ = Assembly.GetExecutingAssembly().Location;

        var functionAppPublishFolder = Path.Combine(@"C:\Code\pro-coded\pro-hybrid-serverless\HybridServerless.Functions\bin\release\net6.0\publish\");

        var blob = new Storage.Blob($"{ROOT_NAME}-blob", new Storage.BlobArgs
        {
            StorageAccountName = storageAccount.Name,
            StorageContainerName = container.Name,
            Source = new FileArchive(functionAppPublishFolder),
            Type = "Block"
        });

        var functionApp = new AppService.FunctionApp($"{ROOT_NAME}-app", new AppService.FunctionAppArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AppServicePlanId = plan.Id,
            StorageAccountName = storageAccount.Name,
            StorageAccountAccessKey = storageAccount.PrimaryAccessKey,
            Version = "~4",
            AppSettings = new InputMap<string>
            {
                { "WEBSITE_RUN_FROM_PACKAGE", Helpers.GetSignedBlobUrl(blob, storageAccount) },
                { "FUNCTIONS_EXTENSION_VERSION", "~4" },
                { "FUNCTIONS_WORKER_RUNTIME", "dotnet-isolated" },
                { "APPINSIGHTS_INSTRUMENTATIONKEY", appInsights.InstrumentationKey },
                { "ServiceBusConnectionString", serviceBusNamespace.DefaultPrimaryKey },
                { "MessageQueueName", queue.Name }
                
                //{ "SERVICEBUS_NAMESPACE", serviceBusNamespace.Name },
                //{ "SERVICEBUS_ACCESS_KEY", serviceBusNamespace.DefaultPrimaryKey },
                //{ "SERVICEBUS_QUEUE_NAME", queue.Name },
                //{ "SERVICEBUS_QUEUE_ENDPOINT", queue.PrimaryEndpoint },
                //{ "SERVICEBUS_QUEUE_ENDPOINT_SECURITY_POLICY", "Send" },
                //{ "SERVICEBUS_QUEUE_ENDPOINT_SECURITY_MODE", "Send" },
                //{ "SERVICEBUS_QUEUE_ENDPOINT_SECURITY_KEY", serviceBusNamespace.PrimaryConnectionString },
                //{ "SERVICEBUS_QUEUE_ENDPOINT_SECURITY_SCHEME", "ServiceBus" },
                //{ "SERVICEBUS_QUEUE_ENDPOINT_SECURITY_AUTHORIZATION_MODE", "Send" },
                //{ "SERVICEBUS_QUEUE_ENDPOINT_SECURITY_SHARED_ACCESS_POLICY", "Send" },
                //{ "SERVICEBUS_QUEUE_ENDPOINT_SECURITY_SHARED_ACCESS_POLICY_NAME", "Send" },
                //{ "SERVICEBUS_QUEUE_ENDPOINT_SECURITY_SHARED_ACCESS_POLICY_RIGHTS", "Send" },
                //{ "SERVICEBUS_QUEUE_ENDPOINT_SECURITY_SHARED_ACCESS_POLICY_SECURITY_KEY", serviceBusNamespace.PrimaryConnectionString },
                //{ "SERVICEBUS_QUEUE_ENDPOINT_SECURITY_SHARED_ACCESS_POLICY_SECURITY_SCHEME", "ServiceBus" },
                //{ "SERVICEBUS_QUEUE_ENDPOINT_SECURITY_SHARED_ACCESS_POLICY_SECURITY_AUTHORIZATION_MODE", "Send" },                
            },
            ConnectionStrings = new InputList<FunctionAppConnectionStringArgs>
            {
               new FunctionAppConnectionStringArgs{Name = "ServiceBusConnectionString", Type="ServiceBus", Value = serviceBusNamespace.DefaultPrimaryConnectionString}
            }
        });

        Url = Output.Format($"https://{functionApp.DefaultHostname}/api/Function1");

        QueueName = queue.Name;
    }

    [Output]
    public Output<string> Url { get; set; }

    [Output]
    public Output<string> QueueName { get; set; }
}