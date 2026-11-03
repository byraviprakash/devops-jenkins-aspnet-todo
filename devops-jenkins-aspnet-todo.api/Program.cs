using Scalar.AspNetCore;

var builder =WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    // OpenAPI document: /openapi/v1.json
//    app.MapOpenApi();

//    // Interactive API UI (Swagger-like): /scalar/v1
//    app.MapScalarApiReference(options =>
//    {
//        options.WithTitle("ToDo API");
//    });
//}
// OpenAPI document: /openapi/v1.json
app.MapOpenApi();

// Interactive API UI (Swagger-like): /scalar/v1
app.MapScalarApiReference(options =>
{
    options.WithTitle("ToDo API");
});

app.UseAuthorization();

app.MapControllers();

app.Run();
