using BookCloud.Models;
using BookCloud.Repositories.Interfaces;

namespace BookCloud.Repositories
{

    #region PRODIMIENTOS ALMACENADOS

    //    procedimiento para recoger libros disponibles
    //Solo muestra libros cuyo vendedor también esté activo.
    //Devuelve los libros activos con stock disponible.

    //CREATE OR ALTER PROCEDURE SP_Libros_Disponibles
    //AS
    //BEGIN
    //    SET NOCOUNT ON;

    //    SELECT
    //        L.Id,
    //        L.Titulo,
    //        L.Autor,
    //        L.Descripcion,
    //        L.Precio,
    //        L.Stock,
    //        L.Foto,
    //        L.FechaPublicacion,
    //        U.Nombre AS Vendedor
    //    FROM Libros L
    //    INNER JOIN Usuarios U ON L.UsuarioId = U.Id
    //    WHERE
    //        L.Activo = 1
    //        AND U.Activo = 1
    //        AND L.Stock > 0;
    //    END

    //--procedimiento crearr Libro:
    //-- Inserta un nuevo libro en el sistema.
    //-- Valida que el usuario vendedor exista y esté activo antes de crear el registro.
    //CREATE OR ALTER PROCEDURE SP_Libro_Crear
    //(
    //    @Titulo NVARCHAR(200),
    //    @Autor NVARCHAR(200),
    //    @Descripcion NVARCHAR(MAX),
    //    @Precio DECIMAL(10,2),
    //    @Stock INT,
    //    @Foto NVARCHAR(500),
    //    @FechaPublicacion DATE,
    //    @UsuarioId INT
    //)
    //AS
    //BEGIN
    //    SET NOCOUNT ON;

    //    IF NOT EXISTS(
    //        SELECT 1 FROM Usuarios
    //        WHERE Id = @UsuarioId AND Activo = 1
    //    )
    //    BEGIN
    //        RAISERROR('El usuario no existe o no está activo.',16,1);
    //    RETURN;
    //    END

    //    INSERT INTO Libros
    //    (
    //        Titulo,
    //        Autor,
    //        Descripcion,
    //        Precio,
    //        Stock,
    //        Foto,
    //        FechaPublicacion,
    //        UsuarioId,
    //        Activo
    //    )
    //    VALUES
    //    (
    //        @Titulo,
    //        @Autor,
    //        @Descripcion,
    //        @Precio,
    //        @Stock,
    //        @Foto,
    //        @FechaPublicacion,
    //        @UsuarioId,
    //        1
    //    );
    //    END

    #endregion

    public class RepositoryLibros : IRepositoryLibros
    {

        public RepositoryLibros() { }
        public Task DeleteLibro(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Libro> GetLibro(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Libro>> GetLibros()
        {
            throw new NotImplementedException();
        }

        public Task InsertLibro(Libro libro)
        {
            throw new NotImplementedException();
        }

        public Task UpdateLibro(Libro libro)
        {
            throw new NotImplementedException();
        }
    }
}
