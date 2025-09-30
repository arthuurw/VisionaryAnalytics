namespace VisionaryAnalytics.Application.DTOs
{
    public class Resultado<T>
    {
        public bool Sucesso { get; }
        public string? Mensagem { get; }
        public T? Value { get; }

        public Resultado(bool sucesso, string? mensagem, T? value)
        {
            Sucesso = sucesso;
            Mensagem = mensagem;
            Value = value;
        }

        public static Resultado<T> Ok(string? mensagem = null, T? value = default) => new(true, mensagem, value);
        public static Resultado<T> Falha(string mensagem, T? value = default) => new(false, mensagem, value);
    }
}
