using PokeAPI.Data;
using PokeAPI.Model;
using PokeAPI.ModelDTO;
using Microsoft.EntityFrameworkCore;

namespace PokeAPI.Endpoints
{
    public static class PricesEndpoints
    {
        public static RouteGroupBuilder MapPricesEndpoints(this RouteGroupBuilder group)
        {
            // GET /prices
            // Restituisce tutte le voci di Prices
            group.MapGet("/prices", async (PokeDbContext db) =>
                Results.Ok(await db.prices.Select(p => new PricesDTO(p)).AsNoTracking().ToListAsync()));

            // GET /prices/{id}
            // Restituisce una singola voce di Prices con l'id specificato
            group.MapGet("/prices/{id}", async (PokeDbContext db, int id) =>
            {
                Prices? prices = await db.prices.FindAsync(id);
                if (prices is null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(new PricesDTO(prices));
            });

            // PUT /prices/{id}
            // Modifica una voce di Prices con l'id specificato
            group.MapPut("/prices/{id}", async (PokeDbContext db, int id, PricesDTO pricesDTO) =>
            {
                // Verifico che la voce di Prices con l'id specificato esista
                Prices? prices = await db.prices.FindAsync(id);
                if (prices is null)
                {
                    return Results.NotFound();
                }
                // Modifico i dati di Prices
                prices.PricesId = pricesDTO.PricesId;
                // Salvo le modifiche
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.NoContent();
            });

            // POST /prices
            // Crea una nuova voce di Prices
            group.MapPost("/prices", async (PokeDbContext db, PricesDTO pricesDTO) =>
            {
                // Creo una nuova voce di Prices
                Prices prices = new()
                {
                    PricesId = pricesDTO.PricesId,
                };
                // Aggiungo la nuova voce di Prices al database
                db.prices.Add(prices);
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.Created($"/prices/{prices.PricesId}", new PricesDTO(prices));
            });

            // DELETE /prices/{id}
            // Elimina una voce di Prices con l'id specificato
            group.MapDelete("/prices/{id}", async (PokeDbContext db, int id) =>
            {
                // Verifico che la voce di Prices con l'id specificato esista
                Prices? prices = await db.prices.FindAsync(id);
                if (prices is null)
                {
                    return Results.NotFound();
                }
                // Elimino la voce di Prices
                db.Remove(prices);
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.NoContent();
            });

            return group;
        }
    }
}
