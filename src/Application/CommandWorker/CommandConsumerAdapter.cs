using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FoodMesh.Application.CommandWorker;

/// <summary>
/// Adapter that receives incoming command messages from message brokers (e.g. RabbitMQ, MassTransit)
/// or background workers and dispatches them to their corresponding MediatR CommandHandler.
/// </summary>
/// <typeparam name="TCommand">The command type being listened for.</typeparam>
public class CommandConsumerAdapter<TCommand> : ICommandConsumer where TCommand : notnull
{
    public async Task ConsumeAsync(
        object commandMessage,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        if (commandMessage is not TCommand command)
            throw new ArgumentException($"Expected command of type '{typeof(TCommand).Name}', but received '{commandMessage.GetType().Name}'.");

        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(command, cancellationToken);
    }
}
