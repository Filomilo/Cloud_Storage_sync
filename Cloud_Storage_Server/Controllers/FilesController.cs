using Cloud_Storage_Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Cloud_Storage_Server.Controllers
{

    public class FileUploudRequest
    {
        public UploudFileData fileData { get;set; }
        [Required]
        public IFormFile file {get;set;}
    }

    [Route("api/[controller]")]
    //[Authorize]
    public class FilesController : Controller
    {
        [Route("list")]
        [HttpGet]
      public IActionResult listOfFiles()
        {
            // tepmorary
            List<FileData> files=new List<FileData>();
            files.Add(new FileData
            {
                Path = "asdasd",
                Name = "123",
                Extenstion = "png",
                Hash = new byte[] { },
                SyncDate = new DateTime(),
                OwnerId = 1
            });
            files.Add(new FileData
            {
                Path = "asdasd",
                Name = "123",
                Extenstion = "png",
                Hash =  new byte[]{},
                SyncDate = new DateTime(),
                OwnerId = 1
            });

            files.Add(new FileData
            {
                Path = "asdasd",
                Name = "123",
                Extenstion = "png",
                Hash = new byte[] { },
                SyncDate = new DateTime(),
                OwnerId = 1
            });
            files.Add(new FileData
            {
                Path = "asdasd",
                Name = "123",
                Extenstion = "png",
                Hash = new byte[] { },
                SyncDate = new DateTime(),
                OwnerId = 1
            });

            return Ok(files);
        }

        [Route("edit")]
        [HttpPost]
        public IActionResult edit([FromBody] FileData file)
        {
            return Ok();
        }
        [Route("uploud")]
        [HttpPost]
        public async Task<IActionResult> UploudFile(
            [FromForm] FileUploudRequest filerequest
            )
        {
            using (var sr = new StreamReader(filerequest.file.OpenReadStream()))
            {
                var content = sr.ReadToEnd();
                return Ok(content);
            }
            return Ok();
        }

        [Route("delete")]
        [HttpDelete]
        public IActionResult edit([FromBody] Guid guid)
        {
            return Ok();
        }

    }
}
