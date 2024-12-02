using FluentValidation;
using LocalFriendzApi.Application.IServices;
using LocalFriendzApi.Commom.Api;
using LocalFriendzApi.Core.Configuration;
using LocalFriendzApi.Core.Requests.Contact;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace LocalFriendzApi.Endpoints
{
    public static class ContactEndpoint
    {
        public static void MapEndpoints(this WebApplication app)
        {
            var contactGroup = app.MapGroup("/Contact")
                                  .WithTags("Contact");

            #region Endpoints

            contactGroup.MapPost("api/create-contact", Create)
            .WithName("CreateContact")
            .WithSummary("Create a new contact record.")
            .WithDescription("Creates and saves a new contact in the system. This endpoint requires valid contact details including name, email, and phone number. Returns the created contact information upon successful save.")
            .Produces((int)HttpStatusCode.Created)
            .Produces((int)HttpStatusCode.BadRequest)
            .Produces((int)HttpStatusCode.InternalServerError)
            .WithOpenApi();

            contactGroup.MapGet("api/list-all", ListAll)
            .WithName("ListAllContact")
            .WithSummary("Retrieve all contact records.")
            .WithDescription("Fetches and returns a list of all contact records stored in the system. This endpoint does not require any parameters and provides a comprehensive list of all available contacts.")
            .Produces((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.InternalServerError)
            .WithOpenApi();

            contactGroup.MapGet("api/list-by-filter", ListByFilter)
            .WithName("ListByFilterContact")
            .WithSummary("Retrieve a contact record by filter.")
            .WithDescription("Fetches a contact record based on the specified filter criteria, such as name, phone, DDD, or email. This endpoint requires valid filter parameters to return the corresponding contact details.")
            .Produces((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.InternalServerError)
            .WithOpenApi();

             contactGroup.MapGet("api/get-by-id", GetById)
            .WithName("GetByIdContact")
            .WithSummary("Retrieve a contact record by id.")
            .WithDescription("Fetches a contact record based on id to return the corresponding contact details.")
            .Produces((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.InternalServerError)
            .WithOpenApi();

            contactGroup.MapPut("api/update-contact", UpdateContact)
            .WithName("UpdateContact")
            .WithSummary("Update an existing contact record.")
            .WithDescription("Updates the details of an existing contact in the system. This endpoint requires the contact's unique identifier and the new information to be updated. If the contact exists, it will be updated with the provided details.")
            .Produces((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.BadRequest)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.InternalServerError)
            .WithOpenApi();

            contactGroup.MapDelete("api/delete-contact", DeleteContact)
            .WithName("DeleteContact")
            .WithSummary("Remove an existing contact record.")
            .WithDescription("Deletes a specific contact from the system based on the provided identifier. This endpoint requires the unique identifier of the contact to be deleted. If the contact exists, it will be removed from the system.")
            .Produces((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.NotFound)
            .Produces((int)HttpStatusCode.InternalServerError)
            .WithOpenApi();

            contactGroup.MapPost("api/create-random-contacts", CreateRandomContacts)
            .WithName("CreateRandomContacts")
            .WithSummary("Create 100 random contacts.")
            .WithDescription("Generates and saves 100 random contacts in the system.")
            .Produces((int)HttpStatusCode.OK)
            .Produces((int)HttpStatusCode.InternalServerError)
            .WithOpenApi();

            #endregion
        }

        /// <summary>
        /// Gerar uma carga de contatos.
        /// </summary>
        /// <param name="contactServices">Contato service</param>
        /// <returns></returns>
        static async Task<IResult> CreateRandomContacts([FromServices] IContactServices contactServices)
        {
            var json = await File.ReadAllTextAsync("config.json", cancellationToken: new CancellationToken());

            // Carregar as configurações do arquivo JSON
            var config = JsonSerializer.Deserialize<RabbitMQConfig>(json);

            // Configurar o ConnectionFactory
            var factory = new ConnectionFactory()
            {
                HostName = config.HostName,
                UserName = config.UserName,
                Password = config.Password,
            };

            // Criar conexão assíncrona
            await using var connection = await factory.CreateConnectionAsync(cancellationToken: new CancellationToken());
            await using var channel = await connection.CreateChannelAsync();

            // Declarar a fila (caso ela não exista)
            await channel.QueueDeclareAsync(
                queue: config.QueueName,
                durable: config.Durable,
                exclusive: config.Exclusive,
                autoDelete: config.AutoDelete,
            arguments: null,
                cancellationToken: new CancellationToken()
            );

            var randomContacts = contactServices.ContactGenerator(100);
            
            var message = JsonSerializer.Serialize(randomContacts);

            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(exchange: "", routingKey: config.QueueName, basicProperties: null, body: body, cancellationToken: new CancellationToken());

            foreach (var contact in randomContacts)
            {
                await contactServices.CreateAsync(new CreateContactRequest
                {
                    Name = contact.Name,
                    Phone = contact.Phone,

                    DDD = contact.DDD,
                    Email = contact.Email,
                });
            }

            return Results.Ok("100 random contacts created successfully.");
        }

        /// <summary>
        /// Deletar contato.
        /// </summary>
        /// <param name="contactServices">Contato service</param>
        /// <param name="id"></param>
        /// <returns></returns>
        static async Task<IResult> DeleteContact([FromServices] IContactServices contactServices, Guid id)
        {
            var response = await contactServices.DeleteContact(id);
            return response.ConfigureResponseStatus();
        }

        /// <summary>
        /// Atualizar contato.
        /// </summary>
        /// <param name="contactServices">Contato service</param>
        /// <param name="id">Id do contato</param>
        /// <param name="request">Modelo do request</param>
        /// <returns></returns>
        static async Task<IResult> UpdateContact([FromServices] IContactServices contactServices,
                                                 Guid id,
                                                 [FromBody] UpdateContactRequest request)
        {
            var validationResult = request.Validator();

            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var response = await contactServices.PutContact(id, request);
            return response.ConfigureResponseStatus();
        }

        /// <summary>
        /// Criar contato.
        /// </summary>
        /// <param name="contactServices">Contato service</param>
        /// <param name="validator">Validador service</param>
        /// <param name="request">Create request</param>
        /// <returns></returns>
        static async Task<IResult> Create([FromServices] IContactServices contactServices,
                                          [FromServices] IValidator<CreateContactRequest> validator,
                                          [FromBody] CreateContactRequest request)
        {
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
                return Results.ValidationProblem(validationResult.ToDictionary());

            var response = await contactServices.CreateAsync(request);

            return response.ConfigureResponseStatus();
        }

        /// <summary>
        /// Listar todos os contatos.
        /// </summary>
        /// <param name="contactServices">Contato service</param>
        /// <returns></returns>
        static async Task<IResult> ListAll([FromServices] IContactServices contactServices)
        {
            GetAllContactRequest request = new();
            var response = await contactServices.GetAll(request);
            return response.ConfigureResponseStatus();
        }

        /// <summary>
        /// Listar os contatos de acordo com o filtro.
        /// </summary>
        /// <param name="contactServices">Contato service</param>
        /// <param name="request">Filtro request</param>
        /// <returns></returns>
        static async Task<IResult> ListByFilter([FromServices] IContactServices contactServices, [AsParameters] GetByParamsRequest request)
        {
            var validationResult = request.Validator();

            if (!validationResult.IsValid)
                return Results.ValidationProblem(validationResult.ToDictionary());

            var response = await contactServices.GetByFilter(request);

            return response.ConfigureResponseStatus();
        }

        /// <summary>
        /// Obtém o contato de acordo com o id.
        /// </summary>
        /// <param name="contactServices">Contato service</param>
        /// <param name="request">Filtro request</param>
        /// <returns></returns>
        static async Task<IResult> GetById([FromServices] IContactServices contactServices, Guid id)
        {
            var response = await contactServices.GetById(id);

            return response.ConfigureResponseStatus();
        }
    }
}
