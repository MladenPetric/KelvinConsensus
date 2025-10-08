# Tema 2

## Naziv teme
<strong>Konzistentan sistem</strong>  

## Specifikacija projekta

* Kreirati aplikaciju u _WCF-u_ koja pokreće 3 senzora temperature, koji na svakih 1 do 10 sekundi (nasumično) mjere temperaturu prostorije i upisuju je u bazu (svaki od senzora ima zasebnu bazu podataka).  

* Kreirati klijentsku aplikaciju koja će, koristeći _WCF_, komunicirati sa senzorima. Klijentska aplikacija mora pročitati iz bar 2 senzora vrijednost koja se nalazi u opsegu &plusmn;5 od srednje vrijednosti svih mjerenja kako bi je smatrala tačnom, inače će pokrenuti **poravnanje**.  

* Na svakih minut vremena, nezavisno od klijentske aplikacije, vrši se **poravnanje** senzora kroz _WCF_, tako da je poslednja vrijednost koja će ostati u svim tabelama **nakon poravnjavanja** jednaka izmedu svih senzora, i koja je jednaka _prosjeku poslednjih mjerenja_. Dok se senzori poravnavaju, svako čitanje koje klijentska aplikacija vrši mora da sačeka.  

* Pogledati _kvorum bazirane replikacije_ i kako mogu one pomoći u izradi projekta.  

* Pogledati _CAP_ teoremu i opisati kako je ona primjenjena na zahtjeve projekta.  

* Napisati detaljnu dokumentaciju o projektu, koja će sadržati opis projekta i njegove implementacije.  