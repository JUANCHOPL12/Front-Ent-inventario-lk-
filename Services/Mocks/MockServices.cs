using TecnologiaDigitalLK.Models;
using TecnologiaDigitalLK.Services.Interfaces;

namespace TecnologiaDigitalLK.Services.Mocks;

// 1. SERVICIO DE SESIÓN Y ROLES (Admin vs Cajero Local 1 / Local 2)
public class MockUserSessionService : IUserSessionService
{
    private readonly MockDataStore _store;
    public MockUserSessionService(MockDataStore store) => _store = store;

    public UsuarioDto? UsuarioActual { get; private set; }
    public bool EstaAutenticado => UsuarioActual != null;
    public bool EsAdmin => UsuarioActual?.Rol == RolUsuario.Admin;
    public int? SedeFiltroId { get; private set; }

    public string NombreVistaActual => SedeFiltroId switch
    {
        1 => "Local 1 (Sede Principal)",
        2 => "Local 2 (Sucursal Norte)",
        _ => "Consolidado Global (Local 1 + Local 2)"
    };

    public event Action? OnChange;

    public Task<bool> LoginAsync(string username, string password)
    {
        var user = _store.Usuarios.FirstOrDefault(u =>
            u.Activo && u.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase));

        if (user == null) return Task.FromResult(false);

        UsuarioActual = user;
        // Si es Admin arranca viendo el consolidado (null); si es Cajero queda fijo en su local
        SedeFiltroId = user.Rol == RolUsuario.Admin ? null : user.SedeId;
        OnChange?.Invoke();
        return Task.FromResult(true);
    }

    public Task LoginRapidoDemoAsync(RolUsuario rol, int? sedeId = null)
    {
        var user = rol == RolUsuario.Admin
            ? _store.Usuarios.First(u => u.Rol == RolUsuario.Admin)
            : _store.Usuarios.First(u => u.Rol == RolUsuario.Cajero && u.SedeId == (sedeId ?? 1));

        UsuarioActual = user;
        SedeFiltroId = rol == RolUsuario.Admin ? null : user.SedeId;
        OnChange?.Invoke();
        return Task.CompletedTask;
    }

    public void CambiarFiltroSedeAdmin(int? sedeId)
    {
        if (!EsAdmin) return; // Solo la dueña puede cambiar entre sedes
        SedeFiltroId = sedeId;
        OnChange?.Invoke();
    }

    public void Logout()
    {
        UsuarioActual = null;
        SedeFiltroId = null;
        OnChange?.Invoke();
    }
}

// 2. SERVICIO DE INVENTARIO, MULTI-PRECIO Y TRASLADOS ENTRE LOCALES
public class MockInventarioService : IInventarioService
{
    private readonly MockDataStore _store;
    public event Action? OnInventarioChanged;
    public MockInventarioService(MockDataStore store) => _store = store;

    public Task<List<ProductoDto>> ObtenerProductosAsync(bool incluirInactivos = true)
    {
        var lista = (incluirInactivos ? _store.Productos : _store.Productos.Where(p => p.Activo))
            .OrderBy(p => p.Nombre)
            .ToList();
        return Task.FromResult(lista);
    }

    public Task<List<CategoriaDto>> ObtenerCategoriasAsync() =>
        Task.FromResult(_store.Categorias.Where(c => c.Activa).ToList());

    public Task GuardarProductoAsync(ProductoDto producto)
    {
        var cat = _store.Categorias.FirstOrDefault(c => c.Id == producto.CategoriaId);
        if (cat != null) producto.CategoriaNombre = cat.Nombre;

        if (producto.Id == 0)
        {
            producto.Id = _store.Productos.Any() ? _store.Productos.Max(p => p.Id) + 1 : 1;
            _store.Productos.Add(producto);
        }
        else
        {
            var ex = _store.Productos.FirstOrDefault(p => p.Id == producto.Id);
            if (ex != null)
            {
                ex.Codigo = producto.Codigo;
                ex.Nombre = producto.Nombre;
                ex.CategoriaId = producto.CategoriaId;
                ex.CategoriaNombre = producto.CategoriaNombre;
                ex.ImagenUrl = producto.ImagenUrl;
                ex.StockLocal1 = producto.StockLocal1;
                ex.StockLocal2 = producto.StockLocal2;
                ex.StockMinimo = producto.StockMinimo;
                ex.CostoAdquisicion = producto.CostoAdquisicion;
                ex.PrecioDetal = producto.PrecioDetal;
                ex.PrecioPorMayor = producto.PrecioPorMayor;
                ex.Activo = producto.Activo;
            }
        }
        OnInventarioChanged?.Invoke();
        return Task.CompletedTask;
    }

    public Task CambiarEstadoProductoAsync(int productoId, bool activo)
    {
        var prod = _store.Productos.FirstOrDefault(p => p.Id == productoId);
        if (prod != null)
        {
            prod.Activo = activo; // Borrado lógico: no borra historial
            OnInventarioChanged?.Invoke();
        }
        return Task.CompletedTask;
    }

    public Task<(bool Exito, string Mensaje)> RegistrarTrasladoAsync(
        int productoId, int sedeOrigenId, int sedeDestinoId, int cantidad, string usuario, string observacion)
    {
        if (sedeOrigenId == sedeDestinoId)
            return Task.FromResult((false, "La sede de origen y destino deben ser diferentes."));
        if (cantidad <= 0)
            return Task.FromResult((false, "La cantidad a trasladar debe ser mayor a cero."));

        var prod = _store.Productos.FirstOrDefault(p => p.Id == productoId);
        if (prod == null)
            return Task.FromResult((false, "Producto no encontrado."));

        int disponible = sedeOrigenId == 1 ? prod.StockLocal1 : prod.StockLocal2;
        if (disponible < cantidad)
            return Task.FromResult((false, $"Stock insuficiente en origen (Disponible: {disponible} und.)."));

        // Mueve stock de un local a otro sin tocar reportes de ventas
        if (sedeOrigenId == 1) { prod.StockLocal1 -= cantidad; prod.StockLocal2 += cantidad; }
        else { prod.StockLocal2 -= cantidad; prod.StockLocal1 += cantidad; }

        _store.Traslados.Insert(0, new TrasladoDto
        {
            Id = _store.Traslados.Count + 1,
            Fecha = DateTime.Now,
            ProductoId = prod.Id,
            ProductoNombre = prod.Nombre,
            SedeOrigenId = sedeOrigenId,
            SedeOrigenNombre = $"Local {sedeOrigenId}",
            SedeDestinoId = sedeDestinoId,
            SedeDestinoNombre = $"Local {sedeDestinoId}",
            Cantidad = cantidad,
            UsuarioResponsable = usuario,
            Observacion = string.IsNullOrWhiteSpace(observacion) ? "Traslado interno entre sedes" : observacion
        });

        OnInventarioChanged?.Invoke();
        return Task.FromResult((true, $"Traslado de {cantidad} und. hacia Local {sedeDestinoId} completado."));
    }

    public Task<List<TrasladoDto>> ObtenerHistorialTrasladosAsync() =>
        Task.FromResult(_store.Traslados.OrderByDescending(t => t.Fecha).ToList());
}

// 3. SERVICIO DE PUNTO DE VENTA (POS) CON VALIDACIÓN DE COSTO MÍNIMO
public class MockVentasService : IVentasService
{
    private readonly MockDataStore _store;
    public event Action? OnVentasChanged;
    public MockVentasService(MockDataStore store) => _store = store;

    public Task<(bool Exito, string Mensaje, VentaDto? Venta)> RegistrarVentaAsync(VentaDto nuevaVenta)
    {
        if (!nuevaVenta.Detalles.Any())
            return Task.FromResult<(bool, string, VentaDto?)>((false, "El carrito está vacío.", null));

        // Regla crítica: impide vender por debajo del costo de adquisición
        if (nuevaVenta.Detalles.Any(d => d.EsDebajoDelCosto))
            return Task.FromResult<(bool, string, VentaDto?)>((false, "Bloqueado: Hay productos con precio inferior al costo de adquisición.", null));

        // Verifica que haya stock en el local donde se está cobrando
        foreach (var item in nuevaVenta.Detalles)
        {
            var prod = _store.Productos.First(p => p.Id == item.ProductoId);
            int stock = nuevaVenta.SedeId == 1 ? prod.StockLocal1 : prod.StockLocal2;
            if (stock < item.Cantidad)
                return Task.FromResult<(bool, string, VentaDto?)>((false, $"Stock insuficiente para {prod.Nombre}.", null));
        }

        // Descuenta en tiempo real el inventario de ese local
        foreach (var item in nuevaVenta.Detalles)
        {
            var prod = _store.Productos.First(p => p.Id == item.ProductoId);
            if (nuevaVenta.SedeId == 1) prod.StockLocal1 -= item.Cantidad;
            else prod.StockLocal2 -= item.Cantidad;
            prod.DiasSinMovimiento = 0;
        }

        nuevaVenta.Id = _store.Ventas.Count + 1;
        nuevaVenta.NumeroFactura = $"FAC-{100 + nuevaVenta.Id:D5}";
        nuevaVenta.Fecha = DateTime.Now;
        nuevaVenta.SedeNombre = $"Local {nuevaVenta.SedeId}";
        nuevaVenta.TotalVenta = nuevaVenta.Detalles.Sum(d => d.Subtotal);
        nuevaVenta.GananciaNetaTotal = nuevaVenta.Detalles.Sum(d => d.GananciaLinea);

        _store.Ventas.Insert(0, nuevaVenta);
        OnVentasChanged?.Invoke();
        return Task.FromResult<(bool, string, VentaDto?)>((true, $"Venta {nuevaVenta.NumeroFactura} registrada.", nuevaVenta));
    }

    public Task<List<VentaDto>> ObtenerVentasAsync(DateTime? fecha = null, int? sedeId = null)
    {
        var q = _store.Ventas.AsEnumerable();
        if (fecha.HasValue) q = q.Where(v => v.Fecha.Date == fecha.Value.Date);
        if (sedeId.HasValue) q = q.Where(v => v.SedeId == sedeId.Value);
        return Task.FromResult(q.OrderByDescending(v => v.Fecha).ToList());
    }
}

// 4. SERVICIO DE CIERRE DE CAJA Y GASTOS OPERATIVOS
public class MockCierreCajaService : ICierreCajaService
{
    private readonly MockDataStore _store;
    public event Action? OnGastosChanged;
    public MockCierreCajaService(MockDataStore store) => _store = store;

    public Task RegistrarGastoAsync(GastoOperativoDto gasto)
    {
        gasto.Id = _store.Gastos.Count + 1;
        gasto.Fecha = DateTime.Now;
        gasto.SedeNombre = $"Local {gasto.SedeId}";
        _store.Gastos.Insert(0, gasto);
        OnGastosChanged?.Invoke();
        return Task.CompletedTask;
    }

    public Task<List<GastoOperativoDto>> ObtenerGastosAsync(DateTime? fecha = null, int? sedeId = null)
    {
        var q = _store.Gastos.AsEnumerable();
        if (fecha.HasValue) q = q.Where(g => g.Fecha.Date == fecha.Value.Date);
        if (sedeId.HasValue) q = q.Where(g => g.SedeId == sedeId.Value);
        return Task.FromResult(q.OrderByDescending(g => g.Fecha).ToList());
    }
}

// 5. SERVICIO DE ADMINISTRACIÓN DE LOCALES Y USUARIOS
public class MockAdministracionService : IAdministracionService
{
    private readonly MockDataStore _store;
    public MockAdministracionService(MockDataStore store) => _store = store;

    public Task<List<SedeDto>> ObtenerSedesAsync() => Task.FromResult(_store.Sedes.ToList());

    public Task GuardarSedeAsync(SedeDto sede)
    {
        if (sede.Id == 0) { sede.Id = _store.Sedes.Count + 1; _store.Sedes.Add(sede); }
        else
        {
            var ex = _store.Sedes.FirstOrDefault(s => s.Id == sede.Id);
            if (ex != null) { ex.Nombre = sede.Nombre; ex.Direccion = sede.Direccion; ex.Activa = sede.Activa; }
        }
        return Task.CompletedTask;
    }

    public Task<List<UsuarioDto>> ObtenerUsuariosAsync() => Task.FromResult(_store.Usuarios.ToList());

    public Task GuardarUsuarioAsync(UsuarioDto usuario)
    {
        usuario.NombreSede = usuario.Rol == RolUsuario.Admin
            ? "Todas las Sedes"
            : _store.Sedes.FirstOrDefault(s => s.Id == usuario.SedeId)?.Nombre ?? "Local 1";

        if (usuario.Id == 0) { usuario.Id = _store.Usuarios.Count + 1; _store.Usuarios.Add(usuario); }
        else
        {
            var ex = _store.Usuarios.FirstOrDefault(u => u.Id == usuario.Id);
            if (ex != null)
            {
                ex.NombreCompleto = usuario.NombreCompleto;
                ex.Username = usuario.Username;
                ex.Rol = usuario.Rol;
                ex.SedeId = usuario.Rol == RolUsuario.Admin ? null : usuario.SedeId;
                ex.NombreSede = usuario.NombreSede;
                ex.Activo = usuario.Activo;
            }
        }
        return Task.CompletedTask;
    }
}