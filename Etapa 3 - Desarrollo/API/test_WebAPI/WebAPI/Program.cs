var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(ValidarApiKey);

app.MapControllers();

app.Run();


static Task ValidarApiKey(HttpContext context, RequestDelegate siguiente)
{
    // Valor de contraseña externalizado para no exponerlo en github
    string? apiKey = context.RequestServices.GetRequiredService<IConfiguration>()["ApiKey"];
    //Si se encuentra vacio o null arroja 500
    if (string.IsNullOrWhiteSpace(apiKey))
    {
        context.Response.StatusCode = 500;
        return context.Response.WriteAsync("Falta configurar la API key en el servidor.");
    }
    string claveRecibida = context.Request.Headers["X-Api-Key"].ToString();
    //Si es erronea se arroja 401
    if (claveRecibida != apiKey)
    {
        context.Response.StatusCode = 401;
        return context.Response.WriteAsync("API key ausente o incorrecta.");
    }

    //Tambien se verifica la cadena de MySQL
    string? cadenaConexion = context.RequestServices.GetRequiredService<IConfiguration>()["MySql"];
    //Si no esta configurada, se arroja 500.
    if (string.IsNullOrWhiteSpace(cadenaConexion))
    {
        context.Response.StatusCode = 500;
        return context.Response.WriteAsync("Falta configurar la conexión a MySQL en el servidor.");
    }

    return siguiente(context);
}