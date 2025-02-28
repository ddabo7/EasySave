# EasySave v3.0

EasySave est une application de sauvegarde avec interface graphique développée en C# .NET Core. Elle permet de gérer et d'exécuter des travaux de sauvegarde avec support pour les sauvegardes complètes et différentielles, ainsi qu'une console déportée pour le contrôle à distance.

## Fonctionnalités

### Interface Utilisateur
- Interface graphique moderne avec WPF
- Support multilingue (Français/Anglais)
- Console déportée pour le contrôle à distance via TCP
- Visualisation en temps réel de l'état des sauvegardes

### Travaux de Sauvegarde
- Gestion des travaux de sauvegarde
- Deux types de sauvegarde :
  - Sauvegarde complète
  - Sauvegarde différentielle
- Support de différents types de stockage :
  - Disques locaux
  - Disques externes
  - Lecteurs réseau

### Sécurité et Performance
- Cryptage des fichiers sensibles
- Gestion des fichiers prioritaires
- Limitation de la bande passante réseau
- Détection des logiciels métier
- Gestion des fichiers volumineux

### Logging et Monitoring
- Journalisation détaillée au format JSON
- Suivi en temps réel de l'état des sauvegardes
- Fichier d'état (state.json) avec :
  - Progression des travaux
  - État des sauvegardes
  - Messages d'état
  - Horodatage des mises à jour

### Console Déportée
- Connexion via TCP (port 5001)
- Commandes disponibles :
  - `list` : Affiche la liste des jobs de sauvegarde
  - `status` : Affiche l'état des jobs en cours
  - `start <nom_job>` : Démarre un job de sauvegarde
  - `stop <nom_job>` : Arrête un job de sauvegarde
  - `help` : Affiche l'aide

## Prérequis

- .NET Core 8.0 ou supérieur
- Système d'exploitation compatible (Windows, macOS, Linux)

## Installation

1. Clonez le dépôt :
```bash
git clone [url-du-depot]
```

2. Accédez au répertoire du projet :
```bash
cd EASYSAVE
```

3. Compilez le projet :
```bash
dotnet build
```

## Utilisation

### Lancement de l'application

```bash
dotnet run --project EasySave/EasySave.csproj
```

### Utilisation de la Console Déportée

1. Lancez l'application EasySave
2. Dans un terminal, connectez-vous à la console déportée :
```bash
nc localhost 5001
```
3. Utilisez les commandes disponibles pour gérer les sauvegardes

### Configuration

Les paramètres suivants peuvent être configurés :
- Extensions des fichiers à crypter
- Extensions des fichiers prioritaires
- Liste des logiciels métier à détecter
- Limite de bande passante réseau
- Langue de l'interface

## Journalisation

### Logs Journaliers (JSON)
- Horodatage
- Nom de la sauvegarde
- Chemins source et cible (format UNC)
- Taille des fichiers
- Temps de transfert et de cryptage
- Type de sauvegarde
- Hash des fichiers (source et cible)

### État en Temps Réel (state.json)
- Nom du job
- État actuel
- Progression
- Message d'état
- Dernière mise à jour
- Nombre de fichiers traités
- Taille totale traitée

## Architecture

L'application suit une architecture MVVM (Model-View-ViewModel) avec :
- Interface utilisateur WPF
- Console déportée TCP
- Gestion des états en temps réel
- Système de logging extensible
- Services modulaires pour :
  - Cryptage
  - Détection des logiciels métier
  - Gestion de la bande passante
  - Contrôle des sauvegardes

## Sécurité

- Cryptage des fichiers sensibles
- Pas de stockage de données sensibles en clair
- Validation des entrées utilisateur
- Gestion sécurisée des connexions TCP

## Évolutions futures

- Amélioration de la console déportée
- Support de nouveaux types de stockage
- Intégration avec d'autres outils de sauvegarde
- Amélioration de la sécurité
- Support de nouvelles langues
