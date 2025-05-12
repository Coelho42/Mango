using Mango.MessageBus;
using Mango.Services.AuthAPI.Data;
using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Service;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adiciona e configura o contexto da base de dados (AppDbContext) no contêiner de serviços
builder.Services.AddDbContext<AppDbContext>(option =>
{
    // Configura a ligação ao base de dados SQL Server utilizando a connection string chamada "DefaultConnection" no appsettings.json
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
// Adiciona e configura o serviço Identity no contêiner de serviços
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    // Especifica que os dados dos users e das roles serão armazenados na base de dados utilizando o AppDbContext
    .AddEntityFrameworkStores<AppDbContext>().
    // Adiciona os token providers padrão, como tokens de recuperação de palavra-passe
    AddDefaultTokenProviders();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("ApiSettings:JwtOptions"));
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IMessageBus, MessageBus>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AUTH API");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
ApplyMigration();
app.Run();


void ApplyMigration()
{
    using (var scope = app.Services.CreateScope())
    {
        var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (_db.Database.GetPendingMigrations().Count() > 0)
        {
            _db.Database.Migrate();
        }
    }
}
