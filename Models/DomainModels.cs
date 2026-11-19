namespace TecnologiaDigitalLK.Models;

// Define los dos tipos de usuario del sistema
public enum RolUsuario
{
    Admin,  // Dueña: ve todo, costos, balances globales y ambos locales
    Cajero  // Empleado: solo opera su local asignado y no ve costos
}

// Define las dos tarifas permitidas en el Punto de Venta
public enum TipoTarifa
{
    Detal,
    PorMayor
}

// Representa cada tienda física (Local 1 y Local 2) -> Tabla BD: Sucursales
public class SedeDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public bool Activa { get; set; } = true;
}

// Representa a cada persona que inicia sesión -> Tabla BD: Usuarios
public class UsuarioDto
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? PasswordTemporal { get; set; } // Se usa solo al crear usuario o cambiar clave (PasswordHash en BD)
    public RolUsuario Rol { get; set; }
    public int? SedeId { get; set; } // Null si es Admin (ve ambos); 1 o 2 si es Cajero
    public string NombreSede { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

// Categorías de productos (Ej: Cargadores, Audio, Protección) -> Tabla BD: Categorias
public class CategoriaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activa { get; set; } = true;
}

// Métodos de pago disponibles -> Tabla BD: MetodosPago
public class MetodoPagoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool EsEfectivo { get; set; }
    public bool Activo { get; set; } = true;
}

// Ficha técnica completa de cada producto del catálogo -> Tablas BD: Productos + StockSucursal
public class ProductoDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public string ImagenUrl { get; set; } = string.Empty;

    // Inventario separado por local a partir de un catálogo único
    public int StockLocal1 { get; set; }
    public int StockLocal2 { get; set; }
    public int StockMinimo { get; set; } = 5; // Para alertas de stock crítico

    // Estructura de costos y multi-precio
    public decimal CostoAdquisicion { get; set; }
    public decimal PrecioDetal { get; set; }
    public decimal PrecioPorMayor { get; set; }

    // Control de estado y rotación
    public bool Activo { get; set; } = true; // Borrado lógico (ocultar sin borrar historial)
    public int DiasSinMovimiento { get; set; } // Para detectar productos estancados

    // --- FÓRMULAS AUTOMÁTICAS (Se calculan solas en pantalla) ---
    public int StockTotal => StockLocal1 + StockLocal2;
    public decimal GananciaUnitariaDetal => PrecioDetal - CostoAdquisicion;
    public decimal GananciaUnitariaMayor => PrecioPorMayor - CostoAdquisicion;

    public decimal MargenDetalPorcentaje => PrecioDetal > 0
        ? Math.Round((GananciaUnitariaDetal / PrecioDetal) * 100, 1) : 0;

    public decimal MargenMayorPorcentaje => PrecioPorMayor > 0
        ? Math.Round((GananciaUnitariaMayor / PrecioPorMayor) * 100, 1) : 0;

    // Devuelve el stock según la sede que se esté consultando (1, 2 o Consolidado)
    public int ObtenerStockPorSede(int? sedeId) => sedeId switch
    {
        1 => StockLocal1,
        2 => StockLocal2,
        _ => StockTotal
    };
}

// Registro de mercancía movida entre Local 1 y Local 2 -> Tablas BD: Traslados + DetalleTraslados
public class TrasladoDto
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int SedeOrigenId { get; set; }
    public string SedeOrigenNombre { get; set; } = string.Empty;
    public int SedeDestinoId { get; set; }
    public string SedeDestinoNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public int UsuarioId { get; set; } // FK a Usuarios en BD
    public string UsuarioResponsable { get; set; } = string.Empty;
    public string Observacion { get; set; } = string.Empty;
}

// Cada línea/producto dentro del carrito de compras del POS -> Tabla BD: DetalleVentas
public class DetalleVentaDto
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public TipoTarifa TarifaAplicada { get; set; }
    public decimal CostoUnitarioSnapshot { get; set; }
    public decimal PrecioListaUnitario { get; set; }
    public decimal PrecioFinalUnitario { get; set; }

    public decimal Subtotal => PrecioFinalUnitario * Cantidad;
    public decimal GananciaLinea => (PrecioFinalUnitario - CostoUnitarioSnapshot) * Cantidad;

    // Regla de seguridad: detecta si el cajero intentó rebajar por debajo del costo
    public bool EsDebajoDelCosto => PrecioFinalUnitario < CostoUnitarioSnapshot;
}

// Encabezado de una factura/venta realizada -> Tabla BD: Ventas
public class VentaDto
{
    public int Id { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.Now;
    public int SedeId { get; set; }
    public string SedeNombre { get; set; } = string.Empty;
    public int UsuarioId { get; set; } // FK a Usuarios en BD
    public string CajeroNombre { get; set; } = string.Empty;
    public int MetodoPagoId { get; set; } = 1; // FK a MetodosPago en BD (1=Efectivo, 2=Nequi, 3=Tarjeta)
    public string MetodoPago { get; set; } = "Efectivo";
    public decimal TotalVenta { get; set; }
    public decimal GananciaNetaTotal { get; set; }
    public List<DetalleVentaDto> Detalles { get; set; } = new();
}

// Gastos operativos del día por local -> Tabla BD: Gastos
public class GastoOperativoDto
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public int SedeId { get; set; }
    public string SedeNombre { get; set; } = string.Empty;
    public int UsuarioId { get; set; } // FK a Usuarios en BD
    public string Concepto { get; set; } = string.Empty;
    public string CategoriaGasto { get; set; } = "Operativo";
    public decimal Monto { get; set; }
    public string RegistradoPor { get; set; } = string.Empty;
}