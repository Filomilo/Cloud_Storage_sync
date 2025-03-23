using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Server.Database.Models;

namespace Cloud_Storage_Desktop_lib.Services
{
    public class FileMangamentSerivce
    {

        public static MultipartFormDataContent GetFormDatForFile(FileData data, byte[] bytes)
        {
            var form = new MultipartFormDataContent();
            //form.Add(new StringContent(rollNumber), "rollNumber");
            //form.Add(new StringContent(name), "name");
            //form.Add(new StringContent(age), "agebytes
            var fileContent = new ByteArrayContent(bytes);
            //pdfContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
            form.Add(fileContent, "file");
            //-F 'fileData.Path=/' \
            //-F 'fileData.Name=string' \
            //-F 'fileData.Extenstion=string' \
            //-F 'fileData.Hash=string' \
            //-F 'fileData.SyncDate=2025-03-23T12:02:33.763Z' \
            form.Add(new StringContent(data.Path), "fileData.Path");
            form.Add(new StringContent(data.Name), "fileData.Name");
            form.Add(new StringContent(data.Extenstion), "fileData.Extenstion");
            form.Add(new StringContent(data.Hash), "fileData.Hash");
            form.Add(new StringContent(data.SyncDate.ToString()), "fileData.SyncDate");
            return form;
        }
    }
}
