using System;
using PokeAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace PokeAPI.Data;

public class PokeDbContext:DbContext
{
	public PokeDbContext(DbContextOptions<PokeDbContext> opt):base(opt) {}
	
    public DbSet<Utente> utenti { get; set; } = null!;

    public DbSet<Datas> datas { get; set; } = null!;
    public DbSet<Attack> attacks { get; set; } = null!;
    public DbSet<Holofoil> holofoils { get; set; } = null!;
    public DbSet<Images> images { get; set; } = null!;
    public DbSet<Legalities> legalities { get; set; } = null!;
    public DbSet<Prices> prices { get; set; } = null!;
    public DbSet<Set> sets { get; set; } = null!;
    public DbSet<Tcgplayer> tcgplayers { get; set; } = null!;
    public DbSet<Weakness> weaknesses { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Datas>()
            .HasMany(d => d.Attacks)
            .WithOne(a => a.Datas)
            .HasForeignKey(a => a.DatasId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Datas>()
            .HasMany(d => d.Weaknesses)
            .WithOne(w => w.Datas)
            .HasForeignKey(w => w.DatasId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Datas>()
            .HasOne(d => d.Set)
            .WithMany()
            .HasForeignKey(d => d.SetId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Datas>()
            .HasOne(d => d.Legalities)
            .WithMany()
            .HasForeignKey(d => d.LegalitiesId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Datas>()
            .HasOne(d => d.Images)
            .WithMany()
            .HasForeignKey(d => d.ImagesId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Datas>()
            .HasOne(d => d.Tcgplayer)
            .WithMany()
            .HasForeignKey(d => d.TcgplayerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Tcgplayer>()
            .HasOne(t => t.Prices)
            .WithMany()
            .HasForeignKey(t => t.PricesId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Set>()
            .HasOne(s => s.Images)
            .WithMany()
            .HasForeignKey(s => s.ImagesId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Prices>()
            .HasOne(p => p.Holofoil)
            .WithOne(h => h.Prices)
            .HasForeignKey<Holofoil>(h => h.PricesId)
            .OnDelete(DeleteBehavior.SetNull);
    

    }
}