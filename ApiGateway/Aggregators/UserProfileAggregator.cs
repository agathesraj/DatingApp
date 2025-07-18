using Ocelot.Middleware;
using Ocelot.Multiplexer;
using System.Net;

namespace ApiGateway.Aggregators
{
    public class UserProfileAggregator : IDefinedAggregator
    {
        public async Task<DownstreamResponse> Aggregate(List<HttpContext> responses)
        {
            var accountResponse = responses.FirstOrDefault()?.Items["DownstreamResponse"] as DownstreamResponse;
            var userResponse = responses.LastOrDefault()?.Items["DownstreamResponse"] as DownstreamResponse;

            if (accountResponse == null || userResponse == null)
            {
                return new DownstreamResponse(
                    new StringContent("Unable to aggregate user profile data"),
                    HttpStatusCode.BadRequest,
                    new List<KeyValuePair<string, IEnumerable<string>>>(),
                    "Bad Request"
                );
            }

            var accountContent = await accountResponse.Content.ReadAsStringAsync();
            var userContent = await userResponse.Content.ReadAsStringAsync();

            var aggregatedContent = $@"{{
                ""account"": {accountContent},
                ""user"": {userContent},
                ""aggregatedAt"": ""{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}""
            }}";

            var headers = new List<KeyValuePair<string, IEnumerable<string>>>
            {
                new("Content-Type", new[] { "application/json" })
            };

            return new DownstreamResponse(
                new StringContent(aggregatedContent),
                HttpStatusCode.OK,
                headers,
                "OK"
            );
        }
    }
}