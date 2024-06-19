namespace Infotrack.Infrastructure.Context;

using Infotrack.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;

public class InfotrackDBContext : DbContext
{
    public InfotrackDBContext(DbContextOptions<InfotrackDBContext> options) : base(options){}

    public DbSet<Search> Searches { get; set; }
}