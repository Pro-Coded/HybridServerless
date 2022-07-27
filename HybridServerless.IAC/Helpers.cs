using Pulumi;
using Pulumi.Azure.Storage;

public static class Helpers
{
    public static Input<string> GetSignedBlobUrl(Blob blob, Account storageAccount)
    {
        const string signatureExpiration = "2100-01-01";

        Output<string>? url = Output.All(new[] { storageAccount.Name, storageAccount.PrimaryConnectionString, blob.StorageContainerName, blob.Name })
        .Apply(async (parameters) =>
        {
            var accountName = parameters[0];
            var connectionString = parameters[1];
            var containerName = parameters[2];
            var blobName = parameters[3];

            GetAccountBlobContainerSASResult? sas = await GetAccountBlobContainerSAS.InvokeAsync(new GetAccountBlobContainerSASArgs
            {
                ConnectionString = connectionString,
                ContainerName = containerName,
                Start = "2020-07-20",
                Expiry = signatureExpiration,
                Permissions = new Pulumi.Azure.Storage.Inputs.GetAccountBlobContainerSASPermissionsArgs
                {
                    Read = true,
                    Write = false,
                    Delete = false,
                    List = false,
                    Add = false,
                    Create = false
                }
            });
            return $"https://{accountName}.blob.core.windows.net/{containerName}/{blobName}{sas.Sas}";
        });

        return url;
    }
}
