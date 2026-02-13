using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using RetroScrap3000.Models;

public sealed class ScrapGame
{
	public string? Name { get; set; }
	public string? Id { get; set; }
	public string? Source { get; set; }
	public string? Description { get; set; }
	public string? Genre { get; set; }
	public string? Players { get; set; }
	public string? Developer { get; set; }
	public string? Publisher { get; set; }
	public double? RatingNormalized { get; set; }  // 0..1
	public string? ReleaseDateRaw { get; set; }
	[JsonIgnore]
	public DateTime? ReleaseDate
	{
		get
		{
			if (string.IsNullOrEmpty(ReleaseDateRaw))
				return null;
			if (DateTime.TryParseExact(
							ReleaseDateRaw,
							"yyyy-MM-dd",
							null,
							System.Globalization.DateTimeStyles.None,
							out var dt))
				return dt;
			if (int.TryParse(ReleaseDateRaw, out int year) && year > 1900 && year < 3000)
				return new DateTime(year, 1, 1);
			return null;
		}
	}

	[JsonIgnore]
	public List<GameMediaSettings> PossibleMedien { get; private set; }
		

	public ScrapGame()
	{
		PossibleMedien = [.. AppSettings.GetMediaSettingsList()];
	}

	public ScrapGame CopyFrom(GameDataBase game, AppSettings opt)
	{
		ScrapGame g = new ScrapGame()
		{
			Description = game.GetDesc(opt),
			Developer = game.developpeur?.text,
			Genre = game.GetGenre(opt),
			Id = game.id,
			Name = game.GetName(opt),
			Players = game.joueurs?.text,
			Publisher = game.editeur?.text,
			ReleaseDateRaw = game.GetReleaseDate(opt),
			RatingNormalized = this.RatingNormalized,
			Source = this.Source,
		};

		g.PossibleMedien = [.. this.PossibleMedien];
		return g;
	}

}

 public class ScrapGameApiResponse
{
    public bool Ok { get; set; } = false;
    public ScrapGame? ScrapGameResult { get; set; }
    public string? Error { get; set; }
    public int? HttpCode { get; set; }

}

public class ScrapRechercheApiResponse
{
    public bool Ok { get; set; } = false;
    public List<GameDataRecherce> RechercheResult { get; set; } = new();
    public string? Error { get; set; }
    public int? HttpCode { get; set; }

}

public class GameRechercheRoot { public GameRechercheResponse? response { get; set; } }
public class GameRechercheResponse
{
	public object? serveurs { get; set; }
	public SsUser? ssuser { get; set; }
	public GameDataRecherce[]? jeux { get; set; }
}


public class GameRoot { public GameResponse? response { get; set; } }
public class GameResponse
{
	public SsUser? ssuser { get; set; }
	public GameData? jeu { get; set; }
}

public class GameDataBase
{
	public string? id { get; set; }
	public RegTxtObj[]? noms { get; set; }
	public IdText? editeur { get; set; }
	public IdText? developpeur { get; set; }
	public TxtObj? joueurs { get; set; }
	public LangTextObj[]? synopsis { get; set; }
	public RegTxtObj[]? dates { get; set; }
	public Genre[]? genres { get; set; }
	public Medium[]? medias { get; set; }

	public string? GetName(AppSettings opt)
	{
		if (noms != null)
		{
			var rrname = noms.FirstOrDefault(x => x.region != null && x.region == opt.Region);
			if (rrname == null)
				rrname = noms.FirstOrDefault(x => x.region != null && x.region.ToLower() == "wor");
			if (rrname == null)
				rrname = noms[0];

			return rrname.text;
		}

		return null;
	}

	public string? GetReleaseDate(AppSettings opt)
	{
		if (dates == null || dates.Count() <= 1)
			return null;
		var date = dates.FirstOrDefault(x => x.region == opt.Region);
		if (date == null)
		{
			date = dates.FirstOrDefault(x => x.region == "eu");
			if (date == null)
				return dates[0].text;
		}
		return date.text;
	}

	public string? GetDesc(AppSettings opt)
	{
		string? retVal = null;
		if (synopsis != null && synopsis.Length > 0)
		{
			var desc = synopsis.FirstOrDefault(x => x.langue != null && x.langue.ToLower() == opt.GetLanguageShortCode());
			if (desc != null)
			{
				retVal = desc.text;
			}
			else
			{
				// Default ist englisch
				desc = synopsis.FirstOrDefault(x => x.langue != null && x.langue.ToLower() == "en");
				if (desc != null)
					retVal = desc.text;
			}
		}
		return retVal;
	}

	public string? GetGenre(AppSettings opt)
	{
		LangTextObj? retVal = null;
		if (genres != null && genres.Length > 0)
		{
			foreach (var g in genres)
			{
				retVal = g.noms?.FirstOrDefault(x => x.langue != null && x.langue == opt.GetLanguageShortCode());
				if (retVal == null)
					retVal = g.noms?.FirstOrDefault(x => x.langue != null && x.langue == "en");
				if (retVal != null)
					break;
			}
		}

		return retVal?.text;
	}

}

public class GameData : GameDataBase
{
	public string? romid { get; set; }
	public TxtObj? note { get; set; } 
	public ApiRom[]? roms { get; set; }
	public ApiRom? rom { get; set; }
}

public class GameDataRecherce : GameDataBase
{
	public IdText? systeme { get; set; }
	public string? topstaff { get; set; }
	public string? rotation { get; set; }
	public string? controles { get; set; }
	public string? couleurs { get; set; }
	public Family[]? familles { get; set; }

}

public class IdText { public string? id { get; set; } public string? text { get; set; } }
public class RegTxtObj { public string? region { get; set; } public string? text { get; set; } }
public class TxtObj { public string? text { get; set; } }
public class Genre { public string? id { get; set; } public string? principale { get; set; } public LangTextObj[]? noms { get; set; } }
public class Medium { public string? type { get; set; } public string? url { get; set; } public string? parent { get; set; } public string? region { get; set; } }
public class Family { public string? id { get; set; } public string? nomcourt { get; set; } public string? principale { get; set; } public string? parentid { get; set; } public List<RegTxtObj>? noms { get; set; } }
public class LangTextObj { public string? langue { get; set; } public string? text { get; set; } }

public class ApiRom
{
	public string? id { get; set; }
	public string? romsize { get; set; }
	public string? romfilename { get; set; }
	public string? romnumsupport { get; set; }
	public string? romtotalsupport { get; set; }
	public string? romcloneof { get; set; }
	public string? romcrc { get; set; }
	public string? rommd5 { get; set; }
	public string? romsha1 { get; set; }
	public string? beta { get; set; }
	public string? demo { get; set; }
	public string? proto { get; set; }
	public string? trad { get; set; }
	public string? hack { get; set; }
	public string? unl { get; set; }
	public string? alt { get; set; }
	public string? best { get; set; }
	public string? netplay { get; set; }
	public string? nbscrap { get; set; }
	
	[JsonIgnore]
	public Tools.Checksums CheckSums 
	{
		get
		{
			return new Tools.Checksums(romfilename)
			{
				SHA1 = this.romsha1 ?? "",
				MD5 = this.rommd5 ?? "",
				CRC32 = this.romcrc ?? ""
			};
		}
	}
}