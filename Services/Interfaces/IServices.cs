using TecnologiaDigitalLK.Models;

namespace TecnologiaDigitalLK.Services.Interfaces;

// 1. Control de Sesión, Roles (Admin / Cajero) y Filtro Multi-Sede (Módulo 4)
public interface IUserSessionService
{
    UsuarioDto? UsuarioActual { get; }
    bool EstaAutenticado { get; }
    bool EsAdmin { get; }
    int? SedeFiltroId { get; } // Null = Consolidado (Admin) | 1 = Local 1 | 2 = Local 2
    string NombreVistaActual { get; }
    event Action? OnChange;

    Task<bool> LoginAsync(string username, string password);
    Task LoginRapidoDemoAsync(RolUsuario rol, int? sedeId = null);
    void CambiarFiltroSedeAdmin(int? sedeId);
    void Logout();
}

// 2. Gestión de Catálogo, Stock por Local y Traslados Internos (Módulo 1)
public interface IInventarioService
{
    event Action? OnInventarioChanged;
    Task<List<ProductoDto>> ObtenerProductosAsync(bool incluirInactivos = true);
    Task<List<CategoriaDto>> ObtenerCategoriasAsync();
    Task GuardarProductoAsync(ProductoDto producto);
    Task CambiarEstadoProductoAsync(int productoId, bool activo); // Borrado lógico
    Task<(bool Exito, string Mensaje)> RegistrarTrasladoAsync(
        int productoId, int sedeOrigenId, int sedeDestinoId, int cantidad, string usuario, string observacion);
    Task<List<TrasladoDto>> ObtenerHistorialTrasladosAsync();
}

// 3. Punto de Venta (POS) y Facturación (Módulo 2)
public interface IVentasService
{
    event Action? OnVentasChanged;
    Task<(bool Exito, string Mensaje, VentaDto? Venta)> RegistrarVentaAsync(VentaDto nuevaVenta);
    Task<List<VentaDto>> ObtenerVentasAsync(DateTime? fecha = null, int? sedeId = null);
}

// 4. Cierre Diario, Gastos Operativos e Histórico Auditable (Módulo 3)
public interface ICierreCajaService
{
    event Action? OnGastosChanged;
    Task RegistrarGastoAsync(GastoOperativoDto gasto);
    Task<List<GastoOperativoDto>> ObtenerGastosAsync(DateTime? fecha = null, int? sedeId = null);
}

// 5. Administración de Sucursales y Usuarios (Módulo 4)
public interface IAdministracionService
{
    Task<List<SedeDto>> ObtenerSedesAsync();
    Task GuardarSedeAsync(SedeDto sede);
    Task<List<UsuarioDto>> ObtenerUsuariosAsync();
    Task GuardarUsuarioAsync(UsuarioDto usuario);
}