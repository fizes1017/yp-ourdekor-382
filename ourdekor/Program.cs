using Microsoft.EntityFrameworkCore;
using ourdekor.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // предотвращает бесконечные циклы при загрузке связанных данных 
        // например, Продукт -> ТипПродукта -> СписокПродуктов...
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

var app = builder.Build();

app.MapControllers();

app.Run();