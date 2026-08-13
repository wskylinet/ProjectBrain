using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectBrain.Api.Common;

namespace ProjectBrain.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/rdp-tools")]
public sealed class RdpToolsController : ControllerBase
{
    private static readonly IReadOnlyDictionary<string, ToolDefinition> Tools =
        new Dictionary<string, ToolDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["installer"] = new("ProjectBrainRdpInstaller.txt", "Install-ProjectBrainRdpProtocol.cmd"),
            ["uninstaller"] = new("ProjectBrainRdpUninstaller.txt", "Uninstall-ProjectBrainRdpProtocol.cmd")
        };

    private readonly IWebHostEnvironment _environment;

    public RdpToolsController(IWebHostEnvironment environment) => _environment = environment;

    [HttpGet("{name}/metadata")]
    public async Task<ActionResult<ApiResult<ToolMetadataDto>>> GetMetadata(string name)
    {
        if (!TryResolve(name, out var tool, out var path)) return NotFound(ApiResult.Fail("远程工具不存在", 404));
        var bytes = await System.IO.File.ReadAllBytesAsync(path, HttpContext.RequestAborted);
        return Ok(ApiResult<ToolMetadataDto>.Ok(new ToolMetadataDto
        {
            FileName = tool.DownloadName,
            Sha256 = Convert.ToHexString(SHA256.HashData(bytes)),
            Size = bytes.LongLength
        }));
    }

    [HttpGet("{name}/download")]
    public IActionResult Download(string name)
    {
        if (!TryResolve(name, out var tool, out var path)) return NotFound(ApiResult.Fail("远程工具不存在", 404));
        Response.Headers.CacheControl = "no-store";
        return PhysicalFile(path, "application/octet-stream", tool.DownloadName, enableRangeProcessing: false);
    }

    private bool TryResolve(string name, out ToolDefinition tool, out string path)
    {
        if (!Tools.TryGetValue(name, out tool!))
        {
            path = string.Empty;
            return false;
        }

        path = Path.Combine(_environment.WebRootPath, "tools", tool.SourceName);
        return System.IO.File.Exists(path);
    }

    private sealed record ToolDefinition(string SourceName, string DownloadName);
}

public sealed class ToolMetadataDto
{
    public string FileName { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;
    public long Size { get; set; }
}
