using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Server.Database.Models;
using Cloud_Storage_Server.Database.Repositories;
using Cloud_Storage_Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cloud_Storage_Server.Controllers
{
    public class FileUploudRequest
    {
        public UploudFileData fileData { get; set; }

        [Required]
        public IFormFile file { get; set; }
    }

    [Route("api/[controller]")]
    //[Authorize]
    public class FilesController : Controller
    {
        private readonly IFileSyncService _FileSyncService;

        public FilesController(IFileSyncService fileSyncService)
        {
            _FileSyncService =
                fileSyncService ?? throw new ArgumentNullException(nameof(fileSyncService));
        }

        [Route("list")]
        [HttpGet]
        public IActionResult listOfFiles()
        {
            User user = UserRepository.getUserByMail(
                AuthService.GetEmailFromToken(Request.Headers.Authorization)
            );
            List<FileData> files = FileRepository.GetAllUserFiles(user.id);
            return Ok(files);
        }

        [Route("edit")]
        [HttpPost]
        public IActionResult edit([FromBody] FileData file)
        {
            return Ok();
        }

        [Route("upload")]
        [HttpPost]
        public async Task<IActionResult> UploadFile([FromForm] FileUploudRequest filerequest)
        {
            try
            {
                User user = UserRepository.getUserByMail(
                    AuthService.GetEmailFromToken(Request.Headers.Authorization)
                );
                using (var memoryStream = new MemoryStream())
                {
                    await filerequest.file.CopyToAsync(memoryStream);

                    byte[] content = memoryStream.ToArray();

                    _FileSyncService.AddNewFile(user, filerequest.fileData, content);
                    return Ok("Succsesfully added file");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("delete")]
        [HttpDelete]
        public IActionResult edit([FromBody] Guid guid)
        {
            return Ok();
        }

        [Route("download")]
        [Authorize]
        [HttpGet]
        public IActionResult DownlaodFile([FromQuery] Guid guid)
        {
            User user = UserRepository.getUserByMail(
                AuthService.GetEmailFromToken(Request.Headers.Authorization)
            );
            FileData fileData = FileRepository.GetFileOfID(guid);

            byte[] data = _FileSyncService.DownloadFile(user, fileData);

            return File(data, "application/octet-stream", fileData.Name);
        }

        /// <summary>
        /// Used after downlaod file to update verison
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [Route("update")]
        [Authorize]
        [HttpPost]
        public IActionResult update([FromBody] FileData file)
        {
            return BadRequest("no itmpelmetned");
        }
    }
}
