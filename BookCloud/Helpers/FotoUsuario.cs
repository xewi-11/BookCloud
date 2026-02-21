using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace BookCloud.Helpers
{
    public enum Folder { Usuarios }
    public class FotoUsuario
    {
        private IWebHostEnvironment webHostEnvironment;
        private IServer server;
        public FotoUsuario(IWebHostEnvironment webHostEnvironment, IServer server)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.server = server;
        }
        public string MapPath(string fileName, Folder folder, int userId)
        {
            if (folder == Folder.Usuarios)
            {
                string carpeta = "usuarios";
                string rootPath = this.webHostEnvironment.WebRootPath;
                string fileNameWithId = $"{userId}{fileName}";
                return Path.Combine(rootPath, "imagenes", carpeta, fileNameWithId);
            }
            else
            {
                return "Folder no reconocido";
            }
        }
        public string MapUrlPath(string fileName, Folder folder, int userId)
        {
            if (folder == Folder.Usuarios)
            {
                string carpeta = "usuarios";
                var adresses = this.server.Features.Get<IServerAddressesFeature>().Addresses;
                string serverUrl = adresses.FirstOrDefault()?.ToString();
                return $"{serverUrl}/imagenes/{carpeta}/{fileName}";
            }
            else
            {
                return "Folder no reconocido";
            }
        }
    }
}