using PokeAPI.Data;
using PokeAPI.Model;
using PokeAPI.ModelDTO;
using Microsoft.EntityFrameworkCore;

namespace PokeAPI.Endpoints
{
    public static class TcgplayerEndpoints
    {
        public static RouteGroupBuilder MapTcgplayerEndpoints(this RouteGroupBuilder group)
        {
            // GET /tcgplayers
            // Restituisce tutti i tcgplayers
            group.MapGet("/tcgplayers", async (PokeDbContext db) =>
                Results.Ok(await db.tcgplayers.Select(t => new TcgplayerDTO(t)).AsNoTracking().ToListAsync()));

            // GET /tcgplayers/{id}
            // Restituisce un tcgplayer con l'id specificato
            group.MapGet("/tcgplayers/{id}", async (PokeDbContext db, int id) =>
            {
                Tcgplayer? tcgplayer = await db.tcgplayers.FindAsync(id);
                if (tcgplayer is null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(new TcgplayerDTO(tcgplayer));
            });

            // PUT /tcgplayers/{id}
            // Modifica un tcgplayer con l'id specificato
            group.MapPut("/tcgplayers/{id}", async (PokeDbContext db, int id, TcgplayerDTO tcgplayerDTO) =>
            {
                // Verifico che il tcgplayer con l'id specificato esista
                Tcgplayer? tcgplayer = await db.tcgplayers.FindAsync(id);
                if (tcgplayer is null)
                {
                    return Results.NotFound();
                }
                // Modifico i dati del tcgplayer
                tcgplayer.Url = tcgplayerDTO.Url;
                tcgplayer.UpdatedAt = tcgplayerDTO.UpdatedAt;
                // Salvo le modifiche
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.NoContent();
            });

            // POST /tcgplayers
            // Crea un nuovo tcgplayer
            group.MapPost("/tcgplayers", async (PokeDbContext db, TcgplayerDTO tcgplayerDTO) =>
            {
                // Creo un nuovo tcgplayer
                Tcgplayer tcgplayer = new()
                {
                    Url = tcgplayerDTO.Url,
                    UpdatedAt = tcgplayerDTO.UpdatedAt
                };
                // Aggiungo il nuovo tcgplayer al database
                db.tcgplayers.Add(tcgplayer);
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.Created($"/tcgplayers/{tcgplayer.TcgplayerId}", new TcgplayerDTO(tcgplayer));
            });

            // DELETE /tcgplayers/{id}
            // Elimina un tcgplayer con l'id specificato
            group.MapDelete("/tcgplayers/{id}", async (PokeDbContext db, int id) =>
            {
                // Verifico che il tcgplayer con l'id specificato esista
                Tcgplayer? tcgplayer = await db.tcgplayers.FindAsync(id);
                if (tcgplayer is null)
                {
                    return Results.NotFound();
                }
                // Elimino il tcgplayer
                db.Remove(tcgplayer);
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.NoContent();
            });

            return group;
        }
    }
}