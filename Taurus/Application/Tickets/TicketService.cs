using System.Net;
using PegasusApi.Abstractions.Tickets;
using Taurus.Application.PegasusApi;
using PegasusUpdateTicketRequest = PegasusApi.Abstractions.Tickets.UpdateTicketRequest;
using PegasusCreateTicketRequest = PegasusApi.Abstractions.Tickets.CreateTicketRequest;

namespace Taurus.Application.Tickets;

public interface ITicketService
{
    Task<IReadOnlyList<Ticket>> GetTicketsAsync(Guid? projectId = null);
    Task<ApplicationResult<TicketDetails>> GetTicketByRefAsync(string ticketRef);
    Task<ApplicationResult<TicketDetails>> CreateTicketAsync(CreateTicketRequest request, Guid userId);
    Task<ApplicationResult> UpdateTicketAsync(UpdateTicketRequest request, Guid userId);
}

public sealed class TicketService(HttpClient httpClient, ILogger<TicketService> logger) : ITicketService
{
    public async Task<IReadOnlyList<Ticket>> GetTicketsAsync(Guid? projectId = null)
    {
        logger.LogInformation("Retrieving tickets from PegasusApi for project {ProjectId}", projectId);

        try
        {
            var requestUri = projectId.HasValue
                ? $"api/tickets?ProjectId={Uri.EscapeDataString(projectId.Value.ToString())}"
                : "api/tickets";

            var response = await httpClient.GetFromJsonAsync<TicketsResponse>(requestUri);
            if (response is null)
            {
                throw new InvalidOperationException("PegasusApi returned an empty tickets response.");
            }

            var tickets = response.Items
                .Select(MapTicket)
                .ToArray();

            logger.LogInformation("Retrieved {TicketCount} tickets from PegasusApi for project {ProjectId}", tickets.Length, projectId);
            return tickets;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to retrieve tickets from PegasusApi for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<ApplicationResult<TicketDetails>> GetTicketByRefAsync(string ticketRef)
    {
        logger.LogInformation("Retrieving ticket {TicketRef} from PegasusApi", ticketRef);

        try
        {
            var escapedTicketRef = Uri.EscapeDataString(ticketRef);

            using var response = await httpClient.GetAsync($"api/tickets/by_ref/{escapedTicketRef}");

            if (response.IsSuccessStatusCode)
            {
                var ticketResponse = await response.Content.ReadFromJsonAsync<TicketResponse>();
                if (ticketResponse is null)
                {
                    throw new InvalidOperationException("PegasusApi returned an empty ticket response.");
                }

                var ticket = MapTicketDetails(ticketResponse);

                logger.LogInformation("Retrieved ticket {TicketRef} with id {TicketId} from PegasusApi", ticket.TicketRef, ticket.Id);

                return ApplicationResult<TicketDetails>.Success(ticket);
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogWarning("PegasusApi could not find ticket {TicketRef}", ticketRef);
                return ApplicationResult<TicketDetails>.Failure($"Ticket '{ticketRef}' could not be found.");
            }

            response.EnsureSuccessStatusCode();

            throw new InvalidOperationException("PegasusApi ticket retrieval failed unexpectedly.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to retrieve ticket {TicketRef} from PegasusApi", ticketRef);
            throw;
        }
    }

    public async Task<ApplicationResult<TicketDetails>> CreateTicketAsync(CreateTicketRequest request, Guid userId)
    {
        logger.LogInformation("Creating ticket in PegasusApi for project {ProjectId}", request.ProjectId);

        try
        {
            var apiRequest = new PegasusCreateTicketRequest
            {
                Title = request.Title,
                Description = request.Description,
                ProjectId = request.ProjectId,
                StatusId = request.StatusId,
                TypeId = request.TypeId,
                PriorityId = request.PriorityId,
                UserId = userId
            };

            using var response = await httpClient.PostAsJsonAsync("api/tickets", apiRequest);

            if (response.StatusCode == HttpStatusCode.Created)
            {
                var ticketResponse = await response.Content.ReadFromJsonAsync<TicketResponse>();
                if (ticketResponse is null)
                {
                    throw new InvalidOperationException(
                        "PegasusApi returned an empty ticket response after ticket creation.");
                }

                var ticket = MapTicketDetails(ticketResponse);

                logger.LogInformation("Created ticket {TicketRef} with id {TicketId} in PegasusApi", ticket.TicketRef, ticket.Id);

                return ApplicationResult<TicketDetails>.Success(ticket);
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorMessage = await PegasusApiFailureReader.ReadAsync(
                    response,
                    "The ticket could not be created because PegasusApi rejected the supplied details.");

                logger.LogWarning(
                    "PegasusApi rejected ticket creation for project {ProjectId} with status code {StatusCode}",
                    request.ProjectId,
                    (int)response.StatusCode);

                return ApplicationResult<TicketDetails>.Failure(errorMessage);
            }

            response.EnsureSuccessStatusCode();

            throw new InvalidOperationException("PegasusApi ticket creation failed unexpectedly.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to create ticket in PegasusApi for project {ProjectId}", request.ProjectId);
            throw;
        }
    }
    
    public async Task<ApplicationResult> UpdateTicketAsync(UpdateTicketRequest request, Guid userId)
    {
        logger.LogInformation("Updating ticket {TicketId} in PegasusApi", request.Id);

        try
        {
            var apiRequest = new PegasusUpdateTicketRequest
            {
                Title = request.Title,
                Description = request.Description,
                ProjectId = request.ProjectId,
                StatusId = request.StatusId,
                TypeId = request.TypeId,
                PriorityId = request.PriorityId,
                FixedInRelease = request.FixedInRelease,
                ParentTicketId = request.ParentTicketId,
                AssignedTo = request.AssignedTo,
                UserId = userId
            };

            using var response = await httpClient.PutAsJsonAsync($"api/tickets/{request.Id}", apiRequest);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Updated ticket {TicketId} in PegasusApi", request.Id);
                return ApplicationResult.Success();
            }

            if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound)
            {
                var errorMessage = await PegasusApiFailureReader.ReadAsync(response,
                    "The ticket could not be updated because PegasusApi rejected the supplied details.");

                logger.LogWarning("PegasusApi rejected update of ticket {TicketId} with status code {StatusCode}", 
                    request.Id,
                    (int)response.StatusCode);

                return ApplicationResult.Failure(errorMessage);
            }

            response.EnsureSuccessStatusCode();

            throw new InvalidOperationException("PegasusApi ticket update failed unexpectedly.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to update ticket {TicketId} in PegasusApi", request.Id);

            throw;
        }
    }

    private static Ticket MapTicket(TicketResponse ticket)
    {
        return new Ticket(
            ticket.Id,
            ticket.TicketRef,
            ticket.Title,
            ticket.StatusId,
            ticket.TypeId,
            ticket.PriorityId,
            ticket.LastModified);
    }

    private static TicketDetails MapTicketDetails(TicketResponse ticket)
    {
        return new TicketDetails(
            ticket.Id,
            ticket.TicketRef,
            ticket.Title,
            ticket.Description,
            ticket.ProjectId,
            ticket.StatusId,
            ticket.TypeId,
            ticket.PriorityId,
            ticket.FixedInRelease,
            ticket.ParentTicketId,
            ticket.AssignedTo);
    }
}