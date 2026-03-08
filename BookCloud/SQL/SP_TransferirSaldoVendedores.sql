-- ==================================================================
-- Procedimiento almacenado para transferir saldo a vendedores
-- ==================================================================
-- Este procedimiento distribuye el pago de un pedido entre los vendedores
-- de los libros comprados, registrando ingresos en sus wallets

CREATE OR ALTER PROCEDURE SP_TransferirSaldoAVendedores
    @PedidoId INT,
    @CompradorId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificar que el pedido existe
        IF NOT EXISTS (SELECT 1 FROM Pedidos WHERE Id = @PedidoId AND Activo = 1)
        BEGIN
            RAISERROR('El pedido no existe o no está activo', 16, 1);
            RETURN;
        END
        
        -- Verificar que no se hayan transferido ya los fondos
        IF EXISTS (
            SELECT 1 
            FROM SaldoMovimientos 
            WHERE PedidoId = @PedidoId 
                AND Tipo = 'Ingreso' 
                AND Descripcion LIKE 'Venta de libro(s) - Pedido%'
                AND Activo = 1
        )
        BEGIN
            RAISERROR('Los fondos ya fueron transferidos para este pedido', 16, 1);
            RETURN;
        END
        
        -- Insertar movimientos de ingreso para cada vendedor
        INSERT INTO SaldoMovimientos (UsuarioId, PedidoId, Monto, Tipo, Descripcion, Fecha, Activo)
        SELECT 
            L.UsuarioId AS VendedorId,
            @PedidoId AS PedidoId,
            SUM(PD.PrecioUnitario * PD.Cantidad) AS TotalVenta,
            'Ingreso' AS Tipo,
            'Venta de libro(s) - Pedido #' + CAST(@PedidoId AS VARCHAR(10)) AS Descripcion,
            GETDATE() AS Fecha,
            1 AS Activo
        FROM PedidoDetalles PD
        INNER JOIN Libros L ON PD.LibroId = L.Id
        WHERE PD.PedidoId = @PedidoId 
            AND PD.Activo = 1
            AND L.Activo = 1
        GROUP BY L.UsuarioId;
        
        -- Verificar que se hayan insertado registros
        IF @@ROWCOUNT = 0
        BEGIN
            RAISERROR('No se pudieron crear los movimientos de ingreso', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        COMMIT TRANSACTION;
        
        SELECT 'OK' AS Resultado, 'Fondos transferidos exitosamente' AS Mensaje;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- ==================================================================
-- Procedimiento para obtener el saldo actualizado de un usuario
-- ==================================================================
CREATE OR ALTER PROCEDURE SP_ObtenerSaldoUsuario
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT ISNULL(SUM(
        CASE 
            WHEN Tipo = 'Ingreso' THEN Monto
            ELSE -Monto
        END
    ), 0) AS SaldoActual
    FROM SaldoMovimientos
    WHERE UsuarioId = @UsuarioId 
        AND Activo = 1;
END
GO

-- ==================================================================
-- Procedimiento para obtener el historial de ventas de un vendedor
-- ==================================================================
CREATE OR ALTER PROCEDURE SP_ObtenerVentasVendedor
    @VendedorId INT,
    @FechaInicio DATETIME = NULL,
    @FechaFin DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SET @FechaInicio = ISNULL(@FechaInicio, DATEADD(MONTH, -1, GETDATE()));
    SET @FechaFin = ISNULL(@FechaFin, GETDATE());
    
    SELECT 
        SM.Id,
        SM.PedidoId,
        SM.Monto,
        SM.Descripcion,
        SM.Fecha,
        P.Total AS TotalPedido,
        P.Estado AS EstadoPedido,
        U.Nombre AS NombreComprador
    FROM SaldoMovimientos SM
    INNER JOIN Pedidos P ON SM.PedidoId = P.Id
    INNER JOIN Usuarios U ON P.UsuarioId = U.Id
    WHERE SM.UsuarioId = @VendedorId
        AND SM.Tipo = 'Ingreso'
        AND SM.Descripcion LIKE 'Venta de libro(s)%'
        AND SM.Activo = 1
        AND SM.Fecha BETWEEN @FechaInicio AND @FechaFin
    ORDER BY SM.Fecha DESC;
END
GO

-- ==================================================================
-- Índices para mejorar rendimiento
-- ==================================================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SaldoMovimientos_UsuarioId_Tipo_Activo')
BEGIN
    CREATE NONCLUSTERED INDEX IX_SaldoMovimientos_UsuarioId_Tipo_Activo
    ON SaldoMovimientos(UsuarioId, Tipo, Activo)
    INCLUDE (Monto, PedidoId, Descripcion, Fecha);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SaldoMovimientos_PedidoId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_SaldoMovimientos_PedidoId
    ON SaldoMovimientos(PedidoId)
    WHERE Activo = 1;
END
GO

-- ==================================================================
-- Ejemplos de uso
-- ==================================================================

-- Ejemplo 1: Transferir saldo después de un pedido
-- EXEC SP_TransferirSaldoAVendedores @PedidoId = 1, @CompradorId = 2;

-- Ejemplo 2: Obtener saldo de un usuario
-- EXEC SP_ObtenerSaldoUsuario @UsuarioId = 1;

-- Ejemplo 3: Obtener ventas de un vendedor
-- EXEC SP_ObtenerVentasVendedor @VendedorId = 1, @FechaInicio = '2024-01-01', @FechaFin = '2024-12-31';
