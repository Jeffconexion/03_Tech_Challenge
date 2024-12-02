using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace LocalFriendzApi.ProcessamentoLote.Consumer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var json = await File.ReadAllTextAsync("config.json", stoppingToken);

            // Carregar as configura��es do arquivo JSON
            var config = JsonSerializer.Deserialize<RabbitMQConfig>(json);

            // Configurar o ConnectionFactory
            var factory = new ConnectionFactory()
            {
                HostName = config.HostName,
                UserName = config.UserName,
                Password = config.Password,
            };

            // Criar conex�o ass�ncrona
            await using var connection = await factory.CreateConnectionAsync(stoppingToken);
            await using var channel = await connection.CreateChannelAsync();

            // Declarar a fila (caso ela n�o exista)
            await channel.QueueDeclareAsync(
                queue: config.QueueName,
                durable: config.Durable,
                exclusive: config.Exclusive,
                autoDelete: config.AutoDelete,
                arguments: null,
                cancellationToken: stoppingToken
            );

            // Criar o consumidor ass�ncrono
            var consumer = new AsyncEventingBasicConsumer(channel);

            // Assinar o evento ReceivedAsync
            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                try
                {
                    var body = eventArgs.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    // Processar a mensagem recebida
                    Console.WriteLine($"Mensagem recebida: {message}");

                    // Simular um processamento longo (se necess�rio)
                    await Task.Delay(500, stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao processar mensagem: {ex.Message}");
                }
            };

            // Consumir mensagens
            await channel.BasicConsumeAsync(
                queue: config.QueueName,
                autoAck: true,
                consumer: consumer,
                cancellationToken: stoppingToken
            );

            _logger.LogInformation("Worker iniciado e aguardando mensagens...");

            try
            {
                // Aguardar o cancelamento
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Worker cancelado.");
            }
        }
    }
}
