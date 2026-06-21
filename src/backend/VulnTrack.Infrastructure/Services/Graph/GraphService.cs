using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;
using VulnTrack.Application.Common.Interfaces;
using VulnTrack.Infrastructure.Settings;

namespace VulnTrack.Infrastructure.Services.Graph;

internal sealed record GraphUserDto(string Id, string? DisplayName, string? Mail, string? JobTitle) : IGraphUserDto;

internal sealed class GraphService(GraphServiceClient graphClient, IOptions<GraphSettings> graphSettings) : IGraphService
{
    private readonly string _senderEmail = graphSettings.Value.SenderEmail;

    public async Task<IGraphUserDto?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await graphClient.Users[userId].GetAsync(cancellationToken: cancellationToken);
        if (user is null) return null;

        return new GraphUserDto(user.Id!, user.DisplayName, user.Mail, user.JobTitle);
    }

    public async Task<IReadOnlyList<IGraphUserDto>> SearchUsersAsync(string query, CancellationToken cancellationToken = default)
    {
        // $search with ConsistencyLevel:eventual is the correct Graph pattern for user typeahead.
        // $filter with 'or' across multiple properties requires the same header but is less flexible.
        var safe = query.Replace("\"", "");
        var result = await graphClient.Users.GetAsync(req =>
        {
            req.QueryParameters.Search = $"\"displayName:{safe}\" OR \"mail:{safe}\" OR \"userPrincipalName:{safe}\"";
            req.QueryParameters.Top = 20;
            req.QueryParameters.Select = ["id", "displayName", "mail", "userPrincipalName", "jobTitle"];
            req.Headers.Add("ConsistencyLevel", "eventual");
        }, cancellationToken);

        return result?.Value?
            .Select(u => (IGraphUserDto)new GraphUserDto(u.Id!, u.DisplayName, u.Mail ?? u.UserPrincipalName, u.JobTitle))
            .ToList() ?? [];
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        var message = new Message
        {
            Subject = subject,
            Body = new ItemBody { ContentType = BodyType.Html, Content = body },
            ToRecipients =
            [
                new Recipient { EmailAddress = new EmailAddress { Address = toEmail } }
            ]
        };

        await graphClient.Users[_senderEmail].SendMail.PostAsync(
            new SendMailPostRequestBody { Message = message, SaveToSentItems = false },
            cancellationToken: cancellationToken);
    }
}
