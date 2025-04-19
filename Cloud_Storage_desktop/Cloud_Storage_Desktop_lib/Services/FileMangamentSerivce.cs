using Cloud_Storage_Common.Models;

namespace Cloud_Storage_Desktop_lib.Services
{
    public class FileMangamentSerivce
    {
        public static MultipartFormDataContent GetFormDatForFile(UploudFileData data, Stream stream)
        {
            var form = new MultipartFormDataContent();
            form.Add(new StreamContent(stream), "file", $"{data.Name}{data.Extenstion}");
            form.Add(new StringContent(data.Path), "fileData.Path");
            form.Add(new StringContent(data.Name), "fileData.Name");
            form.Add(new StringContent(data.Extenstion), "fileData.Extenstion");
            form.Add(new StringContent(data.Hash), "fileData.Hash");

            return form;
        }
    }
}
