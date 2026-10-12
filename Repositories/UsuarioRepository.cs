using Dapper;
using SecurityPRO.ERP.Web.Models;

namespace SecurityPRO.ERP.Web.Repositories;

public class UsuarioRepository
{
    private readonly DbConnectionFactory _factory;

    public UsuarioRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public Usuario? ObtenerPorEmail(string email)
    {
        using var conn = _factory.CreateConnection();

        const string sql = @"
            SELECT
                u.id_usuario AS IdUsuario,
                u.id_rol AS IdRol,
                u.nombres AS Nombres,
                u.email AS Email,
                u.password_hash AS PasswordHash,
                u.telefono AS Telefono,
                u.dni AS Dni,
                u.activo AS Activo,
                u.ultimo_acceso AS UltimoAcceso,
                u.fecha_creacion AS FechaCreacion,
                r.nombre_rol AS NombreRol
            FROM usuarios u
            INNER JOIN roles r
                ON u.id_rol = r.id_rol
            WHERE u.email = @email
              AND u.activo = 1";

        return conn.QueryFirstOrDefault<Usuario>(
            sql,
            new { email }
        );
    }

    public Usuario? ObtenerPorId(int id)
    {
        using var conn = _factory.CreateConnection();

        const string sql = @"
            SELECT
                u.id_usuario AS IdUsuario,
                u.id_rol AS IdRol,
                u.nombres AS Nombres,
                u.email AS Email,
                u.password_hash AS PasswordHash,
                u.telefono AS Telefono,
                u.dni AS Dni,
                u.activo AS Activo,
                u.ultimo_acceso AS UltimoAcceso,
                u.fecha_creacion AS FechaCreacion,
                r.nombre_rol AS NombreRol
            FROM usuarios u
            INNER JOIN roles r
                ON u.id_rol = r.id_rol
            WHERE u.id_usuario = @id";

        return conn.QueryFirstOrDefault<Usuario>(
            sql,
            new { id }
        );
    }

    public IEnumerable<Usuario> ListarTodos()
    {
        using var conn = _factory.CreateConnection();

        const string sql = @"
            SELECT
                u.id_usuario AS IdUsuario,
                u.id_rol AS IdRol,
                u.nombres AS Nombres,
                u.email AS Email,
                u.password_hash AS PasswordHash,
                u.telefono AS Telefono,
                u.dni AS Dni,
                u.activo AS Activo,
                u.ultimo_acceso AS UltimoAcceso,
                u.fecha_creacion AS FechaCreacion,
                r.nombre_rol AS NombreRol
            FROM usuarios u
            INNER JOIN roles r
                ON u.id_rol = r.id_rol
            ORDER BY u.nombres";

        return conn.Query<Usuario>(sql);
    }

    public int Crear(Usuario u)
    {
        using var conn = _factory.CreateConnection();

        const string sql = @"
            INSERT INTO usuarios
            (
                id_rol,
                nombres,
                email,
                password_hash,
                telefono,
                dni,
                activo
            )
            VALUES
            (
                @IdRol,
                @Nombres,
                @Email,
                @PasswordHash,
                @Telefono,
                @Dni,
                1
            );

            SELECT LAST_INSERT_ID();";

        return conn.ExecuteScalar<int>(sql, u);
    }

    public void Actualizar(Usuario u)
    {
        using var conn = _factory.CreateConnection();

        const string sql = @"
            UPDATE usuarios
            SET
                id_rol = @IdRol,
                nombres = @Nombres,
                email = @Email,
                telefono = @Telefono,
                dni = @Dni,
                activo = @Activo
            WHERE id_usuario = @IdUsuario";

        conn.Execute(sql, u);
    }

    public void ActualizarPassword(
        int idUsuario,
        string passwordHash)
    {
        using var conn = _factory.CreateConnection();

        const string sql = @"
            UPDATE usuarios
            SET password_hash = @passwordHash
            WHERE id_usuario = @idUsuario";

        conn.Execute(
            sql,
            new
            {
                idUsuario,
                passwordHash
            }
        );
    }

    public IEnumerable<Rol> ListarRoles()
    {
        using var conn = _factory.CreateConnection();

        const string sql = @"
            SELECT
                id_rol AS IdRol,
                nombre_rol AS NombreRol,
                descripcion AS Descripcion
            FROM roles
            ORDER BY id_rol";

        return conn.Query<Rol>(sql);
    }
}