# Couverture des commandes Greaseweazle

Cette matrice est vérifiée contre la liste `actions` de `greaseweazle/cli.py` dans le dépôt officiel Greaseweazle. Elle indique le parcours choisi dans GW GUI afin qu’aucune commande ne soit perdue dans un écran général surchargé.

| Commande | Emplacement GW GUI | Présentation |
|---|---|---|
| `info` | Options → Diagnostics → Informations | Dialogue ponctuel |
| `read` | Onglet Lecture | Parcours principal avec profils |
| `write` | Onglet Écriture | Parcours principal avec profils et confirmation |
| `convert` | Onglet Conversion | Conversion simple ou multiple |
| `erase` | Onglet Outils | Action destructive avec confirmation |
| `clean` | Onglet Outils | Maintenance avec confirmation du disque de nettoyage |
| `seek` | Options → Diagnostics → Déplacer la tête | Dialogue ponctuel |
| `delays` | Options → Matériel → Temporisations | Dialogue matériel |
| `update` | Options → Matériel → Firmware | Dialogue matériel avec avertissement bootloader |
| `pin` | Options → Matériel → Broches | Dialogue matériel |
| `reset` | Options → Matériel → Réinitialiser | Dialogue matériel |
| `bandwidth` | Options → Diagnostics → Bande passante USB | Dialogue ponctuel |
| `rpm` | Options → Diagnostics → Vitesse RPM | Dialogue ponctuel |
| `align` | Options → Diagnostics → Alignement du lecteur | Dialogue complet de diagnostic mécanique |

`list_ports_windows.py` et `util.py` sont des modules internes des Host Tools, pas des actions proposées par `gw`; ils ne nécessitent donc pas d’écran propre. La détection des ports Windows est néanmoins utilisée par la configuration matérielle.

## Alignement du lecteur

Le dialogue `align` couvre les paramètres publiés par les Host Tools : contrôleur, lecteur, pistes obligatoires, révolutions, nombre de lectures, format, `diskdefs.cfg`, flux brut, faux index ou secteurs matériels, ajustement de vitesse, PLL, densité ou TG43 et inversion flippy. Les combinaisons mutuellement exclusives et les valeurs structurées sont validées avant activation du bouton Exécuter.
