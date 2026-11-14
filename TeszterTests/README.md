# TeszterTests

xUnit tesztprojekt a `TeszterLibrary` és a `TeszterWeb` fontos, hibaérzékeny részeihez.

```
dotnet test TeszterTests
```

| Mappa | Mit véd | Technika |
|---|---|---|
| `Services/ApiClientTests` | az API-kliensek: boríték-kezelés, hibaüzenetek, tokenek elszigetelése, névtelen útvonalak | hamis `HttpMessageHandler`, valódi DI-regisztráció |
| `Services/Images/*` | képfeltöltés: típusfelismerés tartalom alapján, méretkorlát, útvonalbejárás, XSS-tisztítás | ideiglenes mappa, kb. 90 eset |
| `Services/LessonClockTests` | az órai nézet tanóra-időbeosztása (óra / szünet / hétvége / óra után) és az iskolai időzóna (téli-nyári időszámítás) | tiszta logika, `TimeProvider` |
| `Security/PageAuthorizationTests` | minden `@page` oldal deklarálja a jogosultságát; csak az `Index` és az `Error` névtelen; mappánkénti szerepkörök | reflexió a TeszterWeb szerelvényén |
| `Components/*` | `ViewQinTest` (időkorlát, dupla beküldés, életciklus), `CountDown` és az órai nézet (`ClassLessonHost`) | bUnit, valós idő (a teljes futás ~1 perc) |

## Tudnivalók

* A tesztek `IAuthorizationHeaderProvider`-t és `IWebHostEnvironment`-et `DispatchProxy`-val hamisítják, így nincs mocking-könyvtár
  függőség, és a tesztek nem törnek el, ha ezek az interfészek új taggal bővülnek.
* A `TeszterLibrary` `InternalsVisibleTo`-val látja a tesztprojektet (a `TryDecodeDataUri` és társai `internal`-ok).
* A komponens-tesztek a `questions` mezőt és a `Next`/`Check` metódust reflexióval érik el; ha átnevezed őket, a tesztet is igazítsd.
* A `PageAuthorizationTests.KnownUnprotected` az ismert, védtelen maradványokat sorolja fel (jelenleg a WASM-ból örökölt `Authentication`).
  Töröld az oldalt, és vedd ki a listából is (a `Known_leftovers_still_exist` teszt jelzi, ha ezt elfelejted).
