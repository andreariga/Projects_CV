using PokeAPI.Data;
using PokeAPI.Model;
using PokeAPI.ModelDTO;
using Microsoft.EntityFrameworkCore;

namespace PokeAPI.Endpoints
{
    public static class ImagesEndpoints
    {
        public static RouteGroupBuilder MapImagesEndpoints(this RouteGroupBuilder group)
        {
            // GET /images
            // Restituisce tutte le immagini
            group.MapGet("/images", async (PokeDbContext db) =>
                Results.Ok(await db.images.Select(i => new ImagesDTO(i)).AsNoTracking().ToListAsync()));

            // GET /images/{id}
            // Restituisce una immagine con l'id specificato
            group.MapGet("/images/{id}", async (PokeDbContext db, int id) =>
            {
                Images? image = await db.images.FindAsync(id);
                if (image is null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(new ImagesDTO(image));
            });

            // PUT /images/{id}
            // Modifica l'immagine con l'id specificato
            group.MapPut("/images/{id}", async (PokeDbContext db, int id, ImagesDTO imagesDTO) =>
            {
                // Verifico che l'immagine con l'id specificato esista
                Images? image = await db.images.FindAsync(id);
                if (image is null)
                {
                    return Results.NotFound();
                }
                // Modifico l'immagine
                image.Symbol = imagesDTO.Symbol;
                image.Logo = imagesDTO.Logo;
                image.Small = imagesDTO.Small;
                image.Large = imagesDTO.Large;
                // Salvo le modifiche
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.NoContent();
            });

            // POST /images
            // Crea una nuova immagine
            group.MapPost("/images", async (PokeDbContext db, ImagesDTO imagesDTO) =>
            {
                // Creo una nuova immagine
                Images image = new()
                {
                    Symbol = imagesDTO.Symbol,
                    Logo = imagesDTO.Logo,
                    Small = imagesDTO.Small,
                    Large = imagesDTO.Large
                };
                // Aggiungo l'immagine al database
                db.images.Add(image);
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.Created($"/images/{image.ImagesId}", new ImagesDTO(image));
            });

            // DELETE /images/{id}
            // Elimina l'immagine con l'id specificato
            group.MapDelete("/images/{id}", async (PokeDbContext db, int id) =>
            {
                // Verifico che l'immagine con l'id specificato esista
                Images? image = await db.images.FindAsync(id);
                if (image is null)
                {
                    return Results.NotFound();
                }
                // Elimino l'immagine
                db.Remove(image);
                await db.SaveChangesAsync();
                // Restituisco la risposta
                return Results.NoContent();
            });

            return group;
        }
    }
}
