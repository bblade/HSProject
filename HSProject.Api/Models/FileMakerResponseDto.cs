namespace HSProject.Api.Models; 
public class FileMakerResponseDto {
    public class ResponseInfo {
        public string? ScriptResult { get; set; }
    }
    public ResponseInfo? Response { get; set; }
}
