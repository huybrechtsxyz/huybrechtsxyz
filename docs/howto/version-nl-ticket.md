Goedemiddag

Ik probeer in een Docker Swarm cluster via Let's Encrypt wildcard certificaten aan te maken met behulp van de ACME-DNS-01 challenge. De setup maakt gebruik van Traefik v3 als reverse proxy en de LEGO DNS provider voor Versio.

Domeinnaam: huybrechts.dev
DNS-beheer: via Versio.nl
A-Records: - huybrechts.dev → 185.47.174.65 - *.huybrechts.dev → 185.47.174.65

Doel:
Wildcard SSL-certificaten genereren via Traefik/LEGO met ACME-DNS-01 challenge.

Probleem:
Bij het uitvoeren van de certificaat-aanvraag krijg ik volgende foutmelding:
-> acme: error: 401 Unauthorized
-> De gebruikte gebruikersnaam en API-wachtwoord zijn correct gevalideerd door deze hard coded in het docker compose script te plaatsen

Vraag:
Dient het publieke IP-adres van mijn server (185.47.174.65) mogelijk gewhitelist te worden om toegang te krijgen tot de Versio DNS API via ACME? Zo ja, waar in de interface kan ik dit uitvoeren?

Indien er andere vereisten of instellingen nodig zijn voor correct gebruik van de DNS API met LEGO/Traefik, hoor ik dit graag.

Alvast bedankt

Mvg
Vincent