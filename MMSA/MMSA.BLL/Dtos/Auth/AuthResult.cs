namespace MMSA.BLL.Dtos.Auth
{
    public class AuthResult
    {
        public bool Succeeded { get; private init; }
        public AuthResponseDTO? Response { get; private init; }
        public IReadOnlyList<string> Errors { get; private init; } = new List<string>();

        public static AuthResult Success(AuthResponseDTO response) =>
        new() { Succeeded = true, Response = response };

        public static AuthResult Failure(params string[] errors) =>
            new() { Succeeded = false, Errors = errors };

        public static AuthResult Failure(IEnumerable<string> errors) =>
            new() { Succeeded = false, Errors = errors.ToArray() };

    }
}
