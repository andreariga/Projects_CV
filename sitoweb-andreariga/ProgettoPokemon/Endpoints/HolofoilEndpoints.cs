using PokeAPI.Data;
using PokeAPI.Model;
using PokeAPI.ModelDTO;
using Microsoft.EntityFrameworkCore;

namespace PokeAPI.Endpoints
{
    public static class HolofoilEndpoints
    {
        public static RouteGroupBuilder MapHolofoilEndpoints(this RouteGroupBuilder group)
        {
            // GET /holofoils
            // Restituisce tutti gli holofoils
            group.MapGet("/holofoils", async (PokeDbContext db) =>
                Results.Ok(await db.holofoils.Select(h => new HolofoilDTO(h)).AsNoTracking().ToListAsync()));

            // GET /holofoils/{id}
            // Restituisce un holofoil con l'id specificato
            group.MapGet("/holofoils/{id}", async (PokeDbContext db, int id) =>
            {
                Holofoil? holofoil = await db.holofoils.FindAsync(id);
                if (holofoil is null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(new HolofoilDTO(holofoil));
            });

            // PUT /holofoils/{id}
            // Modifica l'holofoil con l'id specificato
            group.MapPut("/holofoils/{id}", async (PokeDbContext db, int id, HolofoilDTO holofoilDTO) =>
            {
                // Verifico che l'holofoil con l'id specificato esista
                Holofoil? holofoil = await db.holofoils.FindAsync(id);
                if (holofoil is null)
                {
                    return Results.NotFound();
                }
                // Modifico l'holofoil
                holofoil.Low = holofoilDTO.Low;
                holofoil.Mid = holofoilDTO.Mid;
                holofoil.High = holofoilDTO.High;
                holofoil.Market = holofoilDTO.Market;
                holofoil.DirectLow = holofoilDTO.DirectLow;
                holofoil.PricesId = holofoilDTO.PricesId;
                // Salvo le modifiche
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.NoContent();
            });

            // POST /holofoils
            // Crea un nuovo holofoil
            group.MapPost("/holofoils", async (PokeDbContext db, HolofoilDTO holofoilDTO) =>
            {
                // Creo un nuovo holofoil
                Holofoil holofoil = new()
                {
                    Low = holofoilDTO.Low,
                    Mid = holofoilDTO.Mid,
                    High = holofoilDTO.High,
                    Market = holofoilDTO.Market,
                    DirectLow = holofoilDTO.DirectLow,
                    PricesId = holofoilDTO.PricesId
                };
                // Aggiungo l'holofoil al database
                db.holofoils.Add(holofoil);
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.Created($"/holofoils/{holofoil.HolofoilId}", new HolofoilDTO(holofoil));
            });

            // DELETE /holofoils/{id}
            // Elimina l'holofoil con l'id specificato
            group.MapDelete("/holofoils/{id}", async (PokeDbContext db, int id) =>
            {
                // Verifico che l'holofoil con l'id specificato esista
                Holofoil? holofoil = await db.holofoils.FindAsync(id);
                if (holofoil is null)
                {
                    return Results.NotFound();
                }
                // Elimino l'holofoil
                db.Remove(holofoil);
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.NoContent();
            });

            return group;
        }
    }
}