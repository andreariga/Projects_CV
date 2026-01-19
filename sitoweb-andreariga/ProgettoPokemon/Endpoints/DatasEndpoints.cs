using PokeAPI.Data;
using PokeAPI.Model;
using PokeAPI.ModelDTO;
using Microsoft.EntityFrameworkCore;

namespace PokeAPI.Endpoints
{
    public static class DatasEndpoints
    {
        public static RouteGroupBuilder MapDatasEndpoints(this RouteGroupBuilder group)
        {
            // GET /datas
            // Restituisce tutte le datas
            group.MapGet("/datas", async (PokeDbContext db) =>
                Results.Ok(await db.datas.Select(d => new DatasDTO(d)).AsNoTracking().ToListAsync()));

            // GET /datas/{id}
            // Restituisce le datas con l'id specificato
            group.MapGet("/datas/{id}", async (PokeDbContext db, int id) =>
            {
                Datas? datas = await db.datas.FindAsync(id);
                if (datas is null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(new DatasDTO(datas));
            });

            // PUT /datas/{id}
            // Modifica le datas con l'id specificato
            group.MapPut("/datas/{id}", async (PokeDbContext db, int id, DatasDTO datasDTO) =>
            {
                // Verifico che la datas con l'id specificato esista
                Datas? datas = await db.datas.FindAsync(id);
                if (datas is null)
                {
                    return Results.NotFound();
                }
                // Modifico la datas
                datas.Name = datasDTO.Name;
                datas.Supertype = datasDTO.Supertype;
                datas.Hp = datasDTO.Hp;
                datas.Number = datasDTO.Number;
                datas.Artist = datasDTO.Artist;
                datas.Rarity = datasDTO.Rarity;
                datas.ConvertedRetreatCost = datasDTO.ConvertedRetreatCost;
                datas.SetId = datasDTO.SetId;
                datas.LegalitiesId = datasDTO.LegalitiesId;
                datas.ImagesId = datasDTO.ImagesId;
                datas.TcgplayerId = datasDTO.TcgplayerId;
                // Salvo le modifiche
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.NoContent();
            });

            // POST /datas
            // Crea una nuova datas
            group.MapPost("/datas", async (PokeDbContext db, DatasDTO datasDTO) =>
            {
                // Creo una nuova datas
                Datas datas = new()
                {
                    Name = datasDTO.Name,
                    Supertype = datasDTO.Supertype,
                    Hp = datasDTO.Hp,
                    Number = datasDTO.Number,
                    Artist = datasDTO.Artist,
                    Rarity = datasDTO.Rarity,
                    ConvertedRetreatCost = datasDTO.ConvertedRetreatCost,
                    SetId = datasDTO.SetId,
                    LegalitiesId = datasDTO.LegalitiesId,
                    ImagesId = datasDTO.ImagesId,
                    TcgplayerId = datasDTO.TcgplayerId
                };
                // Aggiungo la datas al database
                db.datas.Add(datas);
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.Created($"/datas/{datas.DatasId}", new DatasDTO(datas));
            });

            // DELETE /datas/{id}
            // Elimina le datas con l'id specificato
            group.MapDelete("/datas/{id}", async (PokeDbContext db, int id) =>
            {
                // Verifico che la datas con l'id specificato esista
                Datas? datas = await db.datas.FindAsync(id);
                if (datas is null)
                {
                    return Results.NotFound();
                }
                // Elimino la datas
                db.Remove(datas);
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.NoContent();
            });

            return group;
        }
    }
}
