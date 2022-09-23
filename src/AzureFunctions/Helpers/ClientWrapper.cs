namespace AzureFunctions.Helpers;

/// <summary>
/// Generic client wrapper, allows me to have null in DI Container - requested by task
/// </summary>
/// <typeparam name="T"></typeparam>
public class ClientWrapper<T> where T: class
{
    public ClientWrapper(T client = null)
    {
        Client = client;
    }

    public T Client { get; } = null;
    public bool IsValid => Client != null;
}
