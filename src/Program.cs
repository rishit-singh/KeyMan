using Microsoft.EntityFrameworkCore;

namespace KeyMan;

public class Program
{
    public static void Main(string[] args)
    {
        APIKeyDBContext dbContext = new APIKeyDBContext();

        APIKeyManager keyManager = new APIKeyManager(dbContext);
        
        foreach (APIKey key in keyManager.List())
            Console.WriteLine(key.Key);
    }
}