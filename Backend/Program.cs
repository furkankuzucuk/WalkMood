using Microsoft.EntityFrameworkCore;
using WalkMood.API.Data;
using WalkMood.API.Services;

var builder = WebApplication.CreateBuilder(args);
// React'in API'ye erişebilmesi için CORS politikası ekliyoruz
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") // React'in çalıştığı port
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// 1. Controller desteğini servislere ekle (BUNU EKLİYORUZ)
builder.Services.AddControllers();
// HttpClient ve OsmService entegrasyonu
builder.Services.AddHttpClient<IOsmService, OsmService>();
builder.Services.AddScoped<IGraphService, GraphService>();
builder.Services.AddScoped<IRoutingService, RoutingService>();

// Veritabanı bağlantımız (Bunu zaten eklemiştik)
builder.Services.AddDbContext<WalkMoodDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// CORS politikasını aktifleştir
app.UseCors("AllowReactApp");

// 2. Uygulamanın Controller rotalarını tanımasını sağla (BUNU EKLİYORUZ)
app.MapControllers();

app.Run();