# Achever la séparation du backend Bandai

- [ ] Achever la séparation restante du backend Bandai.
  - [ ] Corriger les dépendances encore présentes.
    - [ ] Modifier `src/GWGUI.Emulation.Bandai/Common/Machines/Common/Functions/ConfigurationFunctions.cs`, `RuntimeFunctions.cs`, `Common/Services/Engine.cs`, `EmulationGlobalUsings.cs` et leurs dépendances : déléguer les décisions propres aux cœurs, conserver les contrats neutres dans Common et les conversions natives dans leurs adaptateurs ; préciser les fichiers et déplacements avant réalisation.
