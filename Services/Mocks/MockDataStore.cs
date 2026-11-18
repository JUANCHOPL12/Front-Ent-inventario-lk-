using TecnologiaDigitalLK.Models;

namespace TecnologiaDigitalLK.Services.Mocks;

// Almacén temporal en memoria para la demo visual (Fase 1).
// Contiene datos realistas de Tecnología Digital LK para probar todas las pantallas.
public class MockDataStore
{
    // 1. Las dos sedes físicas del negocio
    public List<SedeDto> Sedes { get; } = new()
    {
        new SedeDto { Id = 1, Nombre = "Local 1 - Sede Principal", Direccion = "Centro Comercial Local 104", Activa = true },
        new SedeDto { Id = 2, Nombre = "Local 2 - Sucursal Norte", Direccion = "Av. Principal # 24-18", Activa = true }
    };

    // 2. Usuarios de prueba (Dueña + 2 Cajeros asignados uno a cada local)
    public List<UsuarioDto> Usuarios { get; } = new()
    {
        new UsuarioDto { Id = 1, NombreCompleto = "Administradora (Dueña)", Username = "admin", Rol = RolUsuario.Admin, SedeId = null, NombreSede = "Todas las Sedes", Activo = true },
        new UsuarioDto { Id = 2, NombreCompleto = "Carlos Méndez", Username = "cajero1", Rol = RolUsuario.Cajero, SedeId = 1, NombreSede = "Local 1 - Sede Principal", Activo = true },
        new UsuarioDto { Id = 3, NombreCompleto = "Andrea Gómez", Username = "cajero2", Rol = RolUsuario.Cajero, SedeId = 2, NombreSede = "Local 2 - Sucursal Norte", Activo = true }
    };

    // 3. Categorías del catálogo
    public List<CategoriaDto> Categorias { get; } = new()
    {
        new CategoriaDto { Id = 1, Nombre = "Cargadores y Cables", Activa = true },
        new CategoriaDto { Id = 2, Nombre = "Audio y Audífonos", Activa = true },
        new CategoriaDto { Id = 3, Nombre = "Protección y Accesorios", Activa = true },
        new CategoriaDto { Id = 4, Nombre = "Energía y PowerBanks", Activa = true }
    };

    // 4. Productos con stock separado por sede, costos y precios Detal / Por Mayor
    public List<ProductoDto> Productos { get; } = new()
    {
        new ProductoDto
        {
            Id = 1, Codigo = "LK-101", Nombre = "Cargador Carga Rápida 25W USB-C", CategoriaId = 1, CategoriaNombre = "Cargadores y Cables",
            StockLocal1 = 18, StockLocal2 = 4, StockMinimo = 5, // Local 2 ya está en alerta de stock crítico (< 5)
            CostoAdquisicion = 22000, PrecioDetal = 45000, PrecioPorMayor = 34000, Activo = true, DiasSinMovimiento = 2
        },
        new ProductoDto
        {
            Id = 2, Codigo = "LK-102", Nombre = "Cable Trenzado USB-C a Lightning 2m", CategoriaId = 1, CategoriaNombre = "Cargadores y Cables",
            StockLocal1 = 25, StockLocal2 = 15, StockMinimo = 8,
            CostoAdquisicion = 9500, PrecioDetal = 22000, PrecioPorMayor = 16000, Activo = true, DiasSinMovimiento = 1
        },
        new ProductoDto
        {
            Id = 3, Codigo = "LK-201", Nombre = "Audífonos Inalámbricos TWS Pro ANC", CategoriaId = 2, CategoriaNombre = "Audio y Audífonos",
            StockLocal1 = 3, StockLocal2 = 9, StockMinimo = 5, // Local 1 en alerta de stock crítico (< 5)
            CostoAdquisicion = 48000, PrecioDetal = 95000, PrecioPorMayor = 75000, Activo = true, DiasSinMovimiento = 3
        },
        new ProductoDto
        {
            Id = 4, Codigo = "LK-301", Nombre = "Vidrio Templado Cerámico Mate 9D", CategoriaId = 3, CategoriaNombre = "Protección y Accesorios",
            StockLocal1 = 42, StockLocal2 = 30, StockMinimo = 10,
            CostoAdquisicion = 3000, PrecioDetal = 15000, PrecioPorMayor = 8500, Activo = true, DiasSinMovimiento = 0
        },
        new ProductoDto
        {
            Id = 5, Codigo = "LK-401", Nombre = "Power Bank 10.000mAh Carga Rápida", CategoriaId = 4, CategoriaNombre = "Energía y PowerBanks",
            StockLocal1 = 7, StockLocal2 = 2, StockMinimo = 4,
            CostoAdquisicion = 42000, PrecioDetal = 85000, PrecioPorMayor = 68000, Activo = true, DiasSinMovimiento = 5
        },
        new ProductoDto
        {
            Id = 6, Codigo = "LK-305", Nombre = "Adaptador Hub USB-C 6 en 1 HDMI", CategoriaId = 3, CategoriaNombre = "Protección y Accesorios",
            StockLocal1 = 6, StockLocal2 = 5, StockMinimo = 3,
            CostoAdquisicion = 55000, PrecioDetal = 110000, PrecioPorMayor = 88000, Activo = true, DiasSinMovimiento = 52 // Producto estancado (baja rotación)
        }
    };

    // 5. Historial inicial de traslados entre sedes
    public List<TrasladoDto> Traslados { get; } = new()
    {
        new TrasladoDto
        {
            Id = 1, Fecha = DateTime.Now.AddHours(-3), ProductoId = 1, ProductoNombre = "Cargador Carga Rápida 25W USB-C",
            SedeOrigenId = 1, SedeOrigenNombre = "Local 1", SedeDestinoId = 2, SedeDestinoNombre = "Local 2",
            Cantidad = 5, UsuarioResponsable = "Administradora (Dueña)", Observacion = "Reposición vitrina Local 2"
        }
    };

    // 6. Ventas iniciales del día para ver movimiento en el cierre de caja
    public List<VentaDto> Ventas { get; } = new()
    {
        new VentaDto
        {
            Id = 1, NumeroFactura = "FAC-00101", Fecha = DateTime.Today.AddHours(10).AddMinutes(15),
            SedeId = 1, SedeNombre = "Local 1", CajeroNombre = "Carlos Méndez", MetodoPago = "Efectivo",
            TotalVenta = 90000, GananciaNetaTotal = 46000,
            Detalles = new List<DetalleVentaDto>
            {
                new DetalleVentaDto { ProductoId = 1, ProductoNombre = "Cargador Carga Rápida 25W USB-C", Cantidad = 2, TarifaAplicada = TipoTarifa.Detal, CostoUnitarioSnapshot = 22000, PrecioListaUnitario = 45000, PrecioFinalUnitario = 45000 }
            }
        }
    };

    // 7. Gastos operativos iniciales del día
    public List<GastoOperativoDto> Gastos { get; } = new()
    {
        new GastoOperativoDto
        {
            Id = 1, Fecha = DateTime.Today.AddHours(9), SedeId = 1, SedeNombre = "Local 1",
            Concepto = "Domicilio mensajería proveedor", CategoriaGasto = "Transporte / Domicilios", Monto = 12000, RegistradoPor = "Carlos Méndez"
        }
    };
}