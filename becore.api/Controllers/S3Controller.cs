using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using becore.api.S3;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace becore.api.Controllers;

[ApiController]
[Route("api/s3")]
public class S3Controller(IOptions<S3Options> options, IAmazonS3 s3Client) : ControllerBase
{
    private readonly S3Options _options = options.Value;
    private readonly IAmazonS3 _s3Client = s3Client;

    [HttpPost("image/{id}")]
    public async Task<ActionResult<PutObjectResponse>> UploadImage([FromRoute] Guid id,
        [FromForm(Name = "Data")] IFormFile file)
    {
        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = id.ToString(),
            ContentType = file.ContentType,
            InputStream = file.OpenReadStream(),
        };
        var response = await _s3Client.PutObjectAsync(request);
        
        return response.HttpStatusCode == HttpStatusCode.OK ? Ok(response) : BadRequest();
    }
    
    [HttpGet("image/{id}")]
    public async Task<IActionResult> GetImage([FromRoute] Guid id)
    {
        var objectRequest = new GetObjectRequest
        {
            BucketName = _options.BucketName,
            Key = id.ToString()
        };

        var response = await _s3Client.GetObjectAsync(objectRequest);
        if (response.HttpStatusCode == HttpStatusCode.OK)
            return File(response.ResponseStream, response.Headers["Content-Type"]);

        return BadRequest();
    }

    [HttpDelete("image/{id}")]
    public async Task<IActionResult> DeleteImage([FromRoute] int id)
    {
        var objectRequest = new DeleteObjectRequest
        {
            BucketName = _options.BucketName,
            Key = $"{id}"
        };
        var response = await _s3Client.DeleteObjectAsync(objectRequest);
        return response.HttpStatusCode == HttpStatusCode.NoContent ? NoContent() : BadRequest();
    }
}