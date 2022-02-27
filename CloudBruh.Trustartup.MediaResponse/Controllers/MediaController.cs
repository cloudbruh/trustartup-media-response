using CloudBruh.Trustartup.MediaResponse.Models;
using CloudBruh.Trustartup.MediaResponse.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudBruh.Trustartup.MediaResponse.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MediaController : ControllerBase
{
    private readonly MediaService _mediaService;

    public MediaController(MediaService mediaService)
    {
        _mediaService = mediaService;
    }

    [HttpGet("download/{filename}")]
    public async Task<IActionResult> DownloadMedia(string filename)
    {
        MediaRawDto? dto = await _mediaService.GetMediumByFileNameAsync(filename);

        if (dto == null)
        {
            return NotFound();
        }

        Stream? stream = await _mediaService.DownloadMedium(dto.Id);
        if (stream == null)
        {
            return NotFound();
        }

        return File(stream, dto.MimeType);
    }

    [Authorize]
    [HttpPost("upload")]
    public async Task<IActionResult> UploadMedia(IFormFile file, bool isPublic = true)
    {
        if (!long.TryParse(User.Claims.FirstOrDefault(claim => claim.Type == "uid")?.Value, out long userId))
        {
            return BadRequest("Invalid uid in jwt token.");
        }

        var media = new MediaRawDto
        {
            UserId = userId,
            IsPublic = isPublic,
            MimeType = file.ContentType
        };

        media = await _mediaService.PostMedium(media);
        if (media == null)
        {
            throw new HttpRequestException("Could not create media object");
        }

        media = await _mediaService.UploadMedium(media.Id, file);
        
        if (media?.Link == null)
        {
            throw new HttpRequestException("Could not upload file");
        }

        return CreatedAtAction("DownloadMedia", new {filename = media.Link}, media);
    }
}