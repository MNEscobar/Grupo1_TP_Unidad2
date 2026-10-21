/// <summary>
/// Contrato común del Command Pattern: encapsula una acción para que
/// quien la dispara (por ejemplo, un botón de un pop-up) no necesite
/// saber qué hace ni de quién depende.
/// </summary>
public interface ICommand
{
    void Execute();
}
