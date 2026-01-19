using System;
using PokeAPI.Model;

namespace PokeAPI.ModelDTO;

public class UtenteDTO
{
	public int Id { get; set; }
	public string Username { get; set; } = null!;
	public string Password { get; set; } = null!;
	public bool IsAdmin { get; set; } = false;

	public UtenteDTO() {}
	public UtenteDTO(Utente u)
	{
		(Id, Username, Password, IsAdmin) = (u.Id, u.Username, u.Password, u.IsAdmin);
	}
}
