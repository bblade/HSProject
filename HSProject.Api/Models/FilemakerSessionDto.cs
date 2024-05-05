namespace HSProject.Api.Models; 
public class FilemakerSessionDto {
    public ResponseInfo? Response { get; set; }
    public class ResponseInfo {
        public string? Token { get; set; }
    }
}
