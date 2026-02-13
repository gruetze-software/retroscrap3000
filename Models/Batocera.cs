using System;
using System.Collections.Generic;
using RetroScrap3000.Models;

public static class BatoceraFolders
{
// Quelle: https://wiki.batocera.org/systems (System shortname == ROM-Ordner)
// Stand: 2025 (inkl. Korrekturen für Model 2/3/Chihiro)
public static readonly HashSet<string> All = new(StringComparer.OrdinalIgnoreCase)
  {
      // Arcade
      "mame","fbneo","dice","daphne","singe","model2","model3","naomi","naomi2",
      "namco2x6","triforce","atomiswave","lindbergh","model1","chihiro","hikaru",

      // Home console
      "channelf","atari2600","odyssey2","astrocde","apfm1000","vc4000",
      "intellivision","atari5200","colecovision","advision","vectrex","crvision","arcadia",
      "nes","sg1000","multivision","videopacplus","pv1000","scv","mastersystem",
      "fds","atari7800","socrates","snes_msu-1","pcengine","megadrive","pcenginecd",
      "supergrafx","snes","neogeo","cdi","amigacdtv","gx4000","segacd","megacd","pico","sgb","supracan",
      "jaguar","3do","amigacd32","sega32x","psx","pcfx","neogeocd","saturn",
      "virtualboy","satellaview","jaguarcd","sufami","n64","dreamcast","n64dd","ps2",
      "gamecube","xbox","vsmile","xbox360","wii","ps3","wiiu","ps4",

      // Portable
      "gameandwatch","lcdgames","gamepock","gb","gb2players","lynx","gamegear",
      "gamate","gmaster","supervision","megaduck","gamecom","gbc","gbc2players",
      "ngp","ngpc","wswan","wswanc","gba","pokemini","gp32","nds","psp","3ds","psvita",

      // Fantasy & Computer
      "uzebox","voxatron","pico8","tic80","lowresnx","wasm4","pyxel","vircon32","arduboy",
      "apple2","atari800","c64","msx1","msx2","msx2+","msxturbor","amiga500","amiga1200",
      "atarist","x68000","pc98","pc88","zxspectrum","dos","windows"
  };

// Bekannte Alias-Korrekturen/Fallspezifika zwischen Ökosystemen:
private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
{
	// Nintendo
	["Nintendo 3DS"] = "3ds",
	["3DS"] = "3ds",
	["Super Nintendo MSU-1"] = "snes_msu-1",
  ["famicom"] = "nes",
  ["Nintendo Entertainment System"] = "nes",
  ["Super Nintendo Entertainment System"] = "snes",
  ["Nintendo GameCube"] = "gamecube",
  ["Nintendo Wii"] = "wii",

  // Bally
  ["Astrocade"] = "astrocde",
	["Bally Astrocade"] = "astrocde",
	["Bally Professional Arcade"] = "astrocde",

	// BBC
	["BBC Micro"] = "bbc",

	// Bandai
	["WonderSwan"] = "wswan",
	["WonderSwan Color"] = "wswanc",

	["Camputers Lynx"] = "camplynx",
	["Mega Duck"] = "megaduck",
	["Arcadia 2001"] = "arcadia",
	["Game Pocket Computer"] = "gamepock",
	["FM-7"] = "fm7",
	["Super A'can"] = "supracan",
	["Game Master"] = "gmaster",
	["V.Smile"] = "vsmile",
	["Game.com"] = "gamecom",
	["Oric 1 / Atmos"] = "oricatmos",
	["CD-i"] = "cdi",
	["Thomson MO/TO"] = "thomson",
	["Linux"] = "flatpak",
	["Visual Pinball"] = "vpinball",
	["Future Pinball"] = "fpinball",
	["Watara Supervision"] = "supervision",
	["FM Towns"] = "fmtowns",
	["WASM-4"] = "wasm4",
	["VC 4000"] = "vc4000",

	// Sony
	["PlayStation 4"] = "ps4",
	["PS4"] = "ps4",
	["PS Vita"] = "psvita",
	["PlayStation Vita"] = "psvita",

	// Atari
	["Atari ST"] = "atarist",
	["Atari STE"] = "atarist",
	["Atari Lynx"] = "lynx",
  ["AtariLynx"] = "lynx",
  ["Atari 2600 Supercharger"] = "atari2600",
	["Jaguar CD"] = "jaguarcd",

	// --- AMIGA ZUORDNUNG 
	["Amiga"] = "amiga500",                         // Standard-Fallback
	["Commodore Amiga"] = "amiga500",               // Häufigster ScreenScraper-Name
	["Amiga 500"] = "amiga500",
	["Amiga 500/600"] = "amiga500",
	["Amiga 600"] = "amiga500",
	["Amiga 1200"] = "amiga1200",
	["Amiga 4000"] = "amiga1200",
	["Commodore Amiga 1200"] = "amiga1200",
	["Amiga CD32"] = "amigacd32",
	["Commodore Amiga CD32"] = "amigacd32",
	["Amiga CDTV"] = "amigacdtv",
	["Commodore Amiga CDTV"] = "amigacdtv",
	["Plus/4"] = "cplus4",

	["Commodore 64"] = "c64",
	["C64"] = "c64",
	["Commodore 128"] = "c128",

	// Batocera 42+ nutzt "megacd" als Ordner für Sega CD; Systeme-Seite führt "segacd".
	// Wir mappen "segacd" -> "megacd" UND erlauben beide in All.
	["segacd"] = "megacd",
	["Mega-CD"] = "megacd",
	["Sega Pico"] = "pico",

	// --- SEGA ARCADE ---
	["Sega Model 2"] = "model2",
	["Model 2"] = "model2",
	["Sega Model 3"] = "model3",
	["Model 3"] = "model3",
	["Sega Chihiro"] = "chihiro",
	["Sega Naomi"] = "naomi",
	["Sega Naomi 2"] = "naomi2",
  ["Sega Master System"] = "mastersystem",
  ["Sega Mega Drive"] = "megadrive",
  ["Sega Genesis"] = "megadrive",
  ["Sega Saturn"] = "saturn",

  // Manche Auflistungen nennen "mame/model1" – Ordner heißt "model1".
  ["mame/model1"] = "model1",

	// RetroPie nennt teils "genesis" – in Batocera heißt das "megadrive".
	["genesis"] = "megadrive",

	// Häufige Schreibvarianten:
	["ZX Spectrum"] = "zxspectrum",
	["Sinclair ZX Spectrum"] = "zxspectrum",
	["Amstrad CPC"] = "amstradcpc",
	["Schneider CPC"] = "amstradcpc",
	["pc-engine"] = "pcengine",
	["PC Engine"] = "pcengine",
	["TurboGrafx-16"] = "pcengine",
	["TurboGrafx16"] = "pcengine",
	["pcengine-cd"] = "pcenginecd",
	["super-grafx"] = "supergrafx",
	["supergrafx"] = "supergrafx",

	// Epoch / Casio / SNK / Entex
	["Super Cassette Vision"] = "scv",
	["PV-1000"] = "pv1000",
	["Neo-Geo"] = "neogeo",
	["Neo-Geo MVS"] = "neogeo",
	["Adventure Vision"] = "advision",

	// Microsoft
	["Xbox 360"] = "xbox360",
  ["Microsoft Xbox"] = "xbox",
  ["PC Dos"] = "dos",
	["PC Win3.xx"] = "windows",
	["PC Win9X"] = "windows",
	["PC Windows"] = "windows",

	// Apple
	["Apple II"] = "apple2",
	["Apple IIgs"] = "apple2gs",

  ["MSX"] = "msx1",
  ["MSX 2"] = "msx2",
  ["MSX 2+"] = "msx2+",
  ["MSX Turbo R"] = "msxturbor",
  ["NEC PC-9801"] = "pc98",
  ["NEC PC-8801"] = "pc88",
  ["Sharp X68000"] = "x68000",
};

	/// <summary>
	/// Wählt den passenden Batocera-ROM-Ordner (Shortname) basierend auf ScreenScraper-Namen.
	/// Reihenfolge: nom_recalbox → nom_retropie (split) → Aliases.
	/// Gibt null zurück, wenn nichts passt.
	/// </summary>
	public static string? MapToBatoceraFolder(SystemNoms? noms)
	{
		if (noms == null)
			return null;

		// 1) Recalbox ist meist 1:1 verwendbar
		if (TryNormalize(noms.nom_recalbox, out var hit))
			return hit;

		// 2) RetroPie: komma-getrennt, wir nehmen den ersten gültigen Treffer
		if (!string.IsNullOrWhiteSpace(noms.nom_retropie))
		{
			foreach (var cand in noms.nom_retropie.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
				if (TryNormalize(cand, out hit))
					return hit;
		}

		// 3) Direkter Alias-Versuch auf anderen Feldern (falls jemand dort Shortnames trägt)
		if (TryNormalize(noms.nom_eu, out hit) || TryNormalize(noms.nom_us, out hit))
			return hit;

		// 4) kein Match
		return null;
	}

	private static bool TryNormalize(string? value, out string? normalized)
	{
		normalized = null;
		if (string.IsNullOrWhiteSpace(value)) return false;

		// Alias-Tabelle zuerst
		if (Aliases.TryGetValue(value, out var mapped))
		{
			// Auch Alias-Ziel muss existieren oder aufgenommen werden
			if (All.Contains(mapped) || Aliases.ContainsKey(mapped))
			{
				normalized = mapped;
				return true;
			}
		}

		// Direkter Treffer?
		if (All.Contains(value))
		{
			normalized = value;
			return true;
		}

		// Kein Treffer
		return false;
	}
}