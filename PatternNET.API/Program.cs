using PatternNET.Application;     
using PatternNET.Infrastructure;  

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// --- BAGIAN PENTING: Menghidupkan Application & Infrastructure ---
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration); // Kirim Configuration supaya bisa baca appsettings.json
// ----------------------------------------------------------------

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();