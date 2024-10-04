import sqlite3

# Connexion à la base de données (création si elle n'existe pas)
conn = sqlite3.connect('Sanssoussi.db')
cursor = conn.cursor()

# Fonction pour insérer des données dans la table AspNetRole
def insert_AspNetRole():
    asp_rnet_role = [
        ('0', 'Admin')
    ]
    cursor.executemany('INSERT INTO AspNetRoles (Id, Name) VALUES (?, ?)', asp_rnet_role)

# Fonction pour insérer des données dans la table AspNetUserRoles
def insert_UserRole():
    user_role = [
        ('050b7806-0fbd-4165-90d6-27ff654da786', '0')
    ]
    cursor.executemany('INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (?, ?)', user_role)


# Exécuter les fonctions pour insérer les données
insert_AspNetRole()
insert_UserRole()

# Commit les changements et ferme la connexion
conn.commit()
conn.close()

print("Base de données peuplée avec succès !")
