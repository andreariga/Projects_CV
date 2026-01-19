using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PokeAPI.Model;

public class Utente
{
	public int Id { get; set; }
	public string Username { get; set; } = null!;
	public string Password { get; set; } = null!;
    public bool IsAdmin { get; set; } = false;

}
