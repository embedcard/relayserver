# Entwicklungssetup

Zur Entwicklung am RelayServer sind folgende Schritte durchzuführen:

* Installieren von Node.js mit einer Version die in der Datei `.nvmrc` vorgegeben ist (NVM für Windows kann `.nvmrc`-Dateien aktuell nicht auswerden).
* Installieren der notwendigen Node packages: `npm install`.
* Gulp mit dem Task `npm run watch` einmal durchlaufen lassen  
  Dies erstellt die .css files aus den .less Dateien.
* In der Datei `App.config` das Setting `ManagementWebLocation` auf den Wert `"..\..\..\Thinktecture.Relay.ManagementWeb"` einstellen, damit der RelayServer das ManagementWeb findet und ausliefern kann.  
  Alternativ dazu kann auch der Ordner `\Thinktecture.Relay.ManagementWeb` in den Ordner `\Thinktecture.Relay.Server\bin\{Configuration}\` kopiert werden.
