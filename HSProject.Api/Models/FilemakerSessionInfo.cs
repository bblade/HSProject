namespace HSProject.Api.Models; 
public class FilemakerSessionInfo {
    public ResponseInfo? Response { get; set; }
    public class ResponseInfo {
        public string? Token { get; set; }
    }
}
