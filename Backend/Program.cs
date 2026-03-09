using Microsoft.EntityFrameworkCore;
using WalkMood.API.Data;
using WalkMood.API.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Controller desteğini servislere ekle (BUNU EKLİYORUZ)
builder.Services.AddControllers();
// HttpClient ve OsmService entegrasyonu
builder.Services.AddHttpClient<IOsmService, OsmService>();

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

// 2. Uygulamanın Controller rotalarını tanımasını sağla (BUNU EKLİYORUZ)
app.MapControllers();

app.Run();