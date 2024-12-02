namespace LocalFriendzApi.ProcessamentoLote.Consumer
{
    internal class RabbitMQConfig
    {
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string QueueName { get; set; }
        public bool Durable { get; set; }
        public bool Exclusive { get; set; }
        public bool AutoDelete { get; set; }
    }

}
