namespace SinaMN75U.InnerServices;

public interface IProcessHandlerService {
	string ProcessId { get; }
	Task<UResponse<UProcessStepGetResponse?>> Get(JwtClaimData userData, CancellationToken ct);
	Task<UResponse<UProcessStepGetResponse?>> Send(JwtClaimData userData, UProcessStepSend p, CancellationToken ct);
}

public interface IProcessService {
	Task<UResponse<UProcessStepGetResponse?>> Get(IdStringParams p, CancellationToken ct);
	Task<UResponse<UProcessStepGetResponse?>> Send(UProcessStepSend p, CancellationToken ct);
}

public static class ProcessHandlerExtensions {
	public static bool OwnsStep(this IProcessHandlerService handlerService, string stepId) => handlerService is IStepOwner owner && owner.StepIds.Contains(stepId);
}

public interface IStepOwner {
	IReadOnlySet<string> StepIds { get; }
}

public class ProcessService(
	IEnumerable<IProcessHandlerService> handlers,
	ILocalizationService ls,
	ITokenService ts
) : IProcessService {
	private readonly Dictionary<string, IProcessHandlerService> _handlers = handlers.ToDictionary(h => h.ProcessId, h => h);

	public async Task<UResponse<UProcessStepGetResponse?>> Get(IdStringParams p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<UProcessStepGetResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		if (!_handlers.TryGetValue(p.Id, out IProcessHandlerService? handler)) return new UResponse<UProcessStepGetResponse?>(null, Usc.NotFound, ls.Get("ProcessNotFound"));
		return await handler.Get(userData, ct);
	}

	public async Task<UResponse<UProcessStepGetResponse?>> Send(UProcessStepSend p, CancellationToken ct) {
		JwtClaimData? userData = ts.ExtractClaims(p.Token);
		if (userData == null) return new UResponse<UProcessStepGetResponse?>(null, Usc.UnAuthorized, ls.Get("AuthorizationRequired"));

		IProcessHandlerService? handler = _handlers.Values.FirstOrDefault(h => h.OwnsStep(p.Id));
		if (handler == null) return new UResponse<UProcessStepGetResponse?>(null, Usc.BadRequest, ls.Get("InvalidStep"));

		return await handler.Send(userData, p, ct);
	}
}
