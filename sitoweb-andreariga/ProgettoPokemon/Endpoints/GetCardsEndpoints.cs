using PokeAPI.Data;
using PokeAPI.Model;
using PokeAPI.ModelDTO;
using Microsoft.EntityFrameworkCore;

namespace PokeAPI.Endpoints
{
    public static class GetCardsEnpoints
    {
        public static RouteGroupBuilder MapGetCardsEnpoints(this RouteGroupBuilder group)
        {
group.MapPost("/import-card", async (PokeDbContext db, Datas importCardDTO) =>
{
    var set = await db.sets.FindAsync(importCardDTO.Set?.SetId);
    if (set == null && importCardDTO.Set != null)
    {
        set = new Set 
        { 
            SetId = importCardDTO.Set.SetId, 
            Name = importCardDTO.Set.Name, 
            Series = importCardDTO.Set.Series 
        };
        db.sets.Add(set);
        await db.SaveChangesAsync();
    }

    var legalities = await db.legalities.FindAsync(importCardDTO.Legalities?.LegalitiesId);
    if (legalities == null && importCardDTO.Legalities != null)
    {
        legalities = new Legalities 
        { 
            LegalitiesId = importCardDTO.Legalities.LegalitiesId, 
            Unlimited = importCardDTO.Legalities.Unlimited 
        };
        db.legalities.Add(legalities);
        await db.SaveChangesAsync();
    }

    Images images = null;
    if (importCardDTO.Images != null)
    {
        images = await db.images.FindAsync(importCardDTO.Images.ImagesId);
        if (images == null)
        {
            images = new Images 
            { 
                ImagesId = importCardDTO.Images.ImagesId, 
                Symbol = importCardDTO.Images.Symbol, 
                Logo = importCardDTO.Images.Logo,
                Large = importCardDTO.Images.Large,
                Small = importCardDTO.Images.Small,
            };
            db.images.Add(images);
            await db.SaveChangesAsync();
        }
    }

    Prices prices = null;
    Holofoil holofoil = null;
    if (importCardDTO.Tcgplayer?.Prices?.Holofoil != null)
    {
        holofoil = new Holofoil
        {
            Low = importCardDTO.Tcgplayer.Prices.Holofoil.Low,
            Mid = importCardDTO.Tcgplayer.Prices.Holofoil.Mid,
            High = importCardDTO.Tcgplayer.Prices.Holofoil.High,
            Market = importCardDTO.Tcgplayer.Prices.Holofoil.Market,
            DirectLow = importCardDTO.Tcgplayer.Prices.Holofoil.DirectLow
        };

        prices = new Prices { Holofoil = holofoil };
        db.prices.Add(prices);
        await db.SaveChangesAsync();

        holofoil.PricesId = prices.PricesId;
        await db.SaveChangesAsync();
    }

    Tcgplayer tcgplayer = null;
    if (importCardDTO.Tcgplayer != null)
    {
        tcgplayer = new Tcgplayer 
        { 
            Url = importCardDTO.Tcgplayer.Url, 
            UpdatedAt = importCardDTO.Tcgplayer.UpdatedAt,
            PricesId = prices?.PricesId,
            Prices = prices
        };
        db.tcgplayers.Add(tcgplayer);
        await db.SaveChangesAsync();
    }
    Datas datas = new()
    {
        Name = importCardDTO.Name,
        Supertype = importCardDTO.Supertype,
        Hp = importCardDTO.Hp,
        Number = importCardDTO.Number,
        Artist = importCardDTO.Artist,
        Rarity = importCardDTO.Rarity,
        ConvertedRetreatCost = importCardDTO.ConvertedRetreatCost,
        SetId = set?.SetId,
        LegalitiesId = legalities?.LegalitiesId,
        ImagesId = images?.ImagesId,  
        TcgplayerId = tcgplayer?.TcgplayerId
    };
    db.datas.Add(datas);
    await db.SaveChangesAsync();

    // 🔹 AGGIUNTA ATTACCHI
    if (importCardDTO.Attacks != null && importCardDTO.Attacks.Any())
    {
        foreach (var attack in importCardDTO.Attacks)
        {
            var newAttack = new Attack
            {
                Name = attack.Name,
                Cost = attack.Cost,
                ConvertedEnergyCost = attack.ConvertedEnergyCost,
                Damage = attack.Damage,
                Text = attack.Text,
                DatasId = datas.DatasId
            };
            db.attacks.Add(newAttack);
        }
        await db.SaveChangesAsync();
    }

    // 🔹 AGGIUNTA DEBOLEZZE
    if (importCardDTO.Weaknesses != null && importCardDTO.Weaknesses.Any())
    {
        foreach (var weakness in importCardDTO.Weaknesses)
        {
            var newWeakness = new Weakness
            {
                Type = weakness.Type,
                Value = weakness.Value,
                DatasId = datas.DatasId
            };
            db.weaknesses.Add(newWeakness);
        }
        await db.SaveChangesAsync();
    }

    await db.SaveChangesAsync();
    return Results.Created($"/datas/{datas.DatasId}", new DatasDTO(datas));
});

group.MapGet("/get-card", async (PokeDbContext db) =>
{
    var cards = await db.datas
        .Include(d => d.Set)
        .Include(d => d.Legalities)
        .Include(d => d.Images)
        .Include(d => d.Tcgplayer)
        .ThenInclude(t => t.Prices)
        .ThenInclude(p => p.Holofoil)
        .Include(d => d.Attacks)
        .Include(d => d.Weaknesses)
        .ToListAsync();

    var result = cards.Select(card => new
    {
        card.DatasId,
        card.Name,
        card.Supertype,
        card.Hp,
        card.Number,
        card.Artist,
        card.Rarity,
        card.ConvertedRetreatCost,
        Set = new
        {
            card?.Set?.SetId,
            card?.Set?.Name,
            card?.Set?.Series
        },
        Legalities = new
        {
            card?.Legalities?.LegalitiesId,
            card?.Legalities?.Unlimited
        },
        Images = new
        {
            card?.Images?.ImagesId,
            card?.Images?.Symbol,
            card?.Images?.Logo,
            card?.Images?.Large,
            card?.Images?.Small,
        },
        Tcgplayer = new
        {
            card?.Tcgplayer?.TcgplayerId,
            card?.Tcgplayer?.Url,
            card?.Tcgplayer?.UpdatedAt,
            Prices = new
            {
                card?.Tcgplayer?.Prices?.Holofoil?.Low,
                card?.Tcgplayer?.Prices?.Holofoil?.Mid,
                card?.Tcgplayer?.Prices?.Holofoil?.High,
                card?.Tcgplayer?.Prices?.Holofoil?.Market
            }
        },
        Attacks = card?.Attacks?.Select(a => new
        {
            a.AttackId,
            a.Name,
            a.Damage,
            a.Text,
            a.Cost,
            a.ConvertedEnergyCost,
        }).ToList(),
        Weaknesses = card?.Weaknesses?.Select(w => new
        {
            w.WeaknessId,
            w.Type,
            w.Value
        }).ToList()
    }).ToList();

    return Results.Ok(result);
});

group.MapDelete("/delete-card/{id}", async (PokeDbContext db, int id) =>
{
    var card = await db.datas
        .Include(d => d.Attacks)
        .Include(d => d.Weaknesses)
        .Include(d => d.Set)
        .Include(d => d.Legalities)
        .Include(d => d.Images)
        .Include(d => d.Tcgplayer)
        .ThenInclude(t => t.Prices)
        .ThenInclude(p => p.Holofoil)
        .FirstOrDefaultAsync(d => d.DatasId == id);

    if (card == null)
    {
        return Results.NotFound("Carta non trovata");
    }

    if (card.Tcgplayer?.Prices?.Holofoil != null)
    {
        db.holofoils.Remove(card.Tcgplayer.Prices.Holofoil);
    }

    if (card.Tcgplayer?.Prices != null)
    {
        db.prices.Remove(card.Tcgplayer.Prices);
    }

    if (card.Tcgplayer != null)
    {
        db.tcgplayers.Remove(card.Tcgplayer);
    }

    db.attacks.RemoveRange(card.Attacks);
    db.weaknesses.RemoveRange(card.Weaknesses);
    db.images.Remove(card.Images);
    db.legalities.Remove(card.Legalities);
    db.sets.Remove(card.Set);
    db.datas.Remove(card);

    await db.SaveChangesAsync();
    return Results.Ok("Carta eliminata con successo");
});

        return group;
        }   
    }
}