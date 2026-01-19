using PokeAPI.Data;
using PokeAPI.Model;
using PokeAPI.ModelDTO;
using Microsoft.EntityFrameworkCore;

namespace PokeAPI.Endpoints
{
    public static class AttackEndpoints
    {
        public static RouteGroupBuilder MapAttackEndpoints(this RouteGroupBuilder group)
        {
            // GET /attacks
            // Restituisce tutti gli attacchi
            group.MapGet("/attacks", async (PokeDbContext db) =>
                Results.Ok(await db.attacks.Select(a => new AttackDTO(a)).AsNoTracking().ToListAsync()));

            // GET /attacks/{id}
            // Restituisce un attacco con l'id specificato
            group.MapGet("/attacks/{id}", async (PokeDbContext db, int id) =>
            {
                Attack? attack = await db.attacks.FindAsync(id);
                if (attack is null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(new AttackDTO(attack));
            });

            // PUT /attacks/{id}
            // Modifica l'attacco con l'id specificato
            group.MapPut("/attacks/{id}", async (PokeDbContext db, int id, AttackDTO attackDTO) =>
            {
                // Verifico che l'attacco con l'id specificato esista
                Attack? attack = await db.attacks.FindAsync(id);
                if (attack is null)
                {
                    return Results.NotFound();
                }
                // Modifico l'attacco
                attack.Name = attackDTO.Name;
                attack.Cost = attackDTO.Cost;
                attack.ConvertedEnergyCost = attackDTO.ConvertedEnergyCost;
                attack.Damage = attackDTO.Damage;
                attack.Text = attackDTO.Text;
                attack.DatasId = attackDTO.DatasId;
                // Salvo le modifiche
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.NoContent();
            });

            // POST /attacks
            // Crea un nuovo attacco
            group.MapPost("/attacks", async (PokeDbContext db, AttackDTO attackDTO) =>
            {
                // Creo un nuovo attacco
                Attack attack = new()
                {
                    Name = attackDTO.Name,
                    Cost = attackDTO.Cost,
                    ConvertedEnergyCost = attackDTO.ConvertedEnergyCost,
                    Damage = attackDTO.Damage,
                    Text = attackDTO.Text,
                    DatasId = attackDTO.DatasId
                };
                // Aggiungo l'attacco al database
                db.attacks.Add(attack);
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.Created($"/attacks/{attack.AttackId}", new AttackDTO(attack));
            });

            // DELETE /attacks/{id}
            // Elimina l'attacco con l'id specificato
            group.MapDelete("/attacks/{id}", async (PokeDbContext db, int id) =>
            {
                // Verifico che l'attacco con l'id specificato esista
                Attack? attack = await db.attacks.FindAsync(id);
                if (attack is null)
                {
                    return Results.NotFound();
                }
                // Elimino l'attacco
                db.Remove(attack);
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.NoContent();
            });

            return group;
        }
    }
}
