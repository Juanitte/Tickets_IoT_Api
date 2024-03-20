namespace Handlers
{
    public class NoEncodingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.AcceptEncoding.Clear();
            request.Headers.Add("GTWSensify", "0000");
            //do stuff and optionally call the base handler..
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
