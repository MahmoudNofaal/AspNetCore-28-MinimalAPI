using MinimalAPI.MapGroups;

namespace MinimalAPI;

public class Program
{
   public static void Main(string[] args)
   {
      var builder = WebApplication.CreateBuilder(args);
      var app = builder.Build();

      var mapGroup = app.MapGroup("/products").ProductsAPI();

      app.Run();
   }
}
