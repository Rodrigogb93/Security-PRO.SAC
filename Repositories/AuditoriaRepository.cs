using Dapper;

namespace SecurityPRO.ERP.Web.Repositories;

public class AuditoriaRepository
{
    private readonly DbConnectionFactory _factory;

    public AuditoriaRepository(DbConnectionFactory factory) => _factory = factory;

    public void Insertar(
        int? idUsuario,
        string ip,
        string modulo,
        string accion,
        string? antes,
        string? despues)
    {
        using var conn = _factory.CreateConnection();

        const string sql = @"
            INSERT INTO auditoria
            (id_usuario, direccion_ip, modulo, accion, valores_anteriores, valores_nuevos)
            VALUES
            (@idUsuario, @ip, @modulo, @accion, @antes, @despues)";

        conn.Execute(sql, new
        {
            idUsuario,
            ip,
            modulo,
            accion,
            antes,
            despues
        });
    }
}