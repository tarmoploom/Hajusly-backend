using System.Text.Json;
using System.Text.Json.Nodes;
using Hajusly.Model;

namespace Hajusly;

public class DbSeeder
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public DbSeeder(IWebHostEnvironment env, AppDbContext context)
    {
        _env = env;
        _context = context;
    }
    
    
    private string GetData(string filePath)
    {
        string rootPath = _env.ContentRootPath;
        filePath = Path.GetFullPath(Path.Combine(rootPath, filePath));
        if (File.Exists(filePath)) {
            using (var r = new StreamReader(filePath))
            {
                var json = r.ReadToEnd();
                return json;
            }
        }
        return "[]";
    }

    public void SeedData<T>(string filePath)
    {
        string data = GetData(filePath);
        var items = JsonSerializer.Deserialize<List<T>>(data);
        if (items != null)
        {
            foreach (var item in items)
            {
                _context.Add(item);
            }
        }
        
        _context.SaveChanges();
    }
    
    
    
    
    
}