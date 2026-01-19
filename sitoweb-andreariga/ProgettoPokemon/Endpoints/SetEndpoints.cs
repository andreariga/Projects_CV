using PokeAPI.Data;
using PokeAPI.Model;
using PokeAPI.ModelDTO;
using Microsoft.EntityFrameworkCore;

namespace PokeAPI.Endpoints
{
    public static class SetEndpoints
    {
        public static RouteGroupBuilder MapSetEndpoints(this RouteGroupBuilder group)
        {
            // GET /sets
            // Restituisce tutti i set
            group.MapGet("/sets", async (PokeDbContext db) =>
                Results.Ok(await db.sets.Select(s => new SetDTO(s)).AsNoTracking().ToListAsync()));

            // GET /sets/{id}
            // Restituisce un set con l'id specificato
            group.MapGet("/sets/{id}", async (PokeDbContext db, int id) =>
            {
                Set? set = await db.sets.FindAsync(id);
                if (set is null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(new SetDTO(set));
            });

            // PUT /sets/{id}
            // Modifica un set con l'id specificato
            group.MapPut("/sets/{id}", async (PokeDbContext db, int id, SetDTO setDTO) =>
            {
                // Verifico che il set con l'id specificato esista
                Set? set = await db.sets.FindAsync(id);
                if (set is null)
                {
                    return Results.NotFound();
                }
                // Modifico i dati del set
                set.Identificatore = setDTO.Identificatore;
                set.Name = setDTO.Name;
                set.Series = setDTO.Series;
                set.PrintedTotal = setDTO.PrintedTotal;
                set.Total = setDTO.Total;
                set.PtcgoCode = setDTO.PtcgoCode;
                set.ReleaseDate = setDTO.ReleaseDate;
                set.UpdatedAt = setDTO.UpdatedAt;
                // Salvo le modifiche
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.NoContent();
            });

            // POST /sets
            // Crea un nuovo set
            group.MapPost("/sets", async (PokeDbContext db, SetDTO setDTO) =>
            {
                // Creo un nuovo set
                Set set = new()
                {
                    Identificatore = setDTO.Identificatore,
                    Name = setDTO.Name,
                    Series = setDTO.Series,
                    PrintedTotal = setDTO.PrintedTotal,
                    Total = setDTO.Total,
                    PtcgoCode = setDTO.PtcgoCode,
                    ReleaseDate = setDTO.ReleaseDate,
                    UpdatedAt = setDTO.UpdatedAt
                };
                // Aggiungo il nuovo set al database
                db.sets.Add(set);
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.Created($"/sets/{set.SetId}", new SetDTO(set));
            });

            // DELETE /sets/{id}
            // Elimina un set con l'id specificato
            group.MapDelete("/sets/{id}", async (PokeDbContext db, int id) =>
            {
                // Verifico che il set con l'id specificato esista
                Set? set = await db.sets.FindAsync(id);
                if (set is null)
                {
                    return Results.NotFound();
                }
                // Elimino il set
                db.Remove(set);
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.NoContent();
            });

            return group;
        }
    }
}
