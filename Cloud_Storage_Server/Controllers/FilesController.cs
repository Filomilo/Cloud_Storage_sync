using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
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
            _FileSyncService = fileSyncService;
        }

        [Route("list")]
        [HttpGet]
        public IActionResult listOfFiles()
        {
            User user = UserRepository.getUserByMail(
                JwtHelpers.GetEmailFromToken(Request.Headers.Authorization)
            );
            List<SyncFileData> files = FileRepository.GetAllUserFiles(user.id);
            return Ok(files);
        }

        [Route("edit")]
        [HttpPost]
        public IActionResult edit([FromBody] FileData file)
        {
            return Ok();
        }

        FileUploudRequest santizeFileUploudRequest([NotNull] FileUploudRequest fileUploudRequest)
        {
            fileUploudRequest.fileData.Extenstion =
                fileUploudRequest.fileData.Extenstion == null
                    ? ""
                    : fileUploudRequest.fileData.Extenstion;
            return fileUploudRequest;
        }

        [Route("upload")]
        [HttpPost]
        public async Task<IActionResult> UploadFile([FromForm] FileUploudRequest filerequest)
        {
            try
            {
                if (filerequest == null)
                {
                    return BadRequest("Request cannot be null");
                }
                filerequest = santizeFileUploudRequest(filerequest);
                User user = UserRepository.getUserByMail(
                    JwtHelpers.GetEmailFromToken(Request.Headers.Authorization)
                );
                string deviceId = JwtHelpers.GetDeviceIDFromAuthString(
                    Request.Headers.Authorization
                );

                using (Stream stream = filerequest.file.OpenReadStream())
                {
                    _FileSyncService.AddNewFile(user, deviceId, filerequest.fileData, stream);
                    return Ok("Succsesfully added file");
                }
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest("Couldn't Autheticate user");
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
                JwtHelpers.GetEmailFromToken(Request.Headers.Authorization)
            );
            SyncFileData fileData = FileRepository.GetFileOfID(guid);

            Stream data = _FileSyncService.DownloadFile(user, fileData);

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
        public IActionResult update([FromBody] UploudFileData file)
        {
            return BadRequest("no itmpelmetned");
        }
    }
}
