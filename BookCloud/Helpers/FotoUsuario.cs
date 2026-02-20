namespace BookCloud.Helpers
{
    public class FotoUsuario
    {
        private readonly string _rutaBase = "wwwroot/imagenes/usuarios";
        private readonly long _tamañoMaximoBytes = 5 * 1024 * 1024; // 5MB
        private readonly string[] _extensionesPermitidas = { ".jpg", ".jpeg", ".png", ".gif" };


        public async Task<(bool exito, string? rutaRelativa, string? error)> GuardarFotoAsync(IFormFile archivo, string idUsuario)
        {
            // Validar que el archivo existe
            if (archivo == null || archivo.Length == 0)
                return (false, null, "No se proporcionó ningún archivo");

            // Validar tamaño
            if (archivo.Length > _tamañoMaximoBytes)
                return (false, null, $"El archivo excede el tamaño máximo de {_tamañoMaximoBytes / 1024 / 1024}MB");

            // Validar extensión
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!_extensionesPermitidas.Contains(extension))
                return (false, null, "Formato de imagen no permitido. Use JPG, PNG o GIF");

            try
            {
                // Crear directorio si no existe
                var rutaCompleta = Path.Combine(Directory.GetCurrentDirectory(), _rutaBase);
                if (!Directory.Exists(rutaCompleta))
                    Directory.CreateDirectory(rutaCompleta);

                // Generar nombre único para evitar colisiones
                var nombreArchivo = $"{idUsuario}_{Guid.NewGuid()}{extension}";
                var rutaArchivo = Path.Combine(rutaCompleta, nombreArchivo);

                // Guardar archivo físico
                using (var stream = new FileStream(rutaArchivo, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                // Retornar ruta relativa para guardar en BD
                var rutaRelativa = $"/imagenes/usuarios/{nombreArchivo}";
                return (true, rutaRelativa, null);
            }
            catch (Exception ex)
            {
                return (false, null, $"Error al guardar la imagen: {ex.Message}");
            }
        }
        public bool EliminarFoto(string rutaRelativa)
        {
            try
            {
                if (string.IsNullOrEmpty(rutaRelativa))
                    return false;

                var rutaCompleta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", rutaRelativa.TrimStart('/'));
                if (File.Exists(rutaCompleta))
                {
                    File.Delete(rutaCompleta);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool ValidarImagen(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return false;

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            return _extensionesPermitidas.Contains(extension) && archivo.Length <= _tamañoMaximoBytes;
        }
    }
}