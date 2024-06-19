
using Infotrack.Domain.Interfaces;
using Infotrack.Domain.Services;
using Infotrack.Infrastructure.Context;
using Infotrack.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var AllowOrigins = "_myAllowOrigins";
const string Base_Address = "https://www.google.co.uk/search";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<ISearchRepository, SearchRepository>();
builder.Services.AddScoped<ISearchService, SearchService>();

//Add In-Memory db
//add it as scoped -> create one onstance for each request
builder.Services.AddDbContext<InfotrackDBContext>(options => options.UseInMemoryDatabase("InfotrackDB"));



//Add HttpClient with Name Client_A
builder.Services.AddHttpClient("Client_A", c =>
{
    c.BaseAddress = new Uri(Base_Address);
});

//Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowOrigins,
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:8080");
                    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.UseCors(AllowOrigins);

app.Run();
