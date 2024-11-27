This program is designed to run on the Jonel Archer batch computer and runs as a service.
Please ensure that Jonel have setup Archer Remote SQL access.  They will provide you with a SQL user / password for readonly access to the Archer database.
Verifi will provide you with an API user / password for the given plant.
Edit the appsettings.json file with site specific settings.
To install it as a service - open an admin command prompt and use:
sc.exe create VerifiWorker binpath=path\VerifiWorker.exe start=auto
