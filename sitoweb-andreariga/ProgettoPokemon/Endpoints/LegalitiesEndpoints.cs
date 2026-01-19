using PokeAPI.Data;
using PokeAPI.Model;
using PokeAPI.ModelDTO;
using Microsoft.EntityFrameworkCore;

namespace PokeAPI.Endpoints
{
    public static class LegalitiesEndpoints
    {
        public static RouteGroupBuilder MapLegalitiesEndpoints(this RouteGroupBuilder group)
        {
            // GET /legalities
            // Restituisce tutte le legalità
            group.MapGet("/legalities", async (PokeDbContext db) =>
                Results.Ok(await db.legalities.Select(l => new LegalitiesDTO(l)).AsNoTracking().ToListAsync()));

            // GET /legalities/{id}
            // Restituisce una legalità con l'id specificato
            group.MapGet("/legalities/{id}", async (PokeDbContext db, int id) =>
            {
                Legalities? legalities = await db.legalities.FindAsync(id);
                if (legalities is null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(new LegalitiesDTO(legalities));
            });

            // PUT /legalities/{id}
            // Modifica la legalità con l'id specificato
            group.MapPut("/legalities/{id}", async (PokeDbContext db, int id, LegalitiesDTO legalitiesDTO) =>
            {
                // Verifico che la legalità con l'id specificato esista
                Legalities? legalities = await db.legalities.FindAsync(id);
                if (legalities is null)
                {
                    return Results.NotFound();
                }
                // Modifico la legalità
                legalities.Unlimited = legalitiesDTO.Unlimited;
                legalities.Expanded = legalitiesDTO.Expanded;
                // Salvo le modifiche
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.NoContent();
            });

            // POST /legalities
            // Crea una nuova legalità
            group.MapPost("/legalities", async (PokeDbContext db, LegalitiesDTO legalitiesDTO) =>
            {
                // Creo una nuova legalità
                Legalities legalities = new()
                {
                    Unlimited = legalitiesDTO.Unlimited,
                    Expanded = legalitiesDTO.Expanded
                };
                // Aggiungo la legalità al database
                db.legalities.Add(legalities);
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.Created($"/legalities/{legalities.LegalitiesId}", new LegalitiesDTO(legalities));
            });

            // DELETE /legalities/{id}
            // Elimina la legalità con l'id specificato
            group.MapDelete("/legalities/{id}", async (PokeDbContext db, int id) =>
            {
                // Verifico che la legalità con l'id specificato esista
                Legalities? legalities = await db.legalities.FindAsync(id);
                if (legalities is null)
                {
                    return Results.NotFound();
                }
                // Elimino la legalità
                db.Remove(legalities);
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.NoContent();
            });

            return group;
        }
    }
}
