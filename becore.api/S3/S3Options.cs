namespace becore.api.S3;

public record S3Options
{
    public required string AccessKey { get; set; }
    public required string SecretKey { get; set; }
    public required string ServiceUrl { get; set; }
    public required string BucketName { get; set; }
}