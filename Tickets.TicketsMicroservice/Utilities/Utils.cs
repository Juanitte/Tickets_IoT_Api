namespace Tickets.TicketsMicroservice.Utilities
{
    public class Utils
    {
        /// <summary>
        ///     Guarda un archivo adjunto en el sistema de archivos
        /// </summary>
        /// <param name="attachment"><see cref="IFormFile"/> con los datos del archivo adjunto a guardar</param>
        /// <returns>la ruta del archivo guardado</returns>
        public static async Task<string> SaveAttachmentToFileSystem(IFormFile attachment, int ticketId)
        {
            var date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

            var fileName = Path.GetFileNameWithoutExtension(attachment.FileName) + "_" + date + Path.GetExtension(attachment.FileName);
            string directoryPath = Path.Combine("C:/ProyectoIoT/Back/ApiTest/AttachmentStorage/", ticketId.ToString());
            string filePath = Path.Combine(directoryPath, fileName);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await attachment.CopyToAsync(stream);
            }

            return filePath;
        }
    }
}
