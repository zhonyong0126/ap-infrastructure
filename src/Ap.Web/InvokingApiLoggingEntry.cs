namespace Ap.Web {
    public class InvokingApiLoggingEntry {
        public string TraceId { get; }
        public string LoggedUser { get; }
        public string Url { get; }
        public string RequestBody { get; }
        public int StatusCode { get; }
        public string ResponseBody { get; }
        public long DurationInMs { get; }

        public InvokingApiLoggingEntry (string traceId, string loggedUser, string url, string requestBody, int statusCode, string responseBody, long durationInMs) {
            this.DurationInMs = durationInMs;
            this.ResponseBody = responseBody;
            this.StatusCode = statusCode;
            this.RequestBody = requestBody;
            this.Url = url;
            this.TraceId = traceId;
            this.LoggedUser = loggedUser;
        }
    }
}