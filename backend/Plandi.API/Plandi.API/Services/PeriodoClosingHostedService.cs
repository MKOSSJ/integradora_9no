using Plandi.Services.Interfaces;

namespace Plandi.API.Services;

public sealed class PeriodoClosingHostedService(IServiceScopeFactory scopeFactory, ILogger<PeriodoClosingHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await CerrarVencidosAsync(stoppingToken);
        using var timer = new PeriodicTimer(TimeSpan.FromHours(1));
        while (await timer.WaitForNextTickAsync(stoppingToken)) await CerrarVencidosAsync(stoppingToken);
    }

    private async Task CerrarVencidosAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var lifecycle = scope.ServiceProvider.GetRequiredService<IPeriodoLifecycleService>();
            var cambios = await lifecycle.ActualizarEstadosAsync(cancellationToken);
            if (cambios > 0) logger.LogInformation("Se actualizaron automáticamente {Cantidad} periodos académicos.", cambios);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception exception)
        {
            logger.LogError(exception, "No fue posible actualizar automáticamente los periodos académicos.");
        }
    }
}
