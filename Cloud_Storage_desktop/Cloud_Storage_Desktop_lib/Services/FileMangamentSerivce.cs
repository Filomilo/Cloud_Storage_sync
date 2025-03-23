using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Storage_Common.Models;

namespace Cloud_Storage_Desktop_lib.Services
{
    public class FileMangamentSerivce
    {
        public static MultipartFormDataContent GetFormDatForFile(UploudFileData data, byte[] bytes)
        {
            var form = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(bytes);
            form.Add(fileContent, "file", $"{data.Name}{data.Extenstion}");
            form.Add(new StringContent(data.Path), "fileData.Path");
            form.Add(new StringContent(data.Name), "fileData.Name");
            form.Add(new StringContent(data.Extenstion), "fileData.Extenstion");
            form.Add(new ByteArrayContent(data.Hash), "fileData.Hash");
            form.Add(new StringContent(data.SyncDate.ToString()), "fileData.SyncDate");
            return form;
        }
    }
}
