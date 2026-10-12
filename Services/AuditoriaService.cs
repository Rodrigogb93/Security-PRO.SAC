using SecurityPRO.ERP.Web.Repositories;

namespace SecurityPRO.ERP.Web.Services;

public class AuditoriaService
{
    private readonly AuditoriaRepository _repo;
    public AuditoriaService(AuditoriaRepository repo) => _repo = repo;

    public void Registrar(int? idUsuario, string ip, string modulo, string accion, string? antes, string? despues)
    {
        _repo.Insertar(idUsuario, ip, modulo, accion, antes, despues);
    }
}