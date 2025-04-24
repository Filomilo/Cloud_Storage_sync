using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Cloud_Storage_Common;
using Cloud_Storage_Common.Models;
using Cloud_Storage_Common.Requests;
using Cloud_Storage_Server.Database.Models;
using Cloud_Storage_Server.Database.Repositories;
using Cloud_Storage_Server.Interfaces;
using Cloud_Storage_Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        private readonly IDataBaseContextGenerator _dataBaseContextGenerator;

        public FilesController(
            IFileSyncService fileSyncService,
            IDataBaseContextGenerator dataBaseContextGenerator
        )
        {
            _FileSyncService = fileSyncService;
            _dataBaseContextGenerator = dataBaseContextGenerator;
        }

        [Route("list")]
        [HttpGet]
        public IActionResult listOfFiles()
        {
            using (var context = _dataBaseContextGenerator.GetDbContext())
            {
                User user = UserRepository.getUserByMail(
                    context,
                    JwtHelpers.GetEmailFromToken(Request.Headers.Authorization)
                );
                List<SyncFileData> files = FileRepository.GetAllACtiveUserFiles(context, user.id);
                return Ok(files);
            }
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
                User user;
                using (var context = this._dataBaseContextGenerator.GetDbContext())
                {
                    user = UserRepository.getUserByMail(
                        context,
                        JwtHelpers.GetEmailFromToken(Request.Headers.Authorization)
                    );
                }

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
        public IActionResult delete([FromQuery] string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return BadRequest("relativePath cannot be null or empty.");
            }

            User user;
            using (var context = _dataBaseContextGenerator.GetDbContext())
            {
                user = UserRepository.getUserByMail(
                    context,
                    JwtHelpers.GetEmailFromToken(Request.Headers.Authorization)
                );
            }

            string deviceId = JwtHelpers.GetDeviceIDFromAuthString(Request.Headers.Authorization);
            FileData fileData = new FileData(relativePath);

            this._FileSyncService.RemoveFile(new FileData(relativePath), user.id, deviceId);

            return Ok();
        }

        [Route("download")]
        [Authorize]
        [HttpGet]
        public IActionResult DownlaodFile([FromQuery] Guid guid)
        {
            try
            {
                User user = null;
                SyncFileData fileData;
                using (var context = _dataBaseContextGenerator.GetDbContext())
                {
                    user = UserRepository.getUserByMail(
                        context,
                        JwtHelpers.GetEmailFromToken(Request.Headers.Authorization)
                    );
                    fileData = FileRepository.GetFileOfID(context, guid);
                }

                string deviceId = JwtHelpers.GetDeviceIDFromAuthString(
                    Request.Headers.Authorization
                );
                Stream data = _FileSyncService.DownloadFile(user, fileData);

                return File(data, "application/octet-stream", fileData.Name);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private void ValidateUpdateFileDataRequest(UpdateFileDataRequest fileUpdate)
        {
            if (fileUpdate.newFileData.BytesSize <= 0)
            {
                //throw new Exception("BytesSize shoudl be larger than zero");
            }
        }

        /// <summary>
        /// Used after downlaod file to update verison
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [Route("update")]
        [Authorize]
        [HttpPost]
        public IActionResult update([FromBody] UpdateFileDataRequest fileUpdate)
        {
            ValidateUpdateFileDataRequest(fileUpdate);
            String email = JwtHelpers.GetEmailFromToken(Request.Headers.Authorization);

            string deviceId = JwtHelpers.GetDeviceIDFromAuthString(Request.Headers.Authorization);
            _FileSyncService.UpdateFileForDevice(email, deviceId, fileUpdate);

            return Ok("updated");
        }

        [Route("setVersion")]
        [Authorize]
        [HttpPost]
        public IActionResult setVersion([FromBody] SetVersionRequest setVersionRequest)
        {
            String email = JwtHelpers.GetEmailFromToken(Request.Headers.Authorization);
            User user = null;
            string deviceId = JwtHelpers.GetDeviceIDFromAuthString(Request.Headers.Authorization);
            using (var context = _dataBaseContextGenerator.GetDbContext())
            {
                user = UserRepository.getUserByMail(
                    context,
                    JwtHelpers.GetEmailFromToken(Request.Headers.Authorization)
                );
            }
            _FileSyncService.SetFileVersion(
                user.id,
                setVersionRequest.FileId,
                setVersionRequest.Version
            );

            return Ok("updated");
        }
    }
}
