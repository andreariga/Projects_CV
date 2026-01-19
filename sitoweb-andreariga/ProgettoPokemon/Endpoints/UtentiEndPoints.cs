using System; 
using PokeAPI.Data;
using PokeAPI.Model;
using PokeAPI.ModelDTO;
using Microsoft.EntityFrameworkCore;

namespace PokeAPI.Endpoints;

public static class UtentiEndpoints
{
	public static RouteGroupBuilder MapUtentiEndpoints(this RouteGroupBuilder group)
	{
		//GET /utenti
		//restituisce tutti gli utenti
		group.MapGet("/utenti", async (PokeDbContext db)=> Results.Ok(await db.utenti.Select(u =>new UtenteDTO(u)).AsNoTracking().ToListAsync()));

		//GET /utenti/{id}
		//restituisce gli utenti con l'id specificato
		group.MapGet("/utenti/{id}", async (PokeDbContext db, int id)=>
		{
			Utente? utenti = await db.utenti.FindAsync(id);
			if(utenti is null)
			{
				return Results.NotFound();
			}
			return Results.Ok(new UtenteDTO(utenti));
		});

		//PUT /utenti/{id} 
		//modifica gli utenti con l'id specificato
		group.MapPut("/utenti/{id}", async (PokeDbContext db, int id, UtenteDTO utenteDTO)=>
		{
			//verifico che l'utente con l'id specificato esista
			Utente? utenti = await db.utenti.FindAsync(id);
			if(utenti is null)
			{
				return Results.NotFound();
			}
			//modifico l'utente
			utenti.Username = utenteDTO.Username;
			utenti.Password = utenteDTO.Password;
			utenti.IsAdmin = utenteDTO.IsAdmin;
			//salvo il utente modificato
			await db.SaveChangesAsync();
			//restituisco la risposta
			return Results.NoContent();
		});
		
		//POST /utenti
		//crea un nuovo utente
		group.MapPost("/utenti", async (PokeDbContext db, UtenteDTO utenteDTO)=>
		{
			//creo un nuovo utente
			Utente utente = new()
			{
				Username = utenteDTO.Username,
				Password = utenteDTO.Password,
				IsAdmin = utenteDTO.IsAdmin
			};
			//aggiungo il utente al database
			db.utenti.Add(utente);
			await db.SaveChangesAsync();
			//restituisco la risposta
			return Results.Created($"/utenti/{utente.Id}", new UtenteDTO(utente));
		});

		//DELETE /utenti/{id}
		//elimina gli utenti con l'id specificato
		group.MapDelete("/utenti/{id}", async (PokeDbContext db, int id) => 
		{
			//verifico che il utente con l'id specificato esista
			Utente? utente = await db.utenti.FindAsync(id);
			if( utente is null)
			{
				return Results.NotFound();
			}
			//elimino l'utente
			db.Remove(utente);
			await db.SaveChangesAsync();
			//restituisco la risposta
			return Results.NoContent();
		});
		
		return group;
	}

}
