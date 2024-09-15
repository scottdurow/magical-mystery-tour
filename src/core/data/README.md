# Data

This folder contains the Contoso Real Estate core reference data and sample data for demos/test
Reference data contains lookups such as 'Fee Types' where the GUID must be the same across all environments.

To import reference data (before sample data) use:

```powershell
pac data import -d <repo_root>\src\core\data\reference-data\data.zip
```

To import sample data (before sample data) use:

```powershell
pac data import -d <repo_root>\src\core\data\sample-data\data.zip
```