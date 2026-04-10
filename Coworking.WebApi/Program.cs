using Coworking.Application.common.Mapping;
using Coworking.Application.Interfaces;
using Coworking.Application.Service;
using Coworking.Infrastructure;
using Coworking.WebApi.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// builder.Services.AddApplication(); // Если ты создал такой метод, если нет — зарегистрируй сервис вручную:
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Настройка CORS для будущего React-приложения
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Порт React по умолчанию
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");
app.MapControllers();


app.Run();