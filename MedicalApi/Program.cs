using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(o =>
    {
        // Пустые и отсутствующие поля проверяем сами в контроллерах, с понятным текстом ошибки
        o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .ConfigureApiBehaviorOptions(o =>
    {
        // Ошибки разбора JSON (число вместо строки, дата не в том формате, пустое тело)
        // возвращаем таким же текстом, как наши проверки, а не английским ProblemDetails
        o.InvalidModelStateResponseFactory = context => new BadRequestObjectResult(DescribeError(context.ModelState));
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "v1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static string DescribeError(ModelStateDictionary modelState)
{
    List<string> keys = modelState.Where(e => e.Value?.Errors.Count > 0).Select(e => e.Key).ToList();

    // Ключ вида "$.birthDate" — поле JSON, которое не удалось прочитать
    string? field = keys.FirstOrDefault(k => k.StartsWith("$."));
    if (field != null)
    {
        return $"Поле «{field.Substring(2)}» заполнено неверно. Текст пишется в кавычках, число и true/false без кавычек, " +
            "дата в формате 2000-05-15 или 2026-11-10T09:30";
    }
    if (keys.Contains("$"))
    {
        return "Тело запроса не является правильным JSON. Проверьте кавычки, запятые и скобки";
    }
    if (keys.Contains(""))
    {
        return "Тело запроса пустое. Отправьте данные в формате JSON";
    }
    return $"Неверное значение параметра «{keys.FirstOrDefault()}»";
}
