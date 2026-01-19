using PokeAPI.Data;
using PokeAPI.Model;
using PokeAPI.ModelDTO;
using Microsoft.EntityFrameworkCore;

namespace PokeAPI.Endpoints
{
    public static class WeaknessEndpoints
    {
        public static RouteGroupBuilder MapWeaknessEndpoints(this RouteGroupBuilder group)
        {
            // GET /weaknesses
            // Restituisce tutte le debolezze
            group.MapGet("/weaknesses", async (PokeDbContext db) =>
                Results.Ok(await db.weaknesses.Select(w => new WeaknessDTO(w)).AsNoTracking().ToListAsync()));

            // GET /weaknesses/{id}
            // Restituisce una debolezza con l'id specificato
            group.MapGet("/weaknesses/{id}", async (PokeDbContext db, int id) =>
            {
                Weakness? weakness = await db.weaknesses.FindAsync(id);
                if (weakness is null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(new WeaknessDTO(weakness));
            });

            // PUT /weaknesses/{id}
            // Modifica una debolezza con l'id specificato
            group.MapPut("/weaknesses/{id}", async (PokeDbContext db, int id, WeaknessDTO weaknessDTO) =>
            {
                // Verifico che la debolezza con l'id specificato esista
                Weakness? weakness = await db.weaknesses.FindAsync(id);
                if (weakness is null)
                {
                    return Results.NotFound();
                }
                // Modifico i dati della debolezza
                weakness.Type = weaknessDTO.Type;
                weakness.Value = weaknessDTO.Value;
                // Salvo le modifiche
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.NoContent();
            });

            // POST /weaknesses
            // Crea una nuova debolezza
            group.MapPost("/weaknesses", async (PokeDbContext db, WeaknessDTO weaknessDTO) =>
            {
                // Creo una nuova debolezza
                Weakness weakness = new()
                {
                    Type = weaknessDTO.Type,
                    Value = weaknessDTO.Value
                };
                // Aggiungo la nuova debolezza al database
                db.weaknesses.Add(weakness);
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.Created($"/weaknesses/{weakness.WeaknessId}", new WeaknessDTO(weakness));
            });

            // DELETE /weaknesses/{id}
            // Elimina una debolezza con l'id specificato
            group.MapDelete("/weaknesses/{id}", async (PokeDbContext db, int id) =>
            {
                // Verifico che la debolezza con l'id specificato esista
                Weakness? weakness = await db.weaknesses.FindAsync(id);
                if (weakness is null)
                {
                    return Results.NotFound();
                }
                // Elimino la debolezza
                db.Remove(weakness);
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.NoContent();
            });

            return group;
        }
    }
}
