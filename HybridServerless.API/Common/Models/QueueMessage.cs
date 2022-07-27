namespace HybridServerless.API.Common.Models;

public record Message(
    string Version,
    string SalesOrderNumber,
    string Date
);

public record MessageInfo(
    string Version,
    string Type
);

public record QueueMessage(
    MessageInfo MessageInfo,
    Message Message
);
